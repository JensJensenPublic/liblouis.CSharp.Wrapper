﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.NetworkInformation;

namespace LibLouisWrapperTestCmd
{
    internal class Program
    {
        static string testInputDir;

        static private void Log(string s)
        {
            Console.WriteLine(s);   
        }
        private static bool CheckTestFileInstallation()
        {
            string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            testInputDir = Path.Combine(executingDirectory, "TestInputFiles");
            if (!DirectoryExists(testInputDir)) return false;
            string[] testFiles = Directory.GetFiles(testInputDir);
            Log(string.Format(": Found {0} testfiles in {1}:", testFiles.Length, testInputDir));
            foreach (string testFile in testFiles)
            {
                Log(string.Format("   {0}", Path.GetFileName(testFile)));
            }
            return true;
        }

        private static bool DirectoryExists(string directoryName)
        { 
            if (Directory.Exists(directoryName)) return true;
            Log(string.Format(": Directory does not exist: '{0}'", directoryName));
            return false;
        }
    

        static TestResult localDanishResult;
        static TestResult localEnglishResult;
        static TestResult localGermanResult;
        static TestResult localTypeFormResult;

        static TestResult overallTestResult; // For collecting results of all test

        static void Main(string[] args)
        {  
            Log(": ---------------------------------------------------");
            Log(string.Format(": Starting {0}",Environment.CommandLine.ToString()));
            Log(string.Format("Setting Console.OutputEncoding to {0} in order do display Braille symbols",Encoding.Unicode));
            Console.OutputEncoding = Encoding.Unicode; 

            if (!CheckTestFileInstallation()) return; // No reason to continue

            int overallTestLoops = 0;          
            int overallLibLouisErrorCount = 0;
            overallTestResult = TestResult.Create();
            try
            {
                for (int i = 0; i < 1; i++) // Prepare for "endurance" test
                {
#if true
                    // Test for handling FormTypeForms
                    using (TestHandler testHandler = TestHandlerForTypeForm.Create(TestHandler.UnifiedEnglishBrailleGrade2,testInputDir))
                    {
                        localTypeFormResult = testHandler.ExecuteTests(); // The "using" clause will cause a call to Dispose()
                        overallTestResult.AddRange(localTypeFormResult);
                        overallLibLouisErrorCount += testHandler.GlobalLibLouisErrorCount;
                    }
#endif

#if true
                    // Test for handling FormTypeForms, now using danish translation tables
                    using (TestHandler testHandler = TestHandlerForTypeForm.Create(TestHandler.DanishBrailleGrade2, testInputDir))
                    {
                        localTypeFormResult = testHandler.ExecuteTests(); // The "using" clause will cause a call to Dispose()
                        overallTestResult.AddRange(localTypeFormResult);
                        overallLibLouisErrorCount += testHandler.GlobalLibLouisErrorCount;
                    }
#endif

#if true
                    // Test for handling FormTypeForms, now using german translation tables
                    using (TestHandler testHandler = TestHandlerForTypeForm.Create(TestHandler.GermanBrailleGrade2, testInputDir))
                    {
                        localTypeFormResult = testHandler.ExecuteTests(); // The "using" clause will cause a call to Dispose()
                        overallTestResult.AddRange(localTypeFormResult);
                        overallLibLouisErrorCount += testHandler.GlobalLibLouisErrorCount;
                    }
#endif
                    

#if false
                    using (TestHandler testHandler = TestHandlerForDanish.Create(testInputDir))
                    {
                        localDanishResult = testHandler.ExecuteTests(); // The "using" clause will cause a call to Dispose()
                        overallTestResult.AddRange(localDanishResult);                    
                        overallLibLouisErrorCount += testHandler.GlobalLibLouisErrorCount;
                    }
#endif
#if true
                    using (TestHandler testHandler = TestHandlerForEnglish.Create(testInputDir))
                    {
                        localEnglishResult = testHandler.ExecuteTests(); // The "using" clause will cause a call to Dispose()
                        overallTestResult.AddRange(localEnglishResult);
                        overallLibLouisErrorCount += testHandler.GlobalLibLouisErrorCount;
                    }
#endif

#warning Add other languages here! 

                    overallTestLoops++;
                }

                Log(string.Format(": No Exception was thrown during test."));
            }
            catch (Exception e)
            {
                Log(string.Format(": Main() failed because of an exception!  Exception.Message='{0}'", e.Message));                         
            }
            TestResult otr = overallTestResult; // Just to reduce amount of text
            Log(string.Format(": Test completed: TestLoops={0} Successes={1} Errors={2} Differences={3}", overallTestLoops,   otr.Successes, otr.ErrorList.Count, otr.AllDiffs.Diffs.Count));
            Log(string.Format(": Number of errors reported by LibLouis={0}", overallLibLouisErrorCount));

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}