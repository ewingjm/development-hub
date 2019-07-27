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
                    Guid.Parse("686e7a7e-1c8e-4b3b-a438-9b9c5e204706"),
                    Guid.Parse("4a89440e-6543-4f47-b562-4d30493c8a95"),
                    new Uri("https://devhubdevelop.crm11.dynamics.com"),
                    "max@devhubci.onmicrosoft.com",
                    "Cadmandr1ve")).Result;

            var oDataSolutionService = new ODataSolutionService(new ODataRepositoryFactory(new ODataClient(new Uri("https://devhubci.crm11.dynamics.com"), accessToken)), new ConsoleLogWriter());

            try
            {
                var result = oDataSolutionService.GetSolutionZipAsync("cap_DemoIssue", true).Result;
            }
            catch (AggregateException ex)
            {
                throw;
            }
        }
    }
}
