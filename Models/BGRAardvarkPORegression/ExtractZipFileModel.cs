using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using CTGAUAardQuadTest.DataClasses;
using System.Linq;
using Spartan.Core;


namespace CTGAUAardQuadTest.Models.BGRAardvarkPORegression
{
    public class ExtractZipFileModel
    {
        private readonly ScriptContext _scriptContext;
        private readonly string _runZipFile;
        private readonly string _sampleFolder;
        private readonly string _runNum;
        private string _runFolder;
        private string _tempFolder;

        //in VM \\meldvdagds03\CCS_Resource\GoldResource
        private readonly string g2pCommand = Path.Combine(Environment.GetEnvironmentVariable("CCS_Resource"), @"Global\Scripts\g2p.cmd");
        
        public ExtractZipFileModel(ScriptContext scriptContext)
        { 
            _scriptContext = scriptContext;
            //get runzip file path from previous step
            _runZipFile = _scriptContext.OutSamples.RunFilePath;

            string[] runZipPathParts = _runZipFile.Split('\\');

            //using length - 2 to get the 2nd last element of the path, which should be the contract folder name
            string contractFolderName = runZipPathParts[runZipPathParts.Length - 2];

            _runNum = _scriptContext.OutSamples.RunNumber;
            _tempFolder = Path.Combine(@"c:\__AutoTemp\", contractFolderName + "\\", _runNum);
            _sampleFolder = _scriptContext.DataSubmit.TestDataAndSamplesFolder;
        }

        /// <summary>
        /// This method is to extract zip file
        /// And calling g2p  
        /// </summary>
        public void ExtractFile()
        {
            //create a temp folder on C drive first
            if (! Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
            }

            //extract run.zip file to temp folder
            ZipFile.ExtractToDirectory(_runZipFile, _tempFolder);

            Thread.Sleep(15000);

            //remove the run number from the GPD files
            RemoveRunNumberFromGPDFiles();

            Thread.Sleep(15000);

            //call g2p command
            G2P();
        }

        /* RemoveRunNumberFromGPDFiles
         * Removed the run number from GPD file names
         * And the gpd file name within the adm xml file
         * This is done so that the codeline in g2p samples do not contain a run number
         */
        public void RemoveRunNumberFromGPDFiles()
        {
            //get required GPD files
            string[] searchPatterns = { "*.gpd", "*.gpd.adm.xml", "*.gpd.ofs" };

            var files = searchPatterns.SelectMany(pattern => Directory.EnumerateFiles(_tempFolder, pattern));

            //loop through files 
            foreach (var file in files)
            {
                //only rename files containing the run number
                if (file.Contains($"_{_runNum}"))
                {
                    string fileName = Path.GetFileName(file);
                    string sourceFilePath = Path.Combine(_tempFolder, fileName);

                    //renaming files
                    string newFileName = fileName.Replace($"_{_runNum}", "");
                    string destinationPath = Path.Combine(_tempFolder, newFileName);
                    File.Move(sourceFilePath, destinationPath);

                    //if file is an adm xml file, also update the GPD filename within the adm xml file
                    if (destinationPath.EndsWith(".gpd.adm.xml"))
                    {
                        string[] admXmlLines = File.ReadAllLines(destinationPath);

                        //go through each line and look for the gpd name in a specific xml tag
                        for (int i = 0; i < admXmlLines.Length; ++i) {
                            if (
                                admXmlLines[i].Contains("<name>")
                                &&
                                admXmlLines[i].Contains($"_{_runNum}")
                                &&
                                admXmlLines[i].Contains(".gpd</name>")
                            )
                            {
                                //update to remove the run number when found
                                admXmlLines[i] = admXmlLines[i].Replace($"_{_runNum}", "");
                            }
                        }

                        //write back out to the same adm xml file
                        File.WriteAllLines(destinationPath, admXmlLines);
                    }
                }
            }
        }

        /// <summary>
        /// This method is used to crate sample folder
        /// And grab the required files
        /// </summary>

        public void GetOutPutFiles()
        {
            //create the sample folder
            string runFolderPath = CreateSampleFolder();
            _scriptContext.samplesFolder = runFolderPath;

            //Copy files
            int totalFiles; 
            CopyFiles(runFolderPath, out totalFiles);

            //check if total number of files had been copied over
            string[] copiedFiles = Directory.GetFiles(runFolderPath);
            if (copiedFiles.Length == totalFiles)
            {
                TestDriver.ResultsLog.LogPass($"Total {totalFiles} numbers of files had been copied to test sample folder");
                
                //if test pass will delete the current run folder
                DeleteFolder();
            }
            else
            {
                TestDriver.ResultsLog.LogFail("Required files had not been copied over");
            }
            TestDriver.ResultsLog.LogStep("Result");
        }

        /// <summary>
        /// This method is run g2p command
        /// </summary>
        public void G2P()
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.WorkingDirectory = _tempFolder;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;

            process.StartInfo = startInfo;
            process.Start();

            //calling g2p using similar arguments to when vmp samples are generated
            process.StandardInput.WriteLine(g2pCommand + " --sg --duplex --stock_code --align --vmp --vmp_allow_no_match --apca_chq --micr_gauge *");
            Thread.Sleep(30000);
            process.StandardInput.Close();
            process.WaitForExit();
            Thread.Sleep(15000);

            if (process.ExitCode != 0)
            {
                TestDriver.ResultsLog.LogFail("G2P command failed and samples had not been generated");
            }
            else
            {
                TestDriver.ResultsLog.LogPass($"G2P samples have been generated successfully");
            }

            TestDriver.ResultsLog.LogStep("Result");
        }

        /// <summary>
        /// This method is to create sample folder and run folder if not exist
        /// </summary>
        public string  CreateSampleFolder()
        {
            _runFolder = Path.Combine(_sampleFolder, "Sample", "Run" + _runNum);
            if (!Directory.Exists(_runFolder))
            {
                Directory.CreateDirectory(_runFolder);
            }
            return _runFolder;
        }

        /// <summary>
        /// This method is to get the pattern of required file and then search
        /// And copy to destination folder
        /// </summary>
        /// <param name="desPath"></param>
        public void CopyFiles(string desPath, out int fileCount)
        {
            //get required pdf and csv files
            string[] searchPatterns = { "*.pdf", "*.csv" };
            
            var files = searchPatterns.SelectMany(pattern => Directory.EnumerateFiles(_tempFolder, pattern));
            fileCount = files.Count();

            //loop through files 
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                string sourceFilePath = Path.Combine(_tempFolder, fileName);

                string lowerCaseFileName = fileName.ToLower();

                if (
                    lowerCaseFileName.StartsWith("s_new")
                    &&
                    lowerCaseFileName.EndsWith(".pdf")
                )
                {
                    //this is a vmp pdf
                    //skip this pdf - g2p was called to create other pdf samples using the --vmp switch
                    --fileCount;
                }
                else {
                    //renaming file and copying to samples folder
                    string newFileName = fileName.Replace($"_{_runNum}", "");
                    string destinationPath = Path.Combine(desPath, newFileName);
                    File.Copy(sourceFilePath, destinationPath, true);
                }
            }
        }

        /// <summary>
        /// This method is used to delete the run folder 
        /// </summary>
        public void DeleteFolder()
        {
            if (Directory.Exists(_tempFolder))
            {
                Directory.Delete(_tempFolder, true);
            }
        }
    }
}
