using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Threading;
using MemPlus.Classes.LOG;
using Microsoft.VisualBasic.Devices;
using Syncfusion.UI.Xaml.Gauges;

namespace MemPlus.Classes.RAM
{
    /// <summary>
    /// Sealed class containing methods and interaction logic in terms of RAM
    /// </summary>
    internal sealed class RamController
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
        /// The Dispatcher object that can be used to update GUI components
        /// </summary>
        private readonly Dispatcher _dispatcher;
        /// <summary>
        /// The SfCircularGauge object that can be used to present RAM usage statistics
        /// </summary>
        private readonly SfCircularGauge _gauge;
        /// <summary>
        /// The Label object that can be used to show the total physical memory
        /// </summary>
        private readonly Label _lblTotal;
        /// <summary>
        /// The Label object that can be used to show the available physical memory
        /// </summary>
        private readonly Label _lblAvailable;
        /// <summary>
        /// The ComputerInfo object that can be used to retrieve RAM usage statistics
        /// </summary>
        private readonly ComputerInfo _info;
        /// <summary>
        /// The list of processes that should be excluded from memory optimisation
        /// </summary>
        private List<string> _processExceptionList;
        /// <summary>
        /// An integer value representative of the percentage of RAM usage that should be reached before RAM optimisation should be called
        /// </summary>
        private double _autoOptimizeRamThreshold;
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
        /// Property containing how much RAM was saved during the last optimisation
        /// </summary>
        internal double RamSavings { get; private set; }
        /// <summary>
        /// Property displaying whether the RAM monitor is enabled or not
        /// </summary>
        internal bool RamMonitorEnabled { get; private set; }
        /// <summary>
        /// Property displaying whether the FileSystem cache should be cleared or not during memory optimisation
        /// </summary>
        internal bool ClearFileSystemCache { get; set; }
        /// <summary>
        /// Property displaying whether the standby cache should be cleared or not during memory optimisation
        /// </summary>
        internal bool ClearStandbyCache { get; set; }
        /// <summary>
        /// Property displaying whether automatic RAM optimisation should occur after a certain RAM usage percentage was reached
        /// </summary>
        internal bool AutoOptimizePercentage { get; set; }
        #endregion

        /// <summary>
        /// Initialize a new RamController object
        /// </summary>
        /// <param name="dispatcher">The Dispatcher object that can be used to update GUI components</param>
        /// <param name="gauge">The SfCircularGauge control that can be used to present RAM usage statistics</param>
        /// <param name="lblTotal">The Label control that can be used to display the total available memory statistics</param>
        /// <param name="lblAvailable">The Label control that can be used to display the available memory statistics</param>
        /// <param name="ramUpdateTimerInterval">The interval for which RAM usage statistics should be updated</param>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        internal RamController(Dispatcher dispatcher, SfCircularGauge gauge, Label lblTotal, Label lblAvailable, int ramUpdateTimerInterval, LogController logController)
        {
            _logController = logController ?? throw new ArgumentNullException(nameof(logController));
            _logController.AddLog(new ApplicationLog("Initializing RamController"));

            if (ramUpdateTimerInterval <= 0) throw new ArgumentException("Timer interval cannot be less than or equal to zero!");

            RamSavings = 0;

            _info = new ComputerInfo();

            _dispatcher = dispatcher ?? throw new ArgumentException("Dispatcher cannot be null!");
            _gauge = gauge ?? throw new ArgumentException("Gauge cannot be null!");
            _lblTotal = lblTotal ?? throw new ArgumentNullException(nameof(lblTotal));
            _lblAvailable = lblAvailable ?? throw new ArgumentNullException(nameof(lblAvailable));

            _ramOptimizer = new RamOptimizer(_logController);
            ClearStandbyCache = true;
            ClearFileSystemCache = true;

            _ramTimer = new Timer();
            _ramTimer.Elapsed += OnTimedEvent;
            _ramTimer.Interval = ramUpdateTimerInterval;
            _ramTimer.Enabled = false;

            _logController.AddLog(new ApplicationLog("Done initializing RamController"));
        }

        internal void SetAutoOptimizeThreshold(double threshold)
        {
            if (threshold < 25) throw new ArgumentException("Threshold is dangerously low!");
            _autoOptimizeRamThreshold = threshold;
        }

        /// <summary>
        /// Enable or disable automatic timed RAM optimisation
        /// </summary>
        /// <param name="enabled">A boolean to indicate whether automatic RAM optimisation should occur or not</param>
        /// <param name="interval">The interval for automatic RAM optimisation</param>
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
        /// Set the list of processes that should excluded from RAM optimisation
        /// </summary>
        /// <param name="processExceptionList">The list of processes that should be excluded from RAM optimisation</param>
        internal void SetProcessExceptionList(List<string> processExceptionList)
        {
            _processExceptionList = processExceptionList;
        }

        /// <summary>
        /// Set the interval for the RAM Monitor updates
        /// </summary>
        /// <param name="interval">The amount of miliseconds before an update should occur</param>
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
            UpdateGuiControls();

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
        /// Update the GUI controls with the available RAM usage statistics
        /// </summary>
        private void UpdateGuiControls()
        {
            _dispatcher.Invoke(() =>
            {
                _gauge.Scales[0].Pointers[0].Value = RamUsagePercentage;
                _gauge.GaugeHeader = "RAM usage (" + RamUsagePercentage.ToString("F2") + "%)";
                _lblTotal.Content = (RamTotal / 1024 / 1024 / 1024).ToString("F2") + " GB";
                _lblAvailable.Content = (RamUsage / 1024 / 1024 / 1024).ToString("F2") + " GB";
            });
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
            UpdateGuiControls();

            _logController.AddLog(new ApplicationLog("Finished RAM monitor timer"));
        }

        /// <summary>
        /// Clear all non-essential RAM
        /// </summary>
        /// <returns>Nothing</returns>
        internal async Task ClearMemory()
        {
            _logController.AddLog(new ApplicationLog("Clearing RAM memory"));

            await Task.Run(async () =>
            {
                UpdateRamUsage();

                double oldUsage = RamUsage;

                _ramOptimizer.EmptyWorkingSetFunction(_processExceptionList);

                if (ClearFileSystemCache)
                {
                    _ramOptimizer.ClearFileSystemCache(ClearStandbyCache);
                }

                await Task.Delay(10000);

                UpdateRamUsage();
                UpdateGuiControls();

                double newUsage = RamUsage;

                RamSavings = oldUsage - newUsage;
            });


            _logController.AddLog(new ApplicationLog("Done clearing RAM memory"));
        }

        /// <summary>
        /// Update RAM usage statistics
        /// </summary>
        private void UpdateRamUsage()
        {
            _logController.AddLog(new ApplicationLog("Updating RAM usage"));

            double total = Convert.ToDouble(_info.TotalPhysicalMemory);
            double usage = total - Convert.ToDouble(_info.AvailablePhysicalMemory);
            double perc = usage / total * 100;

            RamUsage = usage;
            RamUsagePercentage = perc;
            RamTotal = total;

            if (RamUsagePercentage >= _autoOptimizeRamThreshold && AutoOptimizePercentage)
            {
                // This is dangerous. Needs to be fixed by checking last call time
                ClearMemory();
            }

            _logController.AddLog(new ApplicationLog("Finished updating RAM usage"));
        }
    }
}
