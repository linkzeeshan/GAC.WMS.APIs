using System.ComponentModel.DataAnnotations;

namespace GAC.WMS.Integrations.Domain.Entities
{
    public class FileProcessingJob : AuditableEntity
    {
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string FileType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        
        [Required]
        [StringLength(255)]
        public string SourcePath { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? ProcessingPath { get; set; }
        
        [StringLength(255)]
        public string? ArchivePath { get; set; }
        
        [StringLength(50)]
        public string? CustomerCode { get; set; }
        
        [StringLength(100)]
        public string? FileHash { get; set; }
        
        public int TotalRecords { get; set; }
        
        public int ProcessedRecords { get; set; }
        
        public int FailedRecords { get; set; }
        
        public DateTime ScheduledTime { get; set; }
        
        public DateTime? StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }
        
        public int RetryCount { get; set; }
        
        public DateTime? NextRetryTime { get; set; }
        
        // Audit fields inherited from AuditableEntity
    }
}
