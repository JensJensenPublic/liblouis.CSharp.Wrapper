using System;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.SqlServer.Server;
using static System.Net.Mime.MediaTypeNames;

namespace LibLouisWrapper
{
    /// <summary>
    /// Simple wrapper class for using the LibLouis library (LibLouis.dll) from C#
    /// Intensionally only exposes 8 public methods:
    ///  public static Wrapper Create()
    ///  public bool CharsToDots(string chars, out string dots)
    ///  public bool DotsToChars(string dots, out string chars)
    ///  public bool TranslateString(string text, out string dots)
    ///  public bool BackTranslateString(string inputDots, out string outputText)
    ///  public bool TranslateString(string text, out string dots, Typeforms[] sourceTypeformMap)
    ///  public bool BackTranslateString(string inputDots, out string outputText, Typeforms[] sourceTypeformMap)
    ///  public void Free()
    ///  
    /// More public methods can easily be added if needed. 
    /// 
    /// Some ideas were stolen from the GitHub project LibLouis.Net 
    /// Official LibLouis documentation is found at
    /// https://liblouis.io/documentation/liblouis.html
    /// 
    /// Other recommended reading:  
    /// https://github.com/liblouis/liblouis/issues/1280
    /// https://stackoverflow.com/questions/20857649/c-dll-import-throws-marshall-directive-exception-in-c-sharp  
    /// Official LibLouis documentation is found at
    /// https://liblouis.io/documentation/liblouis.html
    /// 
    /// 
    /// https://liblouis.io
    /// https://github.com/liblouis
    /// 
    /// Mail: liblouis-liblouisxml@freelists.org
    /// 
    /// </summary>

    /// <summary>
    /// As defined in liblouis.h
    /// </summary>
    public enum TypeformEnum : ushort
    {
        plain_text = 0x0000,
        italic = 0x0001,
        underline = 0x0002,
        bold = 0x0004,
        emph_4 = 0x0008,
        emph_5 = 0x0010,
        emph_6 = 0x0020,
        emph_7 = 0x0040,
        emph_8 = 0x0080,
        emph_9 = 0x0100,
        emph_10 = 0x0200,
        computer_braille = 0x0400,
        no_translate = 0x0800,
        no_contract = 0x1000,
        // SYLLABLE_MARKER_1  0x2000,
        // SYLLABLE_MARKER_1  0x4000
        // CAPSEMPH  0x4000
    }

    //const TypeformEnum italic = TypeformEnum.emph_1;
    //const TypeformEnum underline = TypeformEnum.emph_2;
    //const TypeformEnum bold = TypeformEnum.emph_3;


    [Flags]
    public enum OptionsEnum
    {
        None = 0,           // Use this for published versions!
        UseLogCallback = 1  // Use of the LibLouis LogCallback mechanism is konwn to cause nullreference exceptions turing heavy test and should only be used in a debug situation!
                            // The exceptionis probably caused by the Garbage Collector moving the delegate, but should of course be further investigated!
    }

    public class Wrapper : IDisposable
    {

        /// <summary>
        /// As defined in liblouis.h
        /// </summary>
        [Flags]
        public enum TranslationModeEnum
        {
            NoContractions = 1,
            CompbrlAtCursor = 2,
            DotsIO = 4,
            // for historic reasons 8 and 16 are free
            CompbrlLeftCursor = 32,
            UnicodeBraille = 64, // In liblouis.h: ucBrl = 64,
            NoUndefined = 128,
            PartialTrans = 256
        }

    

        private readonly int depricatedModeParameter = 0;        
        private static int globalLibLouisErrorCount = 0;
        /// <summary>
        /// // Counts errors reported from LibLouis dll and is used for checking the Logger Callback mechanism
        /// </summary>
        public static int GlobalLibLouisErrorCount { get { return globalLibLouisErrorCount; } }

