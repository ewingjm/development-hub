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
        private const string Tag = nameof(GetEnvironmentVariable);

        /// <summary>
        /// Gets or sets the environment variable to get the value of.
        /// </summary>
        [Input("Environment Variable")]
        [ReferenceTarget(EnvironmentVariableDefinition.EntityLogicalName)]
        [RequiredArgument]
        public InArgument<string> EnvironmentVariable { get; set; }

        /// <summary>
        /// Gets or sets the value of the environment variable.
        /// </summary>
        [Output("Environment Variable Value")]
        public OutArgument<string> Value { get; set; }

        /// <inheritdoc />
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IOrganizationService orgSvc, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            var key = this.EnvironmentVariable.Get(context);

            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "An environment variable was not passed to the code activity.");
            }

            var definition = GetDefinition(repoFactory, key);
            this.Value.Set(
                context,
                GetValue(repoFactory, definition.EnvironmentVariableDefinitionId) ?? definition.DefaultValue ?? string.Empty);
        }

        private static EnvironmentVariableDefinition GetDefinition(IRepositoryFactory repoFactory, string key)
        {
            return repoFactory.GetRepository<DevelopContext, EnvironmentVariableDefinition>().Find(
                env => env.SchemaName == key,
                env => new EnvironmentVariableDefinition
                {
                    EnvironmentVariableDefinitionId = env.EnvironmentVariableDefinitionId,
                    DefaultValue = env.DefaultValue,
                }).First();
        }

        private static string GetValue(IRepositoryFactory repoFactory, Guid? definitionId)
        {
            return repoFactory.GetRepository<DevelopContext, EnvironmentVariableValue>().Find(
                val => val.EnvironmentVariableDefinitionId.Id == definitionId,
                val => new EnvironmentVariableValue
                {
                    Value = val.Value,
                }).FirstOrDefault()?.Value;
        }
    }
}
