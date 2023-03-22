using CTGAUAardQuadTest.DataClasses;
using CTGAUAardQuadTest.DataClasses.BGRAardvarkPORegression;
using Spartan.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System.Diagnostics;

namespace CTGAUAardQuadTest.Models.BGRAardvarkPORegression
{
    public class CheckForSamplesModel
    {
        //7zip program path
        private static readonly string SEVENZIP_PROGRAM =
            Environment.GetEnvironmentVariable("CCS_RESOURCE") +
            "\\Global\\Programs\\7zip\\16.02\\7z.exe";
        //this is the number of milliseconds per minute, so sleep for 1 minute each time
        private static readonly int SLEEP_TIME = 60000;
        //the max number of minutes to loop for
        private static readonly int MINUTES_TO_WAIT = 60;
        //check if file names are defined using a regex for any non-whitespace to make sure it's not blank
        private static readonly Regex NON_WHITE_SPACE = new Regex(@"\S", RegexOptions.Compiled);
        //regex for 7zip parsing - toggle data reading on or off when encountered
        private static readonly Regex TOGGLE_SEVENZIP_DATA_READING = new Regex(@"^\s*-{19}\s+-{5}\s+-{12}\s+-{12}\s+-{24}\s*$", RegexOptions.Compiled);

        //regex for 7zip parsing - reading lines containing files
        //notes
        //group 1 is file date/time
        //group 2 is attributes/size/compressed
        //group 3 is file name
        private static readonly Regex READ_SEVENZIP_DATA = new Regex(@"^\s*([0-9]+\-[0-9]+\-[0-9]+\s+[0-9]+\:[0-9]+\:[0-9]+)\s+(\S+\s+)+(.+)$", RegexOptions.Compiled);
        //regex for match the run number in a run data zip
        private static readonly Regex RUN_NUMBER_DATA_ZIP = new Regex(@"Run([0-9]+)data\.7z$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        private readonly ScriptContext _scriptContext;


        public CheckForSamplesModel(ScriptContext scriptContext)
        {
            _scriptContext = scriptContext;
        }

        public void CheckForSamples()
        {
            List<string> testDataFiles = new List<string>
            {
                _scriptContext.DataSubmit.DataFileName1,
                _scriptContext.DataSubmit.DataFileName2
            };

            //track if data files have been removed
            //starts as true, set to false when file is found
            bool dataFilesRemoved = true;

            //for each file specified, check if they exist
            foreach (string testDataFile in testDataFiles)
            {
                dataFilesRemoved = CheckFileRemoved(testDataFile, _scriptContext.DataSubmit.INDataFolder);

                if (!dataFilesRemoved)
                {
                    //this file still exists, stop looping at this point
                    break;
                }
            }

            if (!dataFilesRemoved)
            {
                //input data still exists at the in folder
                //fail this test because run might not be done, or the run failed
                TestDriver.ResultsLog.LogFail($"Data file(s) still exist at '" + _scriptContext.DataSubmit.INDataFolder + "', run might not be complete or have failed.");
            }
            else
            {
                //input data has been removed, so run may have been successful
                //now check archive folder (out samples folder) for data and run zip

                //check that archive location is not blank
                MatchCollection matches = NON_WHITE_SPACE.Matches(_scriptContext.OutSamples.ArchiveLocation);

                if (matches.Count > 0) {
                    //check that archive location exists
                    if (Directory.Exists(_scriptContext.OutSamples.ArchiveLocation))
                    {
                        TestDriver.ResultsLog.LogComment("'" + _scriptContext.OutSamples.ArchiveLocation + "' exists.");
                        TestDriver.ResultsLog.LogStep("Checked existence of archive location.");

                        //archive location is not blank, now glob for run data zips
                        //sleep for 15 seconds first just in case
                        Thread.Sleep(15000);
                        Matcher glob = new Matcher();
                        glob.AddInclude("Run*data.7z");

                        IEnumerable<string> matchingFiles = glob.GetResultsInFullPath(_scriptContext.OutSamples.ArchiveLocation);

                        //sort by descending date order so we start with the newest run zip first
                        matchingFiles = matchingFiles.OrderByDescending(f => File.GetCreationTime(f)).ToList();

                        //run data zip date/time will be compared to this process's start date time
                        //to make sure run zip was generated after this process was started
                        DateTime processStartDateTime = Process.GetCurrentProcess().StartTime;

                        //looking for the run number
                        string runNumber = "";

                        foreach (string runDataZipPath in matchingFiles)
                        {
                            //if run data zip was created before the start of this process's start date/time, stop looking for a run zip
                            if (File.GetCreationTime(runDataZipPath) < processStartDateTime)
                            {
                                TestDriver.ResultsLog.LogComment("'" + runDataZipPath + "' was created before start of this test process.");
                                TestDriver.ResultsLog.LogStep("Not checking any more run data zips.");
                                break;
                            }

                            TestDriver.ResultsLog.LogComment("Checking '" + runDataZipPath + "' for run number.");
                            TestDriver.ResultsLog.LogStep("Checking run data zip for run number.");

                            //check run data zip's contents for the data files
                            bool filesFoundInZip = CheckZipForFiles(runDataZipPath, testDataFiles);

                            if (filesFoundInZip)
                            {

                                TestDriver.ResultsLog.LogComment("'" + runDataZipPath + "' contains expected data files.");
                                TestDriver.ResultsLog.LogStep("Run data zip contains expected data files.");

                                //data files found in this zip
                                Match match = RUN_NUMBER_DATA_ZIP.Match(runDataZipPath);

                                if (match.Success)
                                {
                                    TestDriver.ResultsLog.LogComment("'" + runDataZipPath + "' contains run number in expected format.");
                                    TestDriver.ResultsLog.LogStep("Run data zip contains run number in expected format.");

                                    //group 1 is the run number
                                    runNumber = match.Groups[1].ToString();

                                    //break so we don't look through more run data zips
                                    break;
                                }
                                else
                                {
                                    //run number not found using match pattern, warn here
                                    TestDriver.ResultsLog.LogWarning("Run number could not be matched from run data zip '" + runDataZipPath + "'");
                                    TestDriver.ResultsLog.LogStep("Run number not found in run data zip name.");
                                }

                            }
                            else
                            {
                                TestDriver.ResultsLog.LogComment("'" + runDataZipPath + "' did not contain expected data files.");
                                TestDriver.ResultsLog.LogStep("Run data zip did not contain expected data files.");
                            }
                        }

                        if (runNumber.Equals(""))
                        {
                            //fail this test because the run number was not found
                            TestDriver.ResultsLog.LogFail($"Run number could not be found");
                        }
                        else
                        {
                            //run number found from run data zip, now get the run zip archive
                            string runZipPath = FindRunZip(_scriptContext.OutSamples.ArchiveLocation, runNumber);

                            if (runZipPath.Equals(""))
                            {
                                //fail this test because run zip was not found
                                TestDriver.ResultsLog.LogFail($"Run zip not found for run number '" + runNumber + "'.");
                            }
                            else
                            {
                                //run zip found, assign it to the script context so it can be used in the next step
                                // _scriptContext.ZipOutput = runZipPath;
                                _scriptContext.OutSamples.RunFilePath = runZipPath;
                                _scriptContext.OutSamples.RunNumber = runNumber;
                                //also mark this step as passed
                                TestDriver.ResultsLog.LogPass($"Found run zip '" + runZipPath + "'.");
                            }
                        }
                    }
                    else
                    {
                        TestDriver.ResultsLog.LogFail("Number of Matches" + "" + matches.Count + "'" + _scriptContext.OutSamples.ArchiveLocation + "' does not exist.");
                    }
                }
                else
                {
                    //fail this test because out folder was not specified
                    TestDriver.ResultsLog.LogFail($"Out folder not specified, not sure where to look for samples/run zip.");
                }
            }

            TestDriver.ResultsLog.LogStep("Result");
        }

        /* CheckFileRemoved
         * checks if a file was removed (no longer exists) or not
         * accepts file name or full file path as first argument
         * if only the file name is specified for first argument, the path without file name can be specified as second argument
         * returns true if the file does not exist after checking for some time, or if the file name is blank
         * returns false otherwise
         */
        public bool CheckFileRemoved(string fileName, string pathWithoutFileName = "")
        {
            //track if data files have been removed
            //starts as true in case no file name is specified
            //this is set to false if file name is specified
            bool fileRemoved = true;

            //check if the file name is not blank
            MatchCollection matches = NON_WHITE_SPACE.Matches(fileName);

            if (matches.Count > 0)
            {
                TestDriver.ResultsLog.LogComment("Checking if file '" + fileName + "' was removed or not, may take up to " + MINUTES_TO_WAIT + " minutes.");
                //file name was specified, now check if file exists
                TestDriver.ResultsLog.LogStep("Checking if file removed.");
                //set flag to false to indicate file may exist
                fileRemoved = false;

                string fullFilePath = Path.Combine(
                    pathWithoutFileName,
                    fileName
                );

                //loop while the file exists for MINUTES_TO_WAIT minutes
                int counter = 0;
                while (
                    File.Exists(fullFilePath)
                    &&
                    counter < MINUTES_TO_WAIT
                )
                {
                    Thread.Sleep(SLEEP_TIME);
                    ++counter;
                }

                //check if the file was removed or not
                if (!File.Exists(fullFilePath))
                {
                    fileRemoved = true;
                }
            }

            return fileRemoved;
        }

        /* CheckZipForFiles
         * checks the provided zip file path for the list of files
         * if all files are found in the zip file, returns true
         * otherwise returns false
         */
        public bool CheckZipForFiles (string zipFile, List<string> filesToCheckFor)
        {
            // Start the child process.
            Process process = new Process();

            // Redirect the output stream of the child process.
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            // Program and arguments
            process.StartInfo.FileName = SEVENZIP_PROGRAM;
            process.StartInfo.Arguments = "l -r \"" + zipFile + "\""; //list files in zip file, subdirectories checked recursively

            TestDriver.ResultsLog.LogComment("Running " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
            TestDriver.ResultsLog.LogStep("Running 7zip list command.");

            //Start process
            process.Start();

            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // Read the output stream first and then wait.
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            //split output into lines
            List<string> outputLines = output.Split('\n').ToList<string>();

            //keep a dictionary (perl hash) of each file and whether or not they were found
            Dictionary<string, bool> eachFileFound = new Dictionary<string, bool>();

            //initialise each file found to false
            foreach (string file in filesToCheckFor)
            {
                //check that file name is not blank
                MatchCollection matches = NON_WHITE_SPACE.Matches(file);

                if (matches.Count > 0)
                {
                    //upper case the key for case insensitive matching
                    eachFileFound[file.ToUpper()] = false;
                }
            }

            //now go through each line to look for the files to check for
            bool readingData = false;

            foreach (string outputLine in outputLines)
            {
                //remove trailing whitespace
                string formattedOutputLine = Regex.Replace(outputLine, @"\s+$", "");

                MatchCollection matches = TOGGLE_SEVENZIP_DATA_READING.Matches(formattedOutputLine);

                if (matches.Count > 0)
                {
                    //line matches the start/end of the file content listing
                    //using bitwise exclusive or here - turn on data reading if it is false (first time),
                    //otherwise turn it off if it is true (second time)
                    readingData ^= true;
                }
                else if (readingData)
                {
                    //now reading lines for data files
                    Match match = READ_SEVENZIP_DATA.Match(formattedOutputLine);

                    if (match.Success)
                    {
                        //group 1 is file date/time
                        //group 2 is attributes/size/compressed
                        //group 3 is file name
                        //after getting the file name, remove preceding folder path if it exists
                        //also upper case the file name for case insensitive matching
                        string fileInZip = match.Groups[3].ToString().Split('\\').Last().ToUpper();

                        if(eachFileFound.ContainsKey(fileInZip))
                        {
                            //file found and is in dictionary, mark as found
                            eachFileFound[fileInZip] = true;
                        }
                    }
                }
            }

            //now check if all files were found
            //start as true, if any file is not found, change to false
            bool filesFound = true;

            foreach (bool fileFound in eachFileFound.Values)
            {
                if (!fileFound)
                {
                    filesFound = false;
                    break;
                }
            }

            return filesFound;
        }

        /* FindRunZip
         * looks for the run zip based on the archive folder and run number supplied
         * returns the full path to the run zip if found
         * returns empty string "" otherwise
         */
        public string FindRunZip (string archiveFolder, string runNumber)
        {
            string runZipPath = archiveFolder + "\\Run" + runNumber + ".zip";

            if(!File.Exists(runZipPath))
            {
                runZipPath = "";
            }

            return runZipPath;
        }
    }
}
