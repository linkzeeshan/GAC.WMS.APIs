using System.ComponentModel.DataAnnotations;

namespace GAC.WMS.Integrations.Application.DTOs.SalesOrders
{
    public class SalesOrderDto
    {
        [Required]
        [StringLength(50)]
        public string SONumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string CustomerId { get; set; } = string.Empty;
        
        [Required]
        public DateTime OrderDate { get; set; }
        
        public DateTime? RequestedDeliveryDate { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Draft";
        
        [StringLength(3)]
        public string? Currency { get; set; } = "USD";
        
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }
        
        // Shipping address
        [StringLength(200)]
        public string? ShippingStreet { get; set; }
        
        [StringLength(100)]
        public string? ShippingCity { get; set; }
        
        [StringLength(100)]
        public string? ShippingStateProvince { get; set; }
        
        [StringLength(20)]
        public string? ShippingPostalCode { get; set; }
        
        [StringLength(100)]
        public string? ShippingCountry { get; set; }
        
        // Billing address
        [StringLength(200)]
        public string? BillingStreet { get; set; }
        
        [StringLength(100)]
        public string? BillingCity { get; set; }
        
        [StringLength(100)]
        public string? BillingStateProvince { get; set; }
        
        [StringLength(20)]
        public string? BillingPostalCode { get; set; }
        
        [StringLength(100)]
        public string? BillingCountry { get; set; }
        
        // Line items
        [Required]
        [MinLength(1, ErrorMessage = "At least one line item is required")]
        public List<SalesOrderLineDto> SOLines { get; set; } = new List<SalesOrderLineDto>();
    }
}
