using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAC.WMS.Integrations.Domain.Entities
{
    public class SalesOrder : AuditableEntity
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
        
        // Foreign key for Customer
        public int? CustomerEntityId { get; set; }
        
        // Navigation properties
        [ForeignKey("CustomerEntityId")]
        public virtual Customer? CustomerEntity { get; set; }
        
        public virtual ICollection<SalesOrderLine> SOLines { get; set; } = new List<SalesOrderLine>();
        
        // Audit fields inherited from AuditableEntity
    }
}
