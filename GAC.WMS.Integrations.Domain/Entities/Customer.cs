using System.ComponentModel.DataAnnotations;
using GAC.WMS.Integrations.Domain.Entities;

namespace GAC.WMS.Integrations.Domain.Entities
{
    public class Customer : AuditableEntity
    {
        
        [Required]
        [StringLength(50)]
        public string CustomerId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        // Address information
        [StringLength(200)]
        public string? Street { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(100)]
        public string? StateProvince { get; set; }
        
        [StringLength(20)]
        public string? PostalCode { get; set; }
        
        [StringLength(100)]
        public string? Country { get; set; }
        
        // Additional properties
        [StringLength(50)]
        public string? CustomerType { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(50)]
        public string? TaxIdentifier { get; set; }
        
        // Navigation properties
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
        
        // Audit fields inherited from AuditableEntity
    }
}
