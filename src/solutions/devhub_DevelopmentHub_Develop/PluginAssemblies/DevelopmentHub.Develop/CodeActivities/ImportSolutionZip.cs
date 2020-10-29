namespace DevelopmentHub.Develop.CodeActivities
{
    using System;
    using System.Activities;
    using System.Net;
    using DevelopmentHub.BusinessLogic;
    using DevelopmentHub.BusinessLogic.Extensions;
    using DevelopmentHub.BusinessLogic.Logging;
    using DevelopmentHub.Develop.BusinessLogic;
    using DevelopmentHub.Develop.Model;
    using DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Import a solution into an environment.
    /// </summary>
    [CrmPluginRegistration(
        nameof(ImportSolutionZip),
        FriendlyName,
        Description,
        Group,
        IsolationModeEnum.Sandbox)]
    public class ImportSolutionZip : IntegratedWorkflowActivity
    {
        /// <summary>
        /// The group of the workflow activity.
        /// </summary>
        public const string Group = "DevelopmentHub.Develop";

        /// <summary>
        /// The friendly name of the workflow activity.
        /// </summary>
        public const string FriendlyName = "Import solution zip";

        /// <summary>
        /// The description of the workflow activity.
        /// </summary>
        public const string Description = "Imports a solution zip file as a Base64 encoded string.";

        /// <summary>
        /// Gets or sets the solution zip as a Base64 encoded string.
        /// </summary>
        [RequiredArgument]
        [Input("Solution zip")]
        public InArgument<string> SolutionZip { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IODataClient oDataClient, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var solutionZip = this.SolutionZip.GetRequired(context, nameof(this.SolutionZip));
            var oDataSolutionService = context.GetExtension<IODataSolutionService>() ?? new ODataSolutionService(new ODataRepositoryFactory(oDataClient), logWriter);

            var importJobData = oDataSolutionService.ImportSolutionZipAsync(Convert.FromBase64String(solutionZip)).Result;

            this.IsSuccessful.Set(context, importJobData.ImportResult == ImportResult.Success);

            if (importJobData.ImportResult == ImportResult.Failure)
            {
                this.Error.Set(context, importJobData.ErrorText);
            }
        }
    }
}
