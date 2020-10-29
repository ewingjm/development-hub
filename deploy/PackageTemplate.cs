namespace DevelopmentHub.Deployment
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Tooling.Connector;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
    using Newtonsoft.Json;

    /// <summary>
    /// Import package starter frame.
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class PackageTemplate : ImportExtension, IDisposable
    {
        private bool? forceImport;
        private string azureDevOpsOrganisation;
        private string azureDevOpsProject;
        private string azureDevOpsExtractBuildDefinitionId;
        private string solutionPublisherPrefix;
        private Guid? servicePrincipalClientId;
        private string servicePrincipalClientSecret;
        private string azureDevOpsConnectionName;
        private string approvalsConnectionName;

        private CrmServiceClient licensedCrmSvc;

        private SolutionDeploymentService solutionDeploymentSvc;
        private FlowDeploymentService flowDeploymentSvc;
        private EnvironmentVariableDeploymentService environmentVariableDeploymentSvc;
        private PluginStepDeploymentService pluginStepDeploymentSvc;

        /// <inheritdoc/>
        public override string GetImportPackageDataFolderName => "PkgFolder";

        /// <inheritdoc/>
        public override string GetImportPackageDescriptionText => "Development Hub";

        /// <inheritdoc/>
        public override string GetLongNameOfImport => "Development Hub";

        /// <summary>
        /// Gets a value indicating whether solutions should import even when solution versions match. Useful for pipelines that run on topic branches.
        /// </summary>
        protected bool ForceImportOnSameVersion
        {
            get
            {
                if (!this.forceImport.HasValue)
                {
                    this.forceImport = this.GetSetting<bool>(nameof(this.ForceImportOnSameVersion));
                }

                return this.forceImport.HasValue && this.forceImport.Value;
            }
        }

        /// <summary>
        /// Gets a value for the target Azure DevOps organisation environment variable (if found).
        /// </summary>
        protected string AzureDevOpsOrganisation
        {
            get
            {
                if (string.IsNullOrEmpty(this.azureDevOpsOrganisation))
                {
                    this.azureDevOpsOrganisation = this.GetSetting<string>(nameof(this.AzureDevOpsOrganisation));
                }

                return this.azureDevOpsOrganisation;
            }
        }

        /// <summary>
        /// Gets a value for the target Azure DevOps project environment variable (if found).
        /// </summary>
        protected string AzureDevOpsProject
        {
            get
            {
                if (string.IsNullOrEmpty(this.azureDevOpsProject))
                {
                    this.azureDevOpsProject = this.GetSetting<string>(nameof(this.AzureDevOpsProject));
                }

                return this.azureDevOpsProject;
            }
        }

        /// <summary>
        /// Gets a value for the target Azure DevOps extract build definition ID environment variable (if found).
        /// </summary>
        protected string AzureDevOpsPipelineId
        {
            get
            {
                if (string.IsNullOrEmpty(this.azureDevOpsExtractBuildDefinitionId))
                {
                    this.azureDevOpsExtractBuildDefinitionId = this.GetSetting<string>(nameof(this.AzureDevOpsPipelineId));
                }

                return this.azureDevOpsExtractBuildDefinitionId;
            }
        }

        /// <summary>
        /// Gets a value for the target solution publisher prefix environment variable (if found).
        /// </summary>
        protected string SolutionPublisherPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(this.solutionPublisherPrefix))
                {
                    this.solutionPublisherPrefix = this.GetSetting<string>(nameof(this.SolutionPublisherPrefix));
                }

                return this.solutionPublisherPrefix;
            }
        }

        /// <summary>
        /// Gets a value for the service principal ID used for the Development Hub (if found).
        /// </summary>
        protected Guid? ServicePrincipalClientId
        {
            get
            {
                if (!this.servicePrincipalClientId.HasValue && Guid.TryParse(this.GetSetting<string>(nameof(this.ServicePrincipalClientId)), out var guid))
                {
                    this.servicePrincipalClientId = guid;
                }

                return this.servicePrincipalClientId;
            }
        }

        /// <summary>
        /// Gets a value for the service principal client secret used for the Development Hub (if found).
        /// </summary>
        protected string ServicePrincipalClientSecret
        {
            get
            {
                if (string.IsNullOrEmpty(this.servicePrincipalClientSecret))
                {
                    this.servicePrincipalClientSecret = this.GetSetting<string>(nameof(this.ServicePrincipalClientSecret));
                }

                return this.servicePrincipalClientSecret;
            }
        }

        /// <summary>
        /// Gets a value for the Azure DevOps connection name used for the Development Hub (if found).
        /// </summary>
        protected string AzureDevOpsConnectionName
        {
            get
            {
                if (string.IsNullOrEmpty(this.azureDevOpsConnectionName))
                {
                    this.azureDevOpsConnectionName = this.GetSetting<string>(nameof(this.AzureDevOpsConnectionName));
                }

                return this.azureDevOpsConnectionName;
            }
        }

        /// <summary>
        /// Gets a value for the approvals connection name used for the Development Hub (if found).
        /// </summary>
        protected string ApprovalsConnectionName
        {
            get
            {
                if (string.IsNullOrEmpty(this.approvalsConnectionName))
                {
                    this.approvalsConnectionName = this.GetSetting<string>(nameof(this.ApprovalsConnectionName));
                }

                return this.approvalsConnectionName;
            }
        }

        /// <summary>
        /// Gets a <see cref="CrmServiceClient"/> instance authenticated as a licensed user (if username and password are provided).
        /// </summary>
        protected CrmServiceClient LicensedCrmSvc
        {
            get
            {
                if (this.licensedCrmSvc == null)
                {
                    var username = this.GetSetting<string>("LicensedUserUsername");
                    var password = this.GetSetting<string>("LicensedUserPassword");

                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    {
                        this.licensedCrmSvc = new CrmServiceClient($"AuthType=OAuth;Username={username};Password={password};Url={this.CrmSvc.ConnectedOrgPublishedEndpoints.First().Value};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Never");
                    }
                }

                return this.licensedCrmSvc;
            }
        }

        /// <summary>
        /// Gets a <see cref="FlowDeploymentSvc"/>.
        /// </summary>
        protected FlowDeploymentService FlowDeploymentSvc
        {
            get
            {
                if (this.flowDeploymentSvc == null)
                {
                    this.flowDeploymentSvc = new FlowDeploymentService(this.LicensedCrmSvc ?? this.CrmSvc, this.PackageLog);
                }

                return this.flowDeploymentSvc;
            }
        }

        /// <summary>
        /// Gets a <see cref="SolutionDeploymentSvc"/>.
        /// </summary>
        protected SolutionDeploymentService SolutionDeploymentSvc
        {
            get
            {
                if (this.solutionDeploymentSvc == null)
                {
                    this.solutionDeploymentSvc = new SolutionDeploymentService(this.CrmSvc, this.PackageLog);
                }

                return this.solutionDeploymentSvc;
            }
        }

        /// <summary>
        /// Gets an <see cref="EnvironmentVariableDeploymentSvc"/>.
        /// </summary>
        protected EnvironmentVariableDeploymentService EnvironmentVariableDeploymentSvc
        {
            get
            {
                if (this.environmentVariableDeploymentSvc == null)
                {
                    this.environmentVariableDeploymentSvc = new EnvironmentVariableDeploymentService(this.CrmSvc, this.PackageLog);
                }

                return this.environmentVariableDeploymentSvc;
            }
        }

        /// <summary>
        /// Gets an <see cref="PluginStepDeploymentService"/>.
        /// </summary>
        protected PluginStepDeploymentService PluginStepDeploymentSvc
        {
            get
            {
                if (this.pluginStepDeploymentSvc == null)
                {
                    this.pluginStepDeploymentSvc = new PluginStepDeploymentService(this.CrmSvc, this.PackageLog);
                }

                return this.pluginStepDeploymentSvc;
            }
        }

        /// <inheritdoc/>
        public override bool AfterPrimaryImport()
        {
            this.SetDevelopmentHubEnvironmentVariables();
            this.SetDevelopmentHubPluginStepConfigurations();
            this.SetDevelopmentHubFlowConnections();

            return true;
        }

        /// <inheritdoc/>
        public override bool BeforeImportStage()
        {
            return true;
        }

        /// <inheritdoc/>
        public override string GetNameOfImport(bool plural) => "Development Hub";

        /// <inheritdoc/>
        public override void InitializeCustomExtension()
        {
            return;
        }

        /// <inheritdoc />
        public override UserRequestedImportAction OverrideSolutionImportDecision(string solutionUniqueName, Version organizationVersion, Version packageSolutionVersion, Version inboundSolutionVersion, Version deployedSolutionVersion, ImportAction systemSelectedImportAction)
        {
            if (this.ForceImportOnSameVersion && systemSelectedImportAction == ImportAction.SkipSameVersion)
            {
                return UserRequestedImportAction.ForceUpdate;
            }
            else
            {
                return UserRequestedImportAction.Default;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            this.licensedCrmSvc?.Dispose();
        }

        /// <summary>
        /// Activates all flows in a solution.
        /// </summary>
        /// <param name="solution">The solution unique name.</param>
        protected void ActivateFlowsInSolution(string solution)
        {
            var solutionId = this.SolutionDeploymentSvc.GetSolutionIdByUniqueName(solution);
            if (!solutionId.HasValue)
            {
                return;
            }

            var solutionWorkflowIds = this.SolutionDeploymentSvc.GetSolutionComponentObjectIdsByType(solutionId.Value, 29);
            if (!solutionWorkflowIds.Any())
            {
                return;
            }

            var solutionFlowIds = this.FlowDeploymentSvc.GetDeployedFlows(solutionWorkflowIds, new ColumnSet(false)).Select(f => f.Id);
            foreach (var flowId in solutionFlowIds)
            {
                this.FlowDeploymentSvc.ActivateFlow(flowId);
            }
        }

        /// <summary>
        /// Gets a setting either from runtime arguments or an environment variable (in that order). Environment variables should be prefixed with 'PACKAGEDEPLOYER_SETTINGS_'.
        /// </summary>
        /// <typeparam name="T">The type of argument.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The setting value (if found).</returns>
        protected T GetSetting<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be empty", nameof(key));
            }

            string value = null;

            if (this.RuntimeSettings != null && this.RuntimeSettings.ContainsKey(key))
            {
                var obj = this.RuntimeSettings[key];

                if (obj is T t)
                {
                    return t;
                }
                else if (obj is string s)
                {
                    value = s;
                }
            }

            if (value == null)
            {
                value = Environment.GetEnvironmentVariable($"PACKAGEDEPLOYER_SETTINGS_{key.ToUpperInvariant()}");
            }

            if (value != null)
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }

            return default;
        }

        private string GetInjectSecureConfigSecureConfiguration()
        {
            return JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                { "ClientId", this.ServicePrincipalClientId },
                { "ClientSecret", this.ServicePrincipalClientSecret },
                { "TenantId", this.CrmSvc.TenantId },
            });
        }

        private void SetDevelopmentHubFlowConnections()
        {
            if (!string.IsNullOrEmpty(this.ApprovalsConnectionName))
            {
                this.FlowDeploymentSvc.SetFlowConnection(new Guid("5004652f-f9b3-ea11-a812-000d3a86ad99"), "shared_approvals", this.ApprovalsConnectionName);
            }

            if (!string.IsNullOrEmpty(this.AzureDevOpsConnectionName))
            {
                this.FlowDeploymentSvc.SetFlowConnection(new Guid("a52d0ab8-54b1-e911-a97b-002248019881"), "shared_visualstudioteamservices_1", this.AzureDevOpsConnectionName);
            }

            this.ActivateFlowsInSolution("devhub_DevelopmentHub_Issues");
            this.ActivateFlowsInSolution("devhub_DevelopmentHub_Develop");
            this.ActivateFlowsInSolution("devhub_DevelopmentHub_AzureDevOps");
        }

        private void SetDevelopmentHubPluginStepConfigurations()
        {
            var secureConfiguration = this.PluginStepDeploymentSvc.CreateSdkMessageProcessingStepSecureConfig(this.GetInjectSecureConfigSecureConfiguration());
            var injectConfigPluginSteps = this.PluginStepDeploymentSvc.GetPluginStepsForHandler(new Guid("fdb0db23-769a-e911-a97d-002248010929"), new ColumnSet(false));

            foreach (var step in injectConfigPluginSteps)
            {
                this.PluginStepDeploymentSvc.SetPluginSecureConfiguration(step.Id, secureConfiguration);
            }
        }

        private void SetDevelopmentHubEnvironmentVariables()
        {
            this.EnvironmentVariableDeploymentSvc.SetEnvironmentVariable("devhub_AzureDevOpsOrganization", this.AzureDevOpsOrganisation);
            this.EnvironmentVariableDeploymentSvc.SetEnvironmentVariable("devhub_AzureDevOpsProject", this.AzureDevOpsProject);
            this.EnvironmentVariableDeploymentSvc.SetEnvironmentVariable("devhub_AzureDevOpsExtractBuildDefinition", this.AzureDevOpsPipelineId);
            this.EnvironmentVariableDeploymentSvc.SetEnvironmentVariable("devhub_SolutionPublisher", this.SolutionPublisherPrefix);
        }
    }
}
