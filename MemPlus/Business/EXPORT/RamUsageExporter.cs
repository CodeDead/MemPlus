using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MemPlus.Business.RAM;

namespace MemPlus.Business.EXPORT
{
    /// <summary>
    /// Internal static class for exporting RamUsage objects
    /// </summary>
    internal static class RamUsageExporter
    {
        /// <summary>
        /// Export a list of RamUsage objects in HTML format to the disk
        /// </summary>
        /// <param name="path">The path where the RamUsage objects should be stored</param>
        /// <param name="ramUsageHistory">The list of RamUsage objects that should be exported</param>
        internal static void ExportHtml(string path, List<RamUsage> ramUsageHistory)
        {
            if (ramUsageHistory == null || ramUsageHistory.Count == 0) throw new ArgumentNullException(nameof(ramUsageHistory));

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>MemPlus - RAM Usage Export</title></head><body><h1>MemPlus - RAM Usage Export (" + DateTime.Now + ")</h1><table border=\"1\"><thead><tr><th>Time</th><th>Total used</th><th>Total</th><th>Percentage</th></tr></thead><tbody>");

            foreach (RamUsage l in ramUsageHistory)
            {
                sb.Append("<tr><td>" + l.RecordedDate + "</td><td>" + l.TotalUsed + "</td><td>" + l.RamTotal + "</td><td>" + l.UsagePercentage + "</td></tr>");
            }

            sb.Append("</tbody></table></body></html>");

            Export(path, sb.ToString());
        }

        /// <summary>
        /// Export a list of RamUsage objects in TEXT format to the disk
        /// </summary>
        /// <param name="path">The path where the RamUsage objects should be stored</param>
        /// <param name="ramUsageHistory">The list of RamUsage objects that should be exported</param>
        internal static void ExportTxt(string path, List<RamUsage> ramUsageHistory)
        {
            if (ramUsageHistory == null || ramUsageHistory.Count == 0) throw new ArgumentNullException(nameof(ramUsageHistory));
            StringBuilder sb = new StringBuilder();
            sb.Append("MemPlus - Ram Usage Export (" + DateTime.Now + ")" + Environment.NewLine);

            for (int i = 0; i < ramUsageHistory.Count; i++)
            {
                sb.Append("[" + ramUsageHistory[i].RecordedDate + "]\t" + ramUsageHistory[i].TotalUsed + "\t" + ramUsageHistory[i].RamTotal + "\t" + ramUsageHistory[i].UsagePercentage);
                if (i == ramUsageHistory.Count - 1) continue;
                sb.Append(Environment.NewLine);
            }

            Export(path, sb.ToString());
        }

        /// <summary>
        /// Export a list of RamUsage objects in CSV format to the disk
        /// </summary>
        /// <param name="path">The path where the RamUsage objects should be stored</param>
        /// <param name="ramUsageHistory">The list of RamUsage objects that should be exported</param>
        internal static void ExportCsv(string path, List<RamUsage> ramUsageHistory)
        {
            if (ramUsageHistory == null || ramUsageHistory.Count == 0) throw new ArgumentNullException(nameof(ramUsageHistory));

            ExportDelimiter(path, ramUsageHistory, ",", true);
        }

        /// <summary>
        /// Export a list of RamUsage objects in Excel format to the disk
        /// </summary>
        /// <param name="path">The path where the RamUsage objects should be stored</param>
        /// <param name="ramUsageHistory">The list of RamUsage objects that should be exported</param>
        internal static void ExportExcel(string path, List<RamUsage> ramUsageHistory)
        {
            if (ramUsageHistory == null || ramUsageHistory.Count == 0) throw new ArgumentNullException(nameof(ramUsageHistory));
            ExportDelimiter(path, ramUsageHistory, ";", false);
        }

        /// <summary>
        /// Export a list of RamUsage objects using a delimiter character to disk
        /// </summary>
        /// <param name="path">The path where the RamUsage objects should be stored</param>
        /// <param name="ramUsageHistory">The list of RamUsage objects that should be exported</param>
        /// <param name="delimiter">The delimiter character that should be used</param>
        /// <param name="useQuotes">True if quotes should be applied to prevent data loss</param>
        private static void ExportDelimiter(string path, IReadOnlyList<RamUsage> ramUsageHistory, string delimiter, bool useQuotes)
        {
            if (ramUsageHistory == null || ramUsageHistory.Count == 0) throw new ArgumentNullException(nameof(ramUsageHistory));
            StringBuilder sb = new StringBuilder();
            sb.Append("Time" + delimiter + "Total usage" + delimiter + "Total" + delimiter + "Percentage" + Environment.NewLine);

            string quotes = "";
            if (useQuotes) quotes = "\"";

            for (int i = 0; i < ramUsageHistory.Count; i++)
            {
                sb.Append(ramUsageHistory[i].RecordedDate + delimiter + quotes + ramUsageHistory[i].TotalUsed + quotes + delimiter + quotes + ramUsageHistory[i].RamTotal + quotes + delimiter +  quotes + ramUsageHistory[i].UsagePercentage + quotes);
                if (i == ramUsageHistory.Count - 1) continue;
                sb.Append(Environment.NewLine);
            }

            Export(path, sb.ToString());
        }

        /// <summary>
        /// Export string data to a specific path
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="data">The string data that should be exported</param>
        private static void Export(string path, string data)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(data);
            }
        }
    }
}
