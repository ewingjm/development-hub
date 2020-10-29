namespace DevelopmentHub.BusinessLogic
{
    using DevelopmentHub.Model;
    using DevelopmentHub.Repositories;

    /// <summary>
    /// Factory for OData repositories.
    /// </summary>
    public interface IODataRepositoryFactory
    {
        /// <summary>
        /// Gets or sets the <see cref="IODataClient"/>.
        /// </summary>
        IODataClient ODataClient { get; set; }

        /// <summary>
        /// Get a repository for the given entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity.</typeparam>
        /// <returns>A repository for the given entity.</returns>
        IODataEntityRepository<TEntity> GetRepository<TEntity>()
            where TEntity : ODataEntity, new();
    }
}
