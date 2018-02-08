using System;
using System.Windows;
using System.Windows.Media;
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
            CgRamUsage.Scales[0].Ranges[0].Stroke = new SolidColorBrush(Properties.Settings.Default.MetroColor);
        }

        private async void BtnClearMemory_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnClearMemory.IsEnabled = false;

                await _ramController.ClearMemory(true);
                double ramSavings = _ramController.RamSavings / 1024 / 1024;
                if (ramSavings < 0)
                {
                    MessageBox.Show("Looks like your RAM usage has increased with " + Math.Abs(ramSavings).ToString("F2") + "MB!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("You saved " + ramSavings.ToString("F2") + "MB of RAM!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                BtnClearMemory.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
