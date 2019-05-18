namespace Capgemini.DevelopmentHub.Tests.Integration.Issues
{
    using Capgemini.DevelopmentHub.Model;
    using Microsoft.Xrm.Sdk.Query;
    using Xunit;

    public class IssueTests : IntegrationTest
    {
        [Fact]
        public void Create_Issue_StatusIsToDo()
        {
            var issueReference = this.CreateTestData(new cap_issue
            {
                cap_url = "https://url.com",
                cap_Description = nameof(this.Create_Issue_StatusIsToDo),
                cap_name = nameof(this.Create_Issue_StatusIsToDo),
                cap_Type = cap_issue_cap_type.Feature
            });

            var issue = this.OrgService
                .Retrieve(cap_issue.EntityLogicalName, issueReference[0].Id, new ColumnSet("statuscode"))
                .ToEntity<cap_issue>();

            Assert.Equal(cap_issue_statuscode.ToDo, issue.statuscode);
        }
    }
}
