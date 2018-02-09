using System;

namespace MemPlus.Classes.LOG
{
    public abstract class Log : ILogMethods
    {
        /// <summary>
        /// The type of log
        /// </summary>
        internal LogType LogType { get; set; }

        protected DateTime Time;
        protected string Data;

        public DateTime GetDate()
        {
            return Time;
        }

        public void AddData(string data)
        {
            Time = DateTime.Now;
            Data = data;
        }

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
        Ram
    }
}
