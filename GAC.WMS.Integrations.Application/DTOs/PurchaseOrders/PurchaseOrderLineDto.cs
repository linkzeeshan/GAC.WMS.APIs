using System.ComponentModel.DataAnnotations;

namespace GAC.WMS.Integrations.Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderLineDto
    {
        [Required]
        public int LineNumber { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
    }
}
