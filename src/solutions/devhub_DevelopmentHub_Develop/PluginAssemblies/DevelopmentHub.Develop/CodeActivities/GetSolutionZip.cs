namespace DevelopmentHub.Develop.CodeActivities
{
    using System;
    using System.Activities;
    using DevelopmentHub.BusinessLogic;
    using DevelopmentHub.BusinessLogic.Extensions;
    using DevelopmentHub.BusinessLogic.Logging;
    using DevelopmentHub.Develop.BusinessLogic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Gets a solution as a Base64 encoded string.
    /// </summary>
    [CrmPluginRegistration(
        nameof(GetSolutionZip),
        FriendlyName,
        Description,
        Group,
        IsolationModeEnum.Sandbox)]
    public class GetSolutionZip : WorkflowActivity
    {
        /// <summary>
        /// The group of the workflow activity.
        /// </summary>
        public const string Group = "DevelopmentHub.Develop";

        /// <summary>
        /// The friendly name of the workflow activity.
        /// </summary>
        public const string FriendlyName = "Get solution zip";

        /// <summary>
        /// The description of the workflow activity.
        /// </summary>
        public const string Description = "Gets a solution zip file as a Base64 encoded string.";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSolutionZip"/> class.
        /// </summary>
        public GetSolutionZip()
        {
        }

        /// <summary>
        /// Gets or sets the unique name of the solution.
        /// </summary>
        [Input("Solution unique name")]
        [RequiredArgument]
        public InArgument<string> SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets whether or not the solution should be managed.
        /// </summary>
        [Input("Managed?")]
        public InArgument<bool> Managed { get; set; }

        /// <summary>
        /// Gets or sets the solution zip as a Base64 encoded string.
        /// </summary>
        [Output("Solution zip")]
        public OutArgument<string> SolutionZip { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IOrganizationService orgSvc, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var solutionUniqueName = this.SolutionUniqueName.GetRequired(context, nameof(this.SolutionUniqueName));
            var managed = this.Managed.Get(context);

            var solutionZip = GetSolutionService(context, logWriter, repoFactory).GetSolutionZip(solutionUniqueName, managed);

            this.SolutionZip.Set(context, Convert.ToBase64String(solutionZip));
        }

        private static ISolutionService GetSolutionService(CodeActivityContext context, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            return context.GetExtension<ISolutionService>() ?? new SolutionService(repoFactory, logWriter);
        }
    }
}
