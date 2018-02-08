using System;
using System.Windows;
using MemPlus.Classes;
using MemPlus.Classes.RAM;

namespace MemPlus.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly RamController _ramController;

        public MainWindow()
        {
            InitializeComponent();
            ChangeVisualStyle();

            _ramController = new RamController(Dispatcher, CgRamUsage, 5000);
            _ramController.EnableMonitor();
        }

        internal void ChangeVisualStyle()
        {
            StyleManager.ChangeStyle(this);
        }

        private async void BtnClearMemory_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await _ramController.ClearMemory(ChbFileSystemCache.IsChecked != null && ChbFileSystemCache.IsChecked.Value);
                double ramSavings = _ramController.RamSavings / 1024 / 1024;
                if (ramSavings < 0)
                {
                    MessageBox.Show("Looks like your RAM usage has increased with " + Math.Abs(ramSavings).ToString("F2") + "MB!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("You saved " + ramSavings.ToString("F2") + "MB of RAM memory!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
