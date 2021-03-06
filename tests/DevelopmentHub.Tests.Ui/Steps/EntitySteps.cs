﻿namespace DevelopmentHub.Tests.Ui.Steps
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
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
        /// Asserts that the following error notifications are shown.
        /// </summary>
        /// <param name="table">The error notification messages.</param>
        [Then("I can see error form notifications stating the following")]
        public static void ThenICanSeeErrorFormNotificationsStatingTheFollowing(Table table)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            foreach (var row in table.Rows)
            {
                Capgemini.PowerApps.SpecFlowBindings.Steps.EntitySteps.ThenICanSeeAnErrorFormNotificationStating(row[0]);
            }
        }

        /// <summary>
        /// Then a mandatory field error is displayed on the following fields.
        /// </summary>
        /// <param name="expectedError">The error message expected on the fields.</param>
        /// <param name="table">The fields that should display a mandatory field error.</param>
        [Then(@"the field error '(.*)' is displayed on the following fields")]
        public static void ThenAMandatoryFieldErrorIsDisplayedOnTheFollowingFields(string expectedError, Table table)
        {
            if (string.IsNullOrEmpty(expectedError))
            {
                throw new ArgumentException("Expected error must not be empty.", nameof(expectedError));
            }

            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var fields = table.Rows.Select((row) => row.Values.First());

            foreach (var field in fields)
            {
                var fieldContainer = Driver.WaitUntilAvailable(
                    By.XPath(
                        AppElements.Xpath[AppReference.Entity.TextFieldContainer].Replace("[NAME]", field)));

                var errorMessageWithDisplayName = fieldContainer
                    .FindElement(By.XPath($"//*[contains(@data-id, \'{field}-error-message\')]"))
                    .Text;
                var errorMessage = Regex.Match(errorMessageWithDisplayName, @".*:\s(.*)").Groups[1].Value;

                errorMessage.Should().Be(expectedError);
            }
        }
    }
}
