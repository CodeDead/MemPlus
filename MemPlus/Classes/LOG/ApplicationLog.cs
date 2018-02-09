using System;

namespace MemPlus.Classes.LOG
{
    /// <inheritdoc />
    /// <summary>
    /// A class that represent a change in the application
    /// </summary>
    internal class ApplicationLog : Log
    {
        /// <summary>
        /// Initialize a new ApplicationLog object
        /// </summary>
        internal ApplicationLog(string data)
        {
            LogType = LogType.Application;
            Data = data;
            Time = DateTime.Now;
        }
    }
}
