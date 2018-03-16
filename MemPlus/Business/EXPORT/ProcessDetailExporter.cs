using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MemPlus.Business.PROCESS;

namespace MemPlus.Business.EXPORT
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
            StringBuilder sb = new StringBuilder();

            sb.Append("MemPlus - Process Analyzer Data (" + DateTime.Now + ")" + Environment.NewLine + "---" + Environment.NewLine);

            for (int i = 0; i < processDetails.Count; i++)
            {
                ProcessDetail pd = processDetails[i];
                sb.Append(pd.ProcessId + "\t" + pd.ProcessName + "\t" + pd.ProcessLocation + "\t" + pd.MemoryUsage);

                if (i != processDetails.Count - 1)
                {
                    sb.Append(Environment.NewLine);
                }
            }

            Export(path, sb.ToString());
        }

        /// <summary>
        /// Export a list of ProcessDetail objects to a specific path in HTML format
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="processDetails">The list of ProcessDetail objects that need to be exported</param>
        internal static void ExportHtml(string path, List<ProcessDetail> processDetails)
        {
            if (processDetails == null || processDetails.Count == 0) throw new ArgumentNullException();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>MemPlus - Process Analyzer Data</title></head><body><h1>MemPlus - Process Analyzer Data (" + DateTime.Now + ")</h1>");
            sb.Append("<table border=\"1\"><thead><tr><th>Process ID</th><th>Process name</th><th>Process location</th><th>Memory usage</th></tr></thead><tbody>");

            foreach (ProcessDetail pd in processDetails)
            {
                sb.Append("<tr><td>" + pd.ProcessId + "</td><td>" + pd.ProcessName + "</td><td>" + pd.ProcessLocation + "</td><td>" + pd.MemoryUsage + "</td>");
            }

            sb.Append("</tbody></table></body></html>");

            Export(path, sb.ToString());
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

            StringBuilder sb = new StringBuilder();
            sb.Append("Process ID" + delimiter + "Process name" + delimiter + "Process location" + delimiter + "Memory usage" + Environment.NewLine);

            for (int i = 0; i < processDetails.Count; i++)
            {
                sb.Append(processDetails[i].ProcessId + delimiter + processDetails[i].ProcessName + delimiter + processDetails[i].ProcessLocation + delimiter + processDetails[i].MemoryUsage);

                if (i == processDetails.Count - 1) continue;
                sb.Append(Environment.NewLine);
            }

            Export(path, sb.ToString());
        }
    }
}
