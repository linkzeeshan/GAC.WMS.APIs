using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAC.WMS.Integrations.Domain.Entities
{
    public class PurchaseOrder : AuditableEntity
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
        
        [Column(TypeName = "decimal(18,2)")]
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
        
        // Foreign key
        public int? CustomerId { get; set; }
        
        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        
        public virtual ICollection<PurchaseOrderLine> POLines { get; set; } = new List<PurchaseOrderLine>();
        
        // Audit fields inherited from AuditableEntity
    }
}
