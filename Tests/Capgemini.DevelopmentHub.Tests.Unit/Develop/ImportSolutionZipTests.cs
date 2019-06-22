namespace Capgemini.DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Capgemini.DevelopmentHub.Develop.BusinessLogic;
    using Capgemini.DevelopmentHub.Develop.CodeActivities;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Develop.Plugins;
    using Capgemini.DevelopmentHub.Develop.Repositories;
    using FakeXrmEasy;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="ImportSolutionZip"/> custom workflow activity.
    /// </summary>
    public class ImportSolutionZipTests : FakedContextTest
    {
        private readonly ImportSolutionZip importSolutionZip;
        private readonly Mock<ISolutionImportService> solutionImportServiceMock;
        private readonly Mock<IOAuthTokenRepository> oAuthTokenRepositoryMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportSolutionZipTests"/> class.
        /// </summary>
        public ImportSolutionZipTests()
        {
            this.solutionImportServiceMock = new Mock<ISolutionImportService>();
            this.oAuthTokenRepositoryMock = new Mock<IOAuthTokenRepository>();
            this.importSolutionZip = new ImportSolutionZip(this.solutionImportServiceMock.Object, this.oAuthTokenRepositoryMock.Object);
        }

        /// <summary>
        /// Tests that executing <see cref="ImportSolutionZip"/> with no target instance throws an exception.
        /// </summary>
        [Fact]
        public void ImportSolutionZip_NoTargetInstanceUrl_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                this.FakedContext.ExecuteCodeActivity<ImportSolutionZip>(new Dictionary<string, object>
                {
                    { nameof(ImportSolutionZip.SolutionZip), "SOLUTION ZIP" },
                    { nameof(ImportSolutionZip.TargetInstanceUrl), null },
                });
            });
        }

        /// <summary>
        /// Tests that executing <see cref="ImportSolutionZip"/> with no solution zip throws an exception.
        /// </summary>
        [Fact]
        public void ImportSolutionZip_NoSolutionZip_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                this.FakedContext.ExecuteCodeActivity<ImportSolutionZip>(new Dictionary<string, object>
                {
                    { nameof(ImportSolutionZip.SolutionZip), null },
                    { nameof(ImportSolutionZip.TargetInstanceUrl), "https://targetinstance.crm11.dynamics.com" },
                });
            });
        }

        /// <summary>
        /// Tests that executing <see cref="ImportSolutionZip"/> with no injected configuration throws an exception.
        /// </summary>
        [Fact]
        public void ImportSolutionZip_NoInjectedConfig_ThrowsConfigurationException()
        {
            Assert.Throws<Exception>(() =>
            {
                this.FakedContext.ExecuteCodeActivity<ImportSolutionZip>(this.GetValidInputs());
            });
        }

        /// <summary>
        /// Tests that the target instance URL is used when making the <see cref="OAuthPasswordGrantRequest"/>.
        /// </summary>
        [Fact]
        public void ImportSolutionZip_PasswordGrantRequest_ResourceSetToTargetInstanceUrl()
        {
            var expectedResource = "https://targetinstance.crm11.dynamics.com";
            this.oAuthTokenRepositoryMock
                .Setup(o => o.GetAccessToken(It.Is<OAuthPasswordGrantRequest>(req => req.Resource == new Uri(expectedResource))))
                .ReturnsAsync(new OAuthToken { AccessToken = "ACCESS TOKEN" })
                .Verifiable();
            this.solutionImportServiceMock.SetReturnsDefault(Task.FromResult(new ImportJobData()));

            this.FakedContext.ExecuteCodeActivity(
                this.GetConfiguredWorkflowContext(),
                new Dictionary<string, object>
                {
                    { nameof(ImportSolutionZip.SolutionZip), Convert.ToBase64String(Encoding.UTF8.GetBytes("SOLUTION ZIP")) },
                    { nameof(ImportSolutionZip.TargetInstanceUrl), expectedResource },
                },
                this.importSolutionZip);

            this.oAuthTokenRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// Tests that the inputted solution zip file is passsed to the solution import service.
        /// </summary>
        [Fact]
        public void ImportSolution_SolutionZip_Imported()
        {
            this.MockAccessTokenResult();
            var expectedSolutionZip = Convert.ToBase64String(Encoding.UTF8.GetBytes("SOLUTION ZIP"));
            this.solutionImportServiceMock
                .Setup(s => s.ImportSolutionZip(It.Is<byte[]>(solution => Convert.ToBase64String(solution) == expectedSolutionZip)))
                .Returns(Task.FromResult(new ImportJobData()))
                .Verifiable();

            this.FakedContext.ExecuteCodeActivity(
                this.GetConfiguredWorkflowContext(),
                this.GetValidInputs(),
                this.importSolutionZip);

            this.solutionImportServiceMock.VerifyAll();
        }

        /// <summary>
        /// Tests that a failed import sets the error message.
        /// </summary>
        [Fact]
        public void ImportSolution_ImportJobFailure_SetsError()
        {
            this.MockAccessTokenResult();

            var error = "Missing dependencies";
            var failedImportJobData = ImportJobData.ParseXml($"<solutionManifest><result result=\"failure\" errortext=\"{error}\"></result></solutionManifest>");
            this.solutionImportServiceMock.SetReturnsDefault(Task.FromResult(failedImportJobData));

            var outputs = this.FakedContext.ExecuteCodeActivity(
                this.GetConfiguredWorkflowContext(),
                this.GetValidInputs(),
                this.importSolutionZip);

            Assert.Equal(error, outputs[nameof(ImportSolutionZip.Error)]);
        }

        /// <summary>
        /// Tests that a failed import sets IsSuccessful to false.
        /// </summary>
        [Fact]
        public void ImportSolution_ImportJobFailure_SetsIsSuccessfulToFalse()
        {
            this.MockAccessTokenResult();

            var failedImportJobData = ImportJobData.ParseXml($"<solutionManifest><result result=\"failure\" errortext=\"\"></result></solutionManifest>");
            this.solutionImportServiceMock.SetReturnsDefault(Task.FromResult(failedImportJobData));

            var outputs = this.FakedContext.ExecuteCodeActivity(
                this.GetConfiguredWorkflowContext(),
                this.GetValidInputs(),
                this.importSolutionZip);

            Assert.Equal(false, outputs[nameof(ImportSolutionZip.IsSuccessful)]);
        }

        /// <summary>
        /// Tests that a successful import sets IsSuccessful to true.
        /// </summary>
        [Fact]
        public void ImportSolution_ImportJobSuccess_SetsIsSuccessfulToTrue()
        {
            this.MockAccessTokenResult();

            var failedImportJobData = ImportJobData.ParseXml($"<solutionManifest><result result=\"success\" errortext=\"\"></result></solutionManifest>");
            this.solutionImportServiceMock.SetReturnsDefault(Task.FromResult(failedImportJobData));

            var outputs = this.FakedContext.ExecuteCodeActivity(
                this.GetConfiguredWorkflowContext(),
                this.GetValidInputs(),
                this.importSolutionZip);

            Assert.Equal(true, outputs[nameof(ImportSolutionZip.IsSuccessful)]);
        }

        private void MockAccessTokenResult()
        {
            this.oAuthTokenRepositoryMock.SetReturnsDefault(Task.FromResult(new OAuthToken { AccessToken = "ACCESS TOKEN" }));
        }

        private Dictionary<string, object> GetValidInputs()
        {
            return new Dictionary<string, object>
                {
                    { nameof(ImportSolutionZip.SolutionZip), Convert.ToBase64String(Encoding.UTF8.GetBytes("SOLUTION ZIP")) },
                    { nameof(ImportSolutionZip.TargetInstanceUrl), "https://targetinstance.crm11.dynamics.com" },
                };
        }

        private XrmFakedWorkflowContext GetConfiguredWorkflowContext()
        {
            var context = this.FakedContext.GetDefaultWorkflowContext();
            var injectedConfig =
                @"{
                    ""ClientId"": ""F5BEF3B4-2299-4156-A866-1C6A26432570"", 
                    ""TenantId"": ""AE099508-7DD7-4293-B96E-E0CB800784EA"", 
                    ""Username"": ""max@devhubdevelop.onmicrosoft.com"", 
                    ""Password"": ""Password123""
                }";
            context.SharedVariables.Add(
                InjectSecureConfig.SharedVariablesKeySecureConfig,
                injectedConfig);

            return context;
        }
    }
}
