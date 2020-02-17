namespace DevelopmentHub.BusinessLogic
{
    using System;
    using System.Activities;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using DevelopmentHub.BusinessLogic.Extensions;
    using DevelopmentHub.BusinessLogic.Logging;
    using DevelopmentHub.Model.Requests;
    using DevelopmentHub.Repositories;
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
        /// Gets or sets if the integration workflow activity was successful.
        /// </summary>
        [Output("Succeeded")]
        [Default("true")]
        public OutArgument<bool> IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the error message encountered (if any).
        /// </summary>
        [Output("Error")]
        [Default("")]
        public OutArgument<string> Error { get; set; }

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
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var tracingSvc = context.GetExtension<ITracingService>();
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var orgSvc = serviceFactory.CreateOrganizationService(workflowContext.UserId);
            var repositoryFactory = new RepositoryFactory(orgSvc);
            var logWriter = new TracingServiceLogWriter(tracingSvc, true);

            logWriter.Log(Severity.Info, Tag, $"Executing integrated workflow activity.");

            try
            {
                var oDataClient = this.GetODataClient(context, workflowContext, logWriter);
                this.ExecuteWorkflowActivity(context, workflowContext, oDataClient, logWriter, repositoryFactory);
            }
            catch (InvalidPluginExecutionException ex)
            {
                this.SetError(context, logWriter, ex.Message);
            }
            catch (AggregateException ex) when (ex.InnerException is WebException)
            {
                this.SetError(context, logWriter, ex.InnerException.Message);
            }
        }

        [ExcludeFromCodeCoverage]
        private static IODataClient GetNewODataClient(Uri targetInstance, CodeActivityContext context, IWorkflowContext workflowContext, ILogWriter logWriter)
        {
            var passwordGrantRequest = GetPasswordGrantRequest(workflowContext, logWriter);
            passwordGrantRequest.Resource = targetInstance;

            logWriter.Log(Severity.Info, Tag, $"Making password grant OAuth request for {passwordGrantRequest.Resource} as {passwordGrantRequest.Username}");

            var token = (context.GetExtension<IOAuthTokenRepository>() ?? new OAuthTokenRepository()).GetAccessToken(passwordGrantRequest).Result;

            return new ODataClient(targetInstance, token);
        }

        private static OAuthPasswordGrantRequest GetPasswordGrantRequest(IWorkflowContext workflowContext, ILogWriter logWriter)
        {
            if (!workflowContext.SharedVariables.ContainsKey(SharedVariablesSecureConfigKey))
            {
                throw new InvalidPluginExecutionException($"Secure configuration not found. Please inject an {nameof(OAuthPasswordGrantRequest)} object with shared variables key '{SharedVariablesSecureConfigKey}'.");
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

        private void SetError(CodeActivityContext context, TracingServiceLogWriter logWriter, string message)
        {
            logWriter.Log(Severity.Error, Tag, message);

            this.Error.Set(context, message);
            this.IsSuccessful.Set(context, false);
        }

        private IODataClient GetODataClient(CodeActivityContext context, IWorkflowContext workflowContext, TracingServiceLogWriter logWriter)
        {
            return context.GetExtension<IODataClient>() ?? GetNewODataClient(
                new Uri(this.TargetInstanceUrl.GetRequired(context, nameof(this.TargetInstanceUrl))),
                context,
                workflowContext,
                logWriter);
        }
    }
}
