using System.ComponentModel.DataAnnotations;

namespace GAC.WMS.Integrations.Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderDto
    {
        [Required]
        [StringLength(50)]
        public string PONumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string VendorId { get; set; } = string.Empty;
        
        [Required]
        public DateTime OrderDate { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
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
        
        public int? CustomerId { get; set; }
        
        // Line items
        [Required]
        [MinLength(1, ErrorMessage = "At least one line item is required")]
        public List<PurchaseOrderLineDto> POLines { get; set; } = new List<PurchaseOrderLineDto>();
    }
}
