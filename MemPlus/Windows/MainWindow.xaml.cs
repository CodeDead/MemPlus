using System;
using System.Windows;
using System.Windows.Media;
using MemPlus.Classes.GUI;
using MemPlus.Classes.LOG;
using MemPlus.Classes.RAM;
using Microsoft.Win32;

namespace MemPlus.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly RamController _ramController;
        private readonly LogController _logController;

        private bool _rmEnabledBeforeInvisible;

        public MainWindow()
        {
            _logController = new LogController(600000);
            _logController.AddLog(new ApplicationLog("Initializing MainWindow"));

            InitializeComponent();
            ChangeVisualStyle();

            _ramController = new RamController(Dispatcher, CgRamUsage, LblTotalPhysicalMemory, LblAvailablePhysicalMemory, Properties.Settings.Default.RamMonitorInterval, _logController);

            Application app = Application.Current;
            app.Activated += Active;
            app.Deactivated += Passive;

            LoadProperties();
            _logController.AddLog(new ApplicationLog("Done initializing MainWindow"));
        }

        internal void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading MainWindow properties"));

            MniDisableInactive.IsChecked = Properties.Settings.Default.DisableOnInactive;
            MniOnTop.IsChecked = Properties.Settings.Default.Topmost;
            MniRamMonitor.IsChecked = Properties.Settings.Default.RamMonitor;

            _ramController.ClearFileSystemCache = Properties.Settings.Default.FileSystemCache;
            _ramController.ClearStandbyCache = Properties.Settings.Default.StandByCache;
            _ramController.SetTimerInterval(Properties.Settings.Default.RamMonitorInterval);

            if (Properties.Settings.Default.RamMonitor)
            {
                _rmEnabledBeforeInvisible = true;
                _ramController.EnableMonitor();
            }
            else
            {
                _rmEnabledBeforeInvisible = false;
            }

            _logController.AddLog(new ApplicationLog("Done loading MainWindow properties"));
        }

        private void Active(object sender, EventArgs args)
        {
            if (!Properties.Settings.Default.DisableOnInactive) return;
            if (Properties.Settings.Default.RamMonitor)
            {
                _ramController.EnableMonitor();
            }
            Overlay.Visibility = Visibility.Collapsed;
        }

        private void Passive(object sender, EventArgs args)
        {
            if (!Properties.Settings.Default.DisableOnInactive) return;
            if (!_ramController.RamMonitorEnabled) return;

            _ramController.DisableMonitor();
            Overlay.Visibility = Visibility.Visible;
        }

        internal void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing MainWindow theme style"));

            StyleManager.ChangeStyle(this);
            CgRamUsage.Scales[0].Ranges[0].Stroke = new SolidColorBrush(Properties.Settings.Default.MetroColor);

            _logController.AddLog(new ApplicationLog("Done changing MainWindow theme style"));
        }

        private async void BtnClearMemory_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.AddLog(new ApplicationLog("Clearing RAM Memory"));

            try
            {
                BtnClearMemory.IsEnabled = false;

                await _ramController.ClearMemory(Properties.Settings.Default.ProcessExceptions);
                double ramSavings = _ramController.RamSavings / 1024 / 1024;
                if (ramSavings < 0)
                {
                    ramSavings = Math.Abs(ramSavings);
                    _logController.AddLog(new ApplicationLog("RAM usage increase: " + ramSavings.ToString("F2") + " MB"));
                    MessageBox.Show("Looks like your RAM usage has increased with " + ramSavings.ToString("F2") + " MB!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logController.AddLog(new ApplicationLog("RAM usage decrease: " + ramSavings.ToString("F2") + " MB"));
                    MessageBox.Show("You saved " + ramSavings.ToString("F2") + " MB of RAM!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                BtnClearMemory.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _logController.AddLog(new ApplicationLog("Done clearing RAM memory"));
        }

        private void ClearLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.ClearLogs();
        }

        private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RamLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new LogWindow(_logController, LogType.Ram).Show();
        }

        private void ApplicationLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new LogWindow(_logController, LogType.Application).Show();
        }

        private void TopMenuItem_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Topmost = MniOnTop.IsChecked;
            Properties.Settings.Default.Topmost = Topmost;
            Properties.Settings.Default.Save();
        }

        private void HomePageMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _logController.AddLog(new ApplicationLog("Opening CodeDead website"));
                System.Diagnostics.Process.Start("https://codedead.com/");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DonateMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _logController.AddLog(new ApplicationLog("Opening donation website"));
                System.Diagnostics.Process.Start("https://codedead.com/?page_id=302");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AboutMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new AboutWindow(_logController).ShowDialog();
        }

        private void ExportLogs(LogType logType)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|HTML file (*.html)|*.html|CSV file (*.csv)|*.csv|Excel file (*.csv)|*.csv"
            };

            if (sfd.ShowDialog() != true) return;
            _logController.AddLog(new ApplicationLog("Exporting RAM logs"));
            ExportType type;
            switch (sfd.FilterIndex)
            {
                default:
                    type = ExportType.Text;
                    break;
                case 2:
                    type = ExportType.Html;
                    break;
                case 3:
                    type = ExportType.Csv;
                    break;
                case 4:
                    type = ExportType.Excel;
                    break;
            }

            try
            {
                _logController.Export(sfd.FileName, logType, type);

                MessageBox.Show("All logs have been exported!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                _logController.AddLog(new ApplicationLog("Done exporting RAM logs"));
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RamExportMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExportLogs(LogType.Ram);
        }

        private void ApplicationExportMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExportLogs(LogType.Application);
        }

        private void OpenTbItem_Click(object sender, RoutedEventArgs e)
        {
            if (IsVisible)
            {
                _logController.AddLog(new ApplicationLog("MainWindow is now hidden"));
                Hide();

                if (_rmEnabledBeforeInvisible)
                {
                    _ramController.DisableMonitor();
                }
            }
            else
            {
                _logController.AddLog(new ApplicationLog("MainWindow is now visible"));
                Show();

                if (_rmEnabledBeforeInvisible)
                {
                    _ramController.EnableMonitor();
                }
            }
        }

        private void DisableInactiveMenuItem_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DisableOnInactive = MniDisableInactive.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void SettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(this, _logController).ShowDialog();
        }

        private void RamMonitorMenuItem_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (MniRamMonitor.IsChecked)
            {
                Properties.Settings.Default.RamMonitor = true;
                _rmEnabledBeforeInvisible = false;
                _ramController.EnableMonitor();
            }
            else
            {
                Properties.Settings.Default.RamMonitor = false;
                _ramController.DisableMonitor();
            }

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Open the file containing the license for MemPlus
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void LicenseMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _logController.AddLog(new ApplicationLog("Opening MemPlus license file"));
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\gpl.pdf");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open the file containing the help documentation for MemPlus
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void HelpMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _logController.AddLog(new ApplicationLog("Opening MemPlus help file"));
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\help.pdf");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
