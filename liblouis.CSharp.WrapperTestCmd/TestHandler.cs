using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using liblouis.CSharp.Wrapper;
using liblouis.CSharp.WrapperTestCmd;

#pragma warning disable IDE0018 // Inline variable declaration

namespace LibLouisWrapperTestCmd
{


    internal abstract class TestHandler : IDisposable, IClient
    {
        protected Wrapper libLouisWrapper;

        //protected bool testResult = true; // Untill an error id found
        protected string testInputDir;
        protected string tableName;

        internal static readonly string UnifiedEnglishBrailleGrade2 = "en-ueb-g2.ctb";
        internal static readonly string DanishBrailleGrade2 = "da-dk-g26.ctb";
        internal static readonly string GermanBrailleGrade2 = "de-g2-detailed.ctb";


        protected TestResult testResult = TestResult.Create();

        private string currentTestFileName = "";
        internal string CurrentTestFileName { get { return currentTestFileName; } }

        internal abstract TestResult ExecuteTests();


        internal abstract string GetTestSubject(); 

        internal int GlobalLibLouisErrorCount {get{ return Wrapper.GlobalLibLouisErrorCount; } }

        public void Dispose()
        {     
            libLouisWrapper.Dispose();                 
               
        }

        protected void Log(string message)
        {
            string cm = Utilities.GetCallingMethod(0);
            Console.WriteLine(string.Format("{0}{1}", cm, message));  
        }

        /// <summary>
        /// Called by the Wrapper on its own initiative
        /// </summary>
        /// <param name="message"></param>
        public void OnWrapperLog(string message)
        {
            string cm = Utilities.GetCallingMethod(3);
            Console.WriteLine(string.Format("{0}{1}",cm,message));        
        }

        /// <summary>
        /// Called by the Wrapper on behalf of the native LibLouis code through the LibLouis Callback mechanism
        /// </summary>
        /// <param name="message"></param>
        public void OnLibLouisLog(string message)
        {
            string cm = Utilities.GetCallingMethod(2);
            Console.WriteLine(string.Format("{0}{1}", cm, message));   
        }

        protected TestHandler(string tableName, string testInputDir)
        {
            this.testInputDir = testInputDir;
            this.tableName = tableName; 
            libLouisWrapper = Wrapper.Create(tableName, OptionsEnum.UseLogCallback, this as IClient); //  Danish table for 6 dots grade 2 forward and backward translation (2022) 
        }

        protected bool CheckWrapper()
        {
            if (null == libLouisWrapper)
            {
                Log(string.Format(": LibLouis directory or file is missing. Please see logfile for details."));
                return false;
            }
            return true;
        }

        protected void FreeWrapper()
        {
            libLouisWrapper.Free();
        }


        /// <summary>
        /// Tests the roundtrip CharsToDots, DotsToChars
        /// </summary>
        /// <param name="text">The string to take through the roundtrip</param>
        /// <returns>True <==> success</returns>
        protected bool CharsToDotsToCharsTest(string text)
        {
            string dots;
            bool ok;
            ok = libLouisWrapper.CharsToDots(text, out dots);
            Log(FormatTranslateResult("CharsToDot", text, ok, dots));

            string newText;
            ok = libLouisWrapper.DotsToChars(dots, out newText);
            Log(FormatTranslateResult("DotsToChar", dots, ok, newText));

            bool equal = (0 == string.Compare(text, newText));
            string message = string.Format("DotsToChars(CharsToDots(text)) {0} text", equal ? "==" : "<>");
            Log(": " + message);
            if (!equal)
            {
                testResult.ErrorList.Add(message);
            }
            return equal;
        }

  

