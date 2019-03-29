using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MemPlus.Business.LOG;

namespace MemPlus.Business.EXPORT
{
    /// <summary>
    /// Internal static class containing the logic for exporting logs
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal static class LogExporter
    {
        /// <summary>
        /// Export a list of Log objects in HTML format to the disk
        /// </summary>
        /// <param name="path">The path where the Log objects should be stored</param>
        /// <param name="logList">The list of Log objects that should be exported</param>
        internal static void ExportHtml(string path, List<Log> logList)
        {
            if (logList == null || logList.Count == 0) throw new ArgumentNullException();
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>MemPlus - Log Export</title></head><body><h1>MemPlus - Log Export (" + DateTime.Now + ")</h1><table border=\"1\"><thead><tr><th>Time</th><th>Data</th></tr></thead><tbody>");

            foreach (Log l in logList)
            {
                sb.Append("<tr><td>" + l.Time + "</td><td>" + l.Data + "</td></tr>");
            }

            sb.Append("</tbody></table></body></html>");

            Export(path, sb.ToString());
        }

        /// <summary>
        /// Export a list of Log objects in TEXT format to the disk
        /// </summary>
        /// <param name="path">The path where the Log objects should be stored</param>
        /// <param name="logList">The list of Log objects that should be exported</param>
        internal static void ExportTxt(string path, List<Log> logList)
        {
            if (logList == null || logList.Count == 0) throw new ArgumentNullException();
            StringBuilder sb = new StringBuilder();
            sb.Append("MemPlus - Log Export (" + DateTime.Now + ")" + Environment.NewLine);

            for (int i = 0; i < logList.Count; i++)
            {
                sb.Append("[" + logList[i].Time + "]\t" + logList[i].Data);
                if (i == logList.Count - 1) continue;
                sb.Append(Environment.NewLine);
            }

            Export(path, sb.ToString());
        }

        /// <summary>
        /// Export a list of Log objects in CSV format to the disk
        /// </summary>
        /// <param name="path">The path where the Log objects should be stored</param>
        /// <param name="logList">The list of Log objects that should be exported</param>
        internal static void ExportCsv(string path, List<Log> logList)
        {
            if (logList == null || logList.Count == 0) throw new ArgumentNullException();
            ExportDelimiter(path, logList, ",");
        }

        /// <summary>
        /// Export a list of Log objects in Excel format to the disk
        /// </summary>
        /// <param name="path">The path where the Log objects should be stored</param>
        /// <param name="logList">The list of Log objects that should be exported</param>
        internal static void ExportExcel(string path, List<Log> logList)
        {
            if (logList == null || logList.Count == 0) throw new ArgumentNullException();
            ExportDelimiter(path, logList, ";");
        }

        /// <summary>
        /// Export a list of Log objects using a delimiter character to disk
        /// </summary>
        /// <param name="path">The path where the Log objects should be stored</param>
        /// <param name="logList">The list of Log objects that should be exported</param>
        /// <param name="delimiter">The delimiter character that should be used</param>
        private static void ExportDelimiter(string path, IReadOnlyList<Log> logList, string delimiter)
        {
            if (logList == null || logList.Count == 0) throw new ArgumentNullException();
            StringBuilder sb = new StringBuilder();
            sb.Append("Time" + delimiter + "Data" + Environment.NewLine);

            for (int i = 0; i < logList.Count; i++)
            {
                sb.Append(logList[i].Time + delimiter + logList[i].Data);
                if (i == logList.Count - 1) continue;
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
