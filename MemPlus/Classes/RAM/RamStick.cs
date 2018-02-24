using System.Collections.Generic;

namespace MemPlus.Classes.RAM.ViewModels
{
    internal sealed class RamStick
    {
        private readonly List<RamData> _ramData;

        internal RamStick()
        {
            _ramData = new List<RamData>();
        }

        internal void AddRamData(RamData ramData)
        {
            _ramData.Add(ramData);
        }

        internal List<RamData> GetRamData()
        {
            return _ramData;
        }

        internal string GetValue(string key)
        {
            foreach(RamData r in _ramData)
            {
                if (r.Key.ToLower() == key.ToLower())
                {
                    return r.Value;
                }
            }
            return "";
        }
    }
}
