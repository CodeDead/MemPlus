using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using Microsoft.VisualBasic.Devices;
using Syncfusion.UI.Xaml.Gauges;

namespace MemPlus.Classes.RAM
{
    internal class RamController
    {
        private readonly Timer _ramTimer;
        private readonly Dispatcher _dispatcher;
        private readonly SfCircularGauge _gauge;
        private readonly ComputerInfo _info;

        internal double RamUsage { get; private set; }
        internal double RamUsagePercentage { get; private set; }
        internal double RamTotal { get; private set; }
        internal double RamSavings { get; private set; }

        internal RamController(Dispatcher dispatcher, SfCircularGauge gauge, int timerInterval)
        {
            if (timerInterval <= 0) throw new ArgumentException("Timer interval cannot be less than or equal to zero!");

            RamSavings = 0;

            _info = new ComputerInfo();
            _dispatcher = dispatcher ?? throw new ArgumentException("Dispatcher cannot be null!");
            _gauge = gauge ?? throw new ArgumentException("Gauge cannot be null!");

            _ramTimer = new Timer();
            _ramTimer.Elapsed += OnTimedEvent;
            _ramTimer.Interval = timerInterval;
        }

        internal void EnableMonitor()
        {
            if (_ramTimer.Enabled) return;

            _ramTimer.Enabled = true;
            OnTimedEvent(null, null);
        }

        internal void DisableMonitor()
        {
            _ramTimer.Enabled = false;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            UpdateRamUsage();
            _dispatcher.Invoke(() =>
            {
                _gauge.Scales[0].Pointers[0].Value = RamUsagePercentage;
                _gauge.GaugeHeader = "RAM usage (" + RamUsagePercentage.ToString("F2") + "%)";
            });
        }

        internal async Task ClearMemory(bool filesystemcache)
        {
            await Task.Run(() =>
            {
                bool wasEnabled = _ramTimer.Enabled;
                if (wasEnabled)
                {
                    DisableMonitor();
                }

                UpdateRamUsage();

                double oldUsage = RamUsage;

                //Clear working set of all processes that the user has access to
                MemPlus.EmptyWorkingSetFunction();
                //Clear file system cache
                MemPlus.ClearFileSystemCache(filesystemcache);

                UpdateRamUsage();
                double newUsage = RamUsage;

                RamSavings = oldUsage - newUsage;

                if (wasEnabled)
                {
                    EnableMonitor();
                }
            });
        }

        private void UpdateRamUsage()
        {
            double total = Convert.ToDouble(_info.TotalPhysicalMemory);
            double usage = total - Convert.ToDouble(_info.AvailablePhysicalMemory);
            double perc = usage / total * 100;

            RamUsage = usage;
            RamUsagePercentage = perc;
            RamTotal = total;
        }
    }
}
