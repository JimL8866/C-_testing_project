using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spartan.Core;
using Spartan.Core.SpecFlow;
using Spartan.Core.Web;
using System;
using TechTalk.SpecFlow;

namespace CTGAUAardQuadTest.Hooks
{
    [Binding]
    public class BeforeAfterScenario
    {
        private static SpecFlowTestContext _specFlowTestContext;

        [BeforeScenario]
        private static void BeforeUiScenario(ScenarioContext scenarioContext)
        {
            _specFlowTestContext = new SpecFlowTestContext(scenarioContext);
            SpecFlowInitHelpers.TestInit(scenarioContext);
        }

        [AfterScenario]
        private static void AfterScenario()
        {


            TestDriver.ResultsLog.LogFinish();

            if (TestDriver.XmlResultsGenerate)
            {
                Results.WriteXml2File();
            }
            _specFlowTestContext.AddFiles(Results.ResultFiles);

            if (TestDriver.ResultsLog.TestResult != Results.Result.FailSuppressed) return;

            TestDriver.ResultsLog.TestStatus = Results.Status.Terminated;
            Assert.Fail(TestDriver.ResultsLog.FailException.Message);

        }
    }
}
