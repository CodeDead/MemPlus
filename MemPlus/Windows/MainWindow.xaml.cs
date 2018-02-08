using System;
using System.Windows;
using MemPlus.Classes;

namespace MemPlus.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly RamMonitor _monitor;

        public MainWindow()
        {
            InitializeComponent();
            ChangeVisualStyle();
            _monitor = new RamMonitor(Dispatcher, CgRamUsage);
            _monitor.Start();
        }

        internal void ChangeVisualStyle()
        {
            StyleManager.ChangeStyle(this);
        }

        private void BtnClearMemory_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Clear working set of all processes that the user has access to
                Classes.MemPlus.EmptyWorkingSetFunction();
                //Clear file system cache
                Classes.MemPlus.ClearFileSystemCache(ChbFileSystemCache.IsChecked != null && ChbFileSystemCache.IsChecked.Value);

                MessageBox.Show("Your memory has now been cleared of any non-essential data.", "MemPlus",MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
