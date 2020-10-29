namespace DevelopmentHub.Repositories
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;

    /// <summary>
    /// Extensions to <see cref="IOrganizationService"/>.
    /// </summary>
    public static class OrganizationServiceExtensions
    {
        /// <summary>
        /// Create a new <see cref="OrganizationServiceContext"/>.
        /// </summary>
        /// <typeparam name="TCrmContext">The type of <see cref="OrganizationServiceContext"/>.</typeparam>
        /// <param name="orgService">Organization service.</param>
        /// <returns>A new context.</returns>
        public static TCrmContext CreateNewCrmContext<TCrmContext>(this IOrganizationService orgService)
            where TCrmContext : OrganizationServiceContext
        {
            var context = (TCrmContext)Activator.CreateInstance(typeof(TCrmContext), orgService);
            context.MergeOption = MergeOption.NoTracking;
            context.SaveChangesDefaultOptions = SaveChangesOptions.None;
            return context;
        }
    }
}