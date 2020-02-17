namespace DevelopmentHub.Tests.Integration.Develop
{
    using System;
    using System.ServiceModel;
    using DevelopmentHub.Develop.Model;
    using DevelopmentHub.Repositories;
    using Microsoft.Xrm.Sdk;
    using Xunit;

    /// <summary>
    /// Tests for the Issue entity.
    /// </summary>
    [Trait("Solution", "devhub_DevelopmentHub_Develop")]
    public class IssueTests : IntegrationTest
    {
        private readonly ICrmRepository<devhub_issue> issueRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueTests"/> class.
        /// </summary>
        public IssueTests()
            : base(new Uri("https://devhubdevelop.crm11.dynamics.com"), "max@devhubdevelop.onmicrosoft.com")
        {
            this.issueRepo = this.RepositoryFactory.GetRepository<DevelopContext, devhub_issue>();
        }

        /// <summary>
        /// Tests that StartDeveloping returns an error if called on an issue that is not in a 'To Do' status.
        /// </summary>
        [Fact]
        public void StartDeveloping_IssueNotToDo_ReturnsError()
        {
            var issue = new devhub_issue();
            var issueReference = this.CreateTestRecord(issue);
            issue.devhub_issueId = issueReference.Id;
            issue.statecode = devhub_issueState.Inactive;
            issue.statuscode = devhub_issue_statuscode.Cancelled;
            this.issueRepo.Update(issue);

            Assert.Throws<FaultException<OrganizationServiceFault>>(() =>
            {
                var response = (devhub_StartDevelopingResponse)this.OrgService.Execute(
                    new devhub_StartDevelopingRequest { Target = issueReference });
            });
        }

        /// <summary>
        /// Tests that StartDeveloping returns a reference to a created solution if called on an issue that is in a 'To Do' status.
        /// </summary>
        [Fact]
        public void StartDeveloping_IssueToDo_CreatesMatchingSolution()
        {
            var issue = new devhub_issue
            {
                devhub_Description = "Allow users to start developing a feature.",
                devhub_name = "Start developing a feature",
                devhub_Type = devhub_issue_devhub_type.Feature,
            };
            var issueReference = this.CreateTestRecord(issue);

            var response = (devhub_StartDevelopingResponse)this.OrgService.Execute(
                new devhub_StartDevelopingRequest { Target = issueReference });
            this.CreatedEntities.Add(response.Solution);

            Assert.NotNull(response.Solution);
        }

        /// <summary>
        /// Tests that StartDeveloping sets the development solution unique name on the issue.
        /// </summary>
        [Fact]
        public void StartDeveloping_IssueToDo_UpdatesDevelopmentSolutionOnIssue()
        {
            var expectedDevelopmentSolution = "devhub_StartDevelopingABug";
            var issue = new devhub_issue
            {
                devhub_Description = "Allow users to start developing a bug.",
                devhub_name = "Start developing a bug",
                devhub_Type = devhub_issue_devhub_type.Feature,
            };
            var issueReference = this.CreateTestRecord(issue);

            var response = (devhub_StartDevelopingResponse)this.OrgService.Execute(
                new devhub_StartDevelopingRequest { Target = issueReference });
            this.CreatedEntities.Add(response.Solution);

            var updatedIssue = this.issueRepo
                .Retrieve(issueReference.Id, new string[] { "devhub_developmentsolution" });

            Assert.Equal(expectedDevelopmentSolution, updatedIssue.devhub_DevelopmentSolution);
        }
    }
}
