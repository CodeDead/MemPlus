using System;

namespace MemPlus.Business.RAM
{
    /// <summary>
    /// Internal sealed class that holds a statistical value of RAM memory usage
    /// </summary>
    internal sealed class RamUsage
    {
        /// <summary>
        /// Property containing how much memory is being used
        /// </summary>
        public double TotalUsed { get; }
        /// <summary>
        /// Property containing the percentage of memory that is being used
        /// </summary>
        public double UsagePercentage { get; }
        /// <summary>
        /// Property containing the total amount of memory that is available
        /// </summary>
        public double RamTotal { get; }
        /// <summary>
        /// DateTime object that displays the time at which this recording was recorded
        /// </summary>
        public DateTime RecordedDate { get; }

        /// <summary>
        /// Initialize a new RamUsage object
        /// </summary>
        /// <param name="totalUsed">The total amount of memory that is used at the time of initializing</param>
        /// <param name="ramTotal">The total amount of memory that is available on the system at the time of initializing</param>
        /// <param name="usagePercentage">The percentage of memory that is being used</param>
        internal RamUsage(double totalUsed, double ramTotal, double usagePercentage)
        {
            TotalUsed = totalUsed;
            RamTotal = ramTotal;
            UsagePercentage = usagePercentage;
            RecordedDate = DateTime.Now;
        }
    }
}