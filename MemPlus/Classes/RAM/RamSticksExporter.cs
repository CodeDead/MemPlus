using System.Collections.Generic;
using System.IO;
using MemPlus.Classes.RAM.ViewModels;

namespace MemPlus.Classes.RAM
{
    /// <summary>
    /// Static class that can be used to export RamStick information
    /// </summary>
    internal static class RamDataExporter
    {
        private static void Export(string path, string data)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(data);
            }
        }

        internal static void ExportText(string path, List<RamStick> ramSticks)
        {

        }

        internal static void ExportHtml(string path, List<RamStick> ramSticks)
        {

        }

        internal static void ExportCsv(string path, List<RamStick> ramSticks)
        {

        }

        internal static void ExportExcel(string path, List<RamStick> ramSticks)
        {

        }
    }
}
