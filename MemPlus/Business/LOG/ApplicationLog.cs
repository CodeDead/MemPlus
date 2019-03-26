using System;

namespace MemPlus.Business.LOG
{
    /// <inheritdoc />
    /// <summary>
    /// A class that represent a change in the application
    /// </summary>
    internal sealed class ApplicationLog : Log
    {
        /// <summary>
        /// Initialize a new ApplicationLog object
        /// </summary>
        /// <param name="data">The data that needs to be added to the Log</param>
        internal ApplicationLog(string data)
        {
            LogType = LogType.Application;
            Data = data;
            Time = DateTime.Now;
        }
    }
}
