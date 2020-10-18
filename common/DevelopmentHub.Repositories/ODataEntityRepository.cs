namespace DevelopmentHub.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DevelopmentHub.Model;

    /// <summary>
    /// Implementation for <see cref="IODataEntityRepository{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The OData entity.</typeparam>
    public class ODataEntityRepository<TEntity> : IODataEntityRepository<TEntity>
        where TEntity : ODataEntity, new()
    {
        private readonly IODataClient oDataClient;
        private readonly string entitySet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntityRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="oDataClient">The OData client.</param>
        public ODataEntityRepository(IODataClient oDataClient)
        {
            this.oDataClient = oDataClient;
            this.entitySet = new TEntity().EntitySet;
        }

        /// <inheritdoc/>
        public async Task<Guid> CreateAsync(TEntity entity)
        {
            var response = await this.oDataClient.PostAsync(this.entitySet, entity).ConfigureAwait(false);

            return Guid.Parse(response.ResponseHeaders.Get(ODataHeaders.ODataEntityId));
        }

        /// <inheritdoc/>
        public Task DeleteAsync(Guid entityId)
        {
            return this.oDataClient.DeleteAsync($"{this.entitySet}({entityId.ToString()})");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TEntity>> FindAsync(string filter, string[] fields)
        {
            var response = await this.oDataClient
                .RetrieveMultipleAsync<TEntity>(this.entitySet, filter: filter, fields: fields)
                .ConfigureAwait(false);

            return response.Value;
        }

        /// <inheritdoc/>
        public Task<TEntity> RetrieveAsync(Guid entityId, string[] fields = null)
        {
            return this.oDataClient.RetrieveAsync<TEntity>(this.entitySet, entityId, fields);
        }

        /// <inheritdoc/>
        public Task UpdateAsync(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return this.oDataClient.PatchAsync($"{this.entitySet}({entity.EntityId})", entity);
        }
    }
}
