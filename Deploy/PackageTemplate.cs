namespace Capgemini.DevelopmentHub.Deployment
{
    using System.ComponentModel.Composition;
    using Capgemini.Xrm.Deployment.PackageDeployer;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// Import package starter frame.
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class PackageTemplate : CapgeminiPackageTemplate
    {
        /// <inheritdoc/>
        public override string GetImportPackageDataFolderName => "PkgFolder";

        /// <inheritdoc/>
        public override string GetImportPackageDescriptionText => "DevelopmentHub";

        /// <inheritdoc/>
        public override string GetLongNameOfImport => "DevelopmentHub";

        /// <inheritdoc/>
        public override string GetNameOfImport(bool plural) => "DevelopmentHub";
    }
}
