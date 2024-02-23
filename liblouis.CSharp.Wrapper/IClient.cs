using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibLouisWrapper
{
    public interface IClient
    {
        /// <summary>
        /// Called by the wrapper for normal logging
        /// </summary>
        /// <param name="message"></param>
        void OnWrapperLog(string message);


        /// <summary>
        /// Called by the wrapper for logging of messages received from LibLouis native code through the Callback mechanism
        /// </summary>
        /// <param name="message"></param>
        void OnLibLouisLog(string message);
    }
}
