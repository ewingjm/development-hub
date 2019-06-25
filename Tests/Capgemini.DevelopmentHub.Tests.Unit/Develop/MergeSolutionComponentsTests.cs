namespace Capgemini.DevelopmentHub.Tests.Unit.Develop
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.Develop.BusinessLogic;
    using Capgemini.DevelopmentHub.Develop.CodeActivities;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the <see cref="MergeSolutionComponents"/> custom workflow activity.
    /// </summary>
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
                    { nameof(MergeSolutionComponents.TargetSolutionUniqueName), "cap_Target" },
                    { nameof(MergeSolutionComponents.DeleteSourceSolutionAfterMerge), true },
                    { nameof(IntegratedWorkflowActivity.TargetInstanceUrl), "https://organization.crm.dynamics.com" },
                });
            });
        }

        [Fact]
        public void MergeSolutionComponents_NullTargetSolution_Throws()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();

            Assert.Throws<ArgumentNullException>(() =>
            {
                this.WorkflowInvoker.Invoke(new Dictionary<string, object>
                {
                    { nameof(MergeSolutionComponents.SourceSolutionUniqueName), "cap_Source" },
                    { nameof(MergeSolutionComponents.TargetSolutionUniqueName), null },
                    { nameof(MergeSolutionComponents.DeleteSourceSolutionAfterMerge), true },
                    { nameof(IntegratedWorkflowActivity.TargetInstanceUrl), "https://organization.crm.dynamics.com" },
                });
            });
        }

        [Fact]
        public void MergeSolutionComponents_MergeThrows_SetsIsSuccessfulToFalse()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            this.oDataSolutionServiceMock
                .Setup(service => service.MergeSolutionComponents(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new AggregateException(new WebException("Some merge failure")));

            var outputs = this.WorkflowInvoker.Invoke(new Dictionary<string, object>
                {
                    { nameof(MergeSolutionComponents.SourceSolutionUniqueName), "cap_Source" },
                    { nameof(MergeSolutionComponents.TargetSolutionUniqueName), "cap_Target" },
                    { nameof(MergeSolutionComponents.DeleteSourceSolutionAfterMerge), true },
                    { nameof(IntegratedWorkflowActivity.TargetInstanceUrl), "https://organization.crm.dynamics.com" },
                });

            Assert.Equal(false, outputs[nameof(MergeSolutionComponents.IsSuccessful)]);
        }

        [Fact]
        public void MergeSolutionComponents_MergeThrows_SetsError()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();
            var error = "Some merge failure";
            this.oDataSolutionServiceMock
                .Setup(service => service.MergeSolutionComponents(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new AggregateException(new WebException(error)));

            var outputs = this.WorkflowInvoker.Invoke(new Dictionary<string, object>
                {
                    { nameof(MergeSolutionComponents.SourceSolutionUniqueName), "cap_Source" },
                    { nameof(MergeSolutionComponents.TargetSolutionUniqueName), "cap_Target" },
                    { nameof(MergeSolutionComponents.DeleteSourceSolutionAfterMerge), true },
                    { nameof(IntegratedWorkflowActivity.TargetInstanceUrl), "https://organization.crm.dynamics.com" },
                });

            Assert.Equal(error, outputs[nameof(MergeSolutionComponents.Error)]);
        }

        [Fact]
        public void MergeSolutionComponents_MergeSuccessful_SetsIsSuccessfulToTrue()
        {
            this.MockPasswordGrantConfiguredContext();
            this.MockAccessTokenResult();

            var outputs = this.WorkflowInvoker.Invoke(new Dictionary<string, object>
                {
                    { nameof(MergeSolutionComponents.SourceSolutionUniqueName), "cap_Source" },
                    { nameof(MergeSolutionComponents.TargetSolutionUniqueName), "cap_Target" },
                    { nameof(MergeSolutionComponents.DeleteSourceSolutionAfterMerge), true },
                    { nameof(IntegratedWorkflowActivity.TargetInstanceUrl), "https://organization.crm.dynamics.com" },
                });

            Assert.Equal(true, outputs[nameof(MergeSolutionComponents.IsSuccessful)]);
        }
    }
}
