namespace Capgemini.DevelopmentHub.Tests.Ui.Steps
{
    using System;
    using System.Globalization;
    using Capgemini.DevelopmentHub.Tests.Ui.Extensions;
    using Capgemini.Test.Xrm.Uci;
    using TechTalk.SpecFlow;

    [Binding]
    public class LoginSteps : XrmUciStepDefiner
    {
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
