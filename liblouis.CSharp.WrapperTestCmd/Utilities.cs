using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace liblouis.CSharp.WrapperTestCmd
{
    internal class Utilities
    {

        static public string GetCallingMethod(int extraLevels)
        {
            return GetCallingMethodInternalImplementation(2 + extraLevels); //  because we use this extra level for calling GetCallingMethod(int levels) !
        }


        static public string GetCallingMethodInternalImplementation(int levels)
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
