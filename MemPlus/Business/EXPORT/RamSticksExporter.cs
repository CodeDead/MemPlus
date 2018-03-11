using System;
using System.Collections.Generic;
using System.IO;
using MemPlus.Business.RAM;

namespace MemPlus.Business.EXPORT
{
    /// <summary>
    /// Static class that can be used to export RamStick information
    /// </summary>
    internal static class RamDataExporter
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
        /// Export a list of RamStick objects to a specific path in text format
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="ramSticks">The list of RamStick objects that need to be exported</param>
        internal static void ExportText(string path, List<RamStick> ramSticks)
        {
            if (ramSticks == null || ramSticks.Count == 0) throw new ArgumentNullException();

            string exportData = "MemPlus - Ram Analyzer Data (" + DateTime.Now + ")";
            exportData += Environment.NewLine;
            exportData += "---";
            exportData += Environment.NewLine;

            for (int index = 0; index < ramSticks.Count; index++)
            {
                RamStick stick = ramSticks[index];
                List<RamData> ramDataList = stick.GetRamData();
                for (int i = 0; i < ramDataList.Count; i++)
                {
                    exportData += ramDataList[i].Key + "\t" + ramDataList[i].Value;
                    if (i != ramDataList.Count - 1)
                    {
                        exportData += Environment.NewLine;
                    }
                }

                if (index == ramSticks.Count - 1) continue;
                exportData += Environment.NewLine;
                exportData += "----------";
                exportData += Environment.NewLine;
            }

            Export(path, exportData);
        }

        /// <summary>
        /// Export a list of RamStick objects to a specific path in HTML format
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="ramSticks">The list of RamStick objects that need to be exported</param>
        internal static void ExportHtml(string path, List<RamStick> ramSticks)
        {
            if (ramSticks == null || ramSticks.Count == 0) throw new ArgumentNullException();

            string exportData = "<html>";

            exportData += "<head>";
            exportData += "<title>MemPlus - Ram Analyzer Data</title>";
            exportData += "</head>";

            exportData += "<body>";
            exportData += "<h1>MemPlus - Ram Analyzer Data (" + DateTime.Now + ")</h1>";

            for (int index = 0; index < ramSticks.Count; index++)
            {
                RamStick stick = ramSticks[index];
                exportData += "<table border=\"1\">";
                exportData += "<thead>";
                exportData += "<tr><th>Key</th><th>Value</th></tr>";
                exportData += "</thead>";
                exportData += "<tbody>";

                foreach (RamData data in stick.GetRamData())
                {
                    exportData += "<tr>";
                    exportData += "<td>" + data.Key + "</td>";
                    exportData += "<td>" + data.Value + "</td>";
                    exportData += "</tr>";
                }

                exportData += "</tbody>";
                exportData += "</table>";

                if (index != ramSticks.Count - 1)
                {
                    exportData += "<br />";
                }
            }

            exportData += "</body>";

            exportData += "</html>";

            Export(path, exportData);
        }

        /// <summary>
        /// Export a list of RamStick objects to a specific path in CSV format
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="ramSticks">The list of RamStick objects that need to be exported</param>
        internal static void ExportCsv(string path, List<RamStick> ramSticks)
        {
            ExportDelimiter(path, ",", ramSticks);
        }

        /// <summary>
        /// Export a list of RamStick objects to a specific path in Excel format
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="ramSticks">The list of RamStick objects that need to be exported</param>
        internal static void ExportExcel(string path, List<RamStick> ramSticks)
        {
            ExportDelimiter(path, ";", ramSticks);
        }

        /// <summary>
        /// Export a list of RamStick objects using a specific delimiter character
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="delimiter">The delimiter that should be used to split the data</param>
        /// <param name="ramSticks">The list of RamStick objects that need to be exported</param>
        private static void ExportDelimiter(string path, string delimiter, IReadOnlyList<RamStick> ramSticks)
        {
            if (ramSticks == null || ramSticks.Count == 0) throw new ArgumentNullException();

            string exportData = "Key" + delimiter + "Value";
            exportData += Environment.NewLine;

            for (int i = 0; i < ramSticks.Count; i++)
            {
                List<RamData> ramData = ramSticks[i].GetRamData();
                for (int index = 0; index < ramData.Count; index++)
                {
                    exportData += ramData[index].Key + delimiter + ramData[index].Value;
                    if (index != ramData.Count - 1)
                    {
                        exportData += Environment.NewLine;
                    }
                }

                if (i == ramSticks.Count - 1) continue;
                exportData += Environment.NewLine;
                exportData += "----------" + delimiter + "----------";
                exportData += Environment.NewLine;
            }

            Export(path, exportData);
        }
    }
}
