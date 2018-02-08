using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using Microsoft.VisualBasic.Devices;
using Syncfusion.UI.Xaml.Gauges;

namespace MemPlus.Classes
{
    internal sealed class RamMonitor
    {
        private readonly ComputerInfo _info;
        private readonly SfCircularGauge _gauge;
        private readonly Dispatcher _dispatcher;
        private readonly Timer _ramTimer;

        internal double RamUsage { get; private set; }
        internal double RamUsagePercentage { get; private set; }
        internal double RamTotal { get; private set; }

        internal bool Enabled => _ramTimer.Enabled;

        internal RamMonitor(Dispatcher dispatcher, SfCircularGauge gauge)
        {
            _info = new ComputerInfo();
            _dispatcher = dispatcher;
            _gauge = gauge;

            _ramTimer = new Timer();
            _ramTimer.Elapsed += OnTimedEvent;
            _ramTimer.Interval = 5000;
        }

        internal void SetTimerInterval(double interval)
        {
            if (interval <= 0) throw new ArgumentException("Interval cannot be less than or equal to zero!");
            _ramTimer.Interval = interval;
        }

        internal void Start()
        {
            if (_ramTimer.Enabled) return;

            _ramTimer.Enabled = true;
            OnTimedEvent(null, null);
        }

        internal void Stop()
        {
            _ramTimer.Enabled = false;
        }

        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            await UpdateRamUsage();
            _dispatcher.Invoke(() =>
            {
                _gauge.Scales[0].Pointers[0].Value = RamUsagePercentage;
                _gauge.GaugeHeader = "RAM usage (" + RamUsagePercentage.ToString("F2") + "%)";
            });
        }

        internal async Task UpdateRamUsage()
        {
            await Task.Run(() =>
            {
                double total = Convert.ToDouble(_info.TotalPhysicalMemory);
                double usage = total - Convert.ToDouble(_info.AvailablePhysicalMemory);
                double perc = usage / total * 100;

                RamUsage = usage;
                RamUsagePercentage = perc;
                RamTotal = total;
            });
        }
    }
}
