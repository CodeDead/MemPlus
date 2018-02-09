using System;

namespace MemPlus.Classes.LOG
{
    internal class Log
    {
        private readonly DateTime _time;
        private readonly string _value;

        internal Log(string data)
        {
            _time = DateTime.Now;
            _value = data;
        }

        internal DateTime GetDate()
        {
            return _time;
        }

        internal string GetValue()
        {
            return _value;
        }
    }
}
