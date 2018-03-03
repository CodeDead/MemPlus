namespace MemPlus.Business.RAM
{
    /// <summary>
    /// ViewModal for displaying RAM information inside a treeview
    /// </summary>
    internal sealed class RamData
    {
        /// <summary>
        /// A key value
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The value that is linked to the specific key
        /// </summary>
        public string Value { get; set; }

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
