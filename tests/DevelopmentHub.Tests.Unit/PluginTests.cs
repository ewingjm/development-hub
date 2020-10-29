namespace DevelopmentHub.Tests.Unit
{
    using System;
    using DevelopmentHub.BusinessLogic;
    using Microsoft.Xrm.Sdk;
    using Moq;

    /// <summary>
    /// Base test class for Common Data Service plugins.
    /// </summary>
    /// <typeparam name="TPlugin">The plugin type.</typeparam>
    public abstract class PluginTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginTests"/> class.
        /// </summary>
        public PluginTests()
        {
            this.ServiceProviderMock = new Mock<IServiceProvider>();
            this.TracingSvcMock = new Mock<ITracingService>();
            this.PluginExecutionContextMock = new Mock<IPluginExecutionContext>();
            this.OrgSvcFactoryMock = new Mock<IOrganizationServiceFactory>();
            this.OrgSvcMock = new Mock<IOrganizationService>();
            this.RepositoryFactoryMock = new Mock<IRepositoryFactory>();

            this.ServiceProviderMock.Setup(s => s.GetService(typeof(ITracingService))).Returns(this.TracingSvcMock.Object);
            this.ServiceProviderMock.Setup(s => s.GetService(typeof(IPluginExecutionContext))).Returns(this.PluginExecutionContextMock.Object);
            this.ServiceProviderMock.Setup(s => s.GetService(typeof(IOrganizationServiceFactory))).Returns(this.OrgSvcFactoryMock.Object);
            this.ServiceProviderMock.Setup(s => s.GetService(typeof(IRepositoryFactory))).Returns(this.RepositoryFactoryMock.Object);

            this.PluginExecutionContextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());
            this.PluginExecutionContextMock.Setup(c => c.SharedVariables).Returns(new ParameterCollection());

            this.OrgSvcFactoryMock.SetReturnsDefault(this.OrgSvcMock.Object);
        }

        /// <summary>
        /// Gets mock repository factory.
        /// </summary>
        protected Mock<IRepositoryFactory> RepositoryFactoryMock { get; }

        /// <summary>
        /// Gets the mocked service provider.
        /// </summary>
        protected Mock<IServiceProvider> ServiceProviderMock { get; }

        /// <summary>
        /// Gets the mocked tracing service.
        /// </summary>
        protected Mock<ITracingService> TracingSvcMock { get; }

        /// <summary>
        /// Gets the mocked plugin execution context.
        /// </summary>
        protected Mock<IPluginExecutionContext> PluginExecutionContextMock { get; }

        /// <summary>
        /// Gets the mocked organization service factory.
        /// </summary>
        protected Mock<IOrganizationServiceFactory> OrgSvcFactoryMock { get; }

        /// <summary>
        /// Gets the mocked organization service.
        /// </summary>
        protected Mock<IOrganizationService> OrgSvcMock { get; }

        /// <summary>
        /// Execute the plugin.
        /// </summary>
        /// <param name="plugin">The plugin to execute.</param>
        protected void Execute(Plugin plugin)
        {
            if (plugin is null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            plugin.Execute(this.ServiceProviderMock.Object);
        }
    }
}
