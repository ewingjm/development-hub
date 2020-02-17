namespace DevelopmentHub.Tests.Integration.Develop
{
    using System;
    using System.ServiceModel;
    using DevelopmentHub.Develop.Model;
    using DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk;
    using Xunit;

    /// <summary>
    /// Tests for the solution merge entity.
    /// </summary>
    [Trait("Solution", "devhub_DevelopmentHub_Develop")]
    public class SolutionMergeTests : IntegrationTest
    {
        private readonly ICrmRepository<devhub_issue> issueRepo;
        private readonly ICrmRepository<devhub_solutionmerge> solutionMergeRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionMergeTests"/> class.
        /// </summary>
        public SolutionMergeTests()
            : base(new Uri("https://dhubdevelop.crm11.dynamics.com"), "max@dhubdevelop.onmicrosoft.com")
        {
            this.issueRepo = this.RepositoryFactory.GetRepository<DevelopContext, devhub_issue>();
            this.solutionMergeRepo = this.RepositoryFactory.GetRepository<DevelopContext, devhub_solutionmerge>();
        }

        /// <summary>
        /// Tests that the issue status is updated to 'Developed' when a solution merge is created.
        /// </summary>
        [Fact]
        public void Create_NoOtherSolutionMergesForIssue_SetsIssueToDeveloped()
        {
            var issue = new devhub_issue
            {
                devhub_Description = "Creating a solution merge sets issue to 'Developed'.",
                devhub_name = "Set issue to developed on create of solution merge",
                devhub_Type = devhub_issue_devhub_type.Feature,
                devhub_DevelopmentSolution = "devhub_SetIssueToDevelopedOnCreateOfSolutionMerge",
                statuscode = devhub_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);

            this.CreateTestRecord(new devhub_solutionmerge
            {
                devhub_Issue = issueReference,
            });

            var updatedIssue = this.issueRepo.Retrieve(issueReference.Id, new string[] { "statuscode" });

            Assert.Equal(devhub_issue_statuscode.Developed, updatedIssue.statuscode);
        }

        /// <summary>
        /// Tests that the solution merge status is 'Awaiting Review' when created.
        /// </summary>
        [Fact]
        public void Create_NoOtherSolutionMergesFoIssue_InitialStatusIsAwaitingReview()
        {
            var issue = new devhub_issue
            {
                devhub_Description = "Solution merge has an initial status of Awaiting Review",
                devhub_name = "Initial solution merge status is Awaiting Review",
                devhub_Type = devhub_issue_devhub_type.Feature,
                devhub_DevelopmentSolution = "devhub_InitialSolutionMergeStatusIsAwaitingReview",
                statuscode = devhub_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);

            var solutionMergeReference = this.CreateTestRecord(new devhub_solutionmerge
            {
                devhub_Issue = issueReference,
            });

            var updatedSolutionMerge = this.solutionMergeRepo.Retrieve(
                solutionMergeReference.Id, new string[] { "statuscode" });

            Assert.Equal(devhub_solutionmerge_statuscode.AwaitingReview, updatedSolutionMerge.statuscode);
        }

        /// <summary>
        /// Tests that an error is thrown on create of solution merge if another active merge exists for the issue.
        /// </summary>
        [Fact]
        public void Create_OtherActiveSolutionMergesForIssue_Throws()
        {
            var issue = new devhub_issue
            {
                devhub_Description = "Prevent users from creating two active solution merges for an issue.",
                devhub_name = "Only one active merge allowed for issue",
                devhub_Type = devhub_issue_devhub_type.Feature,
                devhub_DevelopmentSolution = "devhub_OnlyOneActiveMergeAllowedForIssue",
                statuscode = devhub_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);

            this.CreateTestRecord(new devhub_solutionmerge
            {
                devhub_Issue = issueReference,
            });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
            {
                this.solutionMergeRepo.Create(new devhub_solutionmerge
                {
                    devhub_Issue = issueReference,
                });
            });
        }

        /// <summary>
        /// Tests that an error is thrown on create of solution merge if the issue does not have a development solution.
        /// </summary>
        [Fact]
        public void Create_IssueDoesNotHaveADevelopmentSolution_Throws()
        {
            var issue = new devhub_issue
            {
                devhub_Description = "Error when a solution merge for an issue with no development solution.",
                devhub_name = "Prevent solution merge creation for issues with no solution",
                devhub_Type = devhub_issue_devhub_type.Feature,
            };
            var issueReference = this.CreateTestRecord(issue);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
            {
                this.solutionMergeRepo.Create(new devhub_solutionmerge
                {
                    devhub_Issue = issueReference,
                });
            });
        }

        /// <summary>
        /// Tests that the associated issue is set back to 'In Progress' if the solution merge is cancelled.
        /// </summary>
        [Fact]
        public void Cancel_HasAssociatedIssue_SetsIssueStatusToInProgress()
        {
            var issue = new devhub_issue
            {
                devhub_Description = "Cancelling a solution merge sets issue to 'In Progress'.",
                devhub_name = "Set issue to In Progress on cancel of solution merge",
                devhub_Type = devhub_issue_devhub_type.Feature,
                devhub_DevelopmentSolution = "devhub_SetIssueToInProgressOnCancelOfSolutionMerge",
                statuscode = devhub_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);
            var solutionMergeReference = this.CreateTestRecord(new devhub_solutionmerge
            {
                devhub_Issue = issueReference,
            });

            this.solutionMergeRepo.Update(new devhub_solutionmerge
            {
                devhub_solutionmergeId = solutionMergeReference.Id,
                statecode = devhub_solutionmergeState.Inactive,
                statuscode = devhub_solutionmerge_statuscode.Cancelled,
            });

            var updatedIssue = this.issueRepo.Retrieve(issueReference.Id, new string[] { "statuscode" });

            Assert.Equal(devhub_issue_statuscode.InProgress, updatedIssue.statuscode);
        }

        /// <summary>
        /// Tests that the associated issue is set back to 'In Progress' if the solution merge is cancelled.
        /// </summary>
        [Fact]
        public void Reject_WhenAwaitingReview_SetsIssueStatusToInProgress()
        {
            var issue = new devhub_issue
            {
                devhub_Description = "Rejecting a solution merge sets issue to 'In Progress'.",
                devhub_name = "Set issue to In Progress on reject of solution merge",
                devhub_Type = devhub_issue_devhub_type.Feature,
                devhub_DevelopmentSolution = "devhub_SetIssueToInProgressOnRejectOfSolutionMerge",
                statuscode = devhub_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);
            var solutionMergeReference = this.CreateTestRecord(new devhub_solutionmerge
            {
                devhub_Issue = issueReference,
            });
            this.OrgService.Execute(new devhub_RejectRequest { Target = solutionMergeReference });

            var updatedIssue = this.issueRepo.Retrieve(issueReference.Id, new string[] { "statuscode" });

            Assert.Equal(devhub_issue_statuscode.InProgress, updatedIssue.statuscode);
        }

        /// <summary>
        /// Tests that the associated issue is set back to 'In Progress' if the solution merge is cancelled.
        /// </summary>
        [Fact]
        public void Reject_WhenAwaitingReview_SetsReviewedByAndOnFields()
        {
            var issue = new devhub_issue
            {
                devhub_Description = "Rejecting a solution merge sets reviwed by an reviwed on.",
                devhub_name = "Set reviewed by and on when solution merge is rejected",
                devhub_Type = devhub_issue_devhub_type.Feature,
                devhub_DevelopmentSolution = "devhub_SetReviewFieldsOnRejectOfSolutionMerge",
                statuscode = devhub_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);
            var solutionMergeReference = this.CreateTestRecord(new devhub_solutionmerge
            {
                devhub_Issue = issueReference,
            });
            this.OrgService.Execute(new devhub_RejectRequest { Target = solutionMergeReference });

            var updatedSolutionMerge = this.solutionMergeRepo.Retrieve(solutionMergeReference.Id, new string[] { "devhub_approvedby", "devhub_approvedon" });

            Assert.NotNull(updatedSolutionMerge.devhub_ApprovedBy);
            Assert.NotNull(updatedSolutionMerge.devhub_ApprovedOn);
        }
    }
}
