namespace MemPlus.Classes.RAM.ViewModels
{
    internal sealed class ProcessData
    {
        public string ProcessName { get; set; }
        public string ProcessLocation { get; set; }
        public string WorkingSet { get; set; }
        public int Pid { get; set; }
    }
}
