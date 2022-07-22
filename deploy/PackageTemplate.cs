namespace DevelopmentHub.Deployment
{
    using System;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.ServiceModel;
    using Capgemini.PowerApps.PackageDeployerTemplate;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
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

            return base.AfterPrimaryImport();
        }

        /// <inheritdoc/>
        public override void RunSolutionUpgradeMigrationStep(string solutionName, string oldVersion, string newVersion, Guid oldSolutionId, Guid newSolutionId)
        {
            if (string.IsNullOrEmpty(solutionName))
            {
                throw new ArgumentException($"'{nameof(solutionName)}' cannot be null or empty", nameof(solutionName));
            }

            if (string.IsNullOrEmpty(oldVersion))
            {
                throw new ArgumentException($"'{nameof(oldVersion)}' cannot be null or empty", nameof(oldVersion));
            }

            if (string.IsNullOrEmpty(newVersion))
            {
                throw new ArgumentException($"'{nameof(newVersion)}' cannot be null or empty", nameof(newVersion));
            }

            base.RunSolutionUpgradeMigrationStep(solutionName, oldVersion, newVersion, oldSolutionId, newSolutionId);

            if (solutionName == "devhub_DevelopmentHub_Develop" && oldVersion.StartsWith("0.2", StringComparison.OrdinalIgnoreCase))
            {
                this.DefaultEnvironmentLifetimes();
                this.DefaultMergeStrategies();
            }
        }

        /// <inheritdoc/>
        public override string GetNameOfImport(bool plural) => "Development Hub";

        /// <inheritdoc />
        public override UserRequestedImportAction OverrideSolutionImportDecision(string solutionUniqueName, Version organizationVersion, Version packageSolutionVersion, Version inboundSolutionVersion, Version deployedSolutionVersion, ImportAction systemSelectedImportAction)
        {
            if (this.ForceImportOnSameVersion && (systemSelectedImportAction == ImportAction.SkipSameVersion || systemSelectedImportAction == ImportAction.SkipLowerVersion))
            {
                return UserRequestedImportAction.ForceUpdate;
            }

            return base.OverrideSolutionImportDecision(solutionUniqueName, organizationVersion, packageSolutionVersion, inboundSolutionVersion, deployedSolutionVersion, systemSelectedImportAction);
        }

        private void DefaultEnvironmentLifetimes()
        {
            this.PackageLog.Log("Default existing environment lifetimes to 'Static'.");

            var environmentQuery = new QueryByAttribute("devhub_environment");
            environmentQuery.AddAttributeValue("devhub_lifetime", null);

            var environments = this.CrmServiceAdapter.RetrieveMultiple(environmentQuery);
            this.PackageLog.Log($"Found {environments.Entities.Count} environments to update.");

            foreach (var environment in environments.Entities)
            {
                this.PackageLog.Log($"Updating environment {environment.Id}.");

                environment.Attributes.Add("devhub_lifetime", new OptionSetValue(353400000) /*Static*/);
                try
                {
                    this.CrmSvc.Update(environment);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    this.PackageLog.Log($"Failed to update environment {environment.Id}.", TraceEventType.Error, ex);
                }
            }
        }

        private void DefaultMergeStrategies()
        {
            this.PackageLog.Log("Default existing solutions to a merge strategy of 'Sequential'.");

            var solutionQuery = new QueryByAttribute("devhub_solution");
            solutionQuery.AddAttributeValue("devhub_mergestrategy", null);
            solutionQuery.ColumnSet = new ColumnSet("devhub_stagingenvironment");

            var solutions = this.CrmServiceAdapter.RetrieveMultiple(solutionQuery);
            this.PackageLog.Log($"Found {solutions.Entities.Count} solutions to update.");

            foreach (var solution in solutions.Entities)
            {
                this.PackageLog.Log($"Updating solution {solution.Id}.");
                solution.Attributes.Add("devhub_mergestrategy", new OptionSetValue(353400000) /*Sequential*/);

                try
                {
                    this.CrmSvc.Update(solution);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    this.PackageLog.Log($"Failed to update solution {solution.Id}.", TraceEventType.Error, ex);
                }

                this.PackageLog.Log("Getting solution merges for solution.");

                var solutionMergeQuery = new QueryByAttribute("devhub_solutionmerge");
                solutionMergeQuery.AddAttributeValue("devhub_targetsolution", solution.Id);
                var solutionMerges = this.CrmServiceAdapter.RetrieveMultiple(solutionMergeQuery);

                foreach (var solutionMerge in solutionMerges.Entities)
                {
                    solutionMerge.Attributes.Add("devhub_environment", solution["devhub_stagingenvironment"]);
                    solutionMerge.Attributes.Add("devhub_mergestrategy", new OptionSetValue(353400000) /*Sequential*/);

                    try
                    {
                        this.CrmSvc.Update(solutionMerge);
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        this.PackageLog.Log($"Failed to update solution merge {solutionMerge.Id}.", TraceEventType.Error, ex);
                    }
                }
            }
        }

        private void SetDevelopmentHubEnvironmentVariables()
        {
            this.EnvironmentVariableDeploymentSvc.SetEnvironmentVariable("devhub_AzureDevOpsOrganization", this.AzureDevOpsOrganisation);
            this.EnvironmentVariableDeploymentSvc.SetEnvironmentVariable("devhub_SolutionPublisher", this.SolutionPublisherPrefix);
        }
    }
}
