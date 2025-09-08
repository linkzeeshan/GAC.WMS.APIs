using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAC.WMS.Integrations.Domain.Entities
{
    public class SalesOrderLine : BaseEntity
    {
        
        [Required]
        public int LineNumber { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        
        public DateTime? RequestedDeliveryDate { get; set; }
        
        // Foreign keys
        public int SalesOrderId { get; set; }
        public int ProductId { get; set; }
        
        // Navigation properties
        [ForeignKey("SalesOrderId")]
        public virtual SalesOrder SalesOrder { get; set; } = null!;
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
