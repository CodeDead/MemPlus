using System;

namespace MemPlus.Business.LOG
{
    /// <inheritdoc />
    /// <summary>
    /// A class that represent a change in the RAM Optimizer
    /// </summary>
    internal class RamLog : Log
    {
        /// <summary>
        /// Initialize a new RamLog object
        /// </summary>
        /// <param name="data">The data that needs to be added to the Log</param>
        internal RamLog(string data)
        {
            LogType = LogType.Ram;
            Data = data;
            Time = DateTime.Now;
        }
    }
}
