namespace Capgemini.DevelopmentHub.Tests.Ui.Steps
{
    using System;
    using Capgemini.DevelopmentHub.Tests.Ui.Extensions;
    using Capgemini.Test.Xrm.Uci;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Step bindings related to logging in.
    /// </summary>
    [Binding]
    public class LoginSteps : XrmUciStepDefiner
    {
        /// <summary>
        /// Given you are logged in to the given app as the given user.
        /// </summary>
        /// <param name="appName">The name of the app.</param>
        /// <param name="userAlias">The alias of the user.</param>
        [Given("I am logged in to the (.*) app as (.*)")]
        public void GivenIAmLoggedInAs(string appName, string userAlias)
        {
            var user = this.XrmTestConfig.GetUserConfiguration(userAlias);

            this.XrmApp.OnlineLogin.Login(
                new Uri(this.XrmTestConfig.Url),
                user.Username.ToSecureString(),
                user.Password.ToSecureString());

            this.XrmApp.Navigation.OpenApp(appName);
        }
    }
}
