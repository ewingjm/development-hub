namespace DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DevelopmentHub.Develop.BusinessLogic;
    using DevelopmentHub.Develop.CodeActivities;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="GetSolutionZip"/> custom workflow activity.
    /// </summary>
    public class GetSolutionZipTests : FakedContextTest
    {
        private readonly GetSolutionZip getSolutionZip;
        private readonly Mock<ISolutionService> solutionServiceMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSolutionZipTests"/> class.
        /// </summary>
        public GetSolutionZipTests()
        {
            this.solutionServiceMock = new Mock<ISolutionService>();
            this.getSolutionZip = new GetSolutionZip(this.solutionServiceMock.Object);
        }

        /// <summary>
        /// Tests than an exception is thrown is the solution unique name is empty.
        /// </summary>
        [Fact]
        public void GetSolutionZip_NoSolutionUniqueName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                this.FakedContext.ExecuteCodeActivity<GetSolutionZip>(new Dictionary<string, object>
                {
                    { nameof(GetSolutionZip.SolutionUniqueName), string.Empty },
                });
            });
        }

        /// <summary>
        /// Tests that the output is a Base64 encoded string of the <see cref="SolutionService.GetSolutionZip(string, bool)"/> response.
        /// </summary>
        [Fact]
        public void GetSolutionZip_ReturnsSolutionServiceResponseAsBase64()
        {
            var solutionZip = Encoding.UTF8.GetBytes("test response");
            var expectedResponse = Convert.ToBase64String(solutionZip);
            var solutionUniqueName = "devhub_Solution";
            var managed = false;
            var inputs = new Dictionary<string, object>
            {
                { nameof(GetSolutionZip.SolutionUniqueName), solutionUniqueName },
                { nameof(GetSolutionZip.Managed), managed },
            };
            this.solutionServiceMock
                .Setup(solutionService => solutionService.GetSolutionZip(solutionUniqueName, managed))
                .Returns(solutionZip);

            var outputs = this.FakedContext.ExecuteCodeActivity(inputs, this.getSolutionZip);

            Assert.Equal(expectedResponse, outputs[nameof(GetSolutionZip.SolutionZip)]);
        }
    }
}
