using CTGAUAardQuadTest.DataClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Spartan.Core; 
using iText.Kernel.Pdf.Xobject;
using Path = System.IO.Path;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Geom;

namespace CTGAUAardQuadTest.Models.BGRAardvarkPORegression
{
    class CompareSamples
    {
        private readonly ScriptContext _scriptContext;

        public CompareSamples(ScriptContext scriptContext)
        {
            this._scriptContext = scriptContext;
        }

        public void comparePDFs()
        {
            string beforeSamples = _scriptContext.BaseLineSamples.BaselineSamplesFolder;
            string afterSamples = _scriptContext.samplesFolder;

            beforeSamples = @"C:\Before";
            afterSamples = @"C:\After";

            //string diffSamples = afterSamples + "\\diff";

            string diffSamples = @"C:\Diff\diff";

            if (!Directory.Exists(diffSamples))
            {
                Directory.CreateDirectory(diffSamples);
            }

            //get pdf files from folder
            string[] beforePDFs = Directory.GetFiles(beforeSamples, "*.pdf");
            string[] afterPDFs = Directory.GetFiles(afterSamples, "*.pdf");

            //ensure pdf files are compared in correct order
            Array.Sort(beforePDFs);
            Array.Sort(afterPDFs);

            //loop through each pdf files

            for (int i = 0; i < beforePDFs.Length && i < afterPDFs.Length; i++)
            {
                string beforePDF = beforePDFs[i];
                string afterPDF = afterPDFs[i];

                string pdfName = Path.GetFileNameWithoutExtension(beforePDF);

                //create comparedpdf and text log file
                string diffPDF = Path.Combine(diffSamples, pdfName + ".pdf");
                string cliOutPut = Path.Combine(diffSamples, pdfName + ".txt");

                //Calling Pdiffy 
                Process process = new Process();
                process.StartInfo.FileName = Environment.GetEnvironmentVariable("CCS_RESOURCE") + @"\global\scripts\pdiffy.cmd";
                process.StartInfo.Arguments = string.Join(" ", "--sbs", beforePDF, afterPDF, diffPDF);

                TestDriver.ResultsLog.LogComment("Running '" + process.StartInfo.FileName + "' with arguments " + process.StartInfo.Arguments);

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                StreamReader reader = process.StandardOutput;
                process.WaitForExit();

                //start to reading CLI output
                string line;
                bool differencesLineRead = false;
                bool isTestPass = true;

                string patterns = @".*pages are different.*|.*pages are removed.*|.*pages are inserted.*";

                while ((line = reader.ReadLine()) != null)
                {
                    //generate content for CLI output text file
                    File.AppendAllText(cliOutPut, line + Environment.NewLine);

                    Match m = Regex.Match(line, patterns, RegexOptions.IgnoreCase);

                    if (m.Success)
                    {
                        differencesLineRead = true;

                        string[] text = line.Split('\t');

                        if (Convert.ToInt32(text[0]) != 0)
                        {
                            //if not equal to zero, test fail by default
                            isTestPass = false;

                            if (m.Value.Contains("pages are different"))
                            {
                                //start to text compare if found difference
                                var isTextIdentical = pdfTextCompare(beforePDF, afterPDF);

                                //add condition to compare images
                                var isImageIdentical = pdfImagesCompare(beforePDF, afterPDF);

                                if (isTextIdentical && isImageIdentical)
                                {
                                    isTestPass = true;
                                }
                            }
                        }
                    }
                }

                if (isTestPass)
                {
                    TestDriver.ResultsLog.LogPass($"There are no differences when comparing baseline {beforePDF} with sample {afterPDF} ");
                }
                else
                {
                    TestDriver.ResultsLog.LogFail($"There are differences when comparing baseline {beforePDF} with sample {afterPDF}");
                }

                if (!differencesLineRead)
                {
                    TestDriver.ResultsLog.LogFail("Did not find a line in CLI Output indicating difference count.");
                }

                TestDriver.ResultsLog.LogStep("Result");
            }
        }

