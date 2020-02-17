namespace DevelopmentHub.Develop.CodeActivities
{
    using System.Activities;
    using DevelopmentHub.BusinessLogic;
    using DevelopmentHub.BusinessLogic.Extensions;
    using DevelopmentHub.BusinessLogic.Logging;
    using DevelopmentHub.Develop.BusinessLogic;
    using DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Update the version of a solution.
    /// </summary>
    [CrmPluginRegistration(
        nameof(UpdateSolutionVersion),
        "Update solution version",
        "Updates the version of a solution.",
        "DevelopmentHub.Develop",
        IsolationModeEnum.Sandbox)]
    public class UpdateSolutionVersion : IntegratedWorkflowActivity
    {
        /// <summary>
        /// Gets or sets the unique name of the solution to update.
        /// </summary>
        [Input("Solution unique name")]
        [RequiredArgument]
        public InArgument<string> SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets the major version of the solution.
        /// </summary>
        [Input("Major version")]
        [RequiredArgument]
        public InArgument<int> MajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the minor version of the solution.
        /// </summary>
        [Input("Minor version")]
        [RequiredArgument]
        public InArgument<int> MinorVersion { get; set; }

        /// <summary>
        /// Gets or sets the patch version of the solution.
        /// </summary>
        [Input("Patch version")]
        [RequiredArgument]
        public InArgument<int> PatchVersion { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IODataClient oDataClient, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            var oDataSolutionService = context.GetExtension<IODataSolutionService>() ?? new ODataSolutionService(new ODataRepositoryFactory(oDataClient), logWriter);

            var version = $"{this.MajorVersion.Get(context)}.{this.MinorVersion.Get(context)}.{this.PatchVersion.Get(context)}";
            var solutionUniqueName = this.SolutionUniqueName.GetRequired(context, nameof(this.SolutionUniqueName));

            oDataSolutionService.UpdateSolutionVersionAsync(solutionUniqueName, version).Wait();

            this.IsSuccessful.Set(context, true);
        }
    }
}
