using System;
using System.Collections.Generic;

namespace MemPlus.Classes.LOG
{
    public class LogController
    {
        private readonly List<Log> _logList;
        internal delegate void LogAdded(Log l);
        internal delegate void LogDeleted(Log l);
        internal delegate void LogCleared();
        internal delegate void LogTypeCleared(List<Log> clearedList);

        internal LogAdded LogAddedEvent;
        internal LogDeleted LogDeletedEvent;
        internal LogCleared LogClearedEvent;
        internal LogTypeCleared LogTypeClearedEvent;

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

        internal void ClearLogs(LogType logType)
        {
            List<Log> deleted = new List<Log>();

            for (int i = _logList.Count - 1; i >= 0; i--)
            {
                if (_logList[i].LogType != logType) continue;
                deleted.Add(_logList[i]);
                _logList.RemoveAt(i);
            }

            LogTypeClearedEvent?.Invoke(deleted);
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
