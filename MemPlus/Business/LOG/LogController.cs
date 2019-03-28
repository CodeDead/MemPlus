using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using MemPlus.Business.EXPORT;
using Timer = System.Timers.Timer;

namespace MemPlus.Business.LOG
{
    /// <inheritdoc />
    /// <summary>
    /// Internal class containing methods to control logs
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal sealed class LogController : IDisposable
    {
        #region Variables
        /// <summary>
        /// True if logging is enabled, otherwise false
        /// </summary>
        private bool _loggingEnabled;
        /// <summary>
        /// The list of available Log objects
        /// </summary>
        private readonly List<Log> _logList;
        /// <summary>
        /// The timer that can be used to automatically clear logs
        /// </summary>
        private readonly Timer _autoClearTimer;
        /// <summary>
        /// True if logs should be written to a file, otherwise false
        /// </summary>
        private bool _saveToFile;
        /// <summary>
        /// The DateTime object at which the LogController object was initialized
        /// </summary>
        private readonly DateTime _startTime;
        /// <summary>
        /// The path where logs can be stored
        /// </summary>
        private string _logPath;
        /// <summary>
        /// The FileStream object that can be used to write logs to the disk
        /// </summary>
        private FileStream _fileStream;
        /// <summary>
        /// The StreamWriter object that can be used to write logs to the disk
        /// </summary>
        private StreamWriter _streamWriter;
        /// <summary>
        /// True if the StreamWriter is writing data to the log file, otherwise false
        /// </summary>
        private bool _isWriting;
        #endregion

        #region Delegates
        /// <summary>
        /// Delegate that will be called when a Log object was added
        /// </summary>
        /// <param name="l">The Log object that was added</param>
        // ReSharper disable once InconsistentNaming
        internal delegate void LogAdded(Log l);
        /// <summary>
        /// Delegate that will be called when a Log object was removed
        /// </summary>
        /// <param name="l">The Log object that was deleted</param>
        // ReSharper disable once InconsistentNaming
        internal delegate void LogDeleted(Log l);
        /// <summary>
        /// Delegate that will be called when all Log objects are removed
        /// </summary>
        internal delegate void LogsCleared();
        /// <summary>
        /// Delegate that will be called when a list of Log objects with a specific LogType were removed
        /// </summary>
        /// <param name="clearedList">The list of Log objects that were removed</param>
        // ReSharper disable once InconsistentNaming
        internal delegate void LogTypeCleared(List<Log> clearedList);
        /// <summary>
        /// Method that will be called when a Log object was added
        /// </summary>
        internal event LogAdded LogAddedEvent;
        /// <summary>
        /// Method that will be called when a Log object was removed
        /// </summary>
        internal event LogDeleted LogDeletedEvent;
        /// <summary>
        /// Method that will be called when all Log objects were removed
        /// </summary>
        internal event LogsCleared LogsClearedEvent;
        /// <summary>
        /// Method that will be called when a list of Log objects with a specific LogType were removed
        /// </summary>
        internal event LogTypeCleared LogTypeClearedEvent;
        #endregion

        /// <summary>
        /// Initialize a new LogController object
        /// </summary>
        internal LogController()
        {
            _loggingEnabled = true;
            _logList = new List<Log>();

            _autoClearTimer = new Timer();
            _autoClearTimer.Elapsed += OnTimedEvent;
            _autoClearTimer.Interval = 600000;
            _autoClearTimer.Enabled = true;

            _saveToFile = false;
            _startTime = DateTime.Now;
            _logPath = null;
        }

        /// <summary>
        /// Initialize a new LogController object
        /// </summary>
        /// <param name="enabled">True if logging is enabled, otherwise false</param>
        /// <param name="autoClear">True if the logs should be cleared automatically</param>
        /// <param name="clearInterval">The interval for when ApplicationLog objects should automatically be cleared</param>
        /// <param name="saveToFile">True if logs should be written to a file</param>
        /// <param name="saveDirectory">The directory to which the logs should be written</param>
        internal LogController(bool enabled, bool autoClear, int clearInterval, bool saveToFile, string saveDirectory)
        {
            _loggingEnabled = enabled;
            _logList = new List<Log>();

            _autoClearTimer = new Timer();
            _autoClearTimer.Elapsed += OnTimedEvent;
            _autoClearTimer.Interval = clearInterval;
            SetAutoClear(autoClear);

            _startTime = DateTime.Now;
            // Set this after the DateTime has been established
            SetSaveDirectory(saveDirectory);
            /*
             * Make sure this is the last LogController method that is called
             * because this will only work properly when all other settings (especially the directory)
             * have been set correctly
             */
            SetSaveToFile(saveToFile);
        }

        internal void SetLoggingEnabled(bool enabled)
        {
            _loggingEnabled = enabled;
        }

        /// <summary>
        /// Set whether logs should be cleared automatically or not
        /// </summary>
        /// <param name="autoClear">True if logs should be cleared automatically, otherwise false</param>
        internal void SetAutoClear(bool autoClear)
        {
            bool timerEnabled = autoClear;
            if (!_loggingEnabled) timerEnabled = false;
            _autoClearTimer.Enabled = timerEnabled;
        }

        /// <summary>
        /// Set the automatic log clearing interval
        /// </summary>
        /// <param name="clearInterval">The time it takes in milliseconds before logs are cleared</param>
        internal void SetAutoClearInterval(int clearInterval)
        {
            _autoClearTimer.Interval = clearInterval;
        }

        /// <summary>
        /// Set whether logs should be saved to a file
        /// </summary>
        /// <param name="saveToFile">True if logs should be saved to a file, otherwise false</param>
        internal void SetSaveToFile(bool saveToFile)
        {
            if (!_loggingEnabled && (_saveToFile || saveToFile))
            {
                DisposeFileResources();
                _saveToFile = saveToFile;
                return;
            }
            
            if (_saveToFile && !saveToFile)
            {
                // Make sure the contents of the log file is written before disabling this function
                DisposeFileResources();
            }

            if (_logPath != null && saveToFile && _logPath.Length > 0)
            {
                // Generate a new FileStream that allows other handles to access the file
                _fileStream = new FileStream(_logPath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite);
                _streamWriter = new StreamWriter(_fileStream) {AutoFlush = true};
                _saveToFile = true;
            }
            else
            {
                _saveToFile = false;
            }
        }

        /// <summary>
        /// Set the directory to which logs can be saved
        /// </summary>
        /// <param name="saveDirectory">The directory to which logs can be saved</param>
        internal void SetSaveDirectory(string saveDirectory)
        {
            if (string.IsNullOrEmpty(saveDirectory)) throw new ArgumentNullException(nameof(saveDirectory));
            if (!Directory.Exists(saveDirectory)) throw new IOException("The selected log directory (" + saveDirectory + ") does not exist!");

            // Format the directory string
            if (saveDirectory.Substring(saveDirectory.Length - 1, 1) != "\\")
            {
                saveDirectory += "\\";
            }

            // Generate a new file path for the logs using the starting time of the LogController instance
            // ReSharper disable once StringLiteralTypo
            _logPath = saveDirectory + "memplus_" + _startTime.Year + "-" + _startTime.Month + "-" + _startTime.Day + "_" +
                       _startTime.Hour + "-" + _startTime.Minute + "-" + _startTime.Second + ".log";
        }

        /// <summary>
        /// Event that will be called by a timer object
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The ElapsedEventArgs</param>
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            ClearLogs();
        }

