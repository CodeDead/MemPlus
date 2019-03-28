namespace MemPlus.Business.RAM
{
    /// <summary>
    /// Class for displaying RAM information inside a TreeView component
    /// </summary>
    internal sealed class RamData
    {
        /// <summary>
        /// A key value
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// The value that is linked to the specific key
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initialize a new RamData object
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The corresponding data</param>
        internal RamData(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
