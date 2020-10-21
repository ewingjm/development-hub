namespace DevelopmentHub.Deployment
{
    using System;
    using System.ComponentModel.Composition;
    using System.Globalization;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// Import package starter frame.
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class PackageTemplate : ImportExtension
    {
        private bool? forceImport;

        /// <inheritdoc/>
        public override string GetImportPackageDataFolderName => "PkgFolder";

        /// <inheritdoc/>
        public override string GetImportPackageDescriptionText => "Development Hub";

        /// <inheritdoc/>
        public override string GetLongNameOfImport => "Development Hub";

        /// <summary>
        /// Gets or sets a value indicating whether solutions should import even when solution versions match. Useful for pipelines that run on topic branches.
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

            set
            {
                this.forceImport = value;
            }
        }

        /// <inheritdoc/>
        public override bool AfterPrimaryImport()
        {
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

        private T GetSetting<T>(string key)
        {
            string value = null;

            if (this.RuntimeSettings.ContainsKey(key))
            {
                value = (string)this.RuntimeSettings[key];
            }

            if (value == null)
            {
                value = Environment.GetEnvironmentVariable($"PACKAGEDEPLOYER_SETTINGS_{key.ToUpperInvariant()}");
            }

            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}
