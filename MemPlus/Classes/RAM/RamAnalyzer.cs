using System.Collections.Generic;
using System.Management;
using MemPlus.Classes.RAM.ViewModels;

namespace MemPlus.Classes.RAM
{
    /// <summary>
    /// Static class that can be used to retrieve RAM information
    /// </summary>
    internal static class RamAnalyzer
    {
        /// <summary>
        /// Retrieve RAM information
        /// </summary>
        /// <returns>A list of RAM information</returns>
        internal static List<RamStick> GetRamSticks()
        {
            List<RamStick> ramSticks = new List<RamStick>();

            ConnectionOptions connection = new ConnectionOptions {Impersonation = ImpersonationLevel.Impersonate};

            ManagementScope scope = new ManagementScope("\\root\\CIMV2", connection);
            scope.Connect();

            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_PhysicalMemory");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (ManagementObject queryObj in searcher.Get())
            {
                RamStick stick = new RamStick();
                foreach (PropertyData data in queryObj.Properties)
                {
                    if (data.Value != null)
                    {
                        stick.AddRamData(new RamData(data.Name, data.Value.ToString()));
                    }
                }

                ramSticks.Add(stick);
            }

            return ramSticks;
        }
    }
}