        const int translationMode = (int)(TranslationModeEnum.NoUndefined | TranslationModeEnum.UnicodeBraille | TranslationModeEnum.DotsIO); // Common for all member functions
        const int translationMode1 = (int)(TranslationModeEnum.UnicodeBraille); // For experiment

        /// <summary>
        /// Path to be combined with tableName before passing to LibLouis
        /// Must contain the path to the conversion tables, relative to the path of LibLouis.dll.
        /// LibLouis.dll can find the exact absolute path to the tables using this information.
        /// </summary>
        private const string tableBase = @"liblouis\share\liblouis\tables";

        // The dll is placed in a folder named "binary" instead of "bin" to please the default GitExclude which wil not accept a "bin" folder.
        private const string LibLouisDll = @"Liblouis\binary\liblouis.dll";
        //private const string LibLouisDll = @"Liblouis\bin\liblouis.dll";

        #region LogCallBack
        private delegate void Func(int level, string message);
        private static void MyFunc(int level, string message)
        {
            globalLibLouisErrorCount++;
            if (ignoreError) return; // Do not log simulated  error generated for test-purposes !
            Log(string.Format(": Received callback from LibLouis, describing an error: Level={0} Message={1}", level, message));

        }
        #endregion

        #region DllImport
        [DllImport(LibLouisDll, CallingConvention = CallingConvention.StdCall)]
        private static extern int lou_charSize();

