using System;

namespace MemPlus.Business.LOG
{
    /// <inheritdoc />
    /// <summary>
    /// Abstract class containing logging information
    /// </summary>
    public abstract class Log : ILogMethods
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

        /// <inheritdoc />
        /// <summary>
        /// Retrieve the data of the Log object
        /// </summary>
        /// <returns>The data of the Log object</returns>
        public DateTime GetDate()
        {
            return Time;
        }

        /// <inheritdoc />
        /// <summary>
        /// Add data to the Log object
        /// </summary>
        /// <param name="data">The data that needs to be added to the log</param>
        public void AddData(string data)
        {
            Time = DateTime.Now;
            Data = data;
        }

        /// <inheritdoc />
        /// <summary>
        /// Retrieve the data from the Log object
        /// </summary>
        /// <returns>The data from the Log object</returns>
        public string GetData()
        {
            return Data;
        }
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
