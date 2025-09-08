using System.ComponentModel.DataAnnotations;
using GAC.WMS.Integrations.Domain.Entities;

namespace GAC.WMS.Integrations.Domain.Entities
{
    public class ErrorLog : BaseEntity
    {
        
        [Required]
        [StringLength(100)]
        public string Source { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ErrorType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(4000)]
        public string? StackTrace { get; set; }
        
        [StringLength(255)]
        public string? EntityType { get; set; }
        
        [StringLength(50)]
        public string? EntityId { get; set; }
        
        [StringLength(50)]
        public string? CustomerCode { get; set; }
        
        public int? FileProcessingJobId { get; set; }
        
        public bool IsResolved { get; set; }
        
        public DateTime? ResolvedDate { get; set; }
        
        [StringLength(1000)]
        public string? ResolutionNotes { get; set; }
        
        // Audit field
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
