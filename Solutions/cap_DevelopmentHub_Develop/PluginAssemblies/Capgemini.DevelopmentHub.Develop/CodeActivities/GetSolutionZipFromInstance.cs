namespace Capgemini.DevelopmentHub.Develop.CodeActivities
{
    using System.Activities;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.BusinessLogic.Extensions;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Develop.BusinessLogic;
    using Capgemini.DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Gets a solution zip from another Dynamics 365 instance.
    /// </summary>
    [CrmPluginRegistration(
        nameof(GetSolutionZipFromInstance),
        "Get solution zip from instance",
        "Gets a solution zip from another Dynamics 365 instance",
        "Capgemini.DevelopmentHub.Develop",
        IsolationModeEnum.Sandbox)]
    public class GetSolutionZipFromInstance : IntegratedWorkflowActivity
    {
        /// <summary>
        /// Gets or sets the unique name of the solution to export.
        /// </summary>
        [Input("Solution unique name")]
        [RequiredArgument]
        public InArgument<string> SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets whether or not the solution should be exported as managed.
        /// </summary>
        [Input("Managed?")]
        public InArgument<bool> IsManaged { get; set; }

        /// <summary>
        /// Gets or sets Base64 encoded string representing the exported solution zip.
        /// </summary>
        [Output("Solution zip")]
        public OutArgument<string> SolutionZip { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IODataClient oDataClient, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            var solutionUniqueName = this.SolutionUniqueName.GetRequired(context, nameof(this.SolutionUniqueName));
            var isManaged = this.IsManaged.Get(context);
            var oDataSolutionService = context.GetExtension<IODataSolutionService>() ?? new ODataSolutionService(new ODataRepositoryFactory(oDataClient), logWriter);

            var solutionZip = oDataSolutionService.GetSolutionZipAsync(solutionUniqueName, isManaged).Result;

            logWriter.Log(Severity.Info, nameof(GetSolutionZipFromInstance), solutionZip);

            this.SolutionZip.Set(context, solutionZip);
            this.IsSuccessful.Set(context, true);
        }
    }
}
