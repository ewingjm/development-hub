namespace Capgemini.DevelopmentHub.Develop.CodeActivities
{
    using System.Activities;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.BusinessLogic.Extensions;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Gets the last approved solution merge for an environment.
    /// </summary>
    [CrmPluginRegistration(
        nameof(GetLastApprovedSolutionMerge),
        "Get last approved solution merge",
        "Gets the last approved solution merge for an environment",
        "Capgemini.DevelopmentHub.Develop",
        IsolationModeEnum.Sandbox)]
    public class GetLastApprovedSolutionMerge : WorkflowActivity
    {
        /// <summary>
        /// Gets or sets the environment to get the last approved solution merge for.
        /// </summary>
        [Input("Environment")]
        [ReferenceTarget(cap_environment.EntityLogicalName)]
        [RequiredArgument]
        public InArgument<EntityReference> Environment { get; set; }

        /// <summary>
        /// Gets or sets the last approved solution merge for the environment.
        /// </summary>
        [ReferenceTarget(cap_solutionmerge.EntityLogicalName)]
        [Output("Solution merge")]
        public OutArgument<EntityReference> SolutionMerge { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IOrganizationService orgSvc, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            var response = (cap_GetLastApprovedSolutionMergeResponse)orgSvc.Execute(new cap_GetLastApprovedSolutionMergeRequest
            {
                Target = this.Environment.GetRequired(context, nameof(this.Environment)),
            });

            this.SolutionMerge.Set(context, response.LastApprovedSolutionMerge);
        }
    }
}
