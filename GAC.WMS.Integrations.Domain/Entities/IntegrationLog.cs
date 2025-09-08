using System.ComponentModel.DataAnnotations;

namespace GAC.WMS.Integrations.Domain.Entities
{
    public class IntegrationLog : BaseEntity
    {
        
        [Required]
        [StringLength(50)]
        public string IntegrationType { get; set; } = string.Empty; // API or File
        
        [Required]
        [StringLength(50)]
        public string EntityType { get; set; } = string.Empty; // Customer, Product, PurchaseOrder, SalesOrder
        
        [StringLength(50)]
        public string? EntityId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Success";
        
        [StringLength(1000)]
        public string? Message { get; set; }
        
        [StringLength(4000)]
        public string? RequestData { get; set; }
        
        [StringLength(4000)]
        public string? ResponseData { get; set; }
        
        [StringLength(50)]
        public string? CustomerCode { get; set; }
        
        public int? FileProcessingJobId { get; set; }
        
        // Audit field
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
