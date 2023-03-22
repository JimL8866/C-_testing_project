using CTGAUAardQuadTest.DataClasses;
using CTGAUAardQuadTest.DataClasses.BGRAardvarkPORegression;
using CTGAUAardQuadTest.Models.BGRAardvarkPORegression;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace CTGAUAardQuadTest.StepDefinitions
{
    [Binding]
    public class RegressionBatchTests_FeatureStepDefinitions
    {
        private readonly ScriptContext _scriptContext;
        private SubmitDataModels _submitDataModel;
        private CheckForSamplesModel _checkForSamplesModel;
        private ExtractZipFileModel _extractZipFileModel;
        private CompareSamples _compareSamples;

        public RegressionBatchTests_FeatureStepDefinitions(ScriptContext scriptContext)
        {
            _scriptContext = scriptContext;
            _submitDataModel = new SubmitDataModels(_scriptContext);
            _checkForSamplesModel = new CheckForSamplesModel(_scriptContext);
            _compareSamples = new CompareSamples(_scriptContext);
        }

        [Given(@"I Submit Data Files")]
        public void GivenISubmitDataFiles(Table table)
        {
            //_scriptContext.DataSubmit = table.CreateInstance<DataSubmittedClass>();
            //_submitDataModel.SubmitData();
        }

        [When(@"Run zip is in archive location")]
        public void RunZipIsInArchiveLocation(Table table)
        {
            //_scriptContext.OutSamples = table.CreateInstance<OutSamplesClass>();
            //_checkForSamplesModel.CheckForSamples();
        }

        [Then(@"Create GToP Samples")]
        public void ThenCreateGToPSamples()
        {
            //_extractZipFileModel = new ExtractZipFileModel(_scriptContext);
            //_extractZipFileModel.ExtractFile();
        }

        [Then(@"Copy files to TestFolder")]
        public void ThenCopyFilesToTestFolder()
        {
            //_extractZipFileModel.GetOutPutFiles();
        }


        [Then(@"Compare Files with Baseline Files")]
        public void ThenCompareFilesWithBaselineFiles(Table table)
        {
            _scriptContext.BaseLineSamples = table.CreateInstance<BaseLineSamplesClass>();
            _compareSamples.comparePDFs();
        }
    }
}