        /// <summary>
        /// This method is to extract text from pdf for comparison
        /// Additionally, it will exclude comparing certain text eg: date, runnumber 
        /// </summary>
        public bool pdfTextCompare(string pdf1, string pdf2)
        {
            //create two pdf document instance
            PdfDocument pdfDoc1 = new PdfDocument(new PdfReader(pdf1));
            PdfDocument pdfDoc2 = new PdfDocument(new PdfReader(pdf2));

            bool isIdentical = true;

            //compare the text content of each page in the two PDF files 
            for (int pageNum = 1; pageNum <= pdfDoc1.GetNumberOfPages(); pageNum++)
            {
                var pdftext1 = PdfTextExtractor.GetTextFromPage(pdfDoc1.GetPage(pageNum));

                var pdftext2 = PdfTextExtractor.GetTextFromPage(pdfDoc2.GetPage(pageNum));

                //get runNumber from both PDFs
                var runNum1 = GetRunNumberFromText(pdftext1);
                var runNum2 = GetRunNumberFromText(pdftext2);

                //remove runNumber from both texts before comparing them
                if (runNum1 != null & runNum2 != null)
                {
                    pdftext1 = RemoveRun(pdftext1, runNum1);
                    pdftext2 = RemoveRun(pdftext2, runNum2);
                }

                //remove date from both texts before comparing them
                pdftext1 = RemoveDate(pdftext1);
                pdftext2 = RemoveDate(pdftext2);

                //remove new line characters
                var pdftext1Formated = pdftext1.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
                var pdftext2Formated = pdftext2.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");

                //start to compare both textes
                if (!pdftext1Formated.Equals(pdftext2Formated))
                {
                    isIdentical = false;
                    break;
                }
            }

            pdfDoc1.Close();
            pdfDoc2.Close();

            return isIdentical;
        }

        public bool pdfImagesCompare(string pdf1, string pdf2)
        {

            var isImageIdentical = false;

            //create two pdf document instance
            PdfDocument pdfDoc1 = new PdfDocument(new PdfReader(pdf1));
            PdfDocument pdfDoc2 = new PdfDocument(new PdfReader(pdf2));

            List<byte[]> list1 = new List<byte[]>();
            List<byte[]> list2 = new List<byte[]>();

            List<float> size1 = new List<float>();
            List<float> size2 = new List<float>();

            //compare the text content of each page in the two PDF files 
            for (int pageNum = 1; pageNum <= pdfDoc1.GetNumberOfPages(); pageNum++)
            {
                //get the page's resources dictionary
                PdfResources resourcesPdf1 = pdfDoc1.GetPage(pageNum).GetResources();

                PdfResources resourcesPdf2 = pdfDoc2.GetPage(pageNum).GetResources();
                
                //get the page's graphical Object dictionary
                PdfDictionary xobjects1 = resourcesPdf1.GetResource(PdfName.XObject);
                PdfDictionary xobjects2 = resourcesPdf2.GetResource(PdfName.XObject);


                if (xobjects1 == null && xobjects2 == null)
                {
                    isImageIdentical = true;
                    continue;
                    
                }

                if (xobjects1 == null || xobjects2 == null)
                {
                    isImageIdentical = false;
                    TestDriver.ResultsLog.LogFail("one pdf has image, the other pdf doesn't have");
                    TestDriver.ResultsLog.LogStep("Result");
                    break;
                }
                var counter1 = 1;
                var counter2 = 1;

                //Loop through each Xobject in the dictionary
                foreach (PdfName name in xobjects1.KeySet())
                {
                    //get the pdfstream info
                    PdfStream xobject = (PdfStream)xobjects1.Get(name);

                    //check if the XObject is an image
                    if (xobject.Get(PdfName.Subtype).Equals(PdfName.Image))
                    {
                        var imageData = new PdfImageXObject(xobject).GetImageBytes();
                        var height = new PdfImageXObject(xobject).GetHeight();
                        size1.Add(height);
                        var width = new PdfImageXObject(xobject).GetWidth();
                        size1.Add(width);
                        counter1++;
                        list1.Add(imageData);
                    }
                }

                foreach (PdfName name in xobjects2.KeySet())
                {
                    //get the XObject 
                    PdfStream xobject = (PdfStream)xobjects2.Get(name);

                    //check if the XObject is an image
                    if (xobject.Get(PdfName.Subtype).Equals(PdfName.Image))
                    {
                        var imageData = new PdfImageXObject(xobject).GetImageBytes();
                        var height = new PdfImageXObject(xobject).GetHeight();
                        size2.Add(height);
                        var width = new PdfImageXObject(xobject).GetWidth();
                        size2.Add(width);
                        counter2++;
                        list2.Add(imageData);
                    }
                }

                //compare images by bytes to see if they are the same
                var isByteIdentical = ListofByteCompare(list1, list2);

                //compare image width/height to see if they are the same
                var isSizeIdentical = size1.SequenceEqual(size2);

                //compare image coordinates to see if they are in the same spot
                bool isCoordinateIdentical = CompareImageCoordinates(pageNum, pdfDoc1, pdfDoc2);

                if (!isByteIdentical || !isSizeIdentical || !isCoordinateIdentical)
                {
                    TestDriver.ResultsLog.LogFail(
                        "Images on baseline pdf or sample pdf are either not the same, are different in size or have different positions");
                    TestDriver.ResultsLog.LogStep("Result");
                    isImageIdentical = false;
                    break;
                }

                isImageIdentical = true;

            }

            pdfDoc1.Close();
            pdfDoc2.Close();

            return isImageIdentical;
        }

