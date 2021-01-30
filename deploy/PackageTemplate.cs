namespace DevelopmentHub.Deployment
{
    using System;
    using System.ComponentModel.Composition;
    using Capgemini.PowerApps.PackageDeployerTemplate;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// Import package starter frame.
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class PackageTemplate : PackageTemplateBase
    {
        private bool? forceImport;
        private string azureDevOpsOrganisation;
        private string solutionPublisherPrefix;
        private string azureDevOpsConnectionName;
        private string approvalsConnectionName;
        private EnvironmentVariableDeploymentService environmentVariableDeploymentSvc;

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

        /// <inheritdoc/>
        public override bool AfterPrimaryImport()
        {
            this.SetDevelopmentHubEnvironmentVariables();

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

        private void SetDevelopmentHubEnvironmentVariables()
        {
            this.EnvironmentVariableDeploymentSvc.SetEnvironmentVariable("devhub_AzureDevOpsOrganization", this.AzureDevOpsOrganisation);
            this.EnvironmentVariableDeploymentSvc.SetEnvironmentVariable("devhub_SolutionPublisher", this.SolutionPublisherPrefix);
        }
    }
}
