using System;
using System.Collections.Generic;

namespace MemPlus.Classes.LOG
{
    internal class LogController
    {
        private readonly List<Log> _logList;

        internal LogController()
        {
            _logList = new List<Log>();
        }

        internal void AddLog(Log l)
        {
            _logList.Add(l);
        }

        internal void RemoveLog(Log l)
        {
            if (_logList.Contains(l))
            {
                _logList.Remove(l);
            }
            else
            {
                throw new ArgumentException("Log could not be found!");
            }
        }

        internal void ClearLogs()
        {
            _logList.Clear();
        }

        internal List<Log> GetLogs()
        {
            return _logList;
        }
    }
}
