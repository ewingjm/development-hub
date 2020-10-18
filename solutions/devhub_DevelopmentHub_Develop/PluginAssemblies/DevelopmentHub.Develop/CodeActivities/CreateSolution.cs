namespace DevelopmentHub.Develop.CodeActivities
{
    using System.Activities;
    using System.Globalization;
    using System.Linq;
    using DevelopmentHub.BusinessLogic;
    using DevelopmentHub.BusinessLogic.Extensions;
    using DevelopmentHub.BusinessLogic.Logging;
    using DevelopmentHub.Develop.BusinessLogic;
    using DevelopmentHub.Develop.Model;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Creates a solution.
    /// </summary>
    [CrmPluginRegistration(
        nameof(CreateSolution),
        FriendlyName,
        Description,
        Group,
        IsolationModeEnum.Sandbox)]
    public class CreateSolution : WorkflowActivity
    {
        /// <summary>
        /// The group of the workflow activity.
        /// </summary>
        public const string Group = "DevelopmentHub.Develop";

        /// <summary>
        /// The friendly name of the workflow activity.
        /// </summary>
        public const string FriendlyName = "Create a solution";

        /// <summary>
        /// The description of the workflow activity.
        /// </summary>
        public const string Description = "Creates a solution.";

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSolution"/> class.
        /// </summary>
        public CreateSolution()
        {
        }

        /// <summary>
        /// Gets or sets the unique name of the solution.
        /// </summary>
        [Input("Unique name")]
        [RequiredArgument]
        public InArgument<string> SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the solution.
        /// </summary>
        [Input("Display name")]
        [RequiredArgument]
        public InArgument<string> SolutionDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the solution description.
        /// </summary>
        [Input("Description")]
        public InArgument<string> SolutionDescription { get; set; }

        /// <summary>
        /// Gets or sets a reference to the created solution.
        /// </summary>
        [Output("Created solution")]
        [ReferenceTarget(Solution.EntityLogicalName)]
        public OutArgument<EntityReference> CreatedSolution { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IOrganizationService orgSvc, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            var uniqueName = this.SolutionUniqueName.GetRequired(context, nameof(this.SolutionUniqueName));
            var displayName = this.SolutionDisplayName.GetRequired(context, nameof(this.SolutionDisplayName));
            var description = this.SolutionDescription.Get(context);

            var createdSolution = this.GetSolutionService(context, repoFactory, logWriter)
                .Create(SanitizeUniqueName(uniqueName), displayName, description);

            this.CreatedSolution.Set(context, createdSolution);
        }

        private static string SanitizeUniqueName(string uniqueName)
        {
            var titleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(uniqueName);
            var titleCaseTrimmed = string.Concat(titleCase.Where(c => !char.IsWhiteSpace(c)));
            var titleCaseTrimmedFirstCharLower = char.ToLowerInvariant(titleCaseTrimmed[0]) + titleCaseTrimmed.Substring(1);

            return titleCaseTrimmedFirstCharLower;
        }

        private ISolutionService GetSolutionService(CodeActivityContext context, IRepositoryFactory repoFactory, ILogWriter logWriter)
        {
            return context.GetExtension<ISolutionService>() ?? new SolutionService(repoFactory, logWriter);
        }
    }
}