        // liblouis.h contains: LIBLOUIS_API const char *EXPORT_CALL lou_version(void);
        [DllImport(LibLouisDll, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string lou_version();

        [DllImport(LibLouisDll, CallingConvention = CallingConvention.StdCall)]
        private static extern int lou_charToDots(
            [In][MarshalAs(UnmanagedType.LPStr)] string tableList,
            [In][MarshalAs(UnmanagedType.LPArray)] byte[] inbuf,
            [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] outbuf,
            [In] int length,
            [In] int mode
        );

        [DllImport(LibLouisDll, CallingConvention = CallingConvention.StdCall)]
        private static extern int lou_dotsToChar(
        [In][MarshalAs(UnmanagedType.LPStr)] string tableList,
        [In][MarshalAs(UnmanagedType.LPArray)] byte[] inbuf,
        [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] outbuf,
        [In] int length,
        [In] int mode
        );

        [DllImport(LibLouisDll, CallingConvention = CallingConvention.StdCall)]
        private static extern void lou_free();
  
        [DllImport(LibLouisDll, CallingConvention = CallingConvention.StdCall)]
        private static extern void lou_registerLogCallback(Func callback);
       

        [DllImport(@"liblouis.dll", CharSet = CharSet.Unicode)]
        private static extern unsafe int lou_translateString(
                [In][MarshalAs(UnmanagedType.LPStr)] string tableList, // const char *tableList
                [In] byte[] inbuf,                                     // const widechar *inbuf
                [In, Out] IntPtr inlen,                                // int *inlen
                [Out] byte[] outbuf,                                   // widechar *outbuf 
                [In, Out] IntPtr outlen,                               // int *outlen  
                [In] TypeformEnum[] typeform,                          // formtype *typeform 
                [MarshalAs(UnmanagedType.LPStr)] string spacing,       // char *spacing
                int mode                                               //  int mode 
         );


        [DllImport(@"liblouis.dll", CharSet = CharSet.Unicode)]
        private static extern unsafe int lou_backTranslateString(
                [In][MarshalAs(UnmanagedType.LPStr)] string tableList, // const char *tableList
                [In] byte[] inbuf,                                     // const widechar *inbuf
                [In, Out] IntPtr inlen,                                // int *inlen
                [Out] byte[] outbuf,                                   // widechar *outbuf 
                [In, Out] IntPtr outlen,                               // int *outlen  
                [In,Out] TypeformEnum[] typeform,                      // formtype *typeform 
                [MarshalAs(UnmanagedType.LPStr)] string spacing,       // char *spacing
                int mode                                               //  int mode 
         );
        #endregion


        private enum NativeFunctionEnum
        {
            charsToDots,
            dotsToChars,
            translateString,       // Do NOT Use the TypeFormEnum parameter
            translateStringTfe,    // Use the TypeFormEnum parameter
            backTranslateString,   // Do NOT Use the TypeFormEnum parameter
            backTranslateStringTfe // Use the TypeFormEnum parameter
        }

        public bool CharsToDots(string chars, out string dots) { return CommonNativeCall(NativeFunctionEnum.charsToDots, chars, out dots); }
        public bool DotsToChars(string dots, out string chars) { return CommonNativeCall(NativeFunctionEnum.dotsToChars, dots, out chars); }
        public bool TranslateString(string text, out string dots) { return CommonNativeCall(NativeFunctionEnum.translateString, text, out dots); }
        public bool TranslateStringTFE(string text, out string dots, in TypeformEnum[] tfe) { return CommonNativeCall(NativeFunctionEnum.translateStringTfe, text, out dots, tfe, out TypeformEnum[] dummyTfe);}
        public bool BackTranslateString(string dots, out string text) { return CommonNativeCall(NativeFunctionEnum.backTranslateString, dots, out text); }
        public bool BackTranslateStringTFE(string dots, out string text, out TypeformEnum[] tfe) { return CommonNativeCall(NativeFunctionEnum.backTranslateStringTfe, dots, out text, null, out tfe); }

        public string GetVersion()
        {
#warning TODO Implement  using lou_version()          
            string result = "could not be determined !";
            // 
            // This is just a silly, temporary solution !
            try
            {
                //throw new Exception(""); // For test
                string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string fileName = Path.Combine(executingDirectory, @"liblouis\lib\pkgconfig\liblouis.pc");
                if (File.Exists(fileName))
                {
                    Log(string.Format(": Found file {0}", fileName));
                    string[] lines = File.ReadAllLines(fileName);
                    string versionPrompt = "Version:";
                    foreach (string line in lines)
                    {
                        if (line.StartsWith(versionPrompt))
                        {
                            return line.Replace(versionPrompt,"");
                        }                    
                    }
                }
            }
            catch (Exception e)
            {
                Log(string.Format(": Exception caught while attempting to read LibLouis version. Message={0}", e.Message));
            
            }
            return result;            
        }

        private int GetOutputLength(int inputLength, NativeFunctionEnum nativeFunctionEnum)
        {
            int defaultResult = Math.Max((inputLength * 2), 1024);  // Twice the inputbuffer size, but at least 1kB
            switch (nativeFunctionEnum)
            {
                case NativeFunctionEnum.charsToDots: break;
                case NativeFunctionEnum.dotsToChars: break;
                case NativeFunctionEnum.translateStringTfe: break;
                case NativeFunctionEnum.backTranslateStringTfe: break;
            }
            return defaultResult;
        }

        private byte[] CreateOutputBuffer(int inBufLength, NativeFunctionEnum nativeFunctionEnum)
        {
            int outputLength = GetOutputLength(inBufLength, nativeFunctionEnum);
            byte[] outBuf = new byte[outputLength];
            return outBuf;
        }


        private TypeformEnum[] CreateTfeBuffer(int inputLength, NativeFunctionEnum nativeFunctionEnum, TypeformEnum[] tfeInput)
        {         
            int length = GetTfeLength(inputLength, nativeFunctionEnum);
            TypeformEnum[] result = new TypeformEnum[length];
            if ((null != tfeInput) && (tfeInput.Length <= length))
            {
                Array.Copy(tfeInput, result, tfeInput.Length); // Copy to the common buffer to be passed to native code
            }
            return result;
        }

        private int GetTfeLength(int inputLength, NativeFunctionEnum nativeFunctionEnum)
        {
#warning TODO Find out why a smaller defaultBufferSize, for instance "defaultBufferSize =(inputLength * 2)" causes ctrange crashes !!
            int defaultTfeBufferSize = Math.Max(1024, (inputLength * 2)); // Twice as many Typeform items as input elements, but at least 1024
            switch (nativeFunctionEnum) 
            {
                case NativeFunctionEnum.translateStringTfe: return defaultTfeBufferSize;
                case NativeFunctionEnum.backTranslateStringTfe:return defaultTfeBufferSize;
            }
            return 0; // No buffer needed i these cases
        }

        /// <summary>
        /// The simple, common signature, used by all functions not using a Typeform parameter
        /// </summary>
        private bool CommonNativeCall(NativeFunctionEnum nativeFunctionEnum, string input, out string output)
        {
            if ((nativeFunctionEnum == NativeFunctionEnum.translateStringTfe) || (nativeFunctionEnum == NativeFunctionEnum.backTranslateStringTfe))
            {
                throw new ArgumentException(nativeFunctionEnum.ToString());
            }
            return CommonNativeCall(nativeFunctionEnum, input, out output, null, out TypeformEnum[]  dummyTfe);
        }

        /// <summary>
        /// The common, general signature, taking all possible input parameters. 
        /// By using this common signature we only need all the unsafe code and marchalling precautions at one single location
        /// </summary>
        /// <param name="nativeFunctionEnum">Identifies the native function to call</param>
        /// <param name="input">Input-string for the native function, either text or Braille</param>
        /// <param name="output">Output-string from the native function, either text or Braille</param>
        /// <param name="tfeInput">Optional TypeForm-input for the native function. May be null</param>
        /// <param name="tfeOutput">Optional TypeForm-output from the native function. May be null</param>
        /// <returns></returns>
        private bool CommonNativeCall(NativeFunctionEnum nativeFunctionEnum, string input, out string output, in TypeformEnum[] tfeInput, out TypeformEnum[] tfeOutput)
        {  
            int inputLength = input.Length;          
            byte[] inBuf = encoding.GetBytes(input);
            byte[] outBuf = CreateOutputBuffer(inBuf.Length, nativeFunctionEnum);          
            TypeformEnum[] tfeBuf = CreateTfeBuffer(input.Length, nativeFunctionEnum, tfeInput);
            int outputLength = outBuf.Length;
            int result = 0;
            unsafe
            {
                IntPtr inPtr = new IntPtr(&inputLength);
                IntPtr outPrt = new IntPtr(&outputLength);
                fixed (byte* pInBuf = inBuf, pOutBuf = outBuf) // Prevents GarbageCollector from moving the buffers
                {
                    fixed (TypeformEnum* pTfeBuf = tfeBuf) // Two levels are needed for fixing different types !
                    {
                        switch (nativeFunctionEnum)
                        {
                            case NativeFunctionEnum.charsToDots: result = lou_charToDots(tablePaths, inBuf, outBuf, inputLength, translationMode); break;
                            case NativeFunctionEnum.dotsToChars: result = lou_dotsToChar(tablePaths, inBuf, outBuf, inputLength, depricatedModeParameter); break;
                            case NativeFunctionEnum.translateString:        result = lou_translateString(tablePaths, inBuf, inPtr, outBuf, outPrt, null, null, translationMode); break;
                            case NativeFunctionEnum.translateStringTfe:     result = lou_translateString(tablePaths, inBuf, inPtr, outBuf, outPrt, tfeBuf, null, translationMode); break;
                            case NativeFunctionEnum.backTranslateString:    result = lou_backTranslateString(tablePaths, inBuf, inPtr, outBuf, outPrt, null, null, depricatedModeParameter); break;
                            case NativeFunctionEnum.backTranslateStringTfe: result = lou_backTranslateString(tablePaths, inBuf, inPtr, outBuf, outPrt, tfeBuf, null, depricatedModeParameter); break;
                        }
                        fixed (byte* pInBufAfter = inBuf, pOutBufAfter = outBuf)
                        {
                            CheckPinning("InBuf ", (int)pInBuf, (int)pInBufAfter);
                            CheckPinning("OutBuf", (int)pOutBuf, (int)pOutBufAfter);
                        }
                        fixed (TypeformEnum*  pTfeBufAfter = tfeBuf)
                        {   
                            CheckPinning("TfeBuf ", (int)pTfeBuf, (int)pTfeBufAfter);         
                        }
                    }
                }
            }
            output = null; 
            tfeOutput = null;
            if ((1 != result) && (!ignoreError)) return OnError( "1 != result");
            if ((1 == result) && (outputLength == outBuf.Length)) return OnLengthError(outputLength);
            if (null == outBuf) return OnError("null == outBuf");
            output = GetOutputString(nativeFunctionEnum, outBuf, outputLength, charSize);
            //Log(string.Format("({0},'{1}')='{2}'", nativeFunctionEnum, input, output));
            tfeOutput = GetOutputTypeForms(nativeFunctionEnum, tfeBuf, outputLength); 
            return true;
        }

        /// <summary>
        /// If the length of the outputbuffer received from native code is known we use that information.
        /// Otherwise we just remove any tariling null-vharacters.
        /// </summary>
        /// <param name="nativeFunctionEnum"></param>
        /// <param name="output"></param>
        /// <param name="outputLength"></param>
        /// <param name="charSize"></param>
        /// <returns></returns>
        private string GetOutputString(NativeFunctionEnum nativeFunctionEnum, byte[] output, int outputLength, int charSize)
        {
            string s;
            if (OutputLengthIsKnown(nativeFunctionEnum))
            {
                s = encoding.GetString(output, 0, outputLength * charSize); // Only use the relevalt part of the outputbuffer
            }
            else
            {
                s = encoding.GetString(output); // The whole outputbuffer
            }
            return s.TrimEnd(new char[] { '\0' }); // Remove all trailing null characters
        }


        private TypeformEnum[] GetOutputTypeForms(NativeFunctionEnum nativeFunctionEnum, TypeformEnum[] tfeBuf, int outputLength)
        {
            if (!TfeMustBeCopied(nativeFunctionEnum))
            {
                return new TypeformEnum[0];
            }
            int length = OutputLengthIsKnown(nativeFunctionEnum) ? outputLength : 0;
            TypeformEnum[] result = new TypeformEnum[length];
            Array.Copy(tfeBuf, result, length);
            Log(string.Format("(): Tfe.{0}", TfeToString(result)));
            return result;
        }


        private bool OutputLengthIsKnown(NativeFunctionEnum nativeFunctionEnum)
        {
            switch (nativeFunctionEnum)
            {
                case NativeFunctionEnum.translateString: return true;
                case NativeFunctionEnum.backTranslateString: return true;
                case NativeFunctionEnum.translateStringTfe: return true;
                case NativeFunctionEnum.backTranslateStringTfe: return true;
            }
            return false;
        }

        private bool TfeMustBeCopied(NativeFunctionEnum nativeFunctionEnum)
        {
            switch (nativeFunctionEnum)
            {
                case NativeFunctionEnum.translateStringTfe: return true;
                case NativeFunctionEnum.backTranslateStringTfe: return true;
            }
            return false;
        }



        private string TfeToString(TypeformEnum[] tfe)
        {
            if (null == tfe) return "null";
            StringBuilder sb = new StringBuilder();
            foreach (TypeformEnum t in tfe)
            {
                // When the buffer used for Typeform information in the call to native code is too small a crash seems to occur around here.
                // For this reason we split up in small steps to illustrate that the crash has to do with the use of native code, not with this method!
                int i = (int)t;
                string s = String.Format("0x{0:x} ", i); // Format as HEX
                sb.Append(s);   
            }
            return(string.Format("Length={0} HexValues={1}", tfe.Length, sb.ToString()));
        }

        private void CheckPinning(string id, int pBefore, int pAfter)
        {
            if (pBefore == pAfter)
            {
                // Log(string.Format(": Passed!"));
                return; 
            }
            string message = string.Format(": The buffer '{0}' changed from {1} to {2} during call to native code - even if it was supposed to be pinned!", id, pBefore, pAfter);
            Log(message);
            throw new Exception(message);
        }


        private bool OnLengthError(int outputLength)
        {
            // According to footnote 2 in documentation:
            // "When the output buffer is not big enough, lou_translateString returns a partial translation that is more or less accurate
            // up until the returned inlen/outlen, and treats it as a successful translation, i.e. also returns 1."
            return OnError(string.Format(" Result=1 but output may have been truncated to {0} characters to fit size of outputbuffer", outputLength));
        }

        private bool OnError(string s)
        {
            Log(string.Format(": Error: '{0}'", s));
            return false;        
        }

        public void Free()
        {
            lou_free();
            Log(string.Format(": Call to native method 'lou_free()' returned."));
        }

        public void UnregisterCallback()
        {
            lou_registerLogCallback(null);
            Log(string.Format(": Call to native method 'lou_registerLogCallback(null)' returned."));
        }


        private static void Log(string s)
        {
            Console.WriteLine(s);  // Replace with any logging mechanism             
        }

        /// <summary>
        /// Gets the encoding based on the character size from libluois
        /// </summary>
        /// <param name="size">Character size, 4 bytes is UTF-32, anything else is UTF-16</param>
        /// <returns>Character encoding</returns>
        private static Encoding GetEncoding(int size)
        {
            if (size == 4)
            {
                return Encoding.GetEncoding("UTF-32");
            }

            return Encoding.GetEncoding("UTF-16");
        }

        // Member variables:
        private readonly int charSize;
        private readonly Encoding encoding;
        private string tablePaths;
        private static bool ignoreError = false; // Used by the ExecuteCallbackTest() method for not logging simulated errors.
        private readonly bool useLogCallback = false;

        private string SaveCopy(string s)
        {
            if (null == s) return null;
            return string.Copy(s);        
        }


        /// <summary>
        /// Simple mechanism used by Constructor and  Dispose().
        /// Tests the LibLouis Log-Callback mechanism.
        /// </summary>
        private void ExecuteCallbackTest()
        {
            if (!useLogCallback) return;           
            string testItemName = " the LibLouis Log-Callback mechanism!";
            Log(string.Format(": Simulating error in order to test{0}",testItemName)); 
            
            int savedErrorCount = globalLibLouisErrorCount;         // Save   before test
            bool savedIgnoreError = ignoreError;            // Save   before test
            string savedTablePaths =  SaveCopy(tablePaths); // Save   before test

            ignoreError = true;                             // Modify before test
            tablePaths = Path.Combine(tableBase, "DoesNotExist.xxx"); //  Modify before test: Temporarily set up a nonexisting tablepath
            try
            {              
                bool b = CharsToDots("x", out string teststring); // Is expected to fail and thereby to increase globalErrorCount;
            }
            catch (Exception e)
            {
                Log(string.Format(": Exception thrown while calling CharsToDots(). Message='{0}'", e.Message));
            }
            bool ok = (globalLibLouisErrorCount > savedErrorCount);

            globalLibLouisErrorCount = savedErrorCount;       // Restore after test
            tablePaths = SaveCopy(savedTablePaths);   // Restore after test
            ignoreError = savedIgnoreError;           // Restore after test

            Log(string.Format(": TEST {0}! Simulated error was {1} reported from LibLouis by{2} !", ok ? "PASSED" : "FAILED", ok ? "": "NOT", testItemName));       
        }

     


        /// <summary>
        /// Only for preventing GC from collecting the delegate. MUST BE STATIC to keep the GC away !!
        /// See https://stackoverflow.com/questions/75223488/delegate-getting-gc-even-after-pinning
        /// </summary>
        private static readonly Func myFunc = MyFunc; 

  
        /// <summary>
        /// Private constructor. Use Wrapper.Create() from the outside.
        /// </summary>
        private Wrapper(string tableNames,OptionsEnum options)
        {
            Log(string.Format(": TableNames='{0}'", tableNames));
            this.useLogCallback = (0 != (options & OptionsEnum.UseLogCallback));
            if (useLogCallback)
            {       
                Log(string.Format(": Registering LibLouis LogCallback function"));
                lou_registerLogCallback(myFunc); // Register the static function MyFunc as a callback""
            }
            string version = GetVersion();
            Log(string.Format(": LibLouis Version {0}", version));
            charSize = lou_charSize();
            Log(string.Format(": CharSize={0}", charSize));
            encoding = GetEncoding(charSize);  // Get the encoding type based on the lou_charSize.
            Log(string.Format(": Encoding={0}", encoding.ToString()));

            // Check the Logging callback mechanism:
            //tablePaths = Path.Combine(tableBase, "DoesNotExist.xxx"); // Temporarily set up a nonexisting tablepath while checking
            ExecuteCallbackTest();  
            // Set up the real translation table
            tablePaths = Path.Combine(tableBase,tableNames); // According to the documentation only the first name needs to contain the tableBase !! 
            Log(string.Format(": Tables='{0}'", tablePaths));
        }

        /// <summary>
        /// Pevent use of default constructor
        /// </summary>
        private Wrapper(){ }

        private static bool OnCreationError(string s)
        {
            Log(s);
            return false;
        }

        public static bool DirectoryExists(string path)
        {
            if (Directory.Exists(path)) return true;
            return OnMissingItem("Directory", path);
        }

        private static bool FileExists(string path)
        {
            if (File.Exists(path)) return true;
            return OnMissingItem("File", path);
        }

        private static bool OnMissingItem(string itemType, string path)
        {
            Log(string.Format("{0} does not exist: '{1}'", itemType, path));
            return false;
        }

        /// <summary>
        /// Simple code for checking that all directories and files needed by liblouis are found at the right locations
        /// </summary>
        /// <param name="tableNames"></param>
        /// <returns></returns>
        private static bool CheckInstallation(string tableNames)
        {
            string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string liblouisDir = Path.Combine(executingDirectory, "liblouis");
            if (!DirectoryExists(liblouisDir)) return false;
            string binaryDir = Path.Combine(liblouisDir, "binary");
            if (!DirectoryExists(binaryDir)) return false;
            string liblouisDll = Path.Combine(binaryDir, "liblouis.dll");
            if (!FileExists(liblouisDll)) return false;
            string shareDir = Path.Combine(liblouisDir, "share");
            if (!DirectoryExists(shareDir)) return false;
            string libLouisDir2 = Path.Combine(shareDir, "liblouis");
            if (!DirectoryExists(libLouisDir2)) return false;
            string tablesDir = Path.Combine(libLouisDir2, "tables");
            if (!DirectoryExists(tablesDir)) return false;
      
            string[] names = tableNames.Split(',');
            {                          
                foreach (string name in names)
                {
                    // Only the first name contains the full path !
                    string shortName = Path.GetFileName(name);
                    string fullPath = (Path.Combine(tablesDir, shortName));
                    if (!FileExists(fullPath)) return false;                 
                }
            }
            Log(string.Format(": All tables in '{0}' were found", tableNames));
            return true;
        }


        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                ExecuteCallbackTest(); // Check that the GC has not moved the delegate while the native code has been holding its address.
                Free();                // Clear all tables
                UnregisterCallback();  // Prevent callbacks to delegate belonging to this object
                disposed = true;       // HAndles later async calls from the GC 
            }                   
        }


        public static Wrapper Create(string tableNames, OptionsEnum options)
        {
            if (! CheckInstallation(tableNames)) return null;        
            Wrapper wrapper =  new Wrapper(tableNames,options);
            return wrapper;
        }

    }
}
