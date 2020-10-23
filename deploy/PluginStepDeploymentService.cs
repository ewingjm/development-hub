namespace DevelopmentHub.Deployment
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Tooling.Connector;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// Deployment functionality for plugin steps.
    /// </summary>
    public class PluginStepDeploymentService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginStepDeploymentService"/> class.
        /// </summary>
        /// <param name="crmServiceClient">A service client authenticated as a licensed user.</param>
        /// <param name="packageLog">The logger.</param>
        public PluginStepDeploymentService(CrmServiceClient crmServiceClient, TraceLogger packageLog)
        {
            this.CrmSvc = crmServiceClient ?? throw new ArgumentNullException(nameof(crmServiceClient));
            this.PackageLog = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
        }

        /// <summary>
        /// Gets a service client authenticated as a licensed user.
        /// </summary>
        protected CrmServiceClient CrmSvc { get; private set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected TraceLogger PackageLog { get; private set; }

        /// <summary>
        /// Gets the plugin steps for a particular handler on the target instance.
        /// </summary>
        /// <param name="handlerId">The ID of the plugin handler.</param>
        /// <param name="columnSet">The columns to select.</param>
        /// <returns>A list of configured plugin steps.</returns>
        public IEnumerable<Entity> GetPluginStepsForHandler(Guid handlerId, ColumnSet columnSet)
        {
            this.PackageLog.Log($"Getting plugin steps for plugin handlder {handlerId}.");

            var query = new QueryExpression("sdkmessageprocessingstep")
            {
                ColumnSet = columnSet,
                Criteria = new FilterExpression(),
            };
            query.Criteria.AddCondition("eventhandler", ConditionOperator.Equal, handlerId);

            var results = this.CrmSvc.RetrieveMultiple(query).Entities;
            this.PackageLog.Log($"Found {results.Count} matching steps.");

            return results;
        }

        /// <summary>
        /// Creates a secure configuration record.
        /// </summary>
        /// <param name="secureConfig">The secure configuration.</param>
        /// <returns>A reference to the created secure configuration record.</returns>
        public EntityReference CreateSdkMessageProcessingStepSecureConfig(string secureConfig)
        {
            this.PackageLog.Log("Creating plugin step secure configuration.");

            var entity = new Entity("sdkmessageprocessingstepsecureconfig")
            {
                Attributes = new AttributeCollection
                {
                    { "secureconfig", secureConfig },
                },
            };

            var result = new EntityReference("sdkmessageprocessingstepsecureconfig", this.CrmSvc.Create(entity));
            this.PackageLog.Log($"Created plugin step secure configuration {result}.");

            return result;
        }

        /// <summary>
        /// Sets a secure configuration against an SDK message processing step.
        /// </summary>
        /// <param name="sdkMessageProcessingStepId">The ID of the SDK message processing step.</param>
        /// <param name="secureConfiguration">The secure configuration record.</param>
        public void SetPluginSecureConfiguration(Guid sdkMessageProcessingStepId, EntityReference secureConfiguration)
        {
            if (secureConfiguration is null)
            {
                throw new ArgumentNullException(nameof(secureConfiguration));
            }

            this.PackageLog.Log($"Setting secure configuration {secureConfiguration.Id} for plugin step {sdkMessageProcessingStepId}.");
            var step = new Entity("sdkmessageprocessingstepsecureconfigid", sdkMessageProcessingStepId)
            {
                Attributes = new AttributeCollection
                {
                    { "sdkmessageprocessingstepsecureconfigid", secureConfiguration },
                },
            };

            this.CrmSvc.Update(step);
        }
    }
}
