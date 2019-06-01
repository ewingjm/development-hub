namespace Capgemini.DevelopmentHub.BusinessLogic
{
    using System.Activities;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Base class for Dynamics 365 workflow activities.
    /// </summary>
    public abstract class WorkflowActivity : CodeActivity
    {
        /// <summary>
        /// Execute the custom workflow activity.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="workflowContext">The workflow context.</param>
        /// <param name="orgSvc">Organization service.</param>
        /// <param name="tracingSvc">Tracing service.</param>
        /// <param name="repoFactory">Repository factory.</param>
        protected abstract void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IOrganizationService orgSvc, ITracingService tracingSvc, IRepositoryFactory repoFactory);

        /// <inheritdoc/>
        protected override void Execute(CodeActivityContext context)
        {
            var tracingSvc = context.GetExtension<ITracingService>();
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var orgSvc = serviceFactory.CreateOrganizationService(workflowContext.UserId);
            var repositoryFactory = new RepositoryFactory(orgSvc);

            this.ExecuteWorkflowActivity(context, workflowContext, orgSvc, tracingSvc, repositoryFactory);
        }
    }
}
