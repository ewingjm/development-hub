namespace DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using DevelopmentHub.BusinessLogic;
    using DevelopmentHub.Develop.BusinessLogic;
    using DevelopmentHub.Develop.CodeActivities;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="MergeSolutionComponents"/> custom workflow activity.
    /// </summary>
    [Trait("Solution", "devhub_DevelopmentHub_Develop")]
    public class MergeSolutionComponentsTests : IntegratedWorkflowActivityTests<MergeSolutionComponents>
    {
        private readonly Mock<IODataSolutionService> oDataSolutionServiceMock;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeSolutionComponentsTests"/> class.
        /// </summary>
        public MergeSolutionComponentsTests()
        {
            this.oDataSolutionServiceMock = new Mock<IODataSolutionService>();

            this.WorkflowInvoker.Extensions.Add(this.oDataSolutionServiceMock.Object);
        }

        /// <summary>
        /// Tests that calling <see cref="MergeSolutionComponents"/> with a null souce solution throws an exception.
        /// </summary>
        [Fact]
        public void MergeSolutionComponents_NullSourceSolution_Throws()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();

            Assert.Throws<ArgumentNullException>(() =>
            {
                this.WorkflowInvoker.Invoke(new Dictionary<string, object>
                {
                    { nameof(MergeSolutionComponents.SourceSolutionUniqueName), null },
                    { nameof(MergeSolutionComponents.TargetSolutionUniqueName), "devhub_Target" },
                    { nameof(MergeSolutionComponents.DeleteSourceSolutionAfterMerge), true },
                    { nameof(IntegratedWorkflowActivity.TargetInstanceUrl), "https://organization.crm.dynamics.com" },
                });
            });
        }

        /// <summary>
        /// Tests that calling <see cref="MergeSolutionComponents"/> with a null target solution throws an exception.
        /// </summary>
        [Fact]
        public void MergeSolutionComponents_NullTargetSolution_Throws()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();

            Assert.Throws<ArgumentNullException>(() =>
            {
                this.WorkflowInvoker.Invoke(new Dictionary<string, object>
                {
                    { nameof(MergeSolutionComponents.SourceSolutionUniqueName), "devhub_Source" },
                    { nameof(MergeSolutionComponents.TargetSolutionUniqueName), null },
                    { nameof(MergeSolutionComponents.DeleteSourceSolutionAfterMerge), true },
                    { nameof(IntegratedWorkflowActivity.TargetInstanceUrl), "https://organization.crm.dynamics.com" },
                });
            });
        }

        /// <summary>
        /// Tests that a failure during the merge sets the "IsSuccessful" output argument to false.
        /// </summary>
        [Fact]
        public void MergeSolutionComponents_MergeThrows_SetsIsSuccessfulToFalse()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            this.oDataSolutionServiceMock
                .Setup(service => service.MergeSolutionComponentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new AggregateException(new WebException("Some merge failure")));

            var outputs = this.WorkflowInvoker.Invoke(new Dictionary<string, object>
                {
                    { nameof(MergeSolutionComponents.SourceSolutionUniqueName), "devhub_Source" },
                    { nameof(MergeSolutionComponents.TargetSolutionUniqueName), "devhub_Target" },
                    { nameof(MergeSolutionComponents.DeleteSourceSolutionAfterMerge), true },
                    { nameof(IntegratedWorkflowActivity.TargetInstanceUrl), "https://organization.crm.dynamics.com" },
                });

            Assert.Equal(false, outputs[nameof(MergeSolutionComponents.IsSuccessful)]);
        }

        /// <summary>
        /// Tests that a failure during the merge sets the "Error" output argument to the exception message.
        /// </summary>
        [Fact]
        public void MergeSolutionComponents_MergeThrows_SetsError()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            var error = "Some merge failure";
            this.oDataSolutionServiceMock
                .Setup(service => service.MergeSolutionComponentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new AggregateException(new WebException(error)));

            var outputs = this.WorkflowInvoker.Invoke(new Dictionary<string, object>
                {
                    { nameof(MergeSolutionComponents.SourceSolutionUniqueName), "devhub_Source" },
                    { nameof(MergeSolutionComponents.TargetSolutionUniqueName), "devhub_Target" },
                    { nameof(MergeSolutionComponents.DeleteSourceSolutionAfterMerge), true },
                    { nameof(IntegratedWorkflowActivity.TargetInstanceUrl), "https://organization.crm.dynamics.com" },
                });

            Assert.Equal(error, outputs[nameof(MergeSolutionComponents.Error)]);
        }

        /// <summary>
        /// Tests that a successful merge sets the "IsSuccessful" output parameter to true.
        /// </summary>
        [Fact]
        public void MergeSolutionComponents_MergeSuccessful_SetsIsSuccessfulToTrue()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();

            var outputs = this.WorkflowInvoker.Invoke(new Dictionary<string, object>
                {
                    { nameof(MergeSolutionComponents.SourceSolutionUniqueName), "devhub_Source" },
                    { nameof(MergeSolutionComponents.TargetSolutionUniqueName), "devhub_Target" },
                    { nameof(MergeSolutionComponents.DeleteSourceSolutionAfterMerge), true },
                    { nameof(IntegratedWorkflowActivity.TargetInstanceUrl), "https://organization.crm.dynamics.com" },
                });

            Assert.Equal(true, outputs[nameof(MergeSolutionComponents.IsSuccessful)]);
        }
    }
}
