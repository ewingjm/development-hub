namespace DevelopmentHub.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DevelopmentHub.Model;

    /// <summary>
    /// Interface for an OData entity repository.
    /// </summary>
    /// <typeparam name="TEntity">The OData entity.</typeparam>
    public interface IODataEntityRepository<TEntity>
        where TEntity : ODataEntity, new()
    {
        /// <summary>
        /// Retrieve a record by ID.
        /// </summary>
        /// <param name="entityId">The record ID.</param>
        /// <param name="fields">The fields to select.</param>
        /// <returns>The record.</returns>
        Task<TEntity> RetrieveAsync(Guid entityId, string[] fields = null);

        /// <summary>
        /// Retrieve records using the provided predicate.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="fields">The fields to select.</param>
        /// <returns>Selected records matching the given predicate.</returns>
        Task<IEnumerable<TEntity>> FindAsync(string filter, string[] fields = null);

        /// <summary>
        /// Create a record.
        /// </summary>
        /// <param name="entity">The record to create.</param>
        /// <returns>The ID of the created record.</returns>
        Task<Guid> CreateAsync(TEntity entity);

        /// <summary>
        /// Update a record.
        /// </summary>
        /// <param name="entity">The updated record.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Delete a record.
        /// </summary>
        /// <param name="entityId">The ID of the entity to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteAsync(Guid entityId);
    }
}
