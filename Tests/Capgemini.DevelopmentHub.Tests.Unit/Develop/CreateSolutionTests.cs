namespace Capgemini.DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Collections.Generic;
    using Capgemini.DevelopmentHub.Develop.BusinessLogic;
    using Capgemini.DevelopmentHub.Develop.CodeActivities;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Microsoft.Xrm.Sdk;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="CreateSolution"/> workflow activity.
    /// </summary>
    [Trait("Solution", "cap_DevelopmentHub_Develop")]
    public class CreateSolutionTests : FakedContextTest
    {
        private readonly Mock<ISolutionService> solutionServiceMock;
        private readonly CreateSolution codeActivity;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSolutionTests"/> class.
        /// </summary>
        public CreateSolutionTests()
        {
            this.solutionServiceMock = new Mock<ISolutionService>();
            this.codeActivity = new CreateSolution(this.solutionServiceMock.Object);
        }

        /// <summary>
        /// Tests that invoking the workflow activity with no unique name throws an exception.
        /// </summary>
        [Fact]
        public void CreateSolution_NoUniqueName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => this.FakedContext.ExecuteCodeActivity(
                new Dictionary<string, object>
                {
                    { nameof(CreateSolution.SolutionDescription), "Description" },
                    { nameof(CreateSolution.SolutionDisplayName), "Display Name" },
                    { nameof(CreateSolution.SolutionUniqueName), string.Empty },
                }, this.codeActivity));
        }

        /// <summary>
        /// Tests that invoking the workflow activity with no display name throws an exception.
        /// </summary>
        [Fact]
        public void CreateSolution_NoDisplayName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => this.FakedContext.ExecuteCodeActivity(
                new Dictionary<string, object>
                {
                    { nameof(CreateSolution.SolutionDescription), "Description" },
                    { nameof(CreateSolution.SolutionDisplayName), string.Empty },
                    { nameof(CreateSolution.SolutionUniqueName), "Unique Name" },
                }, this.codeActivity));
        }

        /// <summary>
        /// Tests that invoking the workflow activity with no description does not throw an exception.
        /// </summary>
        [Fact]
        public void CreateSolution_NoDescription_DoesNotThrow()
        {
            this.FakedContext.ExecuteCodeActivity(
                new Dictionary<string, object>
                {
                    { nameof(CreateSolution.SolutionDescription), string.Empty },
                    { nameof(CreateSolution.SolutionDisplayName), "Display Name" },
                    { nameof(CreateSolution.SolutionUniqueName), "Unique Name" },
                }, this.codeActivity);
        }

        /// <summary>
        /// Tests that invoking the workflow activity with valid arguments assigns the out argument as a reference to the created solution.
        /// </summary>
        [Fact]
        public void CreateSolution_ValidArguments_SetsOutArgumentWithSolutionReference()
        {
            var expectedReference = new EntityReference(Solution.EntityLogicalName, Guid.NewGuid());
            this.solutionServiceMock.SetReturnsDefault(expectedReference);

            var outputs = this.FakedContext.ExecuteCodeActivity(
                new Dictionary<string, object>
                {
                    { nameof(CreateSolution.SolutionDescription), string.Empty },
                    { nameof(CreateSolution.SolutionDisplayName), "Display Name" },
                    { nameof(CreateSolution.SolutionUniqueName), "Unique Name" },
                }, this.codeActivity);

            Assert.Equal(expectedReference, outputs[nameof(CreateSolution.CreatedSolution)]);
        }

        /// <summary>
        /// Tests that a unique name without title case and with whitespace is converted to title case without whitespace.
        /// </summary>
        [Fact]
        public void CreateSolution_UniqueNameNotTitleCase_ConvertsUniqueNameToTitleCase()
        {
            var displayName = "Display Name";
            var uniqueName = "Cap_Unique name";
            this.solutionServiceMock.SetReturnsDefault(new EntityReference());

            var outputs = this.FakedContext.ExecuteCodeActivity(
                new Dictionary<string, object>
                {
                    { nameof(CreateSolution.SolutionDescription), string.Empty },
                    { nameof(CreateSolution.SolutionDisplayName), displayName },
                    { nameof(CreateSolution.SolutionUniqueName), uniqueName },
                }, this.codeActivity);

            this.solutionServiceMock
                .Verify((solutionService) => solutionService.Create("cap_UniqueName", displayName, string.Empty));
        }
    }
}
