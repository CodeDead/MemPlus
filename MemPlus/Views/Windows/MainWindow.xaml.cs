using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MemPlus.Business.Classes.EXPORT;
using MemPlus.Business.Classes.GUI;
using MemPlus.Business.Classes.LOG;
using MemPlus.Business.Classes.RAM;
using MemPlus.Business.Classes.UTILS;
using Microsoft.Win32;

namespace MemPlus.Views.Windows
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
        private readonly UpdateManager.Classes.UpdateManager _updateManager;
        /// <summary>
        /// The RamController object that can be used to clear the memory and view memory statistics
        /// </summary>
        private readonly RamController _ramController;
        /// <summary>
        /// The LogController object that can be used to add new logs
        /// </summary>
        private readonly LogController _logController;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new MainWindow object
        /// </summary>
        public MainWindow()
        {
            _logController = new LogController(600000);
            _logController.AddLog(new ApplicationLog("Initializing MainWindow"));
            _updateManager = new UpdateManager.Classes.UpdateManager(Assembly.GetExecutingAssembly().GetName().Version, "https://codedead.com/Software/MemPlus/update.xml", "MemPlus", "Information", "Cancel", "Download", "No new version is currently available.");

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

            try
            {
                if (Properties.Settings.Default.HideOnStart)
                {
                    Hide();
                }

                if (Properties.Settings.Default.StartMinimized)
                {
                    WindowState = WindowState.Minimized;
                }

                if (Properties.Settings.Default.AdministrativeWarning)
                {
                    if (!Utils.IsAdministrator())
                    {
                        MessageBox.Show("MemPlus might not function correctly without administrative rights!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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

                _ramController.AutoOptimizePercentage = Properties.Settings.Default.AutoOptimizePercentage;
                _ramController.SetAutoOptimizeThreshold(Properties.Settings.Default.AutoOptimizePercentageThreshold);

                if (Properties.Settings.Default.RamMonitor)
                {
                    _ramController.EnableMonitor();
                }

                TbiIcon.Visibility = !Properties.Settings.Default.NotifyIcon ? Visibility.Hidden : Visibility.Visible;

                if (Properties.Settings.Default.WindowDragging)
                {
                    MouseDown += OnMouseDown;
                }
                else
                {
                    MouseDown -= OnMouseDown;
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
        /// Method that is called when the Window should be dragged
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The MouseButtonEventArgs</param>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
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

            SolidColorBrush brush = new SolidColorBrush(Properties.Settings.Default.MetroColor);
            CgRamUsage.Scales[0].Ranges[0].Stroke = brush;
            CgRamUsage.Scales[0].Pointers[0].NeedlePointerStroke = brush;
            CgRamUsage.Scales[0].Pointers[0].PointerCapStroke = brush;

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
                    _logController.AddLog(new RamLog("RAM usage increase: " + ramSavings.ToString("F2") + " MB"));
                    MessageBox.Show("Looks like your RAM usage has increased with " + ramSavings.ToString("F2") + " MB!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logController.AddLog(new RamLog("RAM usage decrease: " + ramSavings.ToString("F2") + " MB"));
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
            _logController.AddLog(new ApplicationLog("Opening CodeDead website"));
            try
            {
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
            _logController.AddLog(new ApplicationLog("Opening donation website"));
            try
            {
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
        /// <param name="logType">The LogType that should be exported. Null to export all logs</param>
        private void ExportLogs(LogType? logType)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|HTML file (*.html)|*.html|CSV file (*.csv)|*.csv|Excel file (*.csv)|*.csv"
            };

            if (sfd.ShowDialog() != true) return;
            ExportTypes.ExportType type;
            switch (sfd.FilterIndex)
            {
                default:
                    type = ExportTypes.ExportType.Text;
                    break;
                case 2:
                    type = ExportTypes.ExportType.Html;
                    break;
                case 3:
                    type = ExportTypes.ExportType.Csv;
                    break;
                case 4:
                    type = ExportTypes.ExportType.Excel;
                    break;
            }

            try
            {
                _logController.Export(sfd.FileName, logType, type);

                MessageBox.Show("All logs have been exported!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
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
            try
            {
                if (IsVisible)
                {
                    Hide();
                    if (Properties.Settings.Default.DisableOnInactive)
                    {
                        _ramController.DisableMonitor();
                    }
                    _logController.AddLog(new ApplicationLog("MainWindow is now hidden"));
                }
                else
                {
                    Show();
                    if (Properties.Settings.Default.RamMonitor)
                    {
                        _ramController.EnableMonitor();
                    }
                    _logController.AddLog(new ApplicationLog("MainWindow is now visible"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
                _logController.AddLog(new ApplicationLog(ex.Message));
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
            _logController.AddLog(new ApplicationLog("Opening MemPlus license file"));
            try
            {
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
            _logController.AddLog(new ApplicationLog("Opening MemPlus help file"));
            try
            {
                System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\help.pdf");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the user wants to check for updates
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void UpdateMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.AddLog(new ApplicationLog("Checking for application updates"));
            _updateManager.CheckForUpdate(true, true);
            _logController.AddLog(new ApplicationLog("Done checking for application updates"));
        }

        /// <summary>
        /// Method that is called when the RamAnalyzer window should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void RamAnalyzerMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new AnalyzerWindow(_logController).Show();
        }

        /// <summary>
        /// Method that is called when the RamAnalyzer data should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ExportRamAnalyzerDataMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|HTML file (*.html)|*.html|CSV file (*.csv)|*.csv|Excel file (*.csv)|*.csv"
            };
            if (sfd.ShowDialog() != true) return;
            try
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (sfd.FilterIndex)
                {
                    //Filterindex starts at 1
                    case 1:
                        RamDataExporter.ExportText(sfd.FileName, RamAnalyzer.GetRamSticks());
                        break;
                    case 2:
                        RamDataExporter.ExportHtml(sfd.FileName, RamAnalyzer.GetRamSticks());
                        break;
                    case 3:
                        RamDataExporter.ExportCsv(sfd.FileName, RamAnalyzer.GetRamSticks());
                        break;
                    case 4:
                        RamDataExporter.ExportExcel(sfd.FileName, RamAnalyzer.GetRamSticks());
                        break;
                }
                MessageBox.Show("All data has been exported!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the MainWindow is closing
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The CancelEventArgs</param>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            TbiIcon.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Method that is called when all logs should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ExportAllLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExportLogs(null);
        }

        /// <summary>
        /// Method that is called when the application should restart
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void RestartMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
