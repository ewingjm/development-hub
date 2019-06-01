namespace Capgemini.DevelopmentHub.Tests.Integration.Issues
{
    using System;
    using Capgemini.DevelopmentHub.Issues.Model;
    using Microsoft.Xrm.Sdk.Query;
    using Xunit;

    /// <summary>
    /// Integration tests for the <see cref="cap_issue"/> entity.
    /// </summary>
    [Trait("Solution", "cap_DevelopmentHub_Issues")]
    public class IssueTests : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IssueTests"/> class.
        /// </summary>
        public IssueTests()
            : base(new Uri("https://devhubdev.crm11.dynamics.com"), "max@devhubdev.onmicrosoft.com")
        {
        }

        /// <summary>
        /// Tests that the Issue status is set to 'To Do' on create.
        /// </summary>
        [Fact]
        public void Create_Issue_StatusIsToDo()
        {
            var issueReference = this.CreateTestData(new cap_issue
            {
                cap_url = "https://url.com",
                cap_Description = nameof(this.Create_Issue_StatusIsToDo),
                cap_name = nameof(this.Create_Issue_StatusIsToDo),
                cap_Type = cap_issue_cap_type.Feature,
            });

            var issue = this.OrgService
                .Retrieve(cap_issue.EntityLogicalName, issueReference[0].Id, new ColumnSet("statuscode"))
                .ToEntity<cap_issue>();

            Assert.Equal(cap_issue_statuscode.ToDo, issue.statuscode);
        }
    }
}
