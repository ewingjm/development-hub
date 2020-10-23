namespace DevelopmentHub.Model.Responses
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data contract for a retrieve multiple response.
    /// </summary>
    /// <typeparam name="TEntity">The entity to retrieve.</typeparam>
    [DataContract]
    public class RetrieveMultipleResponse<TEntity>
    {
        /// <summary>
        /// Gets or sets a collection of entity records.
        /// </summary>
        [DataMember(Name = "value")]
        public IEnumerable<TEntity> Value { get; set; }
    }
}
