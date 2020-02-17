namespace DevelopmentHub.Tests.Ui.Steps
{
    using Capgemini.Test.Xrm.Uci;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Step bindings for navigation.
    /// </summary>
    [Binding]
    public class NavigationSteps : XrmUciStepDefiner
    {
        /// <summary>
        /// Given a test record has been opened.
        /// </summary>
        /// <param name="alias">The alias of the test record.</param>
        [Given(@"I have opened (.*)")]
        public void GivenIHaveOpened(string alias)
        {
            this.TestDriver.OpenTestRecord(alias);
        }

        /// <summary>
        /// Given the provided sub-area of the site map is being viewed.
        /// </summary>
        /// <param name="subAreaName">The sub-area to open.</param>
        /// <param name="areaName">The area of the sub-area.</param>
        [Given("I am viewing the (.*) sub area of the (.*) area")]
        public void GivenIAmViewingTheSubArea(string subAreaName, string areaName)
        {
            this.XrmApp.Navigation.OpenSubArea(areaName, subAreaName);
        }
    }
}
