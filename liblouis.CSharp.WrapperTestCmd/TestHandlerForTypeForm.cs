using LibLouisWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LibLouisWrapperTestCmd
{
    internal class TestHandlerForTypeForm : TestHandler
    {
        //**************************************************************************************
        //
        // https://www.pathstoliteracy.org/ueb-lesson-4-typeform-indicators-used-ueb/
        //
        //                 Italics    Underline   Bold
        // Single letter   46,23      456,23      45,23
        // Word            46,2       456,2       45,2  
        // Passage begin   46,2356    456,2356    45,2356
        // Passege end     46,3       456,3       45,3
        //
        //**************************************************************************************

  

        internal static TestHandlerForTypeForm Create(string table,string testInputDir)
        {
            return new TestHandlerForTypeForm(table,testInputDir);
        }

        TestHandlerForTypeForm(string table, string testInputDir) : base(table, testInputDir)
        { }

        private bool TranslateStringTFE(string text, out string dots, TypeformEnum[] tfs)
        {
            bool result = libLouisWrapper.TranslateStringTFE(text, out dots, tfs);
            StringBuilder sb = new StringBuilder();
            foreach (TypeformEnum ft in tfs)
            {
                sb.Append(ft.ToString() + " ");
            }
            Log(string.Format(": TranslateStringTFE({0},[ {1}]) returned Dots[{2}]={3}", text, sb.ToString(), dots.Length, dots));
            return result;
        }

        // Some simple shorthands to reduce amount of text:
        public const TypeformEnum Plain = TypeformEnum.plain_text;
        public const TypeformEnum Italic = TypeformEnum.italic;
        public const TypeformEnum Underline = TypeformEnum.underline;
        public const TypeformEnum Bold = TypeformEnum.bold;

        private TypeformEnum[] repeatedTypeformEnum(TypeformEnum tfe, int count)
        {

            TypeformEnum[] result = new TypeformEnum[count];
            for (int i = 0; (i < count); i++)
            {
                result[i] = tfe;
            }
            return result;
        }


        /// <summary>
        /// Tests the roundtrip TranslateString, BackTranslateString
        /// </summary>
        /// <param name="text">The string to take through the roundtrip</param>
        /// <returns>True <==> success</returns>
        protected bool StringToDotsToStringTFETest(string text, TypeformEnum[] typeForms)
        {
            string dots;
            bool ok;

            ok = this.TranslateStringTFE(text, out dots, typeForms); // Add logging info before calling LibLouisWrapper
            Log(FormatTranslateResultTFE("TranslateStringTFE", text, ok, dots, typeForms));

            string newText;
            TypeformEnum[] typeFormsBack;
            ok = libLouisWrapper.BackTranslateStringTFE(dots, out newText, out typeFormsBack);
#warning todo  Add logging info before calling LibLouisWrapper
            Log(FormatTranslateResultTFE("BackTranslateStringTFE", dots, ok, newText, typeFormsBack));

            bool equal = (0 == string.Compare(text, newText));
            string message = string.Format(": {0} BackTranslateStringTFE(TranslateStringTFE(text)) {1} text", equal ? "PASSED" : "FAILED", equal ? "==" : "<>");
            Log(message);
            if (!equal)
            {
#warning TODO Fix next line
                //testResult.ErrorList.Add(Logger.GetCF(message));
            }
            return equal;
        }


        private string FormatTranslateResultTFE(string method, string input, bool result, string output, TypeformEnum[] tfe)
        {
            return string.Format(": {0}('{1}') returned {2}. Tfe.Length={3} Output[{4}]={5}) ", method, input, result, tfe, output.Length, output);
        }


        internal override TestResult ExecuteTests()
        {  
            Log("+");
            if (!CheckWrapper())
            {
                testResult.Result = false;
                return testResult;
            }

#if false
            string dots;      

            Log(": A single LETTER with 5 different fonttypes");
            testResult.Result &= TranslateStringTFE("a", out dots, new TypeformEnum[] { Plain });        
            testResult.Result &= TranslateStringTFE("a", out dots, new TypeformEnum[] { Italic });
            testResult.Result &= TranslateStringTFE("a", out dots, new TypeformEnum[] { Underline });
            testResult.Result &= TranslateStringTFE("a", out dots, new TypeformEnum[] { Bold });

            Log(": A 4-letter WORD with 4 different fonttypes");
            testResult.Result &= TranslateStringTFE("aaaa", out dots, new TypeformEnum[] { Plain, Italic, Underline, Bold });
            Log("A 4-letter WORD with identical fonttypes");
            testResult.Result &= TranslateStringTFE("aaaa", out dots, new TypeformEnum[] { Plain, Plain, Plain, Plain });
            testResult.Result &= TranslateStringTFE("aaaa", out dots, new TypeformEnum[] { Italic, Italic, Italic, Italic });
            testResult.Result &= TranslateStringTFE("aaaa", out dots, new TypeformEnum[] { Underline, Underline, Underline, Underline });
            testResult.Result &= TranslateStringTFE("aaaa", out dots, new TypeformEnum[] { Bold, Bold, Bold, Bold });

            Log(": A 4-word PASSAGE with identical fonttypes");
            string passage = "aaaa aaaa aaaa aaaa";
            testResult.Result &= TranslateStringTFE(passage, out dots, repeatedTypeformEnum(Plain, passage.Length));
            testResult.Result &= TranslateStringTFE(passage, out dots, repeatedTypeformEnum(Italic, passage.Length));
            testResult.Result &= TranslateStringTFE(passage, out dots, repeatedTypeformEnum(Underline, passage.Length));
            testResult.Result &= TranslateStringTFE(passage, out dots, repeatedTypeformEnum(Bold, passage.Length));
#endif

            // Repeat the tests above, now transforming to dots and back - and comparing the results
            testResult.Result &= StringToDotsToStringTFETest("a", new TypeformEnum[] { Plain });
            testResult.Result &= StringToDotsToStringTFETest("a", new TypeformEnum[] { Italic });
            testResult.Result &= StringToDotsToStringTFETest("a", new TypeformEnum[] { Underline });
            testResult.Result &= StringToDotsToStringTFETest("a", new TypeformEnum[] { Bold });



            Log("-");
            return testResult;            
        }

    }
}
