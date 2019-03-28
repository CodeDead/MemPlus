using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using MemPlus.Business.LOG;
using Microsoft.VisualBasic.Devices;
// ReSharper disable PossibleNullReferenceException

namespace MemPlus.Business.RAM
{
    /// <inheritdoc />
    /// <summary>
    /// Sealed class containing methods and interaction logic in terms of RAM
    /// </summary>
    internal sealed class RamController : IDisposable
    {
        #region Variables
        /// <summary>
        /// The RamOptimizer object that can be called to clear memory
        /// </summary>
        private readonly RamOptimizer _ramOptimizer;
        /// <summary>
        /// The Timer object that will periodically update RAM usage statistics
        /// </summary>
        private readonly Timer _ramTimer;
        /// <summary>
        /// The Timer object that will automatically Optimize the RAM after a certain interval has passed
        /// </summary>
        private Timer _ramAutoOptimizeTimer;
        /// <summary>
        /// The LogController object that can be used to add logs
        /// </summary>
        private readonly LogController _logController;
        /// <summary>
        /// The ComputerInfo object that can be used to retrieve RAM usage statistics
        /// </summary>
        private readonly ComputerInfo _info;
        /// <summary>
        /// The list of processes that should be excluded from memory optimization
        /// </summary>
        private List<string> _processExceptionList;
        /// <summary>
        /// An integer value representative of the percentage of RAM usage that should be reached before RAM optimization should be called
        /// </summary>
        private double _autoOptimizeRamThreshold;
        /// <summary>
        /// The last time automatic RAM optimization was called in terms of RAM percentage threshold settings
        /// </summary>
        private DateTime _lastAutoOptimizeTime;
        #endregion

        #region Properties
        /// <summary>
        /// Property containing how much RAM is being used
        /// </summary>
        internal double RamUsage { get; private set; }
        /// <summary>
        /// Property containing the percentage of RAM that is being used
        /// </summary>
        internal double RamUsagePercentage { get; private set; }
        /// <summary>
        /// Property containing the total amount of RAM available
        /// </summary>
        internal double RamTotal { get; private set; }
        /// <summary>
        /// Property containing how much RAM was saved during the last optimization
        /// </summary>
        internal double RamSavings { get; private set; }
        /// <summary>
        /// Property displaying whether the RAM monitor is enabled or not
        /// </summary>
        internal bool RamMonitorEnabled { get; private set; }
        /// <summary>
        /// Property displaying whether the working set of processes should be emptied
        /// </summary>
        internal bool EmptyWorkingSets { get; set; }
        /// <summary>
        /// Property displaying whether the FileSystem cache should be cleared or not during memory optimization
        /// </summary>
        internal bool ClearFileSystemCache { get; set; }
        /// <summary>
        /// Property displaying whether the standby cache should be cleared or not during memory optimization
        /// </summary>
        internal bool ClearStandbyCache { private get; set; }
        /// <summary>
        /// Property displaying whether automatic RAM optimization should occur after a certain RAM usage percentage was reached
        /// </summary>
        internal bool AutoOptimizePercentage { private get; set; }
        /// <summary>
        /// Property displaying whether the clipboard should be cleared during memory cleaning
        /// </summary>
        internal bool ClearClipboard { get; set; }
        /// <summary>
        /// Property displaying whether the .NET garbage collector should be invoked or not
        /// </summary>
        internal bool InvokeGarbageCollector { private get; set; }
        #endregion

        #region Delegates
        /// <summary>
        /// Event that is called when the GUI should be updated with new RAM statistics
        /// </summary>
        private event UpdateGuiStatistics UpdateGuiStatisticsEvent;
        /// <summary>
        /// Event that is called when RAM clearing has occurred
        /// </summary>
        private event RamClearingCompleted RamClearingCompletedEvent;
        /// <summary>
        /// Delegate void that indicates that a GUI update should occur when called
        /// </summary>
        internal delegate void UpdateGuiStatistics();
        /// <summary>
        /// Delegate void that indicates that a RAM clearing has occured
        /// </summary>
        internal delegate void RamClearingCompleted();
        #endregion

