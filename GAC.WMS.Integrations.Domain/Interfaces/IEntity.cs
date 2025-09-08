namespace GAC.WMS.Integrations.Domain.Interfaces
{
    /// <summary>
    /// Base interface for all entities
    /// </summary>
    public interface IEntity
    {
    }

    /// <summary>
    /// Interface for entities with integer primary key
    /// </summary>
    public interface IEntity<TKey> : IEntity where TKey : IEquatable<TKey>
    {
        TKey Id { get; set; }
    }
}
