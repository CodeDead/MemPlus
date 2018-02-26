using System;

namespace MemPlus.Business.Classes.LOG
{
    /// <summary>
    /// Interface containing methods that are required for Log objects
    /// </summary>
    internal interface ILogMethods
    {
        /// <summary>
        /// Add data to the Log object
        /// </summary>
        /// <param name="data">The data that needs to be added to the log</param>
        void AddData(string data);
        /// <summary>
        /// Retrieve the data from the Log object
        /// </summary>
        /// <returns>The data from the Log object</returns>
        string GetData();
        /// <summary>
        /// Retrieve the creation date of the Log object
        /// </summary>
        /// <returns>The creation date of the Log object</returns>
        DateTime GetDate();
    }
}