        /// <summary>
        /// Initialize a new RamController object
        /// </summary>
        /// <param name="updateGuiStatisticsEvent">An event to indicate that a GUI update should occur</param>
        /// <param name="ramClearingCompletedEvent">An event to indicate that the RAM has been cleared</param>
        /// <param name="ramUpdateTimerInterval">The interval for which RAM usage statistics should be updated</param>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        internal RamController(UpdateGuiStatistics updateGuiStatisticsEvent, RamClearingCompleted ramClearingCompletedEvent, int ramUpdateTimerInterval, LogController logController)
        {
            _logController = logController ?? throw new ArgumentNullException(nameof(logController));
            _logController.AddLog(new ApplicationLog("Initializing RamController"));

            if (ramUpdateTimerInterval <= 0) throw new ArgumentException("Timer interval cannot be less than or equal to zero!");
            UpdateGuiStatisticsEvent = updateGuiStatisticsEvent ?? throw new ArgumentNullException(nameof(updateGuiStatisticsEvent));
            RamClearingCompletedEvent = ramClearingCompletedEvent ?? throw new ArgumentNullException(nameof(ramClearingCompletedEvent));

            RamSavings = 0;

            _info = new ComputerInfo();

            _ramOptimizer = new RamOptimizer(_logController);
            EmptyWorkingSets = true;
            ClearStandbyCache = true;
            ClearFileSystemCache = true;

            _ramTimer = new Timer();
            _ramTimer.Elapsed += OnTimedEvent;
            _ramTimer.Interval = ramUpdateTimerInterval;
            _ramTimer.Enabled = false;

            _logController.AddLog(new ApplicationLog("Done initializing RamController"));
        }

        /// <summary>
        /// Set the threshold percentage for automatic RAM optimization
        /// </summary>
        /// <param name="threshold">The percentage threshold</param>
        internal void SetAutoOptimizeThreshold(double threshold)
        {
            if (threshold < 25) throw new ArgumentException("Threshold is dangerously low!");
            _autoOptimizeRamThreshold = threshold;
        }

        /// <summary>
        /// Enable or disable automatic timed RAM optimization
        /// </summary>
        /// <param name="enabled">A boolean to indicate whether automatic RAM optimization should occur or not</param>
        /// <param name="interval">The interval for automatic RAM optimization</param>
        internal void AutoOptimizeTimed(bool enabled, int interval)
        {
            if (_ramAutoOptimizeTimer == null)
            {
                _ramAutoOptimizeTimer = new Timer();
                _ramAutoOptimizeTimer.Elapsed += RamAutoOptimizeTimerOnElapsed;
            }

            _ramAutoOptimizeTimer.Interval = interval;
            _ramAutoOptimizeTimer.Enabled = enabled;
        }

        /// <summary>
        /// Event that will be called when the timer interval was reached
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="elapsedEventArgs">The ElapsedEventArgs</param>
        private async void RamAutoOptimizeTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            await ClearMemory();
        }

        /// <summary>
        /// Set the list of processes that should excluded from RAM optimization
        /// </summary>
        /// <param name="processExceptionList">The list of processes that should be excluded from RAM optimization</param>
        internal void SetProcessExceptionList(List<string> processExceptionList)
        {
            _processExceptionList = processExceptionList;
        }

        /// <summary>
        /// Set the interval for the RAM Monitor updates
        /// </summary>
        /// <param name="interval">The amount of milliseconds before an update should occur</param>
        internal void SetRamUpdateTimerInterval(int interval)
        {
            _ramTimer.Interval = interval;
        }

        /// <summary>
        /// Enable RAM usage monitoring
        /// </summary>
        internal void EnableMonitor()
        {
            if (_ramTimer.Enabled) return;

            _ramTimer.Enabled = true;
            RamMonitorEnabled = true;

            UpdateRamUsage();
            UpdateGuiStatisticsEvent.Invoke();

            _logController.AddLog(new ApplicationLog("The RAM monitor has been enabled"));
        }

        /// <summary>
        /// Disable RAM usage monitoring
        /// </summary>
        internal void DisableMonitor()
        {
            _ramTimer.Enabled = false;
            RamMonitorEnabled = false;

            _logController.AddLog(new ApplicationLog("The RAM monitor has been disabled"));
        }

