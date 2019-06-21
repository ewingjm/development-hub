namespace Capgemini.DevelopmentHub.Develop.BusinessLogic
{
    using System;
    using System.Threading.Tasks;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Develop.Repositories;

    /// <summary>
    /// Imports solutions.
    /// </summary>
    public class SolutionImportService : ISolutionImportService
    {
        private readonly IODataClient oDataClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionImportService"/> class.
        /// </summary>
        /// <param name="oDataClient">The OData client.</param>
        public SolutionImportService(IODataClient oDataClient)
        {
            this.oDataClient = oDataClient ?? throw new ArgumentNullException(nameof(oDataClient));
        }

        /// <inheritdoc/>
        public async Task<ImportJobData> ImportSolutionZip(byte[] solutionZip)
        {
            if (solutionZip == null)
            {
                throw new ArgumentNullException(nameof(solutionZip));
            }

            var importJobId = Guid.NewGuid();
            var request = new SolutionImportRequest
            {
                ImportJobId = importJobId,
                CustomizationFile = Convert.ToBase64String(solutionZip),
                PublishWorkflows = true,
                OverwriteUnmanagedCustomizations = true,
            };

            await this.oDataClient.PostAsync("ImportSolution", request).ConfigureAwait(false);

            var importJob = await this.oDataClient.RetrieveAsync<ImportJob>($"importjobs({importJobId})").ConfigureAwait(false);

            return ImportJobData.ParseXml(importJob.Data);
        }
    }
}
