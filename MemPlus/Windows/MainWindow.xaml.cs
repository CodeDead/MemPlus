using System;
using System.Reflection;
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
        #region Variables
        /// <summary>
        /// The UpdateManager object that checks for application updates
        /// </summary>
        private readonly UpdateManager.UpdateManager _updateManager;
        /// <summary>
        /// The RamController object that can be used to clear the memory and view memory statistics
        /// </summary>
        private readonly RamController _ramController;
        /// <summary>
        /// The LogController object that can be used to add new logs
        /// </summary>
        private readonly LogController _logController;
        /// <summary>
        /// A boolean to indicate whether the RAM Monitor was enabled or not before an operation
        /// </summary>
        private bool _rmEnabledBeforeInvisible;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new MainWindow object
        /// </summary>
        public MainWindow()
        {
            _logController = new LogController(600000);
            _updateManager = new UpdateManager.UpdateManager(Assembly.GetExecutingAssembly().GetName().Version, "https://codedead.com/Software/MemPlus/update.xml", "MemPlus");
            _logController.AddLog(new ApplicationLog("Initializing MainWindow"));

            InitializeComponent();
            ChangeVisualStyle();

            try
            {
                _ramController = new RamController(Dispatcher, CgRamUsage, LblTotalPhysicalMemory, LblAvailablePhysicalMemory, Properties.Settings.Default.RamMonitorInterval, _logController);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
            }
            

            Application app = Application.Current;
            app.Activated += Active;
            app.Deactivated += Passive;

            LoadProperties();
            AutoUpdate();

            _logController.AddLog(new ApplicationLog("Done initializing MainWindow"));
        }

        /// <summary>
        /// Automatically check for updates
        /// </summary>
        private void AutoUpdate()
        {
            _logController.AddLog(new ApplicationLog("Checking for application updates"));
            try
            {
                if (Properties.Settings.Default.AutoUpdate)
                {
                    _updateManager.CheckForUpdate(false, false);
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _logController.AddLog(new ApplicationLog("Done checking for application updates"));
        }

        /// <summary>
        /// Load the properties into the GUI
        /// </summary>
        internal void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading MainWindow properties"));

            try
            {
                MniDisableInactive.IsChecked = Properties.Settings.Default.DisableOnInactive;
                MniOnTop.IsChecked = Properties.Settings.Default.Topmost;
                MniRamMonitor.IsChecked = Properties.Settings.Default.RamMonitor;

                _ramController.SetProcessExceptionList(Properties.Settings.Default.ProcessExceptions);
                _ramController.ClearFileSystemCache = Properties.Settings.Default.FileSystemCache;
                _ramController.ClearStandbyCache = Properties.Settings.Default.StandByCache;
                _ramController.SetRamUpdateTimerInterval(Properties.Settings.Default.RamMonitorInterval);
                _ramController.AutoOptimizeTimed(Properties.Settings.Default.AutoOptimizeTimed, Properties.Settings.Default.AutoOptimizeTimedInterval);

                if (Properties.Settings.Default.RamMonitor)
                {
                    _rmEnabledBeforeInvisible = true;
                    _ramController.EnableMonitor();
                }
                else
                {
                    _rmEnabledBeforeInvisible = false;
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _logController.AddLog(new ApplicationLog("Done loading MainWindow properties"));
        }

        /// <summary>
        /// Method that is called when the Application is active
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="args">The EventArgs</param>
        private void Active(object sender, EventArgs args)
        {
            try
            {
                if (!Properties.Settings.Default.DisableOnInactive) return;
                if (Properties.Settings.Default.RamMonitor)
                {
                    _ramController.EnableMonitor();
                }
                Overlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the Application is passive
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="args">The EventArgs</param>
        private void Passive(object sender, EventArgs args)
        {
            try
            {
                if (!Properties.Settings.Default.DisableOnInactive) return;
                if (!_ramController.RamMonitorEnabled) return;

                _ramController.DisableMonitor();
                Overlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        internal void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing MainWindow theme style"));

            StyleManager.ChangeStyle(this);
            CgRamUsage.Scales[0].Ranges[0].Stroke = new SolidColorBrush(Properties.Settings.Default.MetroColor);

            _logController.AddLog(new ApplicationLog("Done changing MainWindow theme style"));
        }

        /// <summary>
        /// Method that is called when the memory should be optimized
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private async void BtnClearMemory_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.AddLog(new ApplicationLog("Clearing RAM Memory"));

            try
            {
                BtnClearMemory.IsEnabled = false;

                await _ramController.ClearMemory();
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

        /// <summary>
        /// Method that is called when all logs should be cleared
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ClearLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.ClearLogs();
        }

        /// <summary>
        /// Method that is called when the application should exit
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Method that is called when the RAM Optimizer logs should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void RamLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new LogWindow(_logController, LogType.Ram).Show();
        }

        /// <summary>
        /// Method that is called when the Application logs should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ApplicationLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new LogWindow(_logController, LogType.Application).Show();
        }

        /// <summary>
        /// Method that is called when the Topmost property should be changed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void TopMenuItem_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Topmost = MniOnTop.IsChecked;
                Properties.Settings.Default.Topmost = Topmost;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the CodeDead homepage should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
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

        /// <summary>
        /// Method that is called when the CodeDead donation page should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
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

        /// <summary>
        /// Method that is called when the about information should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void AboutMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new AboutWindow(_logController).ShowDialog();
        }

        /// <summary>
        /// Method that is called when Log objects from a specific type should be exported
        /// </summary>
        /// <param name="logType">The LogType that should be exported</param>
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

        /// <summary>
        /// Method that is called when all RAM Optimizer logs should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void RamExportMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExportLogs(LogType.Ram);
        }

        /// <summary>
        /// Method that is called when all Application logs should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ApplicationExportMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExportLogs(LogType.Application);
        }

        /// <summary>
        /// Method that is called when the notifyicon option is clicked to show or hide the Application
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void OpenTbItem_Click(object sender, RoutedEventArgs e)
        {
            if (IsVisible)
            {
                Hide();

                if (_rmEnabledBeforeInvisible)
                {
                    _ramController.DisableMonitor();
                }
                _logController.AddLog(new ApplicationLog("MainWindow is now hidden"));
            }
            else
            {
                Show();

                if (_rmEnabledBeforeInvisible)
                {
                    _ramController.EnableMonitor();
                }
                _logController.AddLog(new ApplicationLog("MainWindow is now visible"));
            }
        }

        /// <summary>
        /// Method that is called when the RAM Monitor should be disabled or not when the Application is inactive
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void DisableInactiveMenuItem_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.DisableOnInactive = MniDisableInactive.IsChecked;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the SettingsWindow should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void SettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(this, _logController).ShowDialog();
        }

        /// <summary>
        /// Method that is called when the RAM Monitor should be enabled or disabled
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void RamMonitorMenuItem_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void UpdateMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.AddLog(new ApplicationLog("Checking for application updates"));
            _updateManager.CheckForUpdate(true, true);
            _logController.AddLog(new ApplicationLog("Done checking for application updates"));
        }
    }
}