        /// <summary>
        /// Event that will be called when the timer interval was reached
        /// </summary>
        /// <param name="source">The calling object</param>
        /// <param name="e">The ElapsedEventArgs</param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _logController.AddLog(new ApplicationLog("RAM monitor timer has been called"));

            UpdateRamUsage();
            UpdateGuiStatisticsEvent.Invoke();

            _logController.AddLog(new ApplicationLog("Finished RAM monitor timer"));
        }

        /// <summary>
        /// Clear all non-essential RAM
        /// </summary>
        /// <returns>A Task</returns>
        internal async Task ClearMemory()
        {
            _lastAutoOptimizeTime = DateTime.Now;
            _logController.AddLog(new ApplicationLog("Clearing RAM memory"));

            await Task.Run(async () =>
            {
                UpdateRamUsage();

                double oldUsage = RamUsage;

                if (EmptyWorkingSets)
                {
                    _ramOptimizer.EmptyWorkingSetFunction(_processExceptionList);
                    await Task.Delay(10000);
                }

                if (ClearFileSystemCache)
                {
                    _ramOptimizer.ClearFileSystemCache(ClearStandbyCache);
                }

                if (InvokeGarbageCollector)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                UpdateRamUsage();
                UpdateGuiStatisticsEvent.Invoke();

                double newUsage = RamUsage;

                RamSavings = oldUsage - newUsage;
            });

            if (ClearClipboard)
            {
                _ramOptimizer.ClearClipboard();
            }

            RamClearingCompletedEvent.Invoke();

            _logController.AddLog(new ApplicationLog("Done clearing RAM memory"));
        }

        /// <summary>
        /// Clear the working set of all processes, excluding the exclusion list
        /// </summary>
        /// <returns>A Task</returns>
        internal async Task ClearWorkingSets()
        {
            _logController.AddLog(new ApplicationLog("Clearing process working sets"));

            await Task.Run(async () =>
            {
                UpdateRamUsage();

                double oldUsage = RamUsage;

                _ramOptimizer.EmptyWorkingSetFunction(_processExceptionList);

                await Task.Delay(10000);

                UpdateRamUsage();
                UpdateGuiStatisticsEvent.Invoke();

                double newUsage = RamUsage;

                RamSavings = oldUsage - newUsage;
            });

            RamClearingCompletedEvent.Invoke();

            _logController.AddLog(new ApplicationLog("Done clearing process working sets"));
        }

        /// <summary>
        /// Clear the FileSystem cache
        /// </summary>
        /// <returns>A Task</returns>
        internal async Task ClearFileSystemCaches()
        {
            _logController.AddLog(new ApplicationLog("Clearing FileSystem cache"));

            await Task.Run(() =>
            {
                UpdateRamUsage();

                double oldUsage = RamUsage;

                _ramOptimizer.ClearFileSystemCache(ClearStandbyCache);

                UpdateRamUsage();
                UpdateGuiStatisticsEvent.Invoke();

                double newUsage = RamUsage;

                RamSavings = oldUsage - newUsage;
            });

            RamClearingCompletedEvent.Invoke();

            _logController.AddLog(new ApplicationLog("Done clearing FileSystem cache"));
        }

        /// <summary>
        /// Update RAM usage statistics
        /// </summary>
        private void UpdateRamUsage()
        {
            _logController.AddLog(new ApplicationLog("Updating RAM usage"));

            double total = Convert.ToDouble(_info.TotalPhysicalMemory);
            double usage = total - Convert.ToDouble(_info.AvailablePhysicalMemory);
            double percentage = usage / total * 100;

            RamUsage = usage;
            RamUsagePercentage = percentage;
            RamTotal = total;

            if (RamUsagePercentage >= _autoOptimizeRamThreshold && AutoOptimizePercentage)
            {
                double diff = (DateTime.Now - _lastAutoOptimizeTime).TotalSeconds;
                if (diff > 10)
                {
#pragma warning disable 4014
                    ClearMemory();
#pragma warning restore 4014
                }
            }

            _logController.AddLog(new ApplicationLog("Finished updating RAM usage"));
        }

        /// <inheritdoc />
        /// <summary>
        /// Dispose all disposable objects
        /// </summary>
        public void Dispose()
        {
            _ramTimer?.Dispose();
            _ramAutoOptimizeTimer?.Dispose();
        }
    }
}
