namespace Capgemini.DevelopmentHub.Develop.CodeActivities
{
    using System;
    using System.Activities;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.BusinessLogic.Extensions;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Develop.BusinessLogic;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Develop.Plugins;
    using Capgemini.DevelopmentHub.Develop.Repositories;
    using Microsoft.Xrm.Sdk;
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
    public class ImportSolutionZip : WorkflowActivity
    {
        /// <summary>
        /// The group of the workflow activity.
        /// </summary>
        public const string Group = "Capgemini.DevelopmentHub.Develop";

        /// <summary>
        /// The friendly name of the workflow activity.
        /// </summary>
        public const string FriendlyName = "Import solution zip";

        /// <summary>
        /// The description of the workflow activity.
        /// </summary>
        public const string Description = "Imports a solution zip file as a Base64 encoded string.";

        private readonly ISolutionImportService solutionImportService;
        private readonly IOAuthTokenRepository oAuthTokenRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportSolutionZip"/> class.
        /// </summary>
        public ImportSolutionZip()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportSolutionZip"/> class.
        /// This constructor is used for unit-testing only.
        /// </summary>
        /// <param name="solutionImportService">The solution service.</param>
        /// <param name="oAuthTokenRepository">OAUth token repository.</param>
        public ImportSolutionZip(ISolutionImportService solutionImportService, IOAuthTokenRepository oAuthTokenRepository)
        {
            this.solutionImportService = solutionImportService;
            this.oAuthTokenRepository = oAuthTokenRepository;
        }

        /// <summary>
        /// Gets or sets the instance to import into.
        /// </summary>
        [RequiredArgument]
        [Input("Target instance URL")]
        public InArgument<string> TargetInstanceUrl { get; set; }

        /// <summary>
        /// Gets or sets the solution zip as a Base64 encoded string.
        /// </summary>
        [RequiredArgument]
        [Input("Solution zip")]
        public InArgument<string> SolutionZip { get; set; }

        /// <inheritdoc/>
        protected override void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IOrganizationService orgSvc, ILogWriter logWriter, IRepositoryFactory repoFactory)
        {
            var targetInstance = new Uri(this.TargetInstanceUrl.GetRequired(context, nameof(this.TargetInstanceUrl)));
            var solutionZip = this.SolutionZip.GetRequired(context, nameof(this.SolutionZip));

            var passwordGrantRequest = GetPasswordGrantRequest(workflowContext);
            passwordGrantRequest.Resource = targetInstance;

            var token = this.GetOAuthTokenRepository().GetAccessToken(passwordGrantRequest).Result;
            var solutionImportService = this.GetSolutionImportService(targetInstance, token);

            var importJobData = solutionImportService.ImportSolutionZip(Convert.FromBase64String(solutionZip)).Result;
            if (importJobData.ImportResult == ImportResult.Failure)
            {
                throw new Exception(importJobData.ErrorText);
            }
        }

        private static OAuthPasswordGrantRequest GetPasswordGrantRequest(IWorkflowContext workflowContext)
        {
            if (!workflowContext.SharedVariables.ContainsKey(InjectSecureConfig.SharedVariablesKeySecureConfig))
            {
                throw new Exception($"Secure configuration not found. Please inject an {nameof(OAuthPasswordGrantRequest)} using the {nameof(InjectSecureConfig)} plugin.");
            }

            var secureConfig = (string)workflowContext.SharedVariables[InjectSecureConfig.SharedVariablesKeySecureConfig];
            OAuthPasswordGrantRequest passwordGrantRequest;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(secureConfig)))
            {
                passwordGrantRequest = (OAuthPasswordGrantRequest)new DataContractJsonSerializer(typeof(OAuthPasswordGrantRequest)).ReadObject(ms);
            }

            return passwordGrantRequest;
        }

        [ExcludeFromCodeCoverage]
        private ISolutionImportService GetSolutionImportService(Uri targetInstance, OAuthToken accessToken)
        {
            return this.solutionImportService ?? new SolutionImportService(new ODataClient(targetInstance, accessToken));
        }

        [ExcludeFromCodeCoverage]
        private IOAuthTokenRepository GetOAuthTokenRepository()
        {
            return this.oAuthTokenRepository ?? new OAuthTokenRepository();
        }
    }
}
