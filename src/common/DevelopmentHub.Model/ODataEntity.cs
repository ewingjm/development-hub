namespace DevelopmentHub.Model
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base class for OData entities.
    /// </summary>
    [DataContract]
    public abstract class ODataEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntity"/> class.
        /// </summary>
        public ODataEntity()
        {
        }

        /// <summary>
        /// Gets the entity set associated with the OData entity.
        /// </summary>
        [IgnoreDataMember]
        public abstract string EntitySet { get; }

        /// <summary>
        /// Gets the entity ID.
        /// </summary>
        [IgnoreDataMember]
        public abstract Guid EntityId { get; }
    }
}
