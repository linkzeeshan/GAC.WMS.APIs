using GAC.WMS.Integrations.Domain.Entities;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace GAC.WMS.Integrations.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class XmlImportJob : IJob
    {
        private readonly ILogger<XmlImportJob> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public XmlImportJob(
            ILogger<XmlImportJob> logger,
            ApplicationDbContext dbContext,
            IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting XML import job at {Time}", DateTime.UtcNow);

            try
            {
                // Get configuration
                var baseDirectory = _configuration["FileIntegration:BaseDirectory"];
                var sourceDirectory = Path.Combine(baseDirectory!, _configuration["FileIntegration:SourceDirectory"]!);
                var processingDirectory = Path.Combine(baseDirectory!, _configuration["FileIntegration:ProcessingDirectory"]!);
                var archiveDirectory = Path.Combine(baseDirectory!, _configuration["FileIntegration:ArchiveDirectory"]!);
                var errorDirectory = Path.Combine(baseDirectory!, _configuration["FileIntegration:ErrorDirectory"]!);
                
                // Ensure directories exist
                Directory.CreateDirectory(sourceDirectory);
                Directory.CreateDirectory(processingDirectory);
                Directory.CreateDirectory(archiveDirectory);
                Directory.CreateDirectory(errorDirectory);

                // Get file patterns
                var filePatterns = _configuration.GetSection("FileIntegration:FilePatterns").Get<string[]>() ?? new[] { "*.xml" };

                // Find files to process
                var filesToProcess = new List<string>();
                foreach (var pattern in filePatterns)
                {
                    filesToProcess.AddRange(Directory.GetFiles(sourceDirectory, pattern));
                }

                _logger.LogInformation("Found {Count} files to process", filesToProcess.Count);

                // Process each file
                foreach (var filePath in filesToProcess)
                {
                    var fileName = Path.GetFileName(filePath);
                    var processingPath = Path.Combine(processingDirectory, fileName);
                    var archivePath = Path.Combine(archiveDirectory, fileName);
                    var errorPath = Path.Combine(errorDirectory, fileName);

                    try
                    {
                        // Calculate file hash for deduplication
                        var fileHash = await CalculateFileHashAsync(filePath);
                        
                        // Check if file was already processed
                        var existingJob = await _dbContext.FileProcessingJobs
                            .FirstOrDefaultAsync(j => j.FileHash == fileHash);
                        
                        if (existingJob != null)
                        {
                            _logger.LogInformation("Skipping duplicate file {FileName} (hash: {FileHash})", fileName, fileHash);
                            File.Move(filePath, archivePath, true);
                            continue;
                        }

                        // Create file processing job record
                        var fileProcessingJob = new FileProcessingJob
                        {
                            FileName = fileName,
                            FileType = Path.GetExtension(fileName).TrimStart('.').ToUpper(),
                            Status = "Processing",
                            SourcePath = filePath,
                            ProcessingPath = processingPath,
                            ArchivePath = archivePath,
                            FileHash = fileHash,
                            ScheduledTime = DateTime.UtcNow,
                            StartTime = DateTime.UtcNow
                        };

                        _dbContext.FileProcessingJobs.Add(fileProcessingJob);
                        await _dbContext.SaveChangesAsync();

                        // Move file to processing directory
                        File.Move(filePath, processingPath, true);

                        // Process XML file
                        await ProcessXmlFileAsync(processingPath, fileProcessingJob);

                        // Update job status
                        fileProcessingJob.Status = "Completed";
                        fileProcessingJob.EndTime = DateTime.UtcNow;
                        
                        // Move to archive
                        File.Move(processingPath, archivePath, true);
                        
                        _logger.LogInformation("Successfully processed file {FileName}", fileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file {FileName}", fileName);
                        
                        // Move to error directory if file exists
                        if (File.Exists(processingPath))
                        {
                            File.Move(processingPath, errorPath, true);
                        }
                        else if (File.Exists(filePath))
                        {
                            File.Move(filePath, errorPath, true);
                        }
                        
                        // Log error
                        var errorLog = new ErrorLog
                        {
                            Source = "XmlImportJob",
                            ErrorType = ex.GetType().Name,
                            Message = ex.Message,
                            StackTrace = ex.StackTrace,
                            EntityType = "File",
                            EntityId = fileName
                        };
                        _dbContext.ErrorLogs.Add(errorLog);
                        await _dbContext.SaveChangesAsync();
                    }
                }

                _logger.LogInformation("XML import job completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing XML import job");
                
                // Log error
                var errorLog = new ErrorLog
                {
                    Source = "XmlImportJob",
                    ErrorType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    EntityType = "Job",
                    EntityId = "XmlImportJob"
                };
                _dbContext.ErrorLogs.Add(errorLog);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task<string> CalculateFileHashAsync(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = await md5.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private async Task ProcessXmlFileAsync(string filePath, FileProcessingJob job)
        {
            // Load XML document
            var xmlDoc = XDocument.Load(filePath);
            var rootElement = xmlDoc.Root?.Name.LocalName.ToLower();

            if (rootElement == null)
            {
                throw new Exception("Invalid XML file: Root element not found");
            }

            // Process based on root element
            switch (rootElement)
            {
                case "purchaseorders":
                    await ProcessPurchaseOrdersXmlAsync(xmlDoc, job);
                    break;
                case "salesorders":
                    await ProcessSalesOrdersXmlAsync(xmlDoc, job);
                    break;
                default:
                    throw new Exception($"Unsupported XML root element: {rootElement}");
            }
        }

        private async Task ProcessPurchaseOrdersXmlAsync(XDocument xmlDoc, FileProcessingJob job)
        {
            _logger.LogInformation("Processing purchase orders XML file: {FileName}", job.FileName);
            
            var poElements = xmlDoc.Root?.Elements("PurchaseOrder") ?? Enumerable.Empty<XElement>();
            var processedCount = 0;
            var failedCount = 0;

            foreach (var element in poElements)
            {
                try
                {
                    var poNumber = GetElementValue(element, "PONumber");
                    if (string.IsNullOrEmpty(poNumber))
                    {
                        throw new Exception("Purchase order number is required");
                    }

                    // Check if purchase order already exists
                    var existingPO = await _dbContext.PurchaseOrders
                        .FirstOrDefaultAsync(po => po.PONumber == poNumber);

                    if (existingPO != null)
                    {
                        _logger.LogInformation("Purchase order {PONumber} already exists, updating", poNumber);
                        // Update existing purchase order logic would go here
                    }
                    else
                    {
                        _logger.LogInformation("Creating new purchase order {PONumber}", poNumber);
                        // Create new purchase order logic would go here
                    }

                    // Create outbox message for WMS integration
                    var integrationMessage = new IntegrationMessage
                    {
                        Aggregate = "PurchaseOrder",
                        AggregateId = poNumber,
                        Endpoint = "api/purchase-orders",
                        Payload = element.ToString(),
                        Status = IntegrationMessageStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        NextAttemptAt = DateTime.UtcNow
                    };

                    _dbContext.IntegrationMessages.Add(integrationMessage);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing purchase order from XML");
                    failedCount++;
                }
            }

            await _dbContext.SaveChangesAsync();

            // Update job statistics
            job.TotalRecords = processedCount + failedCount;
            job.ProcessedRecords = processedCount;
            job.FailedRecords = failedCount;
        }

        private async Task ProcessSalesOrdersXmlAsync(XDocument xmlDoc, FileProcessingJob job)
        {
            _logger.LogInformation("Processing sales orders XML file: {FileName}", job.FileName);
            
            var soElements = xmlDoc.Root?.Elements("SalesOrder") ?? Enumerable.Empty<XElement>();
            var processedCount = 0;
            var failedCount = 0;

            foreach (var element in soElements)
            {
                try
                {
                    var soNumber = GetElementValue(element, "SONumber");
                    if (string.IsNullOrEmpty(soNumber))
                    {
                        throw new Exception("Sales order number is required");
                    }

                    // Check if sales order already exists
                    var existingSO = await _dbContext.SalesOrders
                        .FirstOrDefaultAsync(so => so.SONumber == soNumber);

                    if (existingSO != null)
                    {
                        _logger.LogInformation("Sales order {SONumber} already exists, updating", soNumber);
                        // Update existing sales order logic would go here
                    }
                    else
                    {
                        _logger.LogInformation("Creating new sales order {SONumber}", soNumber);
                        // Create new sales order logic would go here
                    }

                    // Create outbox message for WMS integration
                    var integrationMessage = new IntegrationMessage
                    {
                        Aggregate = "SalesOrder",
                        AggregateId = soNumber,
                        Endpoint = "api/sales-orders",
                        Payload = element.ToString(),
                        Status = IntegrationMessageStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        NextAttemptAt = DateTime.UtcNow
                    };

                    _dbContext.IntegrationMessages.Add(integrationMessage);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing sales order from XML");
                    failedCount++;
                }
            }

            await _dbContext.SaveChangesAsync();

            // Update job statistics
            job.TotalRecords = processedCount + failedCount;
            job.ProcessedRecords = processedCount;
            job.FailedRecords = failedCount;
        }

        private string? GetElementValue(XElement parent, string elementPath)
        {
            if (parent == null)
                return null;

            if (!elementPath.Contains('/'))
            {
                return parent.Element(elementPath)?.Value;
            }

            var parts = elementPath.Split('/');
            var current = parent;

            foreach (var part in parts)
            {
                current = current.Element(part);
                if (current == null)
                    return null;
            }

            return current.Value;
        }
    }
}
