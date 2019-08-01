namespace Capgemini.DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Develop.BusinessLogic;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Develop.Model.OData;
    using Capgemini.DevelopmentHub.Develop.Model.Requests;
    using Capgemini.DevelopmentHub.Model;
    using Capgemini.DevelopmentHub.Repositories;
    using Moq;
    using Xunit;
    using Solution = Capgemini.DevelopmentHub.Develop.Model.OData.Solution;

    /// <summary>
    /// Tests for the <see cref="ODataSolutionService"/>.
    /// </summary>
    public class ODataSolutionServiceTests
    {
        private const string ImportJobDataString = "<solutionManifest><result result=\"success\" errortext=\"\"></result></solutionManifest>";

        private readonly IODataSolutionService oDataSolutionService;

        private readonly Mock<IODataClient> oDataClientMock;
        private readonly Mock<IODataRepositoryFactory> repositoryFactoryMock;
        private readonly Mock<IODataEntityRepository<Solution>> solutionRepositoryMock;
        private readonly Mock<IODataEntityRepository<ImportJob>> importJobRepositoryMock;
        private readonly Mock<IODataEntityRepository<SolutionComponent>> solutionComponentRepositoryMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSolutionServiceTests"/> class.
        /// </summary>
        public ODataSolutionServiceTests()
        {
            this.oDataClientMock = new Mock<IODataClient>();
            this.solutionRepositoryMock = new Mock<IODataEntityRepository<Solution>>();
            this.importJobRepositoryMock = new Mock<IODataEntityRepository<ImportJob>>();
            this.solutionComponentRepositoryMock = new Mock<IODataEntityRepository<SolutionComponent>>();
            this.repositoryFactoryMock = new Mock<IODataRepositoryFactory>();
            this.repositoryFactoryMock.SetupProperty(rf => rf.ODataClient, this.oDataClientMock.Object);
            this.repositoryFactoryMock.SetReturnsDefault(this.importJobRepositoryMock.Object);
            this.repositoryFactoryMock.SetReturnsDefault(this.solutionComponentRepositoryMock.Object);
            this.repositoryFactoryMock.SetReturnsDefault(this.solutionRepositoryMock.Object);
            this.oDataSolutionService = new ODataSolutionService(this.repositoryFactoryMock.Object, new ConsoleLogWriter());
        }

        /// <summary>
        /// Tests that the constructor throws an exception is a null OData client is provided.
        /// </summary>
        [Fact]
        public void SolutionImportService_NullODataClient_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
             {
                 new ODataSolutionService(null, new ConsoleLogWriter());
             });
        }

        /// <summary>
        /// Tests that <see cref="ODataSolutionService.ImportSolutionZip(byte[], bool, bool)"/> throws an exception if a null OData client is provided.
        /// </summary>
        [Fact]
        public void ImportSolutionZip_NullSolutionZip_Throws()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await this.oDataSolutionService.ImportSolutionZipAsync(null);
            });
        }

        /// <summary>
        /// Verifies that the solution zip bytes are converted to a Base64 encoded string.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ImportSolutionZip_SolutionZip_IsConvertedToBase64EncodedString()
        {
            var solutionBytes = Encoding.UTF8.GetBytes("this is a test string");
            this.oDataClientMock.Setup(
                client => client.PostAsync(
                    "ImportSolution",
                    It.Is<ImportSolutionRequest>(request => request.CustomizationFile == Convert.ToBase64String(solutionBytes))))
                    .Returns(Task.FromResult(new ODataClientResponse(Array.Empty<byte>(), null)))
                    .Verifiable();
            this.importJobRepositoryMock.SetReturnsDefault(Task.FromResult(new ImportJob { Data = ImportJobDataString }));

            await this.oDataSolutionService.ImportSolutionZipAsync(solutionBytes).ConfigureAwait(false);

            this.oDataClientMock.Verify();
        }

        /// <summary>
        /// Asserts that calling <see cref="ODataSolutionService.MergeSolutionComponents(string, string, bool)"/> with a null source solution throws an exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MergeSolutionComponents_NullSourceSolution_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.oDataSolutionService.MergeSolutionComponentsAsync(null, "cap_Target", true))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Asserts that calling <see cref="ODataSolutionService.MergeSolutionComponents(string, string, bool)"/> with a null target solution throws an exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MergeSolutionComponents_NullTargetSolution_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.oDataSolutionService.MergeSolutionComponentsAsync("cap_Source", null, true))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Asserts that calling <see cref="ODataSolutionService.MergeSolutionComponents(string, string, bool)"/> with true for DeleteSourceSolutionAfter merge will delete the source solution.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MergeSolutionComponents_DeleteSourceSolutionAfterMerge_DeletesSourceSolution()
        {
            var sourceSolutionName = "cap_Source";
            var sourceSolutionId = Guid.NewGuid();
            this.MockSolution(sourceSolutionName, sourceSolutionId, Enumerable.Empty<SolutionComponent>());
            var targetSolutionName = "cap_Target";
            var targetSolutionId = Guid.NewGuid();
            this.MockSolution(targetSolutionName, targetSolutionId, Enumerable.Empty<SolutionComponent>());

            this.solutionRepositoryMock
                .Setup(repo => repo.DeleteAsync(It.Is<Guid>(guid => guid == sourceSolutionId)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await this.oDataSolutionService.MergeSolutionComponentsAsync("cap_Source", "cap_Target", true).ConfigureAwait(false);

            this.solutionRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// Asserts that calling <see cref="ODataSolutionService.MergeSolutionComponents(string, string, bool)"/> with false for DeleteSourceSolutionAfter merge will not delete the source solution.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MergeSolutionComponents_DeleteSourceSolutionAfterMergeFalse_DoesNotDeleteSourceSolution()
        {
            var sourceSolutionName = "cap_Source";
            var sourceSolutionId = Guid.NewGuid();
            this.MockSolution(sourceSolutionName, sourceSolutionId, Enumerable.Empty<SolutionComponent>());
            var targetSolutionName = "cap_Target";
            var targetSolutionId = Guid.NewGuid();
            this.MockSolution(targetSolutionName, targetSolutionId, Enumerable.Empty<SolutionComponent>());

            await this.oDataSolutionService.MergeSolutionComponentsAsync("cap_Source", "cap_Target", false).ConfigureAwait(false);

            this.solutionRepositoryMock.Verify(repo => repo.DeleteAsync(It.Is<Guid>(guid => guid == sourceSolutionId)), Times.Never);
        }

        /// <summary>
        /// Assets that calling <see cref="ODataSolutionService.MergeSolutionComponents(string, string, bool)"/> will add solution components to that target that are only found in the source.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MergeSolutionComponents_ComponentInSourceOnly_AddsSolutionComponent()
        {
            var sourceSolutionName = "cap_Source";
            var sourceSolutionId = Guid.NewGuid();
            var sourceSolutionComponent = new SolutionComponent { ObjectId = Guid.NewGuid() };
            this.MockSolution(sourceSolutionName, sourceSolutionId, new SolutionComponent[] { sourceSolutionComponent });
            var targetSolutionName = "cap_Target";
            var targetSolutionId = Guid.NewGuid();
            this.MockSolution(targetSolutionName, targetSolutionId, Enumerable.Empty<SolutionComponent>());

            this.oDataClientMock.
                Setup(client => client.PostAsync("AddSolutionComponent", It.Is<AddSolutionComponentRequest>(req => req.ComponentId == sourceSolutionComponent.ObjectId)))
                .Returns(Task.FromResult(new ODataClientResponse(null, null)))
                .Verifiable();

            await this.oDataSolutionService.MergeSolutionComponentsAsync("cap_Source", "cap_Target", false).ConfigureAwait(false);

            this.oDataClientMock.VerifyAll();
        }

        /// <summary>
        /// Assets that calling <see cref="ODataSolutionService.MergeSolutionComponents(string, string, bool)"/> will update solution components behavior to include metadata if the source component includes metadata.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MergeSolutionComponents_SourceIncludesMetadataTargetExcludesMetadata_UpdatesSolutionComponentToIncludeMetadata()
        {
            var sourceSolutionName = "cap_Source";
            var sourceSolutionId = Guid.NewGuid();
            var sourceSolutionComponent = new SolutionComponent { ObjectId = Guid.NewGuid(), RootComponentBehavior = 1 };
            this.MockSolution(sourceSolutionName, sourceSolutionId, new SolutionComponent[] { sourceSolutionComponent });
            var targetSolutionName = "cap_Target";
            var targetSolutionId = Guid.NewGuid();
            var targetSolutionComponent = new SolutionComponent { ObjectId = sourceSolutionComponent.ObjectId, RootComponentBehavior = 2 };
            this.MockSolution(targetSolutionName, targetSolutionId, new SolutionComponent[] { targetSolutionComponent });

            this.oDataClientMock
                .Setup(client => client.PostAsync("UpdateSolutionComponent", It.Is<UpdateSolutionComponentRequest>(req => req.ComponentId == targetSolutionComponent.ObjectId && req.IncludedComponentSettingsValues == null)))
                .Returns(Task.FromResult(new ODataClientResponse(null, null)))
                .Verifiable();

            await this.oDataSolutionService.MergeSolutionComponentsAsync("cap_Source", "cap_Target", false).ConfigureAwait(false);

            this.oDataClientMock.VerifyAll();
        }

        /// <summary>
        /// Assets that calling <see cref="ODataSolutionService.MergeSolutionComponents(string, string, bool)"/> will update solution components behavior to include metadata if the source component includes metadata.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task MergeSolutionComponents_SourceExcludesMetadataTargetIncludesMetadata_UpdatesSolutionComponentToExcludeMetadata()
        {
            var sourceSolutionName = "cap_Source";
            var sourceSolutionId = Guid.NewGuid();
            var sourceSolutionComponent = new SolutionComponent { ObjectId = Guid.NewGuid(), RootComponentBehavior = 0 };
            this.MockSolution(sourceSolutionName, sourceSolutionId, new SolutionComponent[] { sourceSolutionComponent });
            var targetSolutionName = "cap_Target";
            var targetSolutionId = Guid.NewGuid();
            var targetSolutionComponent = new SolutionComponent { ObjectId = sourceSolutionComponent.ObjectId, RootComponentBehavior = 2 };
            this.MockSolution(targetSolutionName, targetSolutionId, new SolutionComponent[] { targetSolutionComponent });

            this.oDataClientMock.
                Setup(client => client.PostAsync("UpdateSolutionComponent", It.Is<UpdateSolutionComponentRequest>(req => req.ComponentId == targetSolutionComponent.ObjectId && req.IncludedComponentSettingsValues == null)))
                .Returns(Task.FromResult(new ODataClientResponse(null, null)))
                .Verifiable();

            await this.oDataSolutionService.MergeSolutionComponentsAsync("cap_Source", "cap_Target", false).ConfigureAwait(false);

            this.oDataClientMock.VerifyAll();
        }

        private void MockSolution(string solutionUniqueName, Guid solutionId, IEnumerable<SolutionComponent> components)
        {
            var sourceSolution = new Solution { SolutionId = solutionId };
            this.solutionRepositoryMock
                .Setup(repo => repo.FindAsync(It.Is<string>(s => s.Contains(solutionUniqueName)), It.IsAny<string[]>()))
                .Returns(Task.FromResult(new Solution[] { sourceSolution }.AsEnumerable()));
            this.solutionComponentRepositoryMock
                .Setup(repo => repo.FindAsync(It.Is<string>(s => s.Contains(sourceSolution.EntityId.ToString())), It.IsAny<string[]>()))
                .Returns(Task.FromResult(components));
        }
    }
}
