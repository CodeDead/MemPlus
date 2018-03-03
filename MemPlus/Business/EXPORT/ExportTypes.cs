namespace MemPlus.Business.EXPORT
{
    /// <summary>
    /// Sealed class containing all different export types that MemPlus supports
    /// </summary>
    internal sealed class ExportTypes
    {
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
}
