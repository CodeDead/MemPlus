using System.Threading.Tasks;

namespace MemPlus.Classes
{
    internal class RamController
    {
        private readonly RamMonitor _ramMonitor;

        internal double RamSavings { get; private set; }

        internal RamController(RamMonitor monitor)
        {
            RamSavings = 0;
            _ramMonitor = monitor;
        }

        internal async Task Clear(bool filesystemcache)
        {
            await Task.Run(async () =>
            {
                bool wasEnabled = _ramMonitor.Enabled;
                if (wasEnabled)
                {
                    _ramMonitor.Stop();
                }

                await _ramMonitor.UpdateRamUsage();

                double oldUsage = _ramMonitor.RamUsage;

                //Clear working set of all processes that the user has access to
                MemPlus.EmptyWorkingSetFunction();
                //Clear file system cache
                MemPlus.ClearFileSystemCache(filesystemcache);

                await _ramMonitor.UpdateRamUsage();
                double newUsage = _ramMonitor.RamUsage;

                RamSavings = oldUsage - newUsage;

                if (wasEnabled)
                {
                    _ramMonitor.Start();
                }
            });
        }
    }
}
