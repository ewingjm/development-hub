namespace Capgemini.DevelopmentHub.Tests.Ui.Steps
{
    using System;
    using System.Linq;
    using Capgemini.Test.Xrm.Uci;
    using Microsoft.Dynamics365.UIAutomation.Api.UCI;
    using Microsoft.Dynamics365.UIAutomation.Browser;
    using OpenQA.Selenium;
    using TechTalk.SpecFlow;
    using Xunit;

    /// <summary>
    /// Bindings for forms.
    /// </summary>
    [Binding]
    public class FormSteps : XrmUciStepDefiner
    {
        [ThreadStatic]
        private static InvalidOperationException currentException;

        /// <summary>
        /// When I save the record.
        /// </summary>
        [When(@"I save the record")]
        public void WhenISaveTheRecord()
        {
            try
            {
                this.XrmApp.Entity.Save();
            }
            catch (InvalidOperationException ex)
            {
                currentException = ex;
            }
        }

        /// <summary>
        /// Then I can edit the following fields.
        /// </summary>
        /// <param name="table">The fields that should be editable.</param>
        [Then(@"I can edit the following fields")]
        public void ThenICanEditTheFollowingFields(Table table)
        {
            var fields = table.Rows.Select((row) => this.XrmApp.Entity.GetField(row.Values.First()));

            foreach (var field in fields)
            {
                Assert.NotNull(field);
                Assert.True(field.IsVisible);
                Assert.False(field.IsReadOnly);
            }
        }

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
                var fieldContainer = this.Driver.WaitUntilAvailable(
                    By.XPath(
                        AppElements.Xpath[AppReference.Entity.TextFieldContainer].Replace("[NAME]", field)));

                var errorMessage = fieldContainer
                    .FindElement(By.XPath($"//*[contains(@data-id, \'{field}-error-message\')]"))
                    .Text;

                Assert.Equal("A requied field cannot be empty.", errorMessage);
            }
        }
    }
}
