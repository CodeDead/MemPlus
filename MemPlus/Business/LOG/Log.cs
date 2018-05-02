using System;

namespace MemPlus.Business.LOG
{
    /// <summary>
    /// Abstract class containing logging information
    /// </summary>
    public abstract class Log
    {
        /// <summary>
        /// The type of log
        /// </summary>
        internal LogType LogType { get; set; }
        /// <summary>
        /// The creation date of the Log object
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// The data inside the Log object
        /// </summary>
        public string Data { get; set; }
    }

    /// <summary>
    /// An enumeration of all available log types
    /// </summary>
    public enum LogType
    {
        Application,
        Ram,
        Process
    }
}
