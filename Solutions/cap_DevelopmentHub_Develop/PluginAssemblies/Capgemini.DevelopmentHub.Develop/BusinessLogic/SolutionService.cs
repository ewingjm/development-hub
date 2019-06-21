namespace Capgemini.DevelopmentHub.Develop.BusinessLogic
{
    using System;
    using System.Linq;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Repositories;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Builds solutions.
    /// </summary>
    public class SolutionService : ISolutionService
    {
        private const string Tag = nameof(SolutionService);

        private readonly ICrmRepository<Solution> solutionRepo;
        private readonly ICrmRepository<Publisher> publisherRepo;

        private readonly IOrganizationService organizationService;
        private readonly ILogWriter logWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionService"/> class.
        /// </summary>
        /// <param name="repositoryFactory">Repository provider.</param>
        /// <param name="logWriter">Log writer.</param>
        public SolutionService(IRepositoryFactory repositoryFactory, ILogWriter logWriter)
        {
            if (repositoryFactory == null)
            {
                throw new ArgumentNullException(nameof(repositoryFactory));
            }

            this.solutionRepo = repositoryFactory.GetRepository<DevelopContext, Solution>();
            this.publisherRepo = repositoryFactory.GetRepository<DevelopContext, Publisher>();

            this.organizationService = repositoryFactory.OrganizationService;
            this.logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        }

        /// <inheritdoc/>
        public EntityReference Create(string uniqueName, string displayName, string description)
        {
            if (string.IsNullOrEmpty(uniqueName))
            {
                throw new ArgumentException("The solution unique name was null or empty.", nameof(uniqueName));
            }

            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentException("The solution display name was null or empty.", nameof(displayName));
            }

            this.logWriter.Log(Severity.Info, Tag, $"{nameof(this.Create)}: Creating solution {uniqueName}.");

            return new EntityReference(
                Solution.EntityLogicalName,
                this.solutionRepo.Create(
                    new Solution
                    {
                        UniqueName = uniqueName,
                        FriendlyName = displayName,
                        Description = description,
                        PublisherId = this.GetSolutionPublisher(uniqueName),
                    }));
        }

        /// <inheritdoc/>
        public EntityReference GetSolutionPublisher(string uniqueName)
        {
            if (string.IsNullOrEmpty(uniqueName))
            {
                throw new ArgumentException("The solution unique name was null or empty", nameof(uniqueName));
            }

            this.logWriter.Log(Severity.Info, Tag, $"{nameof(this.GetSolutionPublisher)}: Getting publisher for {uniqueName}.");

            var uniqueNameParts = uniqueName.Split('_');

            if (uniqueNameParts.Count() < 2)
            {
                throw new ArgumentException("Unique name must have a publisher prefix.", nameof(uniqueName));
            }

            this.logWriter.Log(Severity.Verbose, Tag, $"{nameof(this.GetSolutionPublisher)}: Getting publisher ID for prefix {uniqueNameParts}.");

            var publisher = this.publisherRepo
                .Find(filter => filter.CustomizationPrefix == uniqueNameParts.First(), select => new Publisher { PublisherId = select.PublisherId })
                .FirstOrDefault();

            if (publisher == null)
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, $"Unable to find publisher for prefix {uniqueNameParts}");
            }

            return publisher.ToEntityReference();
        }

        /// <inheritdoc/>
        public byte[] GetSolutionZip(string solutionUniqueName, bool managed)
        {
            if (string.IsNullOrEmpty(solutionUniqueName))
            {
                throw new ArgumentException("The solution unique name was null or empty", nameof(solutionUniqueName));
            }

            this.logWriter.Log(Severity.Info, Tag, $"Retrieving {solutionUniqueName} as an {(managed ? "managed" : "unmanaged")} solution zip.");

            var response = (ExportSolutionResponse)this.organizationService.Execute(
                new ExportSolutionRequest
                {
                    SolutionName = solutionUniqueName,
                    Managed = managed,
                });

            this.logWriter.Log(Severity.Info, Tag, $"Downloaded {response.ExportSolutionFile.Length / 1024}KB.");

            return response.ExportSolutionFile;
        }
    }
}