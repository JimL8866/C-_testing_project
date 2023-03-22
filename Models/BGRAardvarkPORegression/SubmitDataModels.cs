using CTGAUAardQuadTest.DataClasses;
using CTGAUAardQuadTest.DataClasses.BGRAardvarkPORegression;
using Spartan.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTGAUAardQuadTest.Models.BGRAardvarkPORegression
{
    public class SubmitDataModels
    {
        private readonly ScriptContext _scriptContext;

        public SubmitDataModels(ScriptContext scriptContext)
        {
            _scriptContext = scriptContext;
        }

        public void SubmitData()
        {
            string testDataFile1 = Path.Combine(_scriptContext.DataSubmit.TestDataAndSamplesFolder, _scriptContext.DataSubmit.DataFileName1);
            string testDataFile2 = Path.Combine(_scriptContext.DataSubmit.TestDataAndSamplesFolder, _scriptContext.DataSubmit.DataFileName2);
            string testDataFile1In = Path.Combine(_scriptContext.DataSubmit.INDataFolder, _scriptContext.DataSubmit.DataFileName1);
            string testDataFile2In = Path.Combine(_scriptContext.DataSubmit.INDataFolder, _scriptContext.DataSubmit.DataFileName2);

            List<bool> dataList = new List<bool>
            {
                File.Exists(testDataFile1),
                File.Exists(testDataFile2)
            };

            if (dataList[0] == false && dataList[1] == false)
            {
                TestDriver.ResultsLog.LogFail($"Cannot find two testdata files");
            }
            else
            {
                if (dataList[0] == false)
                {
                    File.Copy(testDataFile2, testDataFile2In, true);
                    TestDriver.ResultsLog.LogPass($"{_scriptContext.DataSubmit.DataFileName2} has been copied to Infolder");
                }
                else if (dataList[1] == false)
                {
                    File.Copy(testDataFile1, testDataFile1In, true);
                    TestDriver.ResultsLog.LogPass($"{_scriptContext.DataSubmit.DataFileName1} has been copied to Infolder");
                }
                else
                {
                    File.Copy(testDataFile1, testDataFile1In, true);
                    File.Copy(testDataFile2, testDataFile2In, true);
                    TestDriver.ResultsLog.LogPass($"Both{_scriptContext.DataSubmit.DataFileName1} and {_scriptContext.DataSubmit.DataFileName2} have been copied to Infolder");
                }
            }

            TestDriver.ResultsLog.LogStep("Result");
        }
    }
}
