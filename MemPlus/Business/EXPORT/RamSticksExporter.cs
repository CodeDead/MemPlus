using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

            StringBuilder sb = new StringBuilder();

            sb.Append("MemPlus - Ram Analyzer Data (" + DateTime.Now + ")");
            sb.Append(Environment.NewLine + "---" + Environment.NewLine);

            for (int index = 0; index < ramSticks.Count; index++)
            {
                RamStick stick = ramSticks[index];
                List<RamData> ramDataList = stick.GetRamData();
                for (int i = 0; i < ramDataList.Count; i++)
                {
                    sb.Append(ramDataList[i].Key + "\t" + ramDataList[i].Value);
                    if (i == ramDataList.Count - 1) continue;
                    sb.Append(Environment.NewLine);
                }

                if (index == ramSticks.Count - 1) continue;
                sb.Append(Environment.NewLine + "----------" + Environment.NewLine);
            }

            Export(path, sb.ToString());
        }

        /// <summary>
        /// Export a list of RamStick objects to a specific path in HTML format
        /// </summary>
        /// <param name="path">The path where the data should be stored</param>
        /// <param name="ramSticks">The list of RamStick objects that need to be exported</param>
        internal static void ExportHtml(string path, List<RamStick> ramSticks)
        {
            if (ramSticks == null || ramSticks.Count == 0) throw new ArgumentNullException();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>MemPlus - Ram Analyzer Data</title></head><body><h1>MemPlus - Ram Analyzer Data (" + DateTime.Now + ")</h1>");

            for (int index = 0; index < ramSticks.Count; index++)
            {
                RamStick stick = ramSticks[index];
                sb.Append("<table border=\"1\"><thead><tr><th>Key</th><th>Value</th></tr></thead><tbody>");

                foreach (RamData data in stick.GetRamData())
                {
                    sb.Append("<tr><td>" + data.Key + "</td><td>" + data.Value + "</td></tr>");
                }

                sb.Append("</tbody></table>");

                if (index == ramSticks.Count - 1) continue;
                sb.Append("<br />");
            }

            sb.Append("</body></html>");

            Export(path, sb.ToString());
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
            StringBuilder sb = new StringBuilder();

            sb.Append("Key" + delimiter + "Value" + Environment.NewLine);

            for (int i = 0; i < ramSticks.Count; i++)
            {
                List<RamData> ramData = ramSticks[i].GetRamData();
                for (int index = 0; index < ramData.Count; index++)
                {
                    sb.Append(ramData[index].Key + delimiter + ramData[index].Value);
                    if (index == ramData.Count - 1) continue;
                    sb.Append(Environment.NewLine);
                }

                if (i == ramSticks.Count - 1) continue;
                sb.Append(Environment.NewLine + "----------" + delimiter + "----------" + Environment.NewLine);
            }

            Export(path, sb.ToString());
        }
    }
}
