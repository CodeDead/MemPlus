using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace MemPlus.Classes
{
    /// <summary>
    /// System Cache Information structure for x86 working set
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SystemCacheInformation
    {
        internal uint CurrentSize;
        internal uint PeakSize;
        internal uint PageFaultCount;
        internal uint MinimumWorkingSet;
        internal uint MaximumWorkingSet;
    }

    /// <summary>
    /// System Cache Information structure for x64 working set
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SystemCacheInformation64Bit
    {
        internal long CurrentSize;
        internal long PeakSize;
        internal long PageFaultCount;
        internal long MinimumWorkingSet;
        internal long MaximumWorkingSet;
    }

    /// <summary>
    /// Token Privileges structure, used for adjusting token privileges
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TokenPrivileges
    {
        internal int Count;
        internal long Luid;
        internal int Attr;
    }

    /// <summary>
    /// Enum containing System Information class values
    /// </summary>
    internal enum SystemInformationClass
    {
        SystemFileCacheInformation = 0x0015,
        SystemMemoryListInformation = 0x0050
    }

    /// <summary>
    /// Class containing methods to 'optimize' or clear memory usage in Windows
    /// </summary>
    internal class MemPlus
    {
        /// <summary>
        /// Constant int used for TokenPrivileges Atrr variable
        /// </summary>
        private const int SePrivilegeEnabled = 2;
        /// <summary>
        /// Adjust memory quotas for a process
        /// </summary>
        private const string SeIncreaseQuotaName = "SeIncreaseQuotaPrivilege";
        /// <summary>
        /// Profile single process
        /// </summary>
        private const string SeProfileSingleProcessName = "SeProfileSingleProcessPrivilege";
        private const int MemoryPurgeStandbyList = 4;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokenPrivileges newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("ntdll.dll")]
        private static extern uint NtSetSystemInformation(int infoClass, IntPtr info, int length);

        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        /// <summary>
        /// Clear the working sets of all processes that are available to the application
        /// </summary>
        internal static void EmptyWorkingSetFunction()
        {
            List<string> successes = new List<string>();
            List<string> failures = new List<string>();

            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    // Empty the working set of the process
                    EmptyWorkingSet(process.Handle);
                    // Add it to the list of successfully cleared processes
                    successes.Add(process.ProcessName);
                }
                catch (Exception ex)
                {
                    failures.Add(process.ProcessName + ": " + ex.Message);
                }
            }

            //Print the lists with successful and failed processes
            Console.WriteLine("Success (count): " + successes.Count);
            Console.WriteLine("---");
            foreach (string success in successes)
            {
                Console.WriteLine(success);
            }

            Console.WriteLine("Failed (count) " + failures.Count);
            Console.WriteLine("---");
            foreach (string error in failures)
            {
                Console.WriteLine(error);
            }
        }

        /// <summary>
        /// Check whether the system is running a x86 or x64 working set
        /// </summary>
        /// <returns>A boolean to indicate whether or not the system is 64 bit</returns>
        private static bool Is64BitMode()
        {
            return Marshal.SizeOf(typeof(IntPtr)) == 8;
        }

        /// <summary>
        /// Clear the FileSystem cache
        /// </summary>
        /// <param name="clearStandbyCache">Set whether or not to clear cache that is in standby</param>
        internal static void ClearFileSystemCache(bool clearStandbyCache)
        {
            try
            {
                //Check if privilege can be increased
                if (SetIncreasePrivilege(SeIncreaseQuotaName))
                {
                    uint ntSetSystemInformationRet;
                    int systemInfoLength;
                    GCHandle gcHandle;
                    //Depending on the working set, call the right external function using the right parameters
                    if (!Is64BitMode())
                    {
                        SystemCacheInformation cacheInformation =
                            new SystemCacheInformation
                            {
                                MinimumWorkingSet = uint.MaxValue,
                                MaximumWorkingSet = uint.MaxValue
                            };
                        systemInfoLength = Marshal.SizeOf(cacheInformation);
                        gcHandle = GCHandle.Alloc(cacheInformation, GCHandleType.Pinned);
                        ntSetSystemInformationRet = NtSetSystemInformation((int)SystemInformationClass.SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), systemInfoLength);
                        gcHandle.Free();
                    }
                    else
                    {
                        SystemCacheInformation64Bit information64Bit =
                            new SystemCacheInformation64Bit
                            {
                                MinimumWorkingSet = -1L,
                                MaximumWorkingSet = -1L
                            };
                        systemInfoLength = Marshal.SizeOf(information64Bit);
                        gcHandle = GCHandle.Alloc(information64Bit, GCHandleType.Pinned);
                        ntSetSystemInformationRet = NtSetSystemInformation((int)SystemInformationClass.SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), systemInfoLength);
                        gcHandle.Free();
                    }
                    // If value is not equal to zero, things didn't go right :(
                    if (ntSetSystemInformationRet != 0) throw new Exception("NtSetSystemInformation: ", new Win32Exception(Marshal.GetLastWin32Error()));
                }

                // If we don't have to clear the standby cache or cannot increase privileges, don't hesitate to clear the standby cache, otherwise we can clear the standby cache
                if (!clearStandbyCache || !SetIncreasePrivilege(SeProfileSingleProcessName)) return;
                {
                    int systemInfoLength = Marshal.SizeOf(MemoryPurgeStandbyList);
                    GCHandle gcHandle = GCHandle.Alloc(MemoryPurgeStandbyList, GCHandleType.Pinned);
                    uint ntSetSystemInformationRet = NtSetSystemInformation((int)SystemInformationClass.SystemMemoryListInformation, gcHandle.AddrOfPinnedObject(), systemInfoLength);
                    gcHandle.Free();
                    if (ntSetSystemInformationRet != 0) throw new Exception("NtSetSystemInformation: ", new Win32Exception(Marshal.GetLastWin32Error()));
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }
        
        /// <summary>
        /// Increase the Privilege
        /// </summary>
        /// <param name="privilegeName">The name of the privilege that needs to be increased</param>
        /// <returns>A boolean value indicating whether or not the operation was successfull</returns>
        private static bool SetIncreasePrivilege(string privilegeName)
        {
            using (WindowsIdentity current = WindowsIdentity.GetCurrent(TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges))
            {
                TokenPrivileges newst;
                newst.Count = 1;
                newst.Luid = 0L;
                newst.Attr = SePrivilegeEnabled;

                // If we can't look up the privilege value, we can't function properly
                if (!LookupPrivilegeValue(null, privilegeName, ref newst.Luid)) throw new Exception("LookupPrivilegeValue: ", new Win32Exception(Marshal.GetLastWin32Error()));

                //Enables or disables privileges in a specified access token
                int adjustTokenPrivilegesRet = AdjustTokenPrivileges(current.Token, false, ref newst, 0, IntPtr.Zero, IntPtr.Zero) ? 1 : 0;
                // Return value of zero indicates an error
                if (adjustTokenPrivilegesRet == 0) throw new Exception("AdjustTokenPrivileges: ", new Win32Exception(Marshal.GetLastWin32Error()));
                return adjustTokenPrivilegesRet != 0;
            }
        }
    }
}
