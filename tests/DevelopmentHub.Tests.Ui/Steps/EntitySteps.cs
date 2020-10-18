namespace DevelopmentHub.Tests.Ui.Steps
{
    using System.Linq;
    using Capgemini.PowerApps.SpecFlowBindings;
    using FluentAssertions;
    using Microsoft.Dynamics365.UIAutomation.Api.UCI;
    using Microsoft.Dynamics365.UIAutomation.Browser;
    using OpenQA.Selenium;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Bindings for forms.
    /// </summary>
    [Binding]
    public class EntitySteps : PowerAppsStepDefiner
    {
        /// <summary>
        /// Then a mandatory field error is displayed on the following fields.
        /// </summary>
        /// <param name="table">The fields that should display a mandatory field error.</param>
        [Then(@"a mandatory field error is displayed on the following fields")]
        public void ThenAMandatoryFieldErrorIsDisplayedOnTheFollowingFields(Table table)
        {
            var fields = table.Rows.Select((row) => row.Values.First());

            foreach (var field in fields)
            {
                var fieldContainer = Driver.WaitUntilAvailable(
                    By.XPath(
                        AppElements.Xpath[AppReference.Entity.TextFieldContainer].Replace("[NAME]", field)));

                var errorMessage = fieldContainer
                    .FindElement(By.XPath($"//*[contains(@data-id, \'{field}-error-message\')]"))
                    .Text;

                errorMessage.Should().Be("A required field cannot be empty.");
            }
        }
    }
}
