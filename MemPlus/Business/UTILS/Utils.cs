using System.Collections.Generic;
using System.Management;
using System.Security.Principal;
using MemPlus.Business.RAM;

namespace MemPlus.Business.UTILS
{
    /// <summary>
    /// Static class containing utility code that can be used by different objects
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Check if the application is running with Administrative rights
        /// </summary>
        /// <returns>True if the application has Administrative rights, otherwise false</returns>
        internal static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Retrieve RAM information
        /// </summary>
        /// <returns>A list of RAM information</returns>
        internal static List<RamStick> GetRamSticks()
        {
            List<RamStick> ramSticks = new List<RamStick>();

            ConnectionOptions connection = new ConnectionOptions { Impersonation = ImpersonationLevel.Impersonate };

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
