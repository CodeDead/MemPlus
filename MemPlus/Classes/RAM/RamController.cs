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
    internal sealed class RamController
    {
        private readonly Timer _ramTimer;
        private readonly LogController _logController;

        private readonly Dispatcher _dispatcher;
        private readonly SfCircularGauge _gauge;
        private readonly Label _lblTotal;
        private readonly Label _lblAvailable;

        private readonly ComputerInfo _info;

        internal double RamUsage { get; private set; }
        internal double RamUsagePercentage { get; private set; }
        internal double RamTotal { get; private set; }
        internal double RamSavings { get; private set; }

        internal RamController(Dispatcher dispatcher, SfCircularGauge gauge, Label lblTotal, Label lblAvailable, int timerInterval, LogController logController)
        {
            _logController = logController ?? throw new ArgumentNullException(nameof(logController));
            _logController.AddLog(new Log("Initializing RamController"));

            if (timerInterval <= 0) throw new ArgumentException("Timer interval cannot be less than or equal to zero!");

            RamSavings = 0;

            _info = new ComputerInfo();

            _dispatcher = dispatcher ?? throw new ArgumentException("Dispatcher cannot be null!");
            _gauge = gauge ?? throw new ArgumentException("Gauge cannot be null!");
            _lblTotal = lblTotal ?? throw new ArgumentNullException(nameof(lblTotal));
            _lblAvailable = lblAvailable ?? throw new ArgumentNullException(nameof(lblAvailable));

            _ramTimer = new Timer();
            _ramTimer.Elapsed += OnTimedEvent;
            _ramTimer.Interval = timerInterval;

            _logController.AddLog(new Log("Done initializing RamController"));
        }

        internal void EnableMonitor()
        {
            if (_ramTimer.Enabled) return;
            _ramTimer.Enabled = true;
            OnTimedEvent(null, null);
            _logController.AddLog(new Log("The RAM monitor has been enabled"));
        }

        internal void DisableMonitor()
        {
            _ramTimer.Enabled = false;
            _logController.AddLog(new Log("The RAM monitor has been disabled"));
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _logController.AddLog(new Log("RAM monitor timer has been called"));

            UpdateRamUsage();

            _dispatcher.Invoke(() =>
            {
                _gauge.Scales[0].Pointers[0].Value = RamUsagePercentage;
                _gauge.GaugeHeader = "RAM usage (" + RamUsagePercentage.ToString("F2") + "%)";
                _lblTotal.Content = (RamTotal / 1024 / 1024 / 1024).ToString("F2") + " GB";
                _lblAvailable.Content = (RamUsage / 1024 / 1024 / 1024).ToString("F2") + " GB";
            });

            _logController.AddLog(new Log("Finished RAM monitor timer"));
        }

        internal async Task ClearMemory(bool filesystemcache)
        {
            _logController.AddLog(new Log("Clearing RAM memory"));

            await Task.Run(async () =>
            {
                UpdateRamUsage();

                double oldUsage = RamUsage;

                //Clear working set of all processes that the user has access to
                MemPlus.EmptyWorkingSetFunction();
                //Clear file system cache
                MemPlus.ClearFileSystemCache(filesystemcache);

                await Task.Delay(10000);

                UpdateRamUsage();
                double newUsage = RamUsage;

                RamSavings = oldUsage - newUsage;
            });

            _logController.AddLog(new Log("Done clearing RAM memory"));
        }

        private void UpdateRamUsage()
        {
            _logController.AddLog(new Log("Updating RAM usage"));

            double total = Convert.ToDouble(_info.TotalPhysicalMemory);
            double usage = total - Convert.ToDouble(_info.AvailablePhysicalMemory);
            double perc = usage / total * 100;

            RamUsage = usage;
            RamUsagePercentage = perc;
            RamTotal = total;

            _logController.AddLog(new Log("Finished updating RAM usage"));
        }
    }
}
