using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace LibLouisWrapperTestCmd
{
    class TestHandlerForDanish : TestHandler
    {
        internal static TestHandlerForDanish Create(string testInputDir)
        {
            return new TestHandlerForDanish(testInputDir);
        }

        internal override string GetTestSubject() { return "Danish  "; }

        TestHandlerForDanish(string testInputDir) : base(DanishBrailleGrade2, testInputDir) //  Danish table for 6 dots grade 2 forward and backward translation (2022)
        {}

        internal override TestResult ExecuteTests()
        {
            Log("+");
            if (!CheckWrapper())
            {
                testResult.Result = false;
                return testResult;
            }

            //string text = "The quick brown fox jumps over the lazy dog";
            string danishCharacters = "abcdefghijklmnopqrstuvwxyzæøå";

            for (int i = 0; ((testResult.Result) && (i < 1)); i++)
            {
                testResult.Result &= CharsToDotsToCharsTest(danishCharacters.ToLower());     // Seems NOT to handle Capital letters !
                testResult.Result &= StringToDotsToStringTest(danishCharacters);               // Seems to handle Capital letters !
 //               testResult.Result &= StringToDotsToStringTFETest(danishCharacters);             // Seems to handle Capital letters !       Disabled because it seems to cause strange errors        
            }

            // Run explicitly named testfiles
            testResult.Result &= RunTestFile(Path.Combine(testInputDir, "Danish.txt"));
            testResult.Result &= RunTestFile(Path.Combine(testInputDir, "DanishGraphics.txt")); // https://blind.dk/punktskrift-2022    Den danske punktskrift 2022    "÷" will fail       
            testResult.Result &= RunTestFile(Path.Combine(testInputDir, "SpecialCharacters.txt"));
            testResult.Result &= RunTestFile(Path.Combine(testInputDir, "EscapeSequences.txt"));


            OnEndOfTestFiles("Danish");

            Log("-");
            return testResult;
        }
    }
}
