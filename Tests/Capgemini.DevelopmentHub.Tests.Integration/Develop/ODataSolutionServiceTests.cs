namespace Capgemini.DevelopmentHub.Tests.Integration.Develop
{
    using System;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Capgemini.DevelopmentHub.BusinessLogic.Logging;
    using Capgemini.DevelopmentHub.Develop.BusinessLogic;
    using Capgemini.DevelopmentHub.Model.Requests;
    using Capgemini.DevelopmentHub.Repositories;
    using Xunit;

    /// <summary>
    /// asdad.
    /// </summary>
    public class ODataSolutionServiceTests : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSolutionServiceTests"/> class.
        /// </summary>
        public ODataSolutionServiceTests()
            : base(new Uri("https://devhubdevelop.crm11.dynamics.com"), "max@devhubdevelop.onmicrosoft.com")
        {
        }

        /// <summary>
        /// adad.
        /// </summary>
        [Fact]
        public void Test()
        {
            var accessToken = new OAuthTokenRepository().GetAccessToken(
                new OAuthPasswordGrantRequest(
                    Guid.Parse("eeb1e2f9-e62e-4d04-93be-738702286237"),
                    Guid.Parse("57dbfea6-e857-4818-b9b2-7c320afd233b"),
                    new Uri("https://dhubtest.crm11.dynamics.com"),
                    "service@dhubtest.onmicrosoft.com",
                    "dhubtest123!")).Result;

            var oDataSolutionService = new ODataSolutionService(new ODataRepositoryFactory(new ODataClient(new Uri("https://dhubtest.crm11.dynamics.com"), accessToken)), new ConsoleLogWriter());

            try
            {
                oDataSolutionService.MergeSolutionComponentsAsync("cap_ExtractMergedSolutionsIntoSourceControl", "cap_DevelopmentHub_Target", false).Wait();
            }
            catch (AggregateException ex)
            {
                throw;
            }
        }
    }
}
