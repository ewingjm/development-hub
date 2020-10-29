namespace DevelopmentHub.Tests.Ui.Hooks
{
    using System.IO;
    using System.Reflection;
    using Capgemini.PowerApps.SpecFlowBindings;
    using NUnit.Framework;
    using TechTalk.SpecFlow;

    /// <summary>
    /// After scenario hooks.
    /// </summary>
    [Binding]
    public class AfterScenarioHooks : PowerAppsStepDefiner
    {
        private readonly ScenarioContext scenarioContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AfterScenarioHooks"/> class.
        /// </summary>
        /// <param name="scenarioContext">The scenario context.</param>
        public AfterScenarioHooks(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        /// <summary>
        /// Publishes the screenshot of the browser when a test fails.
        /// </summary>
        [AfterScenario(Order = 100)]
        public void PublishScreenshotForFailedScenario()
        {
            if (this.scenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.TestError)
            {
                var rootFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var screenshotsFolder = Path.Combine(rootFolder, "screenshots");

                var fileName = string.Concat(this.scenarioContext.ScenarioInfo.Title.Split(Path.GetInvalidFileNameChars()));
                TestContext.AddTestAttachment(Path.Combine(screenshotsFolder, $"{fileName}.jpg"), "Screenshot");
            }
        }
    }
}
