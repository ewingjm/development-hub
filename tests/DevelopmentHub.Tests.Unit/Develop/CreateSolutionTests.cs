namespace DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Collections.Generic;
    using DevelopmentHub.Develop.BusinessLogic;
    using DevelopmentHub.Develop.CodeActivities;
    using DevelopmentHub.Develop.Model;
    using Microsoft.Xrm.Sdk;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="CreateSolution"/> workflow activity.
    /// </summary>
    [Trait("Solution", "devhub_DevelopmentHub_Develop")]
    public class CreateSolutionTests : WorkflowActivityTests<CreateSolution>
    {
        private readonly Mock<ISolutionService> solutionServiceMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSolutionTests"/> class.
        /// </summary>
        public CreateSolutionTests()
        {
            this.solutionServiceMock = new Mock<ISolutionService>();
            this.WorkflowInvoker.Extensions.Add(this.solutionServiceMock.Object);
        }

        /// <summary>
        /// Tests that invoking the workflow activity with no devhub_uniquename throws an exception.
        /// </summary>
        [Fact]
        public void CreateSolution_NoUniqueName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => this.WorkflowInvoker.Invoke(
                new Dictionary<string, object>
                {
                    { nameof(CreateSolution.SolutionDescription), "Description" },
                    { nameof(CreateSolution.SolutionDisplayName), "Display Name" },
                    { nameof(CreateSolution.SolutionUniqueName), string.Empty },
                }));
        }

        /// <summary>
        /// Tests that invoking the workflow activity with no display name throws an exception.
        /// </summary>
        [Fact]
        public void CreateSolution_NoDisplayName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => this.WorkflowInvoker.Invoke(
                new Dictionary<string, object>
                {
                    { nameof(CreateSolution.SolutionDescription), "Description" },
                    { nameof(CreateSolution.SolutionDisplayName), string.Empty },
                    { nameof(CreateSolution.SolutionUniqueName), "devhub_uniquename" },
                }));
        }

        /// <summary>
        /// Tests that invoking the workflow activity with no description does not throw an exception.
        /// </summary>
        [Fact]
        public void CreateSolution_NoDescription_DoesNotThrow()
        {
            this.WorkflowInvoker.Invoke(
                new Dictionary<string, object>
                {
                    { nameof(CreateSolution.SolutionDescription), string.Empty },
                    { nameof(CreateSolution.SolutionDisplayName), "Display Name" },
                    { nameof(CreateSolution.SolutionUniqueName), "devhub_uniquename" },
                });
        }

        /// <summary>
        /// Tests that invoking the workflow activity with valid arguments assigns the out argument as a reference to the created solution.
        /// </summary>
        [Fact]
        public void CreateSolution_ValidArguments_SetsOutArgumentWithSolutionReference()
        {
            var expectedReference = new EntityReference(Solution.EntityLogicalName, Guid.NewGuid());
            this.solutionServiceMock.SetReturnsDefault(expectedReference);

            var outputs = this.WorkflowInvoker.Invoke(
                new Dictionary<string, object>
                {
                    { nameof(CreateSolution.SolutionDescription), string.Empty },
                    { nameof(CreateSolution.SolutionDisplayName), "Display Name" },
                    { nameof(CreateSolution.SolutionUniqueName), "devhub_uniquename" },
                });

            Assert.Equal(expectedReference, outputs[nameof(CreateSolution.CreatedSolution)]);
        }

        /// <summary>
        /// Tests that a devhub_uniquename without title case and with whitespace is converted to title case without whitespace.
        /// </summary>
        [Fact]
        public void CreateSolution_UniqueNameNotTitleCase_ConvertsUniqueNameToTitleCase()
        {
            var displayName = "Display Name";
            var uniqueName = "Devhub_Unique name";
            this.solutionServiceMock.SetReturnsDefault(new EntityReference());

            var outputs = this.WorkflowInvoker.Invoke(
                new Dictionary<string, object>
                {
                    { nameof(CreateSolution.SolutionDescription), string.Empty },
                    { nameof(CreateSolution.SolutionDisplayName), displayName },
                    { nameof(CreateSolution.SolutionUniqueName), uniqueName },
                });

            this.solutionServiceMock
                .Verify((solutionService) => solutionService.Create("devhub_UniqueName", displayName, string.Empty));
        }
    }
}
