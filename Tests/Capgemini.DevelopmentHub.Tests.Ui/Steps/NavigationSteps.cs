namespace Capgemini.DevelopmentHub.Tests.Ui.Steps
{
    using Capgemini.Test.Xrm.Uci;
    using TechTalk.SpecFlow;

    [Binding]
    public class NavigationSteps : XrmUciStepDefiner
    {
        [Given("I am viewing the (.*) sub area of the (.*) area")]
        public void GivenIAmViewingTheSubArea(string subAreaName, string areaName)
        {
            this.XrmApp.Navigation.OpenSubArea(areaName, subAreaName);
        }
    }
}
