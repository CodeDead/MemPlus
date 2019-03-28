using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using MemPlus.Business.LOG;
using MemPlus.Business.UTILS;

namespace MemPlus.Business.RAM
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
        internal uint Unused1;
        internal uint Unused2;
        internal uint Unused3;
        internal uint Unused4;
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
        internal long Unused1;
        internal long Unused2;
        internal long Unused3;
        internal long Unused4;
    }

    /// <summary>
    /// Token Privileges structure, used for adjusting token privileges
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TokenPrivileges
    {
        internal int Count;
        // ReSharper disable once IdentifierTypo
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
    /// Sealed class containing methods to 'optimize' or clear memory usage in Windows
    /// </summary>
    internal sealed class RamOptimizer
    {
        #region Variables
        /// <summary>
        /// Constant int used for TokenPrivileges Attribute variable
        /// </summary>
        private const int PrivilegeEnabled = 2;
        /// <summary>
        /// Adjust memory quotas for a process
        /// </summary>
        private const string IncreaseQuotaName = "SeIncreaseQuotaPrivilege";
        /// <summary>
        /// Profile single process
        /// </summary>
        private const string ProfileSingleProcessName = "SeProfileSingleProcessPrivilege";
        /// <summary>
        /// Memory purge standby list
        /// </summary>
        private const int MemoryPurgeStandbyList = 4;
        /// <summary>
        /// The LogController object that can be called to add logs
        /// </summary>
        private readonly LogController _logController;
        #endregion

        /// <summary>
        /// Initialize a new RamOptimizer object
        /// </summary>
        /// <param name="logController">The LogController object that can be used to add new logs</param>
        internal RamOptimizer(LogController logController)
        {
            _logController = logController ?? throw new ArgumentNullException(nameof(logController));
        }

        /// <summary>
        /// Clear the working sets of all processes that are available to the application
        /// </summary>
        /// <param name="processExceptions">A list of processes that should be excluded from memory optimization</param>
        internal void EmptyWorkingSetFunction(List<string> processExceptions)
        {
            _logController.AddLog(new RamLog("Emptying working set"));

            if (processExceptions != null && processExceptions.Count > 0)
            {
                processExceptions = processExceptions.ConvertAll(d => d.ToLower());
            }

            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    if (processExceptions == null || processExceptions.Count == 0 || !processExceptions.Contains(process.MainModule.FileName.ToLower()))
                    {
                        _logController.AddLog(new RamLog("Emptying working set for process: " + process.ProcessName));
                        NativeMethods.EmptyWorkingSet(process.Handle);
                        _logController.AddLog(new RamLog("Successfully emptied working set for process " + process.ProcessName));
                    }
                    else
                    {
                        _logController.AddLog(new RamLog("Excluded process: " + process.ProcessName));
                    }
                }
                catch (Exception ex)
                {
                    _logController.AddLog(new ErrorLog("Could not empty working set for process " + process.ProcessName + ": " + ex.Message));
                }
            }

            _logController.AddLog(new RamLog("Done emptying working set"));
        }

        /// <summary>
        /// Clear the clipboard using the native Windows API
        /// </summary>
        internal void ClearClipboard()
        {
            _logController.AddLog(new RamLog("Clearing clipboard"));
            try
            {
                // Attempt to open the clipboard first and set associate its handle to the current task
                if (!NativeMethods.OpenClipboard(IntPtr.Zero))
                {
                    throw new Exception("OpenClipboard: ", new Win32Exception(Marshal.GetLastWin32Error()));
                }

                NativeMethods.EmptyClipboard();
                NativeMethods.CloseClipboard();

                _logController.AddLog(new RamLog("Successfully cleared all clipboard data"));
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.ToString()));
            }
        }

        /// <summary>
        /// Check whether the system is running a x86 or x64 working set
        /// </summary>
        /// <returns>A boolean to indicate whether or not the system is 64 bit</returns>
        private bool Is64BitMode()
        {
            _logController.AddLog(new RamLog("Checking if 64 bit mode is enabled"));
            bool is64Bit = Marshal.SizeOf(typeof(IntPtr)) == 8;
            _logController.AddLog(is64Bit ? new RamLog("64 bit mode is enabled") : new RamLog("64 bit mode is disabled"));

            return is64Bit;
        }

        /// <summary>
        /// Clear the FileSystem cache
        /// </summary>
        /// <param name="clearStandbyCache">Set whether or not to clear the standby cache</param>
        internal void ClearFileSystemCache(bool clearStandbyCache)
        {
            _logController.AddLog(new RamLog("Clearing FileSystem cache"));

            try
            {
                // Check if privilege can be increased
                if (SetIncreasePrivilege(IncreaseQuotaName))
                {
                    _logController.AddLog(new RamLog("Privileges have successfully been increased"));

                    uint ntSetSystemInformationRet;
                    int systemInfoLength;
                    GCHandle gcHandle;
                    // Depending on the working set, call NtSetSystemInformation using the right parameters
                    if (!Is64BitMode())
                    {
                        _logController.AddLog(new RamLog("Clearing 32 bit FileSystem cache information"));

                        SystemCacheInformation cacheInformation =
                            new SystemCacheInformation
                            {
                                MinimumWorkingSet = uint.MaxValue,
                                MaximumWorkingSet = uint.MaxValue
                            };
                        systemInfoLength = Marshal.SizeOf(cacheInformation);
                        gcHandle = GCHandle.Alloc(cacheInformation, GCHandleType.Pinned);
                        ntSetSystemInformationRet = NativeMethods.NtSetSystemInformation((int)SystemInformationClass.SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), systemInfoLength);
                        // If value is not equal to zero, things didn't go right :(
                        if (ntSetSystemInformationRet != 0) throw new Exception("NtSetSystemInformation: ", new Win32Exception(Marshal.GetLastWin32Error()));
                        gcHandle.Free();

                        _logController.AddLog(new RamLog("Done clearing 32 bit FileSystem cache information"));
                    }
                    else
                    {
                        _logController.AddLog(new RamLog("Clearing 64 bit FileSystem cache information"));

                        SystemCacheInformation64Bit information64Bit =
                            new SystemCacheInformation64Bit
                            {
                                MinimumWorkingSet = -1L,
                                MaximumWorkingSet = -1L
                            };
                        systemInfoLength = Marshal.SizeOf(information64Bit);
                        gcHandle = GCHandle.Alloc(information64Bit, GCHandleType.Pinned);
                        ntSetSystemInformationRet = NativeMethods.NtSetSystemInformation((int)SystemInformationClass.SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), systemInfoLength);
                        // If value is not equal to zero, things didn't go right :(
                        if (ntSetSystemInformationRet != 0) throw new Exception("NtSetSystemInformation: ", new Win32Exception(Marshal.GetLastWin32Error()));
                        gcHandle.Free();

                        _logController.AddLog(new RamLog("Done clearing 64 bit FileSystem cache information"));
                    }
                }

                // Clear the standby cache if we have to and if we can also increase the privileges
                // If we can't increase the privileges, it's pointless to even try
                if (!clearStandbyCache || !SetIncreasePrivilege(ProfileSingleProcessName)) return;
                {
                    _logController.AddLog(new RamLog("Clearing standby cache"));

                    int systemInfoLength = Marshal.SizeOf(MemoryPurgeStandbyList);
                    GCHandle gcHandle = GCHandle.Alloc(MemoryPurgeStandbyList, GCHandleType.Pinned);
                    uint ntSetSystemInformationRet = NativeMethods.NtSetSystemInformation((int)SystemInformationClass.SystemMemoryListInformation, gcHandle.AddrOfPinnedObject(), systemInfoLength);
                    if (ntSetSystemInformationRet != 0) throw new Exception("NtSetSystemInformation: ", new Win32Exception(Marshal.GetLastWin32Error()));
                    gcHandle.Free();

                    _logController.AddLog(new RamLog("Done clearing standby cache"));
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.ToString()));
            }
        }
        
        /// <summary>
        /// Increase the Privilege using a privilege name
        /// </summary>
        /// <param name="privilegeName">The name of the privilege that needs to be increased</param>
        /// <returns>A boolean value indicating whether or not the operation was successful</returns>
        private bool SetIncreasePrivilege(string privilegeName)
        {
            _logController.AddLog(new RamLog("Increasing privilege: " + privilegeName));

            using (WindowsIdentity current = WindowsIdentity.GetCurrent(TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges))
            {
                TokenPrivileges newst;
                newst.Count = 1;
                newst.Luid = 0L;
                newst.Attr = PrivilegeEnabled;

                _logController.AddLog(new RamLog("Looking up privilege value"));
                // If we can't look up the privilege value, we can't function properly
                if (!NativeMethods.LookupPrivilegeValue(null, privilegeName, ref newst.Luid)) throw new Exception("LookupPrivilegeValue: ", new Win32Exception(Marshal.GetLastWin32Error()));
                _logController.AddLog(new RamLog("Done looking up privilege value"));


                _logController.AddLog(new RamLog("Adjusting token privileges"));
                // Enables or disables privileges in a specified access token
                int adjustTokenPrivilegesRet = NativeMethods.AdjustTokenPrivileges(current.Token, false, ref newst, 0, IntPtr.Zero, IntPtr.Zero) ? 1 : 0;
                // Return value of zero indicates an error
                if (adjustTokenPrivilegesRet == 0) throw new Exception("AdjustTokenPrivileges: ", new Win32Exception(Marshal.GetLastWin32Error()));
                _logController.AddLog(new RamLog("Done adjusting token privileges"));
                return adjustTokenPrivilegesRet != 0;
            }
        }
    }
}
