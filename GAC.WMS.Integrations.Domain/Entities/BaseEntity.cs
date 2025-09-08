using GAC.WMS.Integrations.Domain.Interfaces;

namespace GAC.WMS.Integrations.Domain.Entities
{
    /// <summary>
    /// Base class for all entities with integer primary key
    /// </summary>
    public abstract class BaseEntity : IEntity<int>
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// Base class for all auditable entities
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
