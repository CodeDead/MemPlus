using System;

namespace MemPlus.Business.LOG
{
    internal sealed class ErrorLog : Log
    {
        /// <inheritdoc />
        /// <summary>
        /// Initialize a new ErrorLog object
        /// </summary>
        /// <param name="data">The data that needs to be added to the Log</param>
        internal ErrorLog(string data)
        {
            LogType = LogType.Error;
            Data = data;
            Time = DateTime.Now;
        }
    }
}