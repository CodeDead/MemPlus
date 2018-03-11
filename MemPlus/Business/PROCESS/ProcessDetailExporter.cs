using System;
using System.Collections.Generic;
using System.IO;

namespace MemPlus.Business.PROCESS
{
    /// <summary>
    /// Static class that can be used to export ProcessDetail information
    /// </summary>
    internal static class ProcessDetailExporter
    {
        /// <summary>
        /// Export data to a specific path
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="data">The data that should be exported</param>
        private static void Export(string path, string data)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(data);
            }
        }

        /// <summary>
        /// Export a list of ProcessDetail objects to a specific path in text format
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="processDetails">The list of ProcessDetail objects that need to be exported</param>
        internal static void ExportText(string path, List<ProcessDetail> processDetails)
        {
            if (processDetails == null || processDetails.Count == 0) throw new ArgumentNullException();

            string exportData = "MemPlus - Process Analyzer Data (" + DateTime.Now + ")";
            exportData += Environment.NewLine;
            exportData += "---";
            exportData += Environment.NewLine;

            for (int i = 0; i < processDetails.Count; i++)
            {
                ProcessDetail pd = processDetails[i];
                exportData += pd.ProcessId + "\t" + pd.ProcessName + "\t" + pd.ProcessLocation + "\t" + pd.MemoryUsage;

                if (i != processDetails.Count - 1)
                {
                    exportData += Environment.NewLine;
                }
            }

            Export(path, exportData);
        }

        /// <summary>
        /// Export a list of ProcessDetail objects to a specific path in HTML format
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="processDetails">The list of ProcessDetail objects that need to be exported</param>
        internal static void ExportHtml(string path, List<ProcessDetail> processDetails)
        {
            if (processDetails == null || processDetails.Count == 0) throw new ArgumentNullException();

            string exportData = "<html>";

            exportData += "<head>";
            exportData += "<title>MemPlus - Process Analyzer Data</title>";
            exportData += "</head>";

            exportData += "<body>";
            exportData += "<h1>MemPlus - Process Analyzer Data (" + DateTime.Now + ")</h1>";

            exportData += "<table border=\"1\">";
            exportData += "<thead>";
            exportData += "<tr><th>Process ID</th><th>Process name</th><th>Process location</th><th>Memory usage</th></tr>";
            exportData += "</thead>";
            exportData += "<tbody>";

            foreach (ProcessDetail pd in processDetails)
            {
                exportData += "<tr>";
                exportData += "<td>" + pd.ProcessId + "</td>";
                exportData += "<td>" + pd.ProcessName + "</td>";
                exportData += "<td>" + pd.ProcessLocation + "</td>";
                exportData += "<td>" + pd.MemoryUsage + "</td>";
            }

            exportData += "</tbody>";
            exportData += "</table>";
            exportData += "</body>";
            exportData += "</html>";

            Export(path, exportData);
        }

        /// <summary>
        /// Export a list of ProcessDetail objects to a specific path in CSV format
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="processDetails">The list of ProcessDetail objects that need to be exported</param>
        internal static void ExportCsv(string path, List<ProcessDetail> processDetails)
        {
            ExportDelimiter(path, ",", processDetails);
        }

        /// <summary>
        /// Export a list of ProcessDetail objects to a specific path in Excel format
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="processDetails">The list of ProcessDetail objects that need to be exported</param>
        internal static void ExportExcel(string path, List<ProcessDetail> processDetails)
        {
            ExportDelimiter(path, ";", processDetails);
        }

        /// <summary>
        /// Export a list of ProcessDetail objects using a specific delimiter character
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="delimiter">The delimiter that should be used to split the data</param>
        /// <param name="processDetails">The list of ProcessDetail objects that need to be exported</param>
        private static void ExportDelimiter(string path, string delimiter, IReadOnlyList<ProcessDetail> processDetails)
        {
            if (processDetails == null || processDetails.Count == 0) throw new ArgumentNullException();

            string exportData = "Process ID" + delimiter + "Process name" + delimiter + "Process location" + delimiter + "Memory usage";
            exportData += Environment.NewLine;

            for (int i = 0; i < processDetails.Count; i++)
            {
                exportData += processDetails[i].ProcessId + delimiter + processDetails[i].ProcessName + delimiter +
                              processDetails[i].ProcessLocation + delimiter + processDetails[i].MemoryUsage;

                if (i != processDetails.Count - 1)
                {
                    exportData += Environment.NewLine;
                }
            }

            Export(path, exportData);
        }
    }
}
