namespace DevelopmentHub.BusinessLogic
{
    using System;
    using DevelopmentHub.BusinessLogic.Logging;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Dynamics 365 plugin.
    /// </summary>
    public abstract class Plugin : IPlugin
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        public Plugin()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="unsecureConfig">The unsecure configuration.</param>
        /// <param name="secureConfig">The secure configuration.</param>
        public Plugin(string unsecureConfig, string secureConfig)
            : this()
        {
            this.UnsecureConfig = unsecureConfig;
            this.SecureConfig = secureConfig;
        }

        /// <summary>
        /// Gets the plugin step's unsecure configuration.
        /// </summary>
        protected string UnsecureConfig { get; private set; }

        /// <summary>
        /// Gets the plugin step's secure configuration.
        /// </summary>
        protected string SecureConfig { get; private set; }

        /// <inheritdoc/>
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingSvc = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var orgSvc = serviceFactory.CreateOrganizationService(Guid.Empty);
            var repositoryFactory = new RepositoryFactory(orgSvc);
            var logWriter = new TracingServiceLogWriter(tracingSvc, true);

            this.Execute(context, orgSvc, logWriter, repositoryFactory);
        }

        /// <summary>
        /// Execute the plugin.
        /// </summary>
        /// <param name="context">The plugin execution context.</param>
        /// <param name="orgSvc">The organization service.</param>
        /// <param name="logWriter">The log writer.</param>
        /// <param name="repositoryFactory">The repository factory.</param>
        protected abstract void Execute(IPluginExecutionContext context, IOrganizationService orgSvc, TracingServiceLogWriter logWriter, RepositoryFactory repositoryFactory);
    }
}
