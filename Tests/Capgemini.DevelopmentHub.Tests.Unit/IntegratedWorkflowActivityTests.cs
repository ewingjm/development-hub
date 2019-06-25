namespace Capgemini.DevelopmentHub.Tests.Unit.Develop
{
    using System.Activities;
    using System.Threading.Tasks;
    using Capgemini.DevelopmentHub.Develop.Plugins;
    using Capgemini.DevelopmentHub.Model;
    using Capgemini.DevelopmentHub.Repositories;
    using Moq;

    /// <summary>
    /// Base class for workflow activitys derived from <see cref="BusinessLogic.IntegratedWorkflowActivity"/>.
    /// </summary>
    /// <typeparam name="TCodeActivity">The type of workflow activity.</typeparam>
    public abstract class IntegratedWorkflowActivityTests<TCodeActivity> : WorkflowActivityTests<TCodeActivity>
        where TCodeActivity : CodeActivity, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegratedWorkflowActivityTests{TCodeActivity}"/> class.
        /// </summary>
        public IntegratedWorkflowActivityTests()
        {
            this.OAuthTokenRepositoryMock = new Mock<IOAuthTokenRepository>();
            this.ODataClientMock = new Mock<IODataClient>();

            this.WorkflowInvoker.Extensions.Add(this.OAuthTokenRepositoryMock.Object);
        }

        /// <summary>
        /// Gets mock <see cref="IOAuthTokenRepository"/>.
        /// </summary>
        protected Mock<IOAuthTokenRepository> OAuthTokenRepositoryMock { get; private set; }

        /// <summary>
        /// Gets mock <see cref="IODataClient"/>/.
        /// </summary>
        protected Mock<IODataClient> ODataClientMock { get; private set; }

        /// <summary>
        /// Gets a workflow context that has a <see cref="Model.Requests.OAuthPasswordGrantRequest"/> in the shared variables.
        /// </summary>
        protected void MockPasswordGrantConfiguredContext()
        {
            var injectedConfig =
                @"{
                    ""ClientId"": ""F5BEF3B4-2299-4156-A866-1C6A26432570"", 
                    ""TenantId"": ""AE099508-7DD7-4293-B96E-E0CB800784EA"", 
                    ""Username"": ""max@devhubdevelop.onmicrosoft.com"", 
                    ""Password"": ""Password123""
                }";

            this.WorkflowContextMock.Object.SharedVariables.Add(InjectSecureConfig.SharedVariablesKeySecureConfig, injectedConfig);
        }

        /// <summary>
        /// Mocks a valid response when requesting an OAuth access token.
        /// </summary>
        protected void MockAccessTokenResult()
        {
            this.OAuthTokenRepositoryMock.SetReturnsDefault(Task.FromResult(new OAuthToken { AccessToken = "ACCESS TOKEN" }));
        }
    }
}
