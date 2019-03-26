using System;

namespace MemPlus.Business.LOG
{
    /// <inheritdoc />
    /// <summary>
    /// A class that represent a change in the ProcessAnalyzer
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal sealed class ProcessLog : Log
    {
        /// <summary>
        /// Initialize a new ProcessLog object
        /// </summary>
        /// <param name="data">The data that needs to be added to the Log</param>
        internal ProcessLog(string data)
        {
            LogType = LogType.Process;
            Data = data;
            Time = DateTime.Now;
        }
    }
}
