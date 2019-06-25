namespace Capgemini.DevelopmentHub.BusinessLogic
{
    using System;
    using System.Activities;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using Capgemini.DevelopmentHub.BusinessLogic.Extensions;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Model.Requests;
    using Capgemini.DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;

    /// <summary>
    /// Base class for Dynamics 365 workflow activities that integrate with an external Dynamics 365 instance.
    /// </summary>
    public abstract class IntegratedWorkflowActivity : CodeActivity
    {
        private const string SharedVariablesSecureConfigKey = "SecureConfiguration";
        private const string Tag = nameof(IntegratedWorkflowActivity);

        /// <summary>
        /// Gets or sets the instance to integrate with.
        /// </summary>
        [RequiredArgument]
        [Input("Target instance URL")]
        public InArgument<string> TargetInstanceUrl { get; set; }

        /// <summary>
        /// Execute the custom workflow activity.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="workflowContext">The workflow context.</param>
        /// <param name="oDataClient">The OData client for the external Dynamics instance.</param>
        /// <param name="logWriter">Log writer.</param>
        /// <param name="repoFactory">Repository factory.</param>
        protected abstract void ExecuteWorkflowActivity(CodeActivityContext context, IWorkflowContext workflowContext, IODataClient oDataClient, ILogWriter logWriter, IRepositoryFactory repoFactory);

        /// <inheritdoc/>
        protected override void Execute(CodeActivityContext context)
        {
            var tracingSvc = context.GetExtension<ITracingService>();
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var orgSvc = serviceFactory.CreateOrganizationService(workflowContext.UserId);
            var repositoryFactory = new RepositoryFactory(orgSvc);
            var logWriter = new TracingServiceLogWriter(tracingSvc, true);

            logWriter.Log(Severity.Info, Tag, $"Executing integrated workflow activity.");
            var oDataClient = context.GetExtension<IODataClient>() ?? this.GetODataClient(
                new Uri(this.TargetInstanceUrl.GetRequired(context, nameof(this.TargetInstanceUrl))),
                context,
                workflowContext,
                logWriter);

            this.ExecuteWorkflowActivity(context, workflowContext, oDataClient, logWriter, repositoryFactory);
        }

        private static OAuthPasswordGrantRequest GetPasswordGrantRequest(IWorkflowContext workflowContext, ILogWriter logWriter)
        {
            if (!workflowContext.SharedVariables.ContainsKey(SharedVariablesSecureConfigKey))
            {
                throw new Exception($"Secure configuration not found. Please inject an {nameof(OAuthPasswordGrantRequest)} object with shared variables key '{SharedVariablesSecureConfigKey}'.");
            }

            var secureConfig = (string)workflowContext.SharedVariables[SharedVariablesSecureConfigKey];

            logWriter.Log(Severity.Info, Tag, $"Deserializing password grant request from secure configuration.");

            OAuthPasswordGrantRequest passwordGrantRequest;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(secureConfig)))
            {
                passwordGrantRequest = (OAuthPasswordGrantRequest)new DataContractJsonSerializer(typeof(OAuthPasswordGrantRequest)).ReadObject(ms);
            }

            return passwordGrantRequest;
        }

        [ExcludeFromCodeCoverage]
        private IODataClient GetODataClient(Uri targetInstance, CodeActivityContext context, IWorkflowContext workflowContext, ILogWriter logWriter)
        {
            var passwordGrantRequest = GetPasswordGrantRequest(workflowContext, logWriter);
            passwordGrantRequest.Resource = targetInstance;

            logWriter.Log(Severity.Info, Tag, $"Making password grant OAuth request for {passwordGrantRequest.Resource} as {passwordGrantRequest.Username}");

            var token = (context.GetExtension<IOAuthTokenRepository>() ?? new OAuthTokenRepository()).GetAccessToken(passwordGrantRequest).Result;

            return new ODataClient(targetInstance, token);
        }
    }
}
