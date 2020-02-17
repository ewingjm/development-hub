namespace DevelopmentHub.Tests.Ui.Steps
{
    using System;
    using System.Collections.Generic;
    using Capgemini.Test.Xrm.Uci;
    using Microsoft.Dynamics365.UIAutomation.Api.UCI;
    using Microsoft.Dynamics365.UIAutomation.Browser;
    using OpenQA.Selenium;
    using TechTalk.SpecFlow;
    using Xunit;

    /// <summary>
    /// Step bindings for commands.
    /// </summary>
    [Binding]
    public class CommandSteps : XrmUciStepDefiner
    {
        /// <summary>
        /// When a command with the given name is clicked.
        /// </summary>
        /// <param name="commandName">The name of the command to click.</param>
        [When("I select the (.*) command")]
        public void WhenISelectTheCommand(string commandName)
        {
            this.XrmApp.CommandBar.ClickCommand(commandName);
        }

        /// <summary>
        /// Then a command with the gien name is not visible.
        /// </summary>
        /// <param name="commandName">The name of the command to check for invisibility.</param>
        [Then("I can't see the (.*) command")]
        public void ThenICantSeeTheCommand(string commandName)
        {
            this.XrmApp.ThinkTime(Constants.DefaultThinkTime);

            var items = this.GetRibbonItems();

            Assert.DoesNotContain(
                items,
                x => x.GetAttribute("aria-label").Equals(commandName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Then a command with the gien name is visible.
        /// </summary>
        /// <param name="commandName">The name of the command to check for visibility.</param>
        [Then("I can see the (.*) command")]
        public void ThenICanSeeTheCommand(string commandName)
        {
            this.XrmApp.ThinkTime(Constants.DefaultThinkTime);

            var items = this.GetRibbonItems();

            Assert.DoesNotContain(
                items,
                x => x.GetAttribute("aria-label").Equals(commandName, StringComparison.OrdinalIgnoreCase));
        }

        private IReadOnlyCollection<IWebElement> GetRibbonItems()
        {
            var ribbon = this.Driver.FindElement(By.XPath(AppElements.Xpath[AppReference.CommandBar.ContainerGrid]));

            return ribbon.FindElements(By.TagName("li"));
        }
    }
}
