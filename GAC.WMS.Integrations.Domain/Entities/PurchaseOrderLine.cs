using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAC.WMS.Integrations.Domain.Entities
{
    public class PurchaseOrderLine : BaseEntity
    {
        
        [Required]
        public int LineNumber { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        // Foreign keys
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        
        // Navigation properties
        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
