using System;
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
        #endregion

        /// <summary>
        /// Initialize a new RamController object
        /// </summary>
        /// <param name="dispatcher">The Dispatcher object that can be used to update GUI components</param>
        /// <param name="gauge">The SfCircularGauge control that can be used to present RAM usage statistics</param>
        /// <param name="lblTotal">The Label control that can be used to display the total available memory statistics</param>
        /// <param name="lblAvailable">The Label control that can be used to display the available memory statistics</param>
        /// <param name="timerInterval">The interval for which RAM usage statistics should be updated</param>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        internal RamController(Dispatcher dispatcher, SfCircularGauge gauge, Label lblTotal, Label lblAvailable, int timerInterval, LogController logController)
        {
            _logController = logController ?? throw new ArgumentNullException(nameof(logController));
            _logController.AddLog(new ApplicationLog("Initializing RamController"));

            if (timerInterval <= 0) throw new ArgumentException("Timer interval cannot be less than or equal to zero!");

            RamSavings = 0;

            _info = new ComputerInfo();

            _dispatcher = dispatcher ?? throw new ArgumentException("Dispatcher cannot be null!");
            _gauge = gauge ?? throw new ArgumentException("Gauge cannot be null!");
            _lblTotal = lblTotal ?? throw new ArgumentNullException(nameof(lblTotal));
            _lblAvailable = lblAvailable ?? throw new ArgumentNullException(nameof(lblAvailable));

            _ramOptimizer = new RamOptimizer(_logController);

            _ramTimer = new Timer();
            _ramTimer.Elapsed += OnTimedEvent;
            _ramTimer.Interval = timerInterval;

            _logController.AddLog(new ApplicationLog("Done initializing RamController"));
        }

        /// <summary>
        /// Enable RAM usage monitoring
        /// </summary>
        internal void EnableMonitor()
        {
            if (_ramTimer.Enabled) return;
            _ramTimer.Enabled = true;
            
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
        /// <param name="filesystemcache">A boolean to indicate whether or not to clear the FileSystem cache</param>
        /// <returns></returns>
        internal async Task ClearMemory(bool filesystemcache)
        {
            _logController.AddLog(new ApplicationLog("Clearing RAM memory"));

            await Task.Run(async () =>
            {
                UpdateRamUsage();

                double oldUsage = RamUsage;

                _ramOptimizer.EmptyWorkingSetFunction();
                _ramOptimizer.ClearFileSystemCache(filesystemcache);

                await Task.Delay(10000);

                UpdateRamUsage();
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

            _logController.AddLog(new ApplicationLog("Finished updating RAM usage"));
        }
    }
}
