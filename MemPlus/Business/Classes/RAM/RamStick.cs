using System.Collections.Generic;

namespace MemPlus.Business.Classes.RAM
{
    /// <summary>
    /// Internal sealed class containing logic behind a physical RAM stick
    /// </summary>
    internal sealed class RamStick
    {
        #region Variables
        /// <summary>
        /// List of data that is associated to the RAM stick
        /// </summary>
        private readonly List<RamData> _ramData;
        #endregion

        /// <summary>
        /// Initialize a new RamStick object
        /// </summary>
        internal RamStick()
        {
            _ramData = new List<RamData>();
        }

        /// <summary>
        /// Add a new RamData object to the list of RamData
        /// </summary>
        /// <param name="ramData">The RamData object that needs to be added to the list</param>
        internal void AddRamData(RamData ramData)
        {
            _ramData.Add(ramData);
        }

        /// <summary>
        /// Get the list of RamData objects that is associated with the RamStick object
        /// </summary>
        /// <returns>The list of RamData objects that is associated with the RamStick object</returns>
        internal List<RamData> GetRamData()
        {
            return _ramData;
        }

        /// <summary>
        /// Get the value for a RamData key
        /// </summary>
        /// <param name="key">The key that needs to be found</param>
        /// <returns>The value for the RamData key</returns>
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
