using System;

namespace MemPlus.Classes.LOG
{
    internal interface ILogMethods
    {
        void AddData(string data);
        string GetData();
        DateTime GetDate();
    }
}
