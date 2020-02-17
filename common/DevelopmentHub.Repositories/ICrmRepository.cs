namespace DevelopmentHub.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Late-bound repository interface.
    /// </summary>
    public interface ICrmRepository
    {
        /// <summary>
        /// Retrieve a record by ID.
        /// </summary>
        /// <param name="entityId">The record ID.</param>
        /// <param name="columns">The columns to select.</param>
        /// <returns>The record.</returns>
        Entity Retrieve(Guid entityId, string[] columns);

        /// <summary>
        /// Retrieve records using the provided predicate.
        /// </summary>
        /// <typeparam name="TObject">The type of object to select.</typeparam>
        /// <param name="filter">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>Selected records matching the given predicate.</returns>
        IQueryable<TObject> Find<TObject>(Expression<Func<Entity, bool>> filter, Expression<Func<Entity, TObject>> selector)
             where TObject : class;

        /// <summary>
        /// Retrieve records using the provided predicate and paging.
        /// </summary>
        /// <typeparam name="TObject">The type of object to select.</typeparam>
        /// <param name="filter">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <param name="take">The number of records to take.</param>
        /// <returns>Selected records matching the given predicate and page.</returns>
        IQueryable<TObject> Find<TObject>(Expression<Func<Entity, bool>> filter, Expression<Func<Entity, TObject>> selector, int skip, int take)
            where TObject : class;

        /// <summary>
        /// Create a record.
        /// </summary>
        /// <param name="entity">The record to create.</param>
        /// <returns>The ID of the created record.</returns>
        Guid Create(Entity entity);

        /// <summary>
        /// Update a record.
        /// </summary>
        /// <param name="entity">The updated record.</param>
        void Update(Entity entity);

        /// <summary>
        /// Delete a record.
        /// </summary>
        /// <param name="entity">The record to delete.</param>
        void Delete(Entity entity);

        /// <summary>
        /// Delete multiple records.
        /// </summary>
        /// <param name="entities">The entities to delete.</param>
        /// <param name="batchSize">The number of records to delete per batch.</param>
        void BulkDelete(IEnumerable<Entity> entities, int batchSize = 100);

        /// <summary>
        /// Sets the picture on an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="picture">The picture.</param>
        void SetEntityPicture(Entity entity, byte[] picture);

        /// <summary>
        /// Executes an on-demand workflow for an entity.
        /// </summary>
        /// <param name="entity">The entity to execute the workflow for.</param>
        /// <param name="workflowId">The ID of the workflow to execute.</param>
        void ExecuteWorkflowForEntity(Entity entity, Guid workflowId);
    }
}