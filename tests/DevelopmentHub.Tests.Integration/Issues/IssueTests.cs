namespace DevelopmentHub.Tests.Integration.Issues
{
    using System;
    using DevelopmentHub.Issues.Model;
    using Microsoft.Xrm.Sdk.Query;
    using Xunit;

    /// <summary>
    /// Integration tests for the <see cref="devhub_issue"/> entity.
    /// </summary>
    [Trait("Solution", "devhub_DevelopmentHub_Issues")]
    public class IssueTests : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IssueTests"/> class.
        /// </summary>
        public IssueTests()
            : base()
        {
        }

        /// <summary>
        /// Tests that the Issue status is set to 'To Do' on create.
        /// </summary>
        [Fact]
        public void Create_Issue_StatusIsToDo()
        {
            var issueReference = this.CreateTestData(new devhub_issue
            {
                devhub_url = "https://url.com",
                devhub_Description = nameof(this.Create_Issue_StatusIsToDo),
                devhub_name = nameof(this.Create_Issue_StatusIsToDo),
                devhub_Type = devhub_issue_devhub_type.Feature,
            });

            var issue = this.CrmServiceClient
                .Retrieve(devhub_issue.EntityLogicalName, issueReference[0].Id, new ColumnSet("statuscode"))
                .ToEntity<devhub_issue>();

            Assert.Equal(devhub_issue_statuscode.ToDo, issue.statuscode);
        }
    }
}
