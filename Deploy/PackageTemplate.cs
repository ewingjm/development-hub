namespace DevelopmentHub.Deployment
{
    using System.ComponentModel.Composition;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// Import package starter frame.
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class PackageTemplate : ImportExtension
    {
        /// <inheritdoc/>
        public override string GetImportPackageDataFolderName => "PkgFolder";

        /// <inheritdoc/>
        public override string GetImportPackageDescriptionText => "Development Hub";

        /// <inheritdoc/>
        public override string GetLongNameOfImport => "Development Hub";

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
    }
}
