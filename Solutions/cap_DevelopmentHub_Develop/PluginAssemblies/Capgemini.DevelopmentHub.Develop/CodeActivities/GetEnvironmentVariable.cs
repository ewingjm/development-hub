namespace Capgemini.DevelopmentHub.Develop.CodeActivities
{
    using System;
    using System.Activities;
    using System.Linq;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Get an environment variable by key.
    /// </summary>
    [CrmPluginRegistration(
        nameof(GetEnvironmentVariable),
        "Get Environment Variable",
        "Gets an environment variable value.",
        "Capgemini.DevelopmentHub.Develop",
        IsolationModeEnum.Sandbox)]
    public class GetEnvironmentVariable : WorkflowActivity
    {
        /// <summary>
        /// Gets or sets the environment variable to get the value of.
        /// </summary>
        [Input("Environment Variable")]
        [ReferenceTarget(EnvironmentVariableDefinition.EntityLogicalName)]
        [RequiredArgument]
        public InArgument<EntityReference> EnvironmentVariable { get; set; }

        /// <summary>
        /// Gets or sets the value of the environment variable.
        /// </summary>
        [Output("Environment Variable Value")]
        public OutArgument<string> Value { get; set; }

        /// <inheritdoc />
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IOrganizationService orgSvc, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            var defRef = this.EnvironmentVariable.Get(context);

            if (defRef == null)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "An environment variable was not passed to the code activity.");
            }

            this.Value.Set(
                context,
                GetValue(repoFactory, defRef) ?? GetDefaultValue(repoFactory, defRef) ?? string.Empty);
        }

        private static string GetDefaultValue(IRepositoryFactory repoFactory, EntityReference defRef)
        {
            var definitionRepo = repoFactory.GetRepository<DevelopContext, EnvironmentVariableDefinition>();
            return definitionRepo.Find(
                env => env.EnvironmentVariableDefinitionId == defRef.Id,
                env => new EnvironmentVariableDefinition
                {
                    DefaultValue = env.DefaultValue,
                }).First().DefaultValue;
        }

        private static string GetValue(IRepositoryFactory repoFactory, EntityReference defRef)
        {
            var valueRepo = repoFactory.GetRepository<DevelopContext, EnvironmentVariableValue>();
            return valueRepo.Find(
                val => val.EnvironmentVariableDefinitionId.Id == defRef.Id,
                val => new EnvironmentVariableValue
                {
                    Value = val.Value,
                }).FirstOrDefault()?.Value;
        }
    }
}
