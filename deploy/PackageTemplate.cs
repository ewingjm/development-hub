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
    }
}
