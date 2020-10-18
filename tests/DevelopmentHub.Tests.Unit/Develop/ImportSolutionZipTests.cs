namespace DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using DevelopmentHub.Develop.BusinessLogic;
    using DevelopmentHub.Develop.CodeActivities;
    using DevelopmentHub.Develop.Model;
    using DevelopmentHub.Model;
    using DevelopmentHub.Model.Requests;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="ImportSolutionZip"/> custom workflow activity.
    /// </summary>
    [Trait("Solution", "devhub_DevelopmentHub_Develop")]
    public class ImportSolutionZipTests : IntegratedWorkflowActivityTests<ImportSolutionZip>
    {
        private readonly Mock<IODataSolutionService> oDataSolutionServiceMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportSolutionZipTests"/> class.
        /// </summary>
        public ImportSolutionZipTests()
        {
            this.oDataSolutionServiceMock = new Mock<IODataSolutionService>();

            this.WorkflowInvoker.Extensions.Add(this.oDataSolutionServiceMock.Object);
        }

        /// <summary>
        /// Tests that executing <see cref="ImportSolutionZip"/> with no solution zip throws an exception.
        /// </summary>
        [Fact]
        public void ImportSolutionZip_NoSolutionZip_Throws()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();

            Assert.Throws<ArgumentNullException>(() =>
            {
                this.WorkflowInvoker.Invoke(new Dictionary<string, object>
                {
                    { nameof(ImportSolutionZip.SolutionZip), null },
                    { nameof(ImportSolutionZip.TargetInstanceUrl), "https://targetinstance.crm11.dynamics.com" },
                });
            });
        }

        /// <summary>
        /// Tests that executing <see cref="ImportSolutionZip"/> with no injected configuration then the error is set.
        /// </summary>
        [Fact]
        public void ImportSolutionZip_NoInjectedConfig_SetsError()
        {
            var outputs = this.WorkflowInvoker.Invoke(this.GetValidInputs());

            Assert.NotNull(outputs[nameof(ImportSolutionZip.Error)]);
        }

        /// <summary>
        /// Tests that the target instance URL is used when making the <see cref="OAuthClientCredentialsGrantRequest"/>.
        /// </summary>
        [Fact]
        public void ImportSolutionZip_PasswordGrantRequest_ResourceSetToTargetInstanceUrl()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            var expectedResource = "https://targetinstance.crm11.dynamics.com";
            this.OAuthTokenRepositoryMock
                .Setup(o => o.GetAccessToken(It.Is<OAuthClientCredentialsGrantRequest>(req => req.Resource == new Uri(expectedResource))))
                .ReturnsAsync(new OAuthToken { AccessToken = "ACCESS TOKEN" })
                .Verifiable();
            this.oDataSolutionServiceMock.SetReturnsDefault(Task.FromResult(new ImportJobData()));

            this.WorkflowInvoker.Invoke(
                new Dictionary<string, object>
                {
                    { nameof(ImportSolutionZip.SolutionZip), Convert.ToBase64String(Encoding.UTF8.GetBytes("SOLUTION ZIP")) },
                    { nameof(ImportSolutionZip.TargetInstanceUrl), expectedResource },
                });

            this.OAuthTokenRepositoryMock.VerifyAll();
        }

        /// <summary>
        /// Tests that the inputted solution zip file is passsed to the solution import service.
        /// </summary>
        [Fact]
        public void ImportSolution_SolutionZip_Imported()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            var expectedSolutionZip = Convert.ToBase64String(Encoding.UTF8.GetBytes("SOLUTION ZIP"));
            this.oDataSolutionServiceMock
                .Setup(s => s.ImportSolutionZipAsync(It.Is<byte[]>(solution => Convert.ToBase64String(solution) == expectedSolutionZip)))
                .Returns(Task.FromResult(new ImportJobData()))
                .Verifiable();

            this.WorkflowInvoker.Invoke(this.GetValidInputs());

            this.oDataSolutionServiceMock.VerifyAll();
        }

        /// <summary>
        /// Tests that a failed import sets the error message.
        /// </summary>
        [Fact]
        public void ImportSolution_ImportJobFailure_SetsError()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            var error = "Missing dependencies";
            var failedImportJobData = ImportJobData.ParseXml($"<solutionManifest><result result=\"failure\" errortext=\"{error}\"></result></solutionManifest>");
            this.oDataSolutionServiceMock.SetReturnsDefault(Task.FromResult(failedImportJobData));

            var outputs = this.WorkflowInvoker.Invoke(this.GetValidInputs());

            Assert.Equal(error, outputs[nameof(ImportSolutionZip.Error)]);
        }

        /// <summary>
        /// Tests that a failed import sets IsSuccessful to false.
        /// </summary>
        [Fact]
        public void ImportSolution_ImportJobFailure_SetsIsSuccessfulToFalse()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            var failedImportJobData = ImportJobData.ParseXml($"<solutionManifest><result result=\"failure\" errortext=\"\"></result></solutionManifest>");
            this.oDataSolutionServiceMock.SetReturnsDefault(Task.FromResult(failedImportJobData));

            var outputs = this.WorkflowInvoker.Invoke(this.GetValidInputs());

            Assert.Equal(false, outputs[nameof(ImportSolutionZip.IsSuccessful)]);
        }

        /// <summary>
        /// Tests that a successful import sets IsSuccessful to true.
        /// </summary>
        [Fact]
        public void ImportSolution_ImportJobSuccess_SetsIsSuccessfulToTrue()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            var failedImportJobData = ImportJobData.ParseXml($"<solutionManifest><result result=\"success\" errortext=\"\"></result></solutionManifest>");
            this.oDataSolutionServiceMock.SetReturnsDefault(Task.FromResult(failedImportJobData));

            var outputs = this.WorkflowInvoker.Invoke(this.GetValidInputs());

            Assert.Equal(true, outputs[nameof(ImportSolutionZip.IsSuccessful)]);
        }

        /// <summary>
        /// Tests that an exception thrown during the import will set <see cref="ImportSolutionZip.IsSuccessful"/> to false.
        /// </summary>
        [Fact]
        public void ImportSolution_ImportThrows_SetsIsSuccessfulToFalse()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            this.oDataSolutionServiceMock
                .Setup(s => s.ImportSolutionZipAsync(It.IsAny<byte[]>()))
                .Throws(new AggregateException(new WebException("Missing dependencies")));

            var outputs = this.WorkflowInvoker.Invoke(this.GetValidInputs());

            Assert.Equal(false, outputs[nameof(ImportSolutionZip.IsSuccessful)]);
        }

        /// <summary>
        /// Tests thatan exception thrown during the import will set <see cref="ImportSolutionZip.Error"/>.
        /// </summary>
        [Fact]
        public void ImportSolution_ImportThrows_SetsError()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            var error = "Missing dependencies";
            this.oDataSolutionServiceMock
                .Setup(s => s.ImportSolutionZipAsync(It.IsAny<byte[]>()))
                .Throws(new AggregateException(new WebException(error)));

            var outputs = this.WorkflowInvoker.Invoke(this.GetValidInputs());

            Assert.Equal(error, outputs[nameof(ImportSolutionZip.Error)]);
        }

        private Dictionary<string, object> GetValidInputs()
        {
            return new Dictionary<string, object>
                {
                    { nameof(ImportSolutionZip.SolutionZip), Convert.ToBase64String(Encoding.UTF8.GetBytes("SOLUTION ZIP")) },
                    { nameof(ImportSolutionZip.TargetInstanceUrl), "https://targetinstance.crm11.dynamics.com" },
                };
        }
    }
}