        protected bool StringToDotsToStringTest(string text)
        {
            string dots;
            bool ok;

            ok = libLouisWrapper.TranslateString(text, out dots);
            Log(FormatTranslateResult("TranslateString", text, ok, dots));

            string newText;
            ok = libLouisWrapper.BackTranslateString(dots, out newText);
            Log(FormatTranslateResult("BackTranslateString", dots, ok, newText));

#if true
            text = text.TrimStart('\t'); // Remove all leading tabs
            newText = newText.TrimStart(' '); // Remove all leading spaces
#endif


            bool equal = (0 == string.Compare(text, newText));

            string messageStart = string.Format("BackTranslateString(TranslateString(text))[{0}] {1} text[{2}]", text.Length, equal ? "==" : "<>", newText.Length);
            string messageForLog;
            if (equal)
            {
                messageForLog = string.Format("{0}='{1}'", messageStart, text); // Report successes in one line
                testResult.Successes++;
            }
            else
            {
                string diffReport = GetDiffReport(text, newText);
                messageForLog = string.Format("{0}: {1}\r\n{2}\r\n{3}", messageStart, diffReport, text, newText); // Report failures in 3 lines
                string messageForList = string.Format("{0}: {1}", messageStart, diffReport); // Report failures in 1 line
                testResult.ErrorList.Add(messageForList);
            }
            Log(": " + messageForLog);
            return equal;
        }

        private  string GetDiffReport(string t0, string t1)
        {
            if ((string.IsNullOrEmpty(t0)) || (string.IsNullOrEmpty(t1))) return "At least one argument is null or empty";
            int minLength = Math.Min(t0.Length, t1.Length);
            for (int i = 0; i < minLength; i++)
            {
                if (t0[i] != t1[i])
                {
                    char c0 = t0[i];
                    char c1 = t1[i];
                    string diff = string.Format("(Chars:'{0}' <> '{1}')   (Integers:{2} <> {3})", c0, c1, (int)c0, (int)c1);
                    // Get a variety of information for generating the string describing the difference
                    string cf = Utilities.GetCallingMethod(0); // ClassName and FunctionName for the method calling GetDiffReport
                    string subject = this.GetTestSubject();
                    string fileName = Path.GetFileName(CurrentTestFileName); 
                    testResult.AllDiffs.Add(cf + " " + subject + " " + fileName + " " + diff);
                    return string.Format("First diff found at index {0}: {1}", i, diff);

                }
            }
            return "No difference found";
        }

        private string FormatTranslateResult(string method, string input, bool result, string output)
        {
            return string.Format(": {0}('{1}') returned {2}. OutPut[{3}]='{4}') ", method, input, result, output.Length, output);
        }

        protected bool RunTestFile(string fullFileName)
        {
            this.currentTestFileName = fullFileName; 
            bool result = true;
            Log(string.Format("\r\n\r\n>>>>>>>>>>TestFileName='{0}'<<<<<<<<<<\r\n", Path.GetFileName(fullFileName)));
            string[] lines = File.ReadAllLines(fullFileName);
            foreach (string line in lines)
            {
                result &= StringToDotsToStringTest(line); // StringToDotsToStringTestTFE(texy) fails with text="012345678abcdefghijklmnopqrstuvwxyzæøåABCDEFGHIJKLMNOPQRSTUV"
            }
            return result;
        }


        protected void OnEndOfTestFiles(string language)
        {
            Log(string.Format(": >>>>>>>>>>>>>>>>>>>> End of testFiles for {0}. Test {1} <<<<<<<<<<<<<<<<<<<<", language, testResult.Result ? "PASSED" : "FAILED"));
            if (!testResult.Result)
            {
                // In case of errors report any error information:
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format(": {0} Error{1} detected:", testResult.ErrorList.Count, (1 == testResult.ErrorList.Count) ? "" : "s"));
                foreach (string error in testResult.ErrorList)
                {
                    sb.AppendLine("  " + error);
                }
                string logString = sb.ToString();
                Log(logString.TrimEnd(new char[] { '\r', '\n' })); // Remove trailing cr lf
              
            }
        }
    }
}