        //get image coordinates of all images on a given page for 2 pdfs, then check that they are the same
        //return true if all the same
        //return false otherwise
        private bool CompareImageCoordinates(int pageNum, PdfDocument pdfDoc1, PdfDocument pdfDoc2)
        {
            bool isCoordinateIdentical = true;

            List<Vector> imageCoordinates1 = new List<Vector>();
            MyImageRenderListener listener1 = new MyImageRenderListener(imageCoordinates1);
            PdfCanvasProcessor parser1 = new PdfCanvasProcessor(listener1);

            parser1.ProcessPageContent(pdfDoc1.GetPage(pageNum));

            List<Vector> imageCoordinates2 = new List<Vector>();
            MyImageRenderListener listener2 = new MyImageRenderListener(imageCoordinates2);
            PdfCanvasProcessor parser2 = new PdfCanvasProcessor(listener2);

            parser2.ProcessPageContent(pdfDoc2.GetPage(pageNum));


            if (imageCoordinates1.Count != imageCoordinates2.Count)
            {
                //the number of image coordinates don't match
                //so return false
                isCoordinateIdentical = false;
            }
            else if (imageCoordinates1.Count > 0)
            {
                //comparing all coordinates
                for (int index = 0; index < imageCoordinates1.Count; ++index)
                {
                    if (
                        //comparing x coordinate
                        imageCoordinates1[index].Get(0) != imageCoordinates2[index].Get(0)
                        ||
                        //comparing y coordinate
                        imageCoordinates1[index].Get(1) != imageCoordinates2[index].Get(1)
                    )
                    {
                        //x and/or y coordinates are not the same, return false
                        isCoordinateIdentical = false;
                        break;
                    }
                }
            }

            return isCoordinateIdentical;
        }

        /// <summary>
        /// This method is to get RunNumber from text
        /// </summary>
        public string GetRunNumberFromText(string pdfText)
        {
            string pattern = @"Run No:\s*(\d+)";
            Regex regex = new Regex(pattern, RegexOptions.Compiled);
            Match m = regex.Match(pdfText);
            if (m.Success)
            {
                return m.Groups[1].Value;
            }
            return null;
        }

        /// <summary>
        /// This method is to remove RunNumber from text 
        /// </summary>
        public string RemoveRun(string text, string runNumber)
        {
            string pattern = runNumber;
            return Regex.Replace(text, pattern, string.Empty);
        }

        /// <summary>
        /// This method is to remove date and time from text
        /// </summary>
        private string RemoveDate(string text)
        {
            string pattern = @"\s*\d{1,2}/\d{1,2}/\d{4}\s*\d{1,2}:\d{2}";
            
            Match m = Regex.Match(text, pattern);
            string newText = "";
            if (m.Success)
            {
                DateTime dt = DateTime.ParseExact(m.Value.Trim(), "d/M/yyyy  H:m", CultureInfo.InvariantCulture);
                var monthYear = dt.ToString("MMMyyyy");
                newText= Regex.Replace(text.Replace(monthYear, ""), pattern, string.Empty);
            }
            else
            {
                newText = text;
            }
            return newText;
        }

        public bool ListofByteCompare(List<byte[]> list1, List<byte[]> list2)
        {
            bool equal = true;

            if (list1.Count != list2.Count)
            {
                equal = false;
            }
            else
            {
                for (int i = 0; i < list1.Count; i++)
                {
                    if (!ByteArraysEqual(list1[i], list2[i]))
                    {
                        equal = false;
                        break;
                    }
                }
            }
            return equal;
        }

        public bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
