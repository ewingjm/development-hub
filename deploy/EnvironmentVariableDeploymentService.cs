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

            var definition = this.GetDefinitionByKey(key, new ColumnSet(false));
            if (definition == null)
            {
                throw new ArgumentException($"Environment variable {key} not found on target instance.");
            }

            var definitionReference = definition.ToEntityReference();
            this.PackageLog.Log($"Found environment variable on target instance: {definition.Id}", TraceEventType.Verbose);

            this.UpsertEnvironmentVariableValue(value, definitionReference);
        }

        private void UpsertEnvironmentVariableValue(string value, EntityReference definitionReference)
        {
            var existingValue = this.GetValueByDefinitionId(definitionReference, new ColumnSet("value"));
            if (existingValue != null)
            {
                existingValue["value"] = value;
                this.CrmSvc.Update(existingValue);
            }
            else
            {
                this.SetValue(value, definitionReference);
            }
        }

        private Entity GetValueByDefinitionId(EntityReference definitionReference, ColumnSet columnSet)
        {
            var definitionQuery = new QueryExpression("environmentvariablevalue")
            {
                ColumnSet = columnSet,
                Criteria = new FilterExpression(),
            };
            definitionQuery.Criteria.AddCondition("environmentvariabledefinitionid", ConditionOperator.Equal, definitionReference.Id);

            return this.CrmSvc.RetrieveMultiple(definitionQuery).Entities.FirstOrDefault();
        }

        private void SetValue(string value, EntityReference definition)
        {
            var val = new Entity("environmentvariablevalue")
            {
                Attributes = new AttributeCollection
                {
                    { "value", value },
                    { "environmentvariabledefinitionid", definition },
                },
            };

            this.CrmSvc.Create(val);
        }

        private Entity GetDefinitionByKey(string key, ColumnSet columnSet)
        {
            var definitionQuery = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = columnSet,
                Criteria = new FilterExpression(),
            };
            definitionQuery.Criteria.AddCondition("schemaname", ConditionOperator.Equal, key);

            return this.CrmSvc.RetrieveMultiple(definitionQuery).Entities.FirstOrDefault();
        }
    }
}
