namespace Capgemini.DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Capgemini.DevelopmentHub.Develop.BusinessLogic;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Develop.Repositories;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="SolutionImportService"/>.
    /// </summary>
    public class SolutionImportServiceTests
    {
        private const string ImportJobDataString = "<solutionManifest><result result=\"success\" errortext=\"\"></result></solutionManifest>";

        private readonly ISolutionImportService solutionImportService;
        private readonly Mock<IODataClient> oDataClientMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionImportServiceTests"/> class.
        /// </summary>
        public SolutionImportServiceTests()
        {
            this.oDataClientMock = new Mock<IODataClient>();
            this.solutionImportService = new SolutionImportService(this.oDataClientMock.Object);

            this.oDataClientMock.SetReturnsDefault(Task.FromResult(new ImportJob { Data = ImportJobDataString }));
        }

        /// <summary>
        /// Tests that the constructor throws an exception is a null OData client is provided.
        /// </summary>
        [Fact]
        public void SolutionImportService_NullODataClient_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
             {
                 new SolutionImportService(null);
             });
        }

        /// <summary>
        /// Tests that <see cref="SolutionImportService.ImportSolutionZip(byte[], bool, bool)"/> throws an exception if a null OData client is provided.
        /// </summary>
        [Fact]
        public void ImportSolutionZip_NullSolutionZip_Throws()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await this.solutionImportService.ImportSolutionZip(null);
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
                    It.Is<SolutionImportRequest>(request => request.CustomizationFile == Convert.ToBase64String(solutionBytes))))
                    .Returns(Task.FromResult(Array.Empty<byte>()))
                    .Verifiable();

            await this.solutionImportService.ImportSolutionZip(solutionBytes).ConfigureAwait(false);

            this.oDataClientMock.Verify();
        }
    }
}
