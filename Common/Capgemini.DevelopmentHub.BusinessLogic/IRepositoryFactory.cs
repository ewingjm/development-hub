﻿namespace Capgemini.DevelopmentHub.BusinessLogic
{
    using Capgemini.DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;

    /// <summary>
    /// Factory for repositories.
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Get a repository for the given entity.
        /// </summary>
        /// <typeparam name="TContext">The context.</typeparam>
        /// <typeparam name="TEntity">The entity.</typeparam>
        /// <returns>A repository for the given entity.</returns>
        ICrmRepository<TEntity> GetRepository<TContext, TEntity>()
            where TEntity : Entity, new()
            where TContext : OrganizationServiceContext;
    }
}