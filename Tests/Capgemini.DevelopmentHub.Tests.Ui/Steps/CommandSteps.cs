namespace Capgemini.DevelopmentHub.Tests.Ui.Steps
{
    using Capgemini.Test.Xrm.Uci;
    using TechTalk.SpecFlow;

    [Binding]
    public class CommandSteps : XrmUciStepDefiner
    {
        [When("I select the (.*) command")]
        public void WhenISelectTheCommand(string commandName)
        {
            this.XrmApp.CommandBar.ClickCommand(commandName);
        }
    }
}
