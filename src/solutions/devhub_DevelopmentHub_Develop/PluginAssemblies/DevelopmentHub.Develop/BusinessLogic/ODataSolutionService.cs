﻿namespace DevelopmentHub.Develop.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DevelopmentHub.BusinessLogic;
    using DevelopmentHub.BusinessLogic.Logging;
    using DevelopmentHub.Develop.Model;
    using DevelopmentHub.Develop.Model.OData;
    using DevelopmentHub.Develop.Model.Requests;
    using DevelopmentHub.Repositories;

    /// <summary>
    /// Imports solutions.
    /// </summary>
    public class ODataSolutionService : IODataSolutionService
    {
        private const string Tag = nameof(ODataSolutionService);

        private readonly IODataClient oDataClient;
        private readonly IODataEntityRepository<Model.OData.Solution> solutionRepository;
        private readonly IODataEntityRepository<SolutionComponent> solutionComponentRepository;
        private readonly IODataEntityRepository<ImportJob> importJobRepository;
        private readonly ILogWriter logWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSolutionService"/> class.
        /// </summary>
        /// <param name="repositoryFactory">A repository factory.</param>
        /// <param name="logWriter">Log writer.</param>
        public ODataSolutionService(IODataRepositoryFactory repositoryFactory, ILogWriter logWriter)
        {
            if (repositoryFactory == null)
            {
                throw new ArgumentNullException(nameof(repositoryFactory));
            }

            this.oDataClient = repositoryFactory.ODataClient;
            this.solutionRepository = repositoryFactory.GetRepository<Model.OData.Solution>();
            this.solutionComponentRepository = repositoryFactory.GetRepository<SolutionComponent>();
            this.importJobRepository = repositoryFactory.GetRepository<ImportJob>();
            this.logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        }

        /// <inheritdoc/>
        public async Task<ImportJobData> ImportSolutionZipAsync(byte[] solutionZip)
        {
            if (solutionZip == null)
            {
                throw new ArgumentNullException(nameof(solutionZip));
            }

            var importJobId = Guid.NewGuid();
            var request = new ImportSolutionRequest
            {
                ImportJobId = importJobId,
                CustomizationFile = Convert.ToBase64String(solutionZip),
                PublishWorkflows = true,
                OverwriteUnmanagedCustomizations = true,
            };

            this.logWriter.Log(Severity.Info, Tag, $"Importing solution zip.");
            await this.oDataClient.PostAsync("ImportSolution", request).ConfigureAwait(false);

            this.logWriter.Log(Severity.Info, Tag, $"Retrieving import job {importJobId}.");
            var importJob = await this.importJobRepository.RetrieveAsync(importJobId).ConfigureAwait(false);

            return ImportJobData.ParseXml(importJob.Data);
        }

        /// <inheritdoc/>
        public async Task MergeSolutionComponentsAsync(string sourceSolutionUniqueName, string targetSolutionUniqueName, bool deleteSourceSolutionAfterMerge)
        {
            if (sourceSolutionUniqueName == null)
            {
                throw new ArgumentNullException(nameof(sourceSolutionUniqueName));
            }

            if (targetSolutionUniqueName == null)
            {
                throw new ArgumentNullException(nameof(targetSolutionUniqueName));
            }

            var sourceSolution = await this.GetSolutionByUniqueNameAsync(sourceSolutionUniqueName, new string[] { "solutionid" }).ConfigureAwait(false);
            var sourceSolutionComponents = await this.GetSolutionComponents(sourceSolution.SolutionId).ConfigureAwait(false);

            var targetSolution = await this.GetSolutionByUniqueNameAsync(targetSolutionUniqueName, new string[] { "solutionid" }).ConfigureAwait(false);
            var targetSolutionComponents = await this.GetSolutionComponents(targetSolution.SolutionId).ConfigureAwait(false);

            this.logWriter.Log(Severity.Info, Tag, $"Merging {sourceSolutionComponents.Count()} solution components from {sourceSolutionUniqueName} to {targetSolutionUniqueName}.");

            // Solution components added synchronously due to solutions becoming corrupted when components were added in parallel.
            foreach (var sourceComponent in sourceSolutionComponents)
            {
                await this.GetTaskForComponent(sourceComponent, targetSolutionComponents, targetSolutionUniqueName).ConfigureAwait(false);
            }

            if (deleteSourceSolutionAfterMerge)
            {
                this.logWriter.Log(Severity.Info, Tag, $"Merge complete. Deleting source solution {sourceSolutionUniqueName}.");
                await this.solutionRepository.DeleteAsync(sourceSolution.SolutionId).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetSolutionZipAsync(string solutionUniqueName, bool isManaged)
        {
            if (string.IsNullOrEmpty(solutionUniqueName))
            {
                throw new ArgumentException("message", nameof(solutionUniqueName));
            }

            var exportResponse = await this.oDataClient.PostAsync<ExportSolutionRequest, ExportSolutionResponse>(
                "ExportSolution",
                new ExportSolutionRequest
                {
                    Managed = isManaged,
                    SolutionName = solutionUniqueName,
                }).ConfigureAwait(false);

            return exportResponse?.ExportSolutionFile;
        }

        /// <inheritdoc/>
        public async Task<Model.OData.Solution> GetSolutionByUniqueNameAsync(string uniqueName, string[] fields)
        {
            this.logWriter.Log(Severity.Info, Tag, $"Retrieving solution {uniqueName}.");
            var solutions = await this.solutionRepository.FindAsync($"uniquename eq '{uniqueName}'", fields).ConfigureAwait(false);

            this.logWriter.Log(Severity.Info, Tag, $"Retrieved solution {solutions.First().SolutionId}.");

            if (!solutions.Any())
            {
                throw new ArgumentException($"A target solution with a unique name of {uniqueName} was not found.", nameof(uniqueName));
            }

            return solutions.First();
        }

        /// <inheritdoc/>
        public async Task UpdateSolutionVersionAsync(string solutionUniqueName, string solutionVersion)
        {
            if (string.IsNullOrEmpty(solutionUniqueName))
            {
                throw new ArgumentException("Solution unique name was not provided.", nameof(solutionUniqueName));
            }

            if (string.IsNullOrEmpty(solutionVersion))
            {
                throw new ArgumentException("Solution version was not provided.", nameof(solutionVersion));
            }

            var solution = await this.GetSolutionByUniqueNameAsync(solutionUniqueName, new string[] { "solutionid" }).ConfigureAwait(false);

            this.logWriter.Log(Severity.Info, Tag, $"Updating version of solution {solutionUniqueName} to {solutionVersion}.");
            solution.Version = solutionVersion;

            await this.solutionRepository.UpdateAsync(solution).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task PublishAllAsync()
        {
            return this.oDataClient.PostAsync("PublishAllXml", Array.Empty<byte>());
        }

        private Task GetTaskForComponent(SolutionComponent sourceComponent, IEnumerable<SolutionComponent> targetSolutionComponents, string targetSolutionUniqueName)
        {
            this.logWriter.Log(Severity.Info, Tag, $"Getting task for solution component {sourceComponent.ObjectId}.");

            var targetComponent = targetSolutionComponents.FirstOrDefault(component => component.ObjectId == sourceComponent.ObjectId);

            if (targetComponent == null)
            {
                return this.AddSolutionComponent(sourceComponent, targetSolutionUniqueName);
            }
            else if (targetComponent.RootComponentBehavior.HasValue && targetComponent.RootComponentBehavior != sourceComponent.RootComponentBehavior && targetComponent.RootComponentBehavior != 0)
            {
                return this.UpdateSolutionComponent(sourceComponent, targetComponent, targetSolutionUniqueName);
            }
            else
            {
                this.logWriter.Log(Severity.Info, Tag, $"No task required. Component already present in target solution.");
                return Task.CompletedTask;
            }
        }

        private Task UpdateSolutionComponent(SolutionComponent sourceComponent, SolutionComponent targetComponent, string targetSolutionUniqueName)
        {
            this.logWriter.Log(Severity.Info, Tag, $"Updating solution component behaviour {sourceComponent.ObjectId} for solution {targetSolutionUniqueName}.");
            return this.oDataClient.PostAsync(
                "UpdateSolutionComponent",
                new UpdateSolutionComponentRequest(
                    targetComponent.ObjectId,
                    targetComponent.ComponentType,
                    targetSolutionUniqueName,
                    sourceComponent.RootComponentBehavior == 2 ? Array.Empty<string>() : null));
        }

        private Task AddSolutionComponent(SolutionComponent sourceComponent, string targetSolutionUniqueName)
        {
            this.logWriter.Log(Severity.Info, Tag, $"Adding solution component {sourceComponent.ObjectId} for solution {targetSolutionUniqueName}.");
            return this.oDataClient.PostAsync(
                "AddSolutionComponent",
                new AddSolutionComponentRequest(
                    sourceComponent.ObjectId,
                    sourceComponent.ComponentType,
                    targetSolutionUniqueName,
                    false,
                    sourceComponent.RootComponentBehavior != 0,
                    sourceComponent.RootComponentBehavior == 2 ? Array.Empty<string>() : null));
        }

        private Task<IEnumerable<SolutionComponent>> GetSolutionComponents(Guid solutionId)
        {
            this.logWriter.Log(Severity.Info, Tag, $"Retrieving solution components for solution {solutionId}.");
            var filter = $"_solutionid_value eq {solutionId}";
            var fields = new string[] { "solutioncomponentid", "componenttype", "objectid", "rootcomponentbehavior", "rootsolutioncomponentid" };

            return this.solutionComponentRepository.FindAsync(filter, fields);
        }
    }
}
