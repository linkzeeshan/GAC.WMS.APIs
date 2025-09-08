using System.ComponentModel.DataAnnotations;

namespace GAC.WMS.Integrations.Domain.Entities
{
    public class IntegrationMessage : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Aggregate { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string AggregateId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Endpoint { get; set; } = string.Empty;
        
        [Required]
        public string Payload { get; set; } = string.Empty;
        
        public int Attempts { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime NextAttemptAt { get; set; }
        
        public DateTime? ProcessedAt { get; set; }
        
        [MaxLength(4000)]
        public string? LastError { get; set; }
        
        public IntegrationMessageStatus Status { get; set; }
    }

    public enum IntegrationMessageStatus
    {
        Pending = 0,
        Processing = 1,
        Succeeded = 2,
        Failed = 3,
        Abandoned = 4
    }
}
