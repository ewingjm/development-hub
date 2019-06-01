namespace Capgemini.DevelopmentHub.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Early-bound repository interface.
    /// </summary>
    /// <typeparam name="TEntity">Early-bound entity.</typeparam>
    public interface ICrmRepository<TEntity>
        where TEntity : Entity
    {
        /// <summary>
        /// Retrieve a record by ID.
        /// </summary>
        /// <param name="entityId">The record ID.</param>
        /// <param name="columns">The columns to select.</param>
        /// <returns>The record.</returns>
        TEntity Retrieve(Guid entityId, string[] columns);

        /// <summary>
        /// Retrieve a record using the provided predicate.
        /// </summary>
        /// <typeparam name="TObject">The type of object to select.</typeparam>
        /// <param name="filter">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>The first record matching the given predicate.</returns>
        TObject Retrieve<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector)
            where TObject : class;

        /// <summary>
        /// Retrieve records using the provided predicate.
        /// </summary>
        /// <typeparam name="TObject">The type of object to select.</typeparam>
        /// <param name="filter">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <returns>Selected records matching the given predicate.</returns>
        IQueryable<TObject> Find<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector)
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
        IQueryable<TObject> Find<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector, int skip, int take)
            where TObject : class;

        /// <summary>
        /// Retrieve records in bulk using the provided predicate and paging.
        /// </summary>
        /// <typeparam name="TObject">The type of object to select.</typeparam>
        /// <param name="filter">The predicate.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="maxRecords">The maximum number of records to return.</param>
        /// <returns>Selected records matching the given predicate and page.</returns>
        IList<TObject> FindAll<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector, int pageSize = 5000, int maxRecords = 200000)
            where TObject : class;

        /// <summary>
        /// Create a record.
        /// </summary>
        /// <param name="entity">The record to create.</param>
        /// <returns>The ID of the created record.</returns>
        Guid Create(TEntity entity);

        /// <summary>
        /// Update a record.
        /// </summary>
        /// <param name="entity">The updated record.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Delete a record.
        /// </summary>
        /// <param name="entity">The record to delete.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Delete multiple records.
        /// </summary>
        /// <param name="entities">The entities to delete.</param>
        /// <param name="batchSize">The number of records to delete per batch.</param>
        void BulkDelete(List<TEntity> entities, int batchSize = 100);
    }
}