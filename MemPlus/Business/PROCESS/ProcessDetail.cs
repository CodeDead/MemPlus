namespace MemPlus.Business.PROCESS
{
    /// <summary>
    /// Internal class that represents the presentable details of a Process object
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class ProcessDetail
    {
        /// <summary>
        /// The ID of the Process
        /// </summary>
        public int ProcessId { get; set; }
        /// <summary>
        /// The name of the Process
        /// </summary>
        public string ProcessName { get; set; }
        /// <summary>
        /// The location of the Process
        /// </summary>
        public string ProcessLocation { get; set; }
        /// <summary>
        /// The current memory usage of the Process in MB
        /// </summary>
        public string MemoryUsage { get; set; }
        /// <summary>
        /// The current memory usage of the Process
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public long MemoryUsageLong { get; set; }
    }
}
