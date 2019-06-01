namespace Capgemini.DevelopmentHub.Tests.Integration.Develop
{
    using System;
    using System.ServiceModel;
    using Capgemini.DevelopmentHub.Develop.Model;
    using Capgemini.DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk;
    using Xunit;

    /// <summary>
    /// Tests for the Issue entity.
    /// </summary>
    [Trait("Solution", "cap_DevelopmentHub_Develop")]
    public class IssueTests : IntegrationTest
    {
        private readonly ICrmRepository<cap_issue> issueRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueTests"/> class.
        /// </summary>
        public IssueTests()
            : base(new Uri("https://devhubdevelop.crm11.dynamics.com"), "max@devhubdevelop.onmicrosoft.com")
        {
            this.issueRepo = this.RepositoryFactory.GetRepository<DevelopContext, cap_issue>();
        }

        /// <summary>
        /// Tests that StartDeveloping returns an error if called on an issue that is not in a 'To Do' status.
        /// </summary>
        [Fact]
        public void StartDeveloping_IssueNotToDo_ReturnsError()
        {
            var issue = new cap_issue();
            var issueReference = this.CreateTestRecord(issue);
            issue.cap_issueId = issueReference.Id;
            issue.statecode = cap_issueState.Inactive;
            issue.statuscode = cap_issue_statuscode.Cancelled;
            this.issueRepo.Update(issue);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
            {
                var response = (cap_StartDevelopingResponse)this.OrgService.Execute(
                    new cap_StartDevelopingRequest { Target = issueReference });
            });
        }

        /// <summary>
        /// Tests that StartDeveloping returns a reference to a created solution if called on an issue that is in a 'To Do' status.
        /// </summary>
        [Fact]
        public void StartDeveloping_IssueToDo_CreatesMatchingSolution()
        {
            var issue = new cap_issue
            {
                cap_Description = "Allow users to start developing a feature.",
                cap_name = "Start developing a feature",
                cap_Type = cap_issue_cap_type.Feature,
            };
            var issueReference = this.CreateTestRecord(issue);

            var response = (cap_StartDevelopingResponse)this.OrgService.Execute(
                new cap_StartDevelopingRequest { Target = issueReference });
            this.CreatedEntities.Add(response.Solution);

            Assert.NotNull(response.Solution);
        }

        /// <summary>
        /// Tests that StartDeveloping sets the development solution unique name on the issue.
        /// </summary>
        [Fact]
        public void StartDeveloping_IssueToDo_UpdatesDevelopmentSolutionOnIssue()
        {
            var expectedDevelopmentSolution = "cap_StartDevelopingABug";
            var issue = new cap_issue
            {
                cap_Description = "Allow users to start developing a bug.",
                cap_name = "Start developing a bug",
                cap_Type = cap_issue_cap_type.Feature,
            };
            var issueReference = this.CreateTestRecord(issue);

            var response = (cap_StartDevelopingResponse)this.OrgService.Execute(
                new cap_StartDevelopingRequest { Target = issueReference });
            this.CreatedEntities.Add(response.Solution);

            var updatedIssue = this.issueRepo
                .Retrieve(issueReference.Id, new string[] { "cap_developmentsolution" });

            Assert.Equal(expectedDevelopmentSolution, updatedIssue.cap_DevelopmentSolution);
        }
    }
}
