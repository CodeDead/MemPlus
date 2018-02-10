using System.Collections.Generic;

namespace MemPlus.Classes.LOG
{
    /// <summary>
    /// Interaction logic for exporting logs
    /// </summary>
    internal static class LogExporter
    {
        internal static void ExportHtml(string path, List<Log> logList)
        {

        }

        internal static void ExportTxt(string path, List<Log> logList)
        {

        }

        internal static void ExportCsv(string path, List<Log> logList)
        {

        }

        internal static void ExportExcel(string path, List<Log> logList)
        {

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
