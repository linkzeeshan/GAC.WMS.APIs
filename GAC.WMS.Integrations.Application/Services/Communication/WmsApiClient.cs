using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GAC.WMS.Integrations.Application.DTOs.Common;
using GAC.WMS.Integrations.Application.DTOs.Customers;
using GAC.WMS.Integrations.Application.DTOs.Products;
using GAC.WMS.Integrations.Application.DTOs.PurchaseOrders;
using GAC.WMS.Integrations.Application.DTOs.SalesOrders;
using GAC.WMS.Integrations.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace GAC.WMS.Integrations.Application.Services.Communication
{
    public class WmsApiClient : IWmsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WmsApiClient> _logger;
        private readonly AsyncPolicyWrap _resiliencePolicy;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IConfiguration _configuration;
        public WmsApiClient(
            HttpClient httpClient,
            ILogger<WmsApiClient> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            // Configure base URL and API key
            var baseUrl = _configuration["WmsApi:BaseUrl"];
            var apiKey = _configuration["WmsApi:ApiKey"];
            
            _httpClient.BaseAddress = new Uri(baseUrl ?? "https://api.example.com/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            }

            // Configure JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            // Configure resilience policies
            var retryCount = _configuration.GetValue<int>("Resilience:RetryCount", 3);
            var retryDelayMs = _configuration.GetValue<int>("Resilience:RetryDelayMilliseconds", 1000);
            var circuitBreakerFailureThreshold = _configuration.GetValue<double>("Resilience:CircuitBreakerFailureThreshold", 0.5);
            var circuitBreakerSamplingDuration = _configuration.GetValue<int>("Resilience:CircuitBreakerSamplingDurationSeconds", 60);
            var circuitBreakerDurationOfBreak = _configuration.GetValue<int>("Resilience:CircuitBreakerDurationOfBreakSeconds", 30);
            var timeoutSeconds = _configuration.GetValue<int>("Resilience:TimeoutSeconds", 30);

            // Retry policy
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                    retryCount,
                    retryAttempt => TimeSpan.FromMilliseconds(retryDelayMs * Math.Pow(2, retryAttempt - 1)), // Exponential backoff
                    (ex, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(ex, "Error calling WMS API (Attempt {RetryCount}). Retrying in {RetryDelay}ms", 
                            retryCount, timeSpan.TotalMilliseconds);
                    });

            // Circuit breaker policy
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: circuitBreakerFailureThreshold,
                    samplingDuration: TimeSpan.FromSeconds(circuitBreakerSamplingDuration),
                    minimumThroughput: 10,
                    durationOfBreak: TimeSpan.FromSeconds(circuitBreakerDurationOfBreak),
                    onBreak: (ex, breakDelay) =>
                    {
                        _logger.LogWarning(ex, "Circuit breaker opened for {BreakDelay}s", breakDelay.TotalSeconds);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Circuit breaker half-open");
                    });

            // Timeout policy
            var timeoutPolicy = Policy
                .TimeoutAsync(TimeSpan.FromSeconds(timeoutSeconds), TimeoutStrategy.Pessimistic, 
                    (context, timeSpan, task) =>
                    {
                        _logger.LogWarning("WMS API call timed out after {Timeout}s", timeSpan.TotalSeconds);
                        return Task.CompletedTask;
                    });

            // Combine policies
            _resiliencePolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        }

        public async Task<ApiResponseDto<CustomerDto>> CreateCustomerAsync(CustomerDto customer)
        {
            return await SendRequestAsync<CustomerDto>(HttpMethod.Post, "customers", customer);
        }

        public async Task<ApiResponseDto<object>> CreateCustomerAsync(string rawPayload)
        {
            return await SendRawRequestAsync<object>(HttpMethod.Post, "customers", rawPayload);
        }

        public async Task<ApiResponseDto<string>> CreateCustomersBatchAsync(BatchRequestDto<CustomerDto> batchRequest)
        {
            return await SendRequestAsync<string>(HttpMethod.Post, "customers/batch", batchRequest);
        }

        public async Task<ApiResponseDto<ProductDto>> CreateProductAsync(ProductDto product)
        {
            return await SendRequestAsync<ProductDto>(HttpMethod.Post, "products", product);
        }

        public async Task<ApiResponseDto<object>> CreateProductAsync(string rawPayload)
        {
            return await SendRawRequestAsync<object>(HttpMethod.Post, "products", rawPayload);
        }

        public async Task<ApiResponseDto<string>> CreateProductsBatchAsync(BatchRequestDto<ProductDto> batchRequest)
        {
            return await SendRequestAsync<string>(HttpMethod.Post, "products/batch", batchRequest);
        }

        public async Task<ApiResponseDto<PurchaseOrderDto>> CreatePurchaseOrderAsync(PurchaseOrderDto purchaseOrder)
        {
            return await SendRequestAsync<PurchaseOrderDto>(HttpMethod.Post, "purchase-orders", purchaseOrder);
        }

        public async Task<ApiResponseDto<object>> CreatePurchaseOrderAsync(string rawPayload)
        {
            return await SendRawRequestAsync<object>(HttpMethod.Post, "purchase-orders", rawPayload);
        }

        public async Task<ApiResponseDto<string>> CreatePurchaseOrdersBatchAsync(BatchRequestDto<PurchaseOrderDto> batchRequest)
        {
            return await SendRequestAsync<string>(HttpMethod.Post, "purchase-orders/batch", batchRequest);
        }

        public async Task<ApiResponseDto<SalesOrderDto>> CreateSalesOrderAsync(SalesOrderDto salesOrder)
        {
            return await SendRequestAsync<SalesOrderDto>(HttpMethod.Post, "sales-orders", salesOrder);
        }

        public async Task<ApiResponseDto<object>> CreateSalesOrderAsync(string rawPayload)
        {
            return await SendRawRequestAsync<object>(HttpMethod.Post, "sales-orders", rawPayload);
        }

        public async Task<ApiResponseDto<string>> CreateSalesOrdersBatchAsync(BatchRequestDto<SalesOrderDto> batchRequest)
        {
            return await SendRequestAsync<string>(HttpMethod.Post, "sales-orders/batch", batchRequest);
        }

        private async Task<ApiResponseDto<T>> SendRequestAsync<T>(HttpMethod method, string endpoint, object? data = null)
        {
            try
            {
                return await _resiliencePolicy.ExecuteAsync(async () =>
                {
                    var request = new HttpRequestMessage(method, endpoint);

                    if (data != null)
                    {
                        var json = JsonSerializer.Serialize(data, _jsonOptions);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    }

                    var response = await _httpClient.SendAsync(request);
                    
                    // Ensure successful response
                    response.EnsureSuccessStatusCode();

                    // Parse response
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponseDto<T>>(_jsonOptions);
                    
                    if (apiResponse == null)
                    {
                        throw new Exception("Failed to parse API response");
                    }

                    return apiResponse;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling WMS API endpoint: {Endpoint}", endpoint);
                
                return ApiResponseDto<T>.ErrorResponse("Error calling WMS API", new List<string> { ex.Message });
            }
        }

        private async Task<ApiResponseDto<T>> SendRawRequestAsync<T>(HttpMethod method, string endpoint, string rawPayload)
        {
            try
            {
                return await _resiliencePolicy.ExecuteAsync(async () =>
                {
                    var request = new HttpRequestMessage(method, endpoint);

                    if (!string.IsNullOrEmpty(rawPayload))
                    {
                        request.Content = new StringContent(rawPayload, Encoding.UTF8, "application/json");
                    }

                    var response = await _httpClient.SendAsync(request);
                    
                    // Ensure successful response
                    response.EnsureSuccessStatusCode();

                    // Parse response
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponseDto<T>>(_jsonOptions);
                    
                    if (apiResponse == null)
                    {
                        throw new Exception("Failed to parse API response");
                    }

                    return apiResponse;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling WMS API endpoint with raw payload: {Endpoint}", endpoint);
                
                return ApiResponseDto<T>.ErrorResponse("Error calling WMS API", new List<string> { ex.Message });
            }
        }
    }
}
