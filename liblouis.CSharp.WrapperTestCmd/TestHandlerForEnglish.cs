using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibLouisWrapperTestCmd
{
    internal class TestHandlerForEnglish : TestHandler
    {
        internal static TestHandlerForEnglish Create(string testInputDir)
        {
            return new TestHandlerForEnglish(testInputDir);
        }

        TestHandlerForEnglish(string testInputDir) : base(UnifiedEnglishBrailleGrade2,testInputDir)
        {}

        internal override string GetTestSubject() { return "English "; }

        internal override TestResult ExecuteTests()
        {
            Log("+");
            if (!CheckWrapper())
            {
                testResult.Result = false;
                return testResult;
            }

            string englishCharacters = "abcdefghijklmnopqrstuvwxyz"; // No æøå
            for (int i = 0; ((testResult.Result) && (i < 1)); i++)
            {
                testResult.Result &= CharsToDotsToCharsTest(englishCharacters.ToLower());     // Seems NOT to handle Capital letters !
                testResult.Result &= StringToDotsToStringTest(englishCharacters);             // Seems to handle Capital letters !
                //testResult.Result &= StringToDotsToStringTFETest(englishCharacters);        // Seems to handle Capital letters !         Disabled because it seems to cause strange errors           
            }

            testResult.Result &= RunTestFile(Path.Combine(testInputDir, "EscapeSequences.txt"));
            testResult.Result &= RunTestFile(Path.Combine(testInputDir, "SpecialCharacters.txt")); // ";" will fail !
            testResult.Result &= RunTestFile(Path.Combine(testInputDir, "English.txt")); // Through the mirror


            OnEndOfTestFiles("English");
            Log("-");
            return testResult;
            
        }

    }
}