        /// <summary>
        /// Add a Log object to the list of logs
        /// </summary>
        /// <param name="l">The Log object that needs to be added</param>
        internal void AddLog(Log l)
        {
            if (!_loggingEnabled) return;
            
            _logList.Add(l);
            LogAddedEvent?.Invoke(l);
            if (_saveToFile) WriteLogToFile(l);
        }

        /// <summary>
        /// Write a log to a file
        /// </summary>
        /// <param name="l">The log that should be written to a file</param>
        private void WriteLogToFile(Log l)
        {
            _isWriting = true;
            _streamWriter.WriteLine("[" + l.Time + "]\t" + l.Data);
            _isWriting = false;
        }

        /// <summary>
        /// Remove a Log object from the list of logs
        /// </summary>
        /// <param name="l">The Log object that needs to be removed</param>
        internal void RemoveLog(Log l)
        {
            if (_logList.Contains(l))
            {
                _logList.Remove(l);
                LogDeletedEvent?.Invoke(l);
            }
            else
            {
                throw new ArgumentException("Log could not be found!");
            }
        }

        /// <summary>
        /// Clear all Log objects that have a specific LogType
        /// </summary>
        /// <param name="logType">The LogType that Log objects need to contain in order to be removed</param>
        internal void ClearLogs(LogType? logType)
        {
            if (logType == null)
            {
                ClearLogs();
                return;
            }

            List<Log> deleted = new List<Log>();

            for (int i = _logList.Count - 1; i >= 0; i--)
            {
                if (_logList[i].LogType != logType) continue;
                deleted.Add(_logList[i]);
                _logList.RemoveAt(i);
            }

            LogTypeClearedEvent?.Invoke(deleted);
        }

