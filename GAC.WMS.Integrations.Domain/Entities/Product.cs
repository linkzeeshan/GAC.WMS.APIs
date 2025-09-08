using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GAC.WMS.Integrations.Domain.Entities
{
    public class Product : AuditableEntity
    {
        
        [Required]
        [StringLength(50)]
        public string ProductId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? Category { get; set; }
        
        [StringLength(20)]
        public string? UnitOfMeasure { get; set; }
        
        public decimal? Weight { get; set; }
        
        public decimal? Length { get; set; }
        
        public decimal? Width { get; set; }
        
        public decimal? Height { get; set; }
        
        [StringLength(50)]
        public string? Barcode { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Store additional attributes as JSON
        public string? Attributes { get; set; }
        
        // Helper methods for attributes
        public void SetAttributes(Dictionary<string, object> attributes)
        {
            Attributes = JsonSerializer.Serialize(attributes);
        }
        
        public Dictionary<string, object>? GetAttributes()
        {
            if (string.IsNullOrEmpty(Attributes))
                return null;
                
            return JsonSerializer.Deserialize<Dictionary<string, object>>(Attributes);
        }
        
        // Navigation properties
        public virtual ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = new List<PurchaseOrderLine>();
        public virtual ICollection<SalesOrderLine> SalesOrderLines { get; set; } = new List<SalesOrderLine>();
        
        // Audit fields inherited from AuditableEntity
    }
}
