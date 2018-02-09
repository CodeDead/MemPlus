using System;
using System.Collections.Generic;

namespace MemPlus.Classes.LOG
{
    internal class LogController
    {
        private readonly List<Log> _logList;
        internal delegate void LogAdded(Log l);
        internal delegate void LogDeleted(Log l);
        internal delegate void LogCleared();

        internal LogAdded LogAddedEvent;
        internal LogDeleted LogDeletedEvent;
        internal LogCleared LogClearedEvent;

        internal LogController()
        {
            _logList = new List<Log>();
        }

        internal void AddLog(Log l)
        {
            _logList.Add(l);
            LogAddedEvent?.Invoke(l);
        }

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

        internal void ClearLogs()
        {
            _logList.Clear();
            LogClearedEvent?.Invoke();
        }

        internal List<Log> GetLogs()
        {
            return _logList;
        }
    }
}
