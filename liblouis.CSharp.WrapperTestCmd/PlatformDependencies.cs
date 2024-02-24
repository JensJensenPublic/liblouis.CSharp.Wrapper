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
            string cm = GetCallingMethod(1);
            Console.WriteLine(string.Format("{0} {1}",cm,s));  
        }

        static internal void OnWrapperLog(string s)
        {
            string cm = GetCallingMethod(2);
            Console.WriteLine(string.Format("{0} {1}", cm, s));
        }
        static internal void OnLibLouisLog(string s)
        {
            string cm = GetCallingMethod(3);
            Console.WriteLine(string.Format("{0} {1}", cm, s));
        }

        static private string GetCallingMethod(int extraLevels)
        {
            return GetCallingMethodInternalImplementation(2 + extraLevels); //  because we use this extra level for calling GetCallingMethod(int levels) !
        }


        static private string GetCallingMethodInternalImplementation(int levels)
        {
            try
            {
                StackTrace stackTrace = new StackTrace();
                MethodBase methodBase = stackTrace.GetFrame(levels + 1).GetMethod(); // "levels+1" in order to compensate for calling "GetCallingMethod()"
                Type type = methodBase.ReflectedType;
                return string.Format("{0}.{1}", type.Name, methodBase.Name);
            }
            catch (Exception e)
            {
                return string.Format("Logger.GetCallingMethod threw an exception: {0}", e.ToString());
            }
        }



    }
}
