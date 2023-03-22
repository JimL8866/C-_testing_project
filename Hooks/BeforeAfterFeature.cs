using Spartan.Core;
using Spartan.Core.SpecFlow;
using Spartan.Core.Web;
using System;
using System.Linq;
using TechTalk.SpecFlow;

namespace CTGAUAardQuadTest.Hooks
{
    [Binding]
    public class BeforeAfterFeature
    {
        [BeforeFeature]
        private static void BeforeFeature(FeatureContext featureContext)
        {
            var testData = "";
            foreach (var tag in featureContext.FeatureInfo.Tags)
            {
                if (tag.Contains("FeatureTestDataID"))
                    testData = tag.Split('=').Last();
            }
            if (string.IsNullOrEmpty(testData)) SpecFlowInitHelpers.FeatureInit(featureContext);
            else SpecFlowInitHelpers.FeatureInit(featureContext, testData);

            TestDriver.ResultsLog.LogFeatureSteps();
            TestDriver.ResultsLog.LogFinishFeature();
        }

        [AfterFeature]
        private static void AfterFeature()
        {
            try
            {
                VariableContainer.Feature.ClearDictionary();
                VariableContainer.Scenario.ClearDictionary();
            }
            finally
            {


            }

        }
    }
}
