using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace LibLouisWrapperTestCmd
{
    /// <summary>
    /// Class for handling all platform dependencies at one place.
    /// Must exist for each platform.
    /// This variant makes use of the standard functionality only
    /// </summary>
    internal class PlatformDependencies
    {
        /// <summary>
        /// Dummy implementation. No logfile generated, Output only to Console.
        /// </summary>
        /// <param name="path"></param>
        static internal void OpenLogFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return; 
            Console.WriteLine(string.Format("Logger.Open({0})",path));  
        }

        static internal void Log(string s)
        {
            string cm = Utilities.GetCallingMethod(1);
            Console.WriteLine(string.Format("{0} {1}",cm,s));  
        }

        static internal void OnWrapperLog(string s)
        {
            string cm = Utilities.GetCallingMethod(2);
            Console.WriteLine(string.Format("{0} {1}", cm, s));
        }
        static internal void OnLibLouisLog(string s)
        {
            string cm = Utilities.GetCallingMethod(3);
            Console.WriteLine(string.Format("{0} {1}", cm, s));
        }

    }
}