        /// <summary>
        /// Clear all Log objects
        /// </summary>
        internal void ClearLogs()
        {
            _logList.Clear();
            LogsClearedEvent?.Invoke();
        }

        /// <summary>
        /// Retrieve the list of available Log objects
        /// </summary>
        /// <returns>The list of available Log objects</returns>
        private List<Log> GetLogs()
        {
            return _logList;
        }

        /// <summary>
        /// Retrieve the list of available Log objects of a specific LogType
        /// </summary>
        /// <param name="logType">The LogType of the Log objects that should be returned</param>
        /// <returns>A list of Log objects that are of the specified LogType</returns>
        internal List<Log> GetLogs(LogType? logType)
        {
            if (logType == null) return GetLogs();

            List<Log> logList = _logList.Where(l => l.LogType == logType).ToList();
            return logList;
        }

        /// <summary>
        /// Export logs to the disk
        /// </summary>
        /// <param name="path">The path where logs should be stored</param>
        /// <param name="logType">The type of logs that should be saved. Can be null if all logs should be saved</param>
        /// <param name="exportType">The type of export that should be performed</param>
        internal void Export(string path, LogType? logType, ExportType exportType)
        {
            if (_logList.Count == 0) return;
            List<Log> exportList;

            if (logType != null)
            {
                exportList = new List<Log>();
                foreach (Log l in _logList)
                {
                    if (l.LogType == logType)
                    {
                        exportList.Add(l);
                    }
                }
            }
            else
            {
                exportList = _logList;
            }

            if (exportList == null || exportList.Count == 0) throw new ArgumentNullException();

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (exportType)
            {
                case ExportType.Html:
                    LogExporter.ExportHtml(path, exportList);
                    break;
                default:
                    LogExporter.ExportTxt(path, exportList);
                    break;
                case ExportType.Csv:
                    LogExporter.ExportCsv(path, exportList);
                    break;
                case ExportType.Excel:
                    LogExporter.ExportExcel(path, exportList);
                    break;
            }
        }

        /// <summary>
        /// Dispose of and flush the StreamWriter and FileStream objects that are in use to write logs to the disk
        /// </summary>
        private void DisposeFileResources()
        {
            while (_isWriting)
            {
                // Wait for the StreamWriter to complete
            }

            // Flush and close the StreamWriter if applicable
            _streamWriter?.Close();

            // Close the FileStream if applicable
            _fileStream?.Close();
        }

        /// <inheritdoc />
        /// <summary>
        /// Flush and close all used resources
        /// </summary>
        public void Dispose()
        {
            _autoClearTimer?.Dispose();
            DisposeFileResources();
        }
    }
}
