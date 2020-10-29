namespace DevelopmentHub.BusinessLogic
{
    using DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;

    /// <summary>
    /// Concrete implementation of <see cref="IRepositoryFactory"/>.
    /// </summary>
    public class RepositoryFactory : IRepositoryFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryFactory"/> class.
        /// </summary>
        /// <param name="orgSvc">Organization service.</param>
        public RepositoryFactory(IOrganizationService orgSvc)
        {
            this.OrganizationService = orgSvc;
        }

        /// <summary>
        /// Gets the organization service.
        /// </summary>
        public IOrganizationService OrganizationService { get; private set; }

        /// <inheritdoc/>
        public ICrmRepository<TEntity> GetRepository<TContext, TEntity>()
            where TEntity : Entity, new()
            where TContext : OrganizationServiceContext
        {
            return new CrmRepository<TContext, TEntity>(this.OrganizationService);
        }
    }
}