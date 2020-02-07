namespace Capgemini.DevelopmentHub.Tests.Integration.Develop
{
    using System;
    using System.ServiceModel;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk;
    using Xunit;

    /// <summary>
    /// Tests for the solution merge entity.
    /// </summary>
    [Trait("Solution", "cap_DevelopmentHub_Develop")]
    public class SolutionMergeTests : IntegrationTest
    {
        private readonly ICrmRepository<cap_issue> issueRepo;
        private readonly ICrmRepository<cap_solutionmerge> solutionMergeRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionMergeTests"/> class.
        /// </summary>
        public SolutionMergeTests()
            : base(new Uri("https://dhubdevelop.crm11.dynamics.com"), "max@dhubdevelop.onmicrosoft.com")
        {
            this.issueRepo = this.RepositoryFactory.GetRepository<DevelopContext, cap_issue>();
            this.solutionMergeRepo = this.RepositoryFactory.GetRepository<DevelopContext, cap_solutionmerge>();
        }

        /// <summary>
        /// Tests that the issue status is updated to 'Developed' when a solution merge is created.
        /// </summary>
        [Fact]
        public void Create_NoOtherSolutionMergesForIssue_SetsIssueToDeveloped()
        {
            var issue = new cap_issue
            {
                cap_Description = "Creating a solution merge sets issue to 'Developed'.",
                cap_name = "Set issue to developed on create of solution merge",
                cap_Type = cap_issue_cap_type.Feature,
                cap_DevelopmentSolution = "cap_SetIssueToDevelopedOnCreateOfSolutionMerge",
                statuscode = cap_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);

            this.CreateTestRecord(new cap_solutionmerge
            {
                cap_Issue = issueReference,
            });

            var updatedIssue = this.issueRepo.Retrieve(issueReference.Id, new string[] { "statuscode" });

            Assert.Equal(cap_issue_statuscode.Developed, updatedIssue.statuscode);
        }

        /// <summary>
        /// Tests that the solution merge status is 'Awaiting Review' when created.
        /// </summary>
        [Fact]
        public void Create_NoOtherSolutionMergesFoIssue_InitialStatusIsAwaitingReview()
        {
            var issue = new cap_issue
            {
                cap_Description = "Solution merge has an initial status of Awaiting Review",
                cap_name = "Initial solution merge status is Awaiting Review",
                cap_Type = cap_issue_cap_type.Feature,
                cap_DevelopmentSolution = "cap_InitialSolutionMergeStatusIsAwaitingReview",
                statuscode = cap_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);

            var solutionMergeReference = this.CreateTestRecord(new cap_solutionmerge
            {
                cap_Issue = issueReference,
            });

            var updatedSolutionMerge = this.solutionMergeRepo.Retrieve(
                solutionMergeReference.Id, new string[] { "statuscode" });

            Assert.Equal(cap_solutionmerge_statuscode.AwaitingReview, updatedSolutionMerge.statuscode);
        }

        /// <summary>
        /// Tests that an error is thrown on create of solution merge if another active merge exists for the issue.
        /// </summary>
        [Fact]
        public void Create_OtherActiveSolutionMergesForIssue_Throws()
        {
            var issue = new cap_issue
            {
                cap_Description = "Prevent users from creating two active solution merges for an issue.",
                cap_name = "Only one active merge allowed for issue",
                cap_Type = cap_issue_cap_type.Feature,
                cap_DevelopmentSolution = "cap_OnlyOneActiveMergeAllowedForIssue",
                statuscode = cap_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);

            this.CreateTestRecord(new cap_solutionmerge
            {
                cap_Issue = issueReference,
            });

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
            {
                this.solutionMergeRepo.Create(new cap_solutionmerge
                {
                    cap_Issue = issueReference,
                });
            });
        }

        /// <summary>
        /// Tests that an error is thrown on create of solution merge if the issue does not have a development solution.
        /// </summary>
        [Fact]
        public void Create_IssueDoesNotHaveADevelopmentSolution_Throws()
        {
            var issue = new cap_issue
            {
                cap_Description = "Error when a solution merge for an issue with no development solution.",
                cap_name = "Prevent solution merge creation for issues with no solution",
                cap_Type = cap_issue_cap_type.Feature,
            };
            var issueReference = this.CreateTestRecord(issue);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
            {
                this.solutionMergeRepo.Create(new cap_solutionmerge
                {
                    cap_Issue = issueReference,
                });
            });
        }

        /// <summary>
        /// Tests that the associated issue is set back to 'In Progress' if the solution merge is cancelled.
        /// </summary>
        [Fact]
        public void Cancel_HasAssociatedIssue_SetsIssueStatusToInProgress()
        {
            var issue = new cap_issue
            {
                cap_Description = "Cancelling a solution merge sets issue to 'In Progress'.",
                cap_name = "Set issue to In Progress on cancel of solution merge",
                cap_Type = cap_issue_cap_type.Feature,
                cap_DevelopmentSolution = "cap_SetIssueToInProgressOnCancelOfSolutionMerge",
                statuscode = cap_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);
            var solutionMergeReference = this.CreateTestRecord(new cap_solutionmerge
            {
                cap_Issue = issueReference,
            });

            this.solutionMergeRepo.Update(new cap_solutionmerge
            {
                cap_solutionmergeId = solutionMergeReference.Id,
                statecode = cap_solutionmergeState.Inactive,
                statuscode = cap_solutionmerge_statuscode.Cancelled,
            });

            var updatedIssue = this.issueRepo.Retrieve(issueReference.Id, new string[] { "statuscode" });

            Assert.Equal(cap_issue_statuscode.InProgress, updatedIssue.statuscode);
        }

        /// <summary>
        /// Tests that the associated issue is set back to 'In Progress' if the solution merge is cancelled.
        /// </summary>
        [Fact]
        public void Reject_WhenAwaitingReview_SetsIssueStatusToInProgress()
        {
            var issue = new cap_issue
            {
                cap_Description = "Rejecting a solution merge sets issue to 'In Progress'.",
                cap_name = "Set issue to In Progress on reject of solution merge",
                cap_Type = cap_issue_cap_type.Feature,
                cap_DevelopmentSolution = "cap_SetIssueToInProgressOnRejectOfSolutionMerge",
                statuscode = cap_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);
            var solutionMergeReference = this.CreateTestRecord(new cap_solutionmerge
            {
                cap_Issue = issueReference,
            });
            this.OrgService.Execute(new cap_RejectRequest { Target = solutionMergeReference });

            var updatedIssue = this.issueRepo.Retrieve(issueReference.Id, new string[] { "statuscode" });

            Assert.Equal(cap_issue_statuscode.InProgress, updatedIssue.statuscode);
        }

        /// <summary>
        /// Tests that the associated issue is set back to 'In Progress' if the solution merge is cancelled.
        /// </summary>
        [Fact]
        public void Reject_WhenAwaitingReview_SetsReviewedByAndOnFields()
        {
            var issue = new cap_issue
            {
                cap_Description = "Rejecting a solution merge sets reviwed by an reviwed on.",
                cap_name = "Set reviewed by and on when solution merge is rejected",
                cap_Type = cap_issue_cap_type.Feature,
                cap_DevelopmentSolution = "cap_SetReviewFieldsOnRejectOfSolutionMerge",
                statuscode = cap_issue_statuscode.InProgress,
            };
            var issueReference = this.CreateTestRecord(issue);
            var solutionMergeReference = this.CreateTestRecord(new cap_solutionmerge
            {
                cap_Issue = issueReference,
            });
            this.OrgService.Execute(new cap_RejectRequest { Target = solutionMergeReference });

            var updatedSolutionMerge = this.solutionMergeRepo.Retrieve(solutionMergeReference.Id, new string[] { "cap_approvedby", "cap_approvedon" });

            Assert.NotNull(updatedSolutionMerge.cap_ApprovedBy);
            Assert.NotNull(updatedSolutionMerge.cap_ApprovedOn);
        }
    }
}
