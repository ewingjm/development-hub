namespace DevelopmentHub.BusinessLogic
{
    using DevelopmentHub.Model;
    using DevelopmentHub.Repositories;

    /// <summary>
    /// Implementation of <see cref="IODataRepositoryFactory"/>.
    /// </summary>
    public class ODataRepositoryFactory : IODataRepositoryFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRepositoryFactory"/> class.
        /// </summary>
        /// <param name="oDataClient">The OData client.</param>
        public ODataRepositoryFactory(IODataClient oDataClient)
        {
            this.ODataClient = oDataClient;
        }

        /// <inheritdoc/>
        public IODataClient ODataClient { get; set; }

        /// <inheritdoc/>
        public IODataEntityRepository<TEntity> GetRepository<TEntity>()
            where TEntity : ODataEntity, new()
        {
            return new ODataEntityRepository<TEntity>(this.ODataClient);
        }
    }
}
