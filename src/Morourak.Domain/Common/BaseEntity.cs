namespace Morourak.Domain.Common
{
    /// <summary>
    /// Base class for all entities.
    /// Contains common properties like Id, CreatedAt, and UpdatedAt.
    /// </summary>
    public abstract class BaseEntity<TId>
    {
        /// <summary>
        /// Primary key of the entity.
        /// </summary>
        public TId Id { get; set; }

        /// <summary>
        /// The date and time when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the entity was last updated.
        /// Nullable if never updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}