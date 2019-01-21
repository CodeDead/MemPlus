using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using MemPlus.Business.EXPORT;
using MemPlus.Business.LOG;
using MemPlus.Business.PROCESS;
using MemPlus.Business.RAM;
using Microsoft.Win32;

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
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        /// <summary>
        /// Run the application using Administrative rights
        /// </summary>
        internal static void RunAsAdministrator(LogController logController)
        {
            try
            {
                Process proc = new Process
                {
                    StartInfo =
                    {
                        FileName = Assembly.GetExecutingAssembly().Location,
                        UseShellExecute = true,
                        Verb = "runas"
                    }
                };
                proc.Start();
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Retrieve RAM information
        /// </summary>
        /// <returns>A list of RamStick objects</returns>
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

        /// <summary>
        /// Export all RamStick objects
        /// </summary>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        /// <returns>True if the operation completed successfully, otherwise false</returns>
        internal static bool ExportRamSticks(LogController logController)
        {
            List<RamStick> ramSticks = GetRamSticks();
            if (ramSticks == null || ramSticks.Count == 0) return false;

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|HTML file (*.html)|*.html|CSV file (*.csv)|*.csv|Excel file (*.csv)|*.csv"
            };
            if (sfd.ShowDialog() != true) return false;
            try
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (sfd.FilterIndex)
                {
                    //Filterindex starts at 1
                    case 1:
                        RamDataExporter.ExportText(sfd.FileName, ramSticks);
                        break;
                    case 2:
                        RamDataExporter.ExportHtml(sfd.FileName, ramSticks);
                        break;
                    case 3:
                        RamDataExporter.ExportCsv(sfd.FileName, ramSticks);
                        break;
                    case 4:
                        RamDataExporter.ExportExcel(sfd.FileName, ramSticks);
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        /// <summary>
        /// Export logs to the disk
        /// </summary>
        /// <param name="logType">The LogType that should be exported (can be null to export all logs)</param>
        /// <param name="logController">The LogController object that can be used to export logs</param>
        /// <returns>True if the operation completed successfully, otherwise false</returns>
        internal static bool ExportLogs(LogType? logType, LogController logController)
        {
            if (logType != null)
            {
                if (logController.GetLogs(logType).Count == 0) return false;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|HTML file (*.html)|*.html|CSV file (*.csv)|*.csv|Excel file (*.csv)|*.csv"
            };

            if (sfd.ShowDialog() != true) return false;
            ExportTypes.ExportType type;
            switch (sfd.FilterIndex)
            {
                default:
                    type = ExportTypes.ExportType.Text;
                    break;
                case 2:
                    type = ExportTypes.ExportType.Html;
                    break;
                case 3:
                    type = ExportTypes.ExportType.Csv;
                    break;
                case 4:
                    type = ExportTypes.ExportType.Excel;
                    break;
            }

            try
            {
                logController.Export(sfd.FileName, logType, type);
                return true;
            }
            catch (Exception ex)
            {
                logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        /// <summary>
        /// Export all ProcessDetail objects
        /// </summary>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        /// <returns>True if the operation completed successfully, otherwise false</returns>
        internal static async Task<bool> ExportProcessDetails(LogController logController)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|HTML file (*.html)|*.html|CSV file (*.csv)|*.csv|Excel file (*.csv)|*.csv"
            };
            if (sfd.ShowDialog() != true) return false;
            try
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (sfd.FilterIndex)
                {
                    // Filter index starts at 1
                    case 1:
                        ProcessDetailExporter.ExportText(sfd.FileName, await GetProcessDetails(logController));
                        break;
                    case 2:
                        ProcessDetailExporter.ExportHtml(sfd.FileName, await GetProcessDetails(logController));
                        break;
                    case 3:
                        ProcessDetailExporter.ExportCsv(sfd.FileName, await GetProcessDetails(logController));
                        break;
                    case 4:
                        ProcessDetailExporter.ExportExcel(sfd.FileName, await GetProcessDetails(logController));
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        /// <summary>
        /// Retrieve a list of ProcessDetail objects
        /// </summary>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        /// <returns>A list of ProcessDetail objects that are currently available</returns>
        internal static async Task<List<ProcessDetail>> GetProcessDetails(LogController logController)
        {
            logController.AddLog(new ProcessLog("Retrieving process details"));
            List<ProcessDetail> processDetailsList = new List<ProcessDetail>();

            await Task.Run(() =>
            {
                foreach (Process p in Process.GetProcesses())
                {
                    try
                    {
                        ProcessDetail pd = new ProcessDetail
                        {
                            ProcessId = p.Id,
                            ProcessName = p.ProcessName,
                            ProcessLocation = p.MainModule.FileName,
                            MemoryUsage = (p.WorkingSet64 / (1024 * 1024)).ToString("F2") + " MB",
                            MemoryUsageLong = p.WorkingSet64
                        };
                        processDetailsList.Add(pd);
                    }
                    catch (Exception ex)
                    {
                        logController.AddLog(new ProcessLog(p.ProcessName + ": " + ex.Message));
                    }
                }
            });

            logController.AddLog(new ProcessLog("Done retrieving process details"));
            return processDetailsList;
        }

        /// <summary>
        /// Check if the program starts automatically.
        /// </summary>
        /// <returns>A boolean to represent whether the program starts automatically or not.</returns>
        internal static bool AutoStartUp()
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "MemPlus", "").ToString() == Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// Get the process for a specific file, if applicable
        /// </summary>
        /// <param name="file">The file for which the Process object should be retrieved</param>
        /// <returns></returns>
        internal static Process GetProcessForFile(string file)
        {
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (p.MainModule.FileName == file) return p;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return null;
        }
    }
}
