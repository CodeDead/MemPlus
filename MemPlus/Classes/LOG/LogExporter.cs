using System;
using System.Collections.Generic;
using System.IO;

namespace MemPlus.Classes.LOG
{
    /// <summary>
    /// Interaction logic for exporting logs
    /// </summary>
    internal static class LogExporter
    {
        /// <summary>
        /// Export a list of logs in HTML format to the disk
        /// </summary>
        /// <param name="path">The path where the logs should be stored</param>
        /// <param name="logList">The list of Log objects that should be exported</param>
        internal static void ExportHtml(string path, List<Log> logList)
        {
            string exportData = "<html>";

            exportData += "<head>";
            exportData += "<title>MemPlus - Export</title>";
            exportData += "</head>";

            exportData += "<body>";
            exportData += "<h1>MemPlus - Export (" + DateTime.Now + ")</h1>";
            exportData += "<table border=\"1\">";
            exportData += "<thead>";
            exportData += "<tr><th>Time</th><th>Data</th></tr>";
            exportData += "</thead>";
            exportData += "<tbody>";

            foreach (Log l in logList)
            {
                exportData += "<tr>";
                exportData += "<td>" + l.Time + "</td>";
                exportData += "<td>" + l.Data + "</td>";
                exportData += "</tr>";
            }

            exportData += "</tbody>";
            exportData += "</table>";
            exportData += "</body>";

            exportData += "</html>";

            Export(path, exportData);
        }

        /// <summary>
        /// Export a list of logs in TEXT format to the disk
        /// </summary>
        /// <param name="path">The path where the logs should be stored</param>
        /// <param name="logList">The list of Log objects that should be exported</param>
        internal static void ExportTxt(string path, List<Log> logList)
        {
            string exportData = "MemPlus - Export (" + DateTime.Now + ")";
            exportData += Environment.NewLine;

            for (int i = 0; i < logList.Count; i++)
            {
                exportData += "[" + logList[i].Time + "]\t" + logList[i].Data;
                if (i != logList.Count - 1)
                {
                    exportData += Environment.NewLine;
                }
            }

            Export(path, exportData);
        }

        /// <summary>
        /// Export a list of logs in CSV format to the disk
        /// </summary>
        /// <param name="path">The path where the logs should be stored</param>
        /// <param name="logList">The list of Log objects that should be exported</param>
        internal static void ExportCsv(string path, List<Log> logList)
        {
            ExportDelimiter(path, logList, ",");
        }

        /// <summary>
        /// Export a list of logs in Excel format to the disk
        /// </summary>
        /// <param name="path">The path where the logs should be stored</param>
        /// <param name="logList">The list of Log objects that should be exported</param>
        internal static void ExportExcel(string path, List<Log> logList)
        {
            ExportDelimiter(path, logList, ";");
        }

        /// <summary>
        /// Export a list of logs using a delimiter character to disk
        /// </summary>
        /// <param name="path">The path where the logs should be stored</param>
        /// <param name="logList">The list of Log objects that should be exported</param>
        /// <param name="delimiter">The delimiter character that should be used</param>
        private static void ExportDelimiter(string path, IReadOnlyList<Log> logList, string delimiter)
        {
            string exportData = "Time" + delimiter + "Data";
            exportData += Environment.NewLine;

            for (int i = 0; i < logList.Count; i++)
            {
                exportData += logList[i].Time + delimiter + logList[i].Data;
                if (i != logList.Count - 1)
                {
                    exportData += Environment.NewLine;
                }
            }

            Export(path, exportData);
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

    /// <summary>
    /// Enumaration containing all the different export types
    /// </summary>
    internal enum ExportType
    {
        Html,
        Text,
        Csv,
        Excel
    }
}
