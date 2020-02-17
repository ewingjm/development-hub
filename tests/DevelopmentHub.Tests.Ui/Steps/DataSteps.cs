namespace DevelopmentHub.Tests.Ui.Steps
{
    using Capgemini.Test.Xrm.Uci;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Step bindings for data.
    /// </summary>
    [Binding]
    public class DataSteps : XrmUciStepDefiner
    {
        /// <summary>
        /// Given the test record has been created.
        /// </summary>
        /// <param name="fileName">The name of the file containing the test record.</param>
        [Given(@"I have created (.*)")]
        public void GivenIHaveCreated(string fileName)
        {
            this.TestDriver.LoadTestData(this.TestDataRepository.GetTestData(fileName));
        }
    }
}
