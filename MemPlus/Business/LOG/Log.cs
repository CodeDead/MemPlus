using System;

// ReSharper disable InconsistentNaming
namespace MemPlus.Business.LOG
{
    /// <summary>
    /// Abstract class containing logging information
    /// </summary>
    internal abstract class Log
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
}
