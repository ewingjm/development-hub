namespace Capgemini.DevelopmentHub.BusinessLogic
{
    using Capgemini.DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;

    /// <summary>
    /// Concrete implementation of <see cref="IRepositoryFactory"/>.
    /// </summary>
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IOrganizationService orgSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryFactory"/> class.
        /// </summary>
        /// <param name="orgSvc">Organization service.</param>
        public RepositoryFactory(IOrganizationService orgSvc)
        {
            this.orgSvc = orgSvc;
        }

        /// <inheritdoc/>
        public ICrmRepository<TEntity> GetRepository<TContext, TEntity>()
            where TEntity : Entity, new()
            where TContext : OrganizationServiceContext
        {
            return new CrmRepository<TContext, TEntity>(this.orgSvc);
        }
    }
}