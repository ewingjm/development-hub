namespace DevelopmentHub.Develop.CodeActivities
{
    using System.Activities;
    using DevelopmentHub.BusinessLogic;
    using DevelopmentHub.BusinessLogic.Extensions;
    using DevelopmentHub.BusinessLogic.Logging;
    using DevelopmentHub.Develop.Model;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Gets the last approved solution merge for an environment.
    /// </summary>
    [CrmPluginRegistration(
        nameof(GetLastApprovedSolutionMerge),
        "Get last approved solution merge",
        "Gets the last approved solution merge for an environment",
        "DevelopmentHub.Develop",
        IsolationModeEnum.Sandbox)]
    public class GetLastApprovedSolutionMerge : WorkflowActivity
    {
        /// <summary>
        /// Gets or sets the environment to get the last approved solution merge for.
        /// </summary>
        [Input("Environment")]
        [ReferenceTarget(devhub_environment.EntityLogicalName)]
        [RequiredArgument]
        public InArgument<EntityReference> Environment { get; set; }

        /// <summary>
        /// Gets or sets the last approved solution merge for the environment.
        /// </summary>
        [ReferenceTarget(devhub_solutionmerge.EntityLogicalName)]
        [Output("Solution merge")]
        public OutArgument<EntityReference> SolutionMerge { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IOrganizationService orgSvc, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            if (orgSvc is null)
            {
                throw new System.ArgumentNullException(nameof(orgSvc));
            }

            var response = (devhub_GetLastApprovedSolutionMergeResponse)orgSvc.Execute(new devhub_GetLastApprovedSolutionMergeRequest
            {
                Target = this.Environment.GetRequired(context, nameof(this.Environment)),
            });

            this.SolutionMerge.Set(context, response.LastApprovedSolutionMerge);
        }
    }
}
