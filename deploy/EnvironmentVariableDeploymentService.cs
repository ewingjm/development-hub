namespace DevelopmentHub.Deployment
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Tooling.Connector;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// Deployment functionality for environment variables.
    /// </summary>
    public class EnvironmentVariableDeploymentService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariableDeploymentService"/> class.
        /// </summary>
        /// <param name="crmServiceClient">A service client authenticated as a licensed user.</param>
        /// <param name="packageLog">The logger.</param>
        public EnvironmentVariableDeploymentService(CrmServiceClient crmServiceClient, TraceLogger packageLog)
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
        /// Sets an environment variable on the target Common Data Service environment.
        /// </summary>
        /// <param name="key">Environment variable key.</param>
        /// <param name="value">Environment variable value.</param>
        public void SetEnvironmentVariable(string key, string value)
        {
            this.PackageLog.Log($"Setting {key} environment variable to {value}.");

            var definitionQuery = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet(false),
                Criteria = new FilterExpression(),
            };
            definitionQuery.Criteria.AddCondition("schemaname", ConditionOperator.Equal, key);

            var definition = this.CrmSvc.RetrieveMultiple(definitionQuery).Entities.FirstOrDefault();
            if (definition == null)
            {
                throw new ArgumentException($"Environment variable {key} not found on target instance.");
            }

            this.PackageLog.Log($"Found environment variable on target instance: {definition.Id}", TraceEventType.Verbose);

            var val = new Entity("environmentvariablevalue")
            {
                Attributes = new AttributeCollection
                {
                    { "value", value },
                    { "environmentvariabledefinitionid", definition.ToEntityReference() },
                },
            };

            this.CrmSvc.Create(val);
        }
    }
}
