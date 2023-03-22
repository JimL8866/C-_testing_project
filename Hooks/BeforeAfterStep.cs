using Computershare.DevAutomation.CommonMethods.SpecFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spartan.Core;
using TechTalk.SpecFlow;

namespace CTGAUAardQuadTest.Hooks
{
    [Binding]
    public class BeforeAfterStep
    {

        [BeforeStep]
        private static void BeforeStep()
        {

        }

        [AfterStep]
        private static void AfterStep(ScenarioContext scenarioContext)
        {
            if (TestDriver.ResultsLog.TestStatus == Results.Status.Terminated)
                return;

            if (scenarioContext.StepContext.StepInfo.Table != null)
            {
                var dataTable = scenarioContext.StepContext.StepInfo.Table.CastToDataTable();

                TestDriver.ResultsLog.LogSubTest(scenarioContext.StepContext.StepInfo.StepDefinitionType + " " + scenarioContext.StepContext.StepInfo.Text.GetData(), dataTable);
            }
            else
            {
                TestDriver.ResultsLog.LogSubTest(scenarioContext.StepContext.StepInfo.StepDefinitionType + " " + scenarioContext.StepContext.StepInfo.Text.GetData());
            }

            if (TestDriver.ResultsLog.TestResult != Results.Result.Fail) return;

            TestDriver.ResultsLog.TestStatus = Results.Status.Terminated;
            Assert.Fail(TestDriver.ResultsLog.FailException.Message);
        }
    }
}
