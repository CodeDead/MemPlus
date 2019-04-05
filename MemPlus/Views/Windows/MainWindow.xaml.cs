using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Hardcodet.Wpf.TaskbarNotification;
using MemPlus.Business.GUI;
using MemPlus.Business.LOG;
using MemPlus.Business.RAM;
using MemPlus.Business.UTILS;
using Syncfusion.UI.Xaml.Gauges;
using UpdateManager.Classes;
using Application = System.Windows.Application;
using DataFormats = System.Windows.DataFormats;
using MessageBox = System.Windows.MessageBox;

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
        /// <summary>
        /// A boolean to indicate whether RAM cleaning is currently in progress
        /// </summary>
        private bool _clearingMemory;
        /// <summary>
        /// The HotKeyController that can be used to register a hotkey for fast memory cleaning
        /// </summary>
        private HotKeyController _hotKeyController;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new MainWindow object
        /// </summary>
        public MainWindow()
        {
            try
            {
                if (Properties.Settings.Default.SaveLogsToFile)
                {
                    if (string.IsNullOrEmpty(Properties.Settings.Default.LogPath))
                    {
                        Properties.Settings.Default.SaveLogsToFile = false;
                        Properties.Settings.Default.Save();
                    }
                }

                _logController = new LogController
                (
                    Properties.Settings.Default.LoggingEnabled,
                    Properties.Settings.Default.LogClearAuto,
                    Properties.Settings.Default.LogClearInterval,
                    Properties.Settings.Default.SaveLogsToFile,
                    Properties.Settings.Default.LogPath
                );
            }
            catch (Exception ex)
            {
                _logController = new LogController();
                _logController.AddLog(new ErrorLog(ex.Message));
            }

            _logController.AddLog(new ApplicationLog("Initializing MainWindow"));

            _clearingMemory = false;

            LoadLanguage();
            InitializeComponent();
            ChangeVisualStyle();

            _updateManager = new UpdateManager.Classes.UpdateManager(Assembly.GetExecutingAssembly().GetName().Version, "https://codedead.com/Software/MemPlus/update.xml", LoadUpdateManagerStrings());

            try
            {
                _ramController = new RamController
                (
                    UpdateGuiStatistics,
                    RamClearingCompleted,
                    Properties.Settings.Default.RamMonitorInterval,
                    Properties.Settings.Default.EnableRamStatistics,
                    Properties.Settings.Default.RamMaxUsageHistoryCount,
                    _logController
                );
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
            }

            Application app = Application.Current;
            app.Activated += Active;
            app.Deactivated += Passive;

            LoadProperties();

            try
            {
                if (!Utils.IsAdministrator())
                {
                    if (Properties.Settings.Default.RunAsAdministrator)
                    {
                        Utils.RunAsAdministrator(_logController);
                    }
                    else if (Properties.Settings.Default.AdministrativeWarning)
                    {
                        MessageBox.Show((string)Application.Current.FindResource("AdministrativeRightsWarning"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                _logController.AddLog(new ApplicationLog("Checking for application updates"));
                if (Properties.Settings.Default.AutoUpdate)
                {
                    _updateManager.CheckForUpdate(false, false);
                }
                _logController.AddLog(new ApplicationLog("Done checking for application updates"));

                if (Properties.Settings.Default.HideOnStart)
                {
                    Hide();
                }
                if (Properties.Settings.Default.StartMinimized)
                {
                    WindowState = WindowState.Minimized;
                }

                if (Properties.Settings.Default.StartupMemoryClear)
                {
                    ClearMemory(0);
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _logController.AddLog(new ApplicationLog("Done initializing MainWindow"));
        }

        /// <summary>
        /// Load the strings that are required for the UpdateManager library
        /// </summary>
        /// <returns></returns>
        private static StringVariables LoadUpdateManagerStrings()
        {
            StringVariables stringVariables = new StringVariables
            {
                CancelButtonText = (string)Application.Current.FindResource("Cancel"),
                DownloadButtonText = (string)Application.Current.FindResource("Download"),
                InformationButtonText = (string)Application.Current.FindResource("Information"),
                NoNewVersionText = (string)Application.Current.FindResource("RunningLatestVersion"),
                TitleText = "MemPlus",
                UpdateNowText = (string)Application.Current.FindResource("UpdateNow")
            };
            return stringVariables;
        }

        /// <summary>
        /// Change the language of MemPlus
        /// </summary>
        internal void LoadLanguage()
        {
            GuiManager.ChangeLanguage(_logController);

            try
            {
                _updateManager?.SetStringVariables(LoadUpdateManagerStrings());
                TbiIcon?.InvalidateVisual();
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the GUI statistics should be updated
        /// </summary>
        private void UpdateGuiStatistics(RamUsage ramUsage)
        {
            Dispatcher.Invoke(() =>
            {
                string ramTotal = (ramUsage.RamTotal / 1024 / 1024 / 1024).ToString("F2") + " GB";
                string ramAvailable = (ramUsage.TotalUsed / 1024 / 1024 / 1024).ToString("F2") + " GB";
                CgRamUsage.Scales[0].Pointers[0].Value = ramUsage.UsagePercentage;
                CgRamUsage.GaugeHeader = ((string)Application.Current.FindResource("GaugeRamUsage"))?.Replace("%", ramUsage.UsagePercentage.ToString("F2") + "%");
                LblTotalPhysicalMemory.Content = ramTotal;
                LblAvailablePhysicalMemory.Content = ramAvailable;

                LblAvailablePhysicalMemory.Foreground = ramUsage.UsagePercentage >= Properties.Settings.Default.WarningLevel ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);


                if (!Properties.Settings.Default.NotifyIconStatistics) return;
                string tooltipText = "MemPlus";
                tooltipText += Environment.NewLine;
                tooltipText += (string)Application.Current.FindResource("TotalPhysicalMemory") + " " + ramTotal;
                tooltipText += Environment.NewLine;
                tooltipText += (string)Application.Current.FindResource("UsedPhysicalMemory") + " " + ramAvailable;

                TbiIcon.ToolTipText = tooltipText;
            });
        }

        /// <summary>
        /// Method that is called when a RAM clearing has occurred and statistics could be shown to the user
        /// </summary>
        private void RamClearingCompleted()
        {
            double ramSavings = _ramController.RamSavings / 1024 / 1024;
            string message;
            if (ramSavings < 0)
            {
                ramSavings = Math.Abs(ramSavings);
                _logController.AddLog(new RamLog("RAM usage increase: " + ramSavings.ToString("F2") + " MB"));
                message = ((string)Application.Current.FindResource("RamUsageIncreased"))?.Replace("%", ramSavings.ToString("F2"));
            }
            else
            {
                _logController.AddLog(new RamLog("RAM usage decrease: " + ramSavings.ToString("F2") + " MB"));
                message = ((string)Application.Current.FindResource("RamUsageSaved"))?.Replace("%", ramSavings.ToString("F2"));
            }

            try
            {
                if (!Properties.Settings.Default.RamCleaningMessage) return;
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (Visibility)
                {
                    default:
                        MessageBox.Show(message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case Visibility.Hidden when TbiIcon.Visibility == Visibility.Visible:
                        TbiIcon.ShowBalloonTip("MemPlus", message, BalloonIcon.Info);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Load the properties into the GUI
        /// </summary>
        internal void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading MainWindow properties"));

            try
            {
                MniWindowDraggable.IsChecked = Properties.Settings.Default.WindowDragging;
                MniRamStatistics.IsChecked = Properties.Settings.Default.WindowRamStatistics;
                MniRamGauge.IsChecked = Properties.Settings.Default.DisplayGauge;
                MniRamMonitor.IsChecked = Properties.Settings.Default.RamMonitor;

                _ramController.EnableRamStatistics = Properties.Settings.Default.EnableRamStatistics;
                _ramController.MaxUsageHistoryCount = Properties.Settings.Default.RamMaxUsageHistoryCount;
                _ramController.SetProcessExceptionList(Properties.Settings.Default.ProcessExceptions);
                _ramController.EmptyWorkingSets = Properties.Settings.Default.EmptyWorkingSet;
                _ramController.ClearFileSystemCache = Properties.Settings.Default.FileSystemCache;
                _ramController.ClearStandbyCache = Properties.Settings.Default.StandByCache;
                _ramController.FillRam = Properties.Settings.Default.FillRam;
                _ramController.FillRamMaxRuns = Properties.Settings.Default.FillRamMaxRuns;
                _ramController.InvokeGarbageCollector = Properties.Settings.Default.InvokeGarbageCollector;
                _ramController.SetRamUpdateTimerInterval(Properties.Settings.Default.RamMonitorInterval);
                _ramController.AutoOptimizeTimed(Properties.Settings.Default.AutoOptimizeTimed, Properties.Settings.Default.AutoOptimizeTimedInterval);

                if (!Properties.Settings.Default.EnableRamStatistics)
                {
                    _ramController.ClearRamUsageHistory();
                }

                if (!Properties.Settings.Default.NotifyIconStatistics)
                {
                    TbiIcon.ToolTipText = "MemPlus";
                }

                _ramController.AutoOptimizePercentage = Properties.Settings.Default.AutoOptimizePercentage;
                _ramController.SetAutoOptimizeThreshold(Properties.Settings.Default.AutoOptimizePercentageThreshold);
                _ramController.ClearClipboard = Properties.Settings.Default.ClearClipboard;

                if (Properties.Settings.Default.RamMonitor)
                {
                    _ramController.EnableMonitor();
                }

                if (Properties.Settings.Default.DragDropClear)
                {
                    AllowDrop = true;
                    Drop += MainWindow_Drop;
                }
                else
                {
                    AllowDrop = false;
                    Drop -= MainWindow_Drop;
                }

                TbiIcon.Visibility = !Properties.Settings.Default.NotifyIcon ? Visibility.Hidden : Visibility.Visible;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            RamGaugeVisibility();
            RamStatisticsVisibility();
            WindowDraggable();

            _logController.AddLog(new ApplicationLog("Done loading MainWindow properties"));
        }

        /// <inheritdoc />
        /// <summary>
        /// Method that is called when the source is initialized
        /// </summary>
        /// <param name="e">The EventArgs</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HotKeyModifier(new WindowInteropHelper(this));
        }

        /// <summary>
        /// Register a hotkey or not, depending on the properties
        /// </summary>
        internal void HotKeyModifier(WindowInteropHelper helper)
        {
            _logController?.AddLog(new ApplicationLog("Initializing hotkey hook"));
            try
            {
                if (Properties.Settings.Default.UseHotKey)
                {
                    _hotKeyController?.Dispose();

                    if (Properties.Settings.Default.HotKey == Key.None) return;

                    _hotKeyController = new HotKeyController(helper, _logController);
                    _hotKeyController.HotKeyPressedEvent += HotKeyPressed;

                    string[] mods = Properties.Settings.Default.HotKeyModifiers.Split('+');

                    uint values = 0;
                    foreach (string s in mods)
                    {
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (s)
                        {
                            case "Ctrl":
                                values |= (uint)ModifierKeys.Control;
                                break;
                            case "Alt":
                                values |= (uint)ModifierKeys.Alt;
                                break;
                            case "Shift":
                                values |= (uint)ModifierKeys.Shift;
                                break;
                        }
                    }
                    _hotKeyController.RegisterHotKey(values, (Keys)KeyInterop.VirtualKeyFromKey(Properties.Settings.Default.HotKey));
                }
                else
                {
                    _hotKeyController?.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logController?.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _logController?.AddLog(new ApplicationLog("Done initializing hotkey hook"));
        }

        /// <summary>
        /// Method that is called when a specific set of keys was pressed
        /// </summary>
        private void HotKeyPressed()
        {
            ClearMemory(0);
        }

        /// <summary>
        /// Change the visibility of the RAM Gauge
        /// </summary>
        private void RamGaugeVisibility()
        {
            try
            {
                if (Properties.Settings.Default.DisplayGauge)
                {
                    CgRamUsage.Visibility = Visibility.Visible;
                    SepGauge.Visibility = Visibility.Visible;
                }
                else
                {
                    CgRamUsage.Visibility = Visibility.Collapsed;
                    SepGauge.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Check whether the RAM statistics should be displayed in the MainWindow
        /// </summary>
        private void RamStatisticsVisibility()
        {
            try
            {
                GrdRamStatistics.Visibility = Properties.Settings.Default.WindowRamStatistics ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Check whether the Window should be draggable or not
        /// </summary>
        private void WindowDraggable()
        {
            try
            {
                if (Properties.Settings.Default.WindowDragging)
                {
                    // Delete event handler first to prevent duplicate handlers
                    MouseDown -= OnMouseDown;
                    MouseDown += OnMouseDown;
                }
                else
                {
                    MouseDown -= OnMouseDown;
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                _logController.AddLog(new ErrorLog(ex.Message));
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
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        internal void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing MainWindow theme style"));

            GuiManager.ChangeStyle(this);

            try
            {
                SolidColorBrush brush = new SolidColorBrush(Properties.Settings.Default.MetroColor);

                CircularRange rangeNormal = CgRamUsage.Scales[0].Ranges[0];
                CircularRange rangeWarning = CgRamUsage.Scales[0].Ranges[1];

                rangeNormal.Stroke = brush;
                rangeNormal.StartValue = 0;
                rangeNormal.EndValue = Properties.Settings.Default.WarningLevel;

                rangeWarning.StartValue = Properties.Settings.Default.WarningLevel;
                rangeWarning.EndValue = 100;

                CgRamUsage.Scales[0].Pointers[0].NeedlePointerStroke = brush;
                CgRamUsage.Scales[0].Pointers[0].PointerCapStroke = brush;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _logController.AddLog(new ApplicationLog("Done changing MainWindow theme style"));
        }

        /// <summary>
        /// Method that is called when the memory should be optimized
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnClearMemory_OnClick(object sender, RoutedEventArgs e)
        {
            ClearMemory(0);
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
        /// Method that is called when all the Log objects should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void AllMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new LogWindow(_logController, null).Show();
        }

        /// <summary>
        /// Method that is called when the RamLogs should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void RamLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new LogWindow(_logController, LogType.Ram).Show();
        }

        /// <summary>
        /// Method that is called when the ProcessLog objects should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ProcessLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new LogWindow(_logController, LogType.Process).Show();
        }

        /// <summary>
        /// Method that is called when the ApplicationLog objects should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ApplicationLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new LogWindow(_logController, LogType.Application).Show();
        }

        /// <summary>
        /// Method that is called when the ErrorLog objects should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ErrorLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new LogWindow(_logController, LogType.Error).Show();
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
                Process.Start("https://codedead.com/");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
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
                Process.Start("https://codedead.com/?page_id=302");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
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
        /// Method that is called when all RamLog objects should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void RamExportMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExportLogs(LogType.Ram);
        }

        /// <summary>
        /// Method that is called when all ProcessLog objects should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ProcessExportMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExportLogs(LogType.Process);
        }

        /// <summary>
        /// Method that is called when all ApplicationLog objects should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ApplicationExportMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExportLogs(LogType.Application);
        }

        /// <summary>
        /// Method that is called when all ErrorLog objects should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ErrorExportMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExportLogs(LogType.Error);
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
        /// Export logs of a certain type
        /// </summary>
        /// <param name="logType">The type of log that needs to be exported</param>
        private void ExportLogs(LogType? logType)
        {
            if (Utils.ExportLogs(logType, _logController))
            {
                MessageBox.Show((string)Application.Current.FindResource("ExportedAllData"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the Window draggable setting should be changed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void WindowDraggableMenuItem_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.WindowDragging = MniWindowDraggable.IsChecked;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            WindowDraggable();
        }

        /// <summary>
        /// Method that is called when the RAM statistics visibility should be changed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void RamStatisticsMenuItem_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.WindowRamStatistics = MniRamStatistics.IsChecked;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            RamStatisticsVisibility();
        }

        /// <summary>
        /// Method that is called when the Ram Gauge visibility should change
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void RamGaugeMenuItem_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.DisplayGauge = MniRamGauge.IsChecked;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            RamGaugeVisibility();
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
                _logController.AddLog(new ErrorLog(ex.Message));
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
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\gpl.pdf");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
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
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\help.pdf");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
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
            new RamAnalyzerWindow(_logController).Show();
        }

        /// <summary>
        /// Method that is called when the RamStatisticsWindow should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The </param>
        private void RamStatisticsMenuitem_OnClick(object sender, RoutedEventArgs e)
        {
            new RamStatisticsWindow(_ramController, _logController).Show();
        }

        /// <summary>
        /// Method that is called when the ProcessAnalyzer window should be displayed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ProcessAnalyzerMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new ProcessAnalyzerWindow(_logController).Show();
        }

        /// <summary>
        /// Method that is called when the RamAnalyzer data should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ExportRamAnalyzerDataMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (Utils.ExportRamSticks(_logController))
            {
                MessageBox.Show((string)Application.Current.FindResource("ExportedAllData"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Method that is called when the RamUsage objects should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ExportRamStatisticsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (Utils.ExportRamUsage(_ramController, _logController))
            {
                MessageBox.Show((string)Application.Current.FindResource("ExportedAllData"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Method that is called when the ProcessAnalyzer data should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private async void ExportProcessAnalyzerDataMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (await Utils.ExportProcessDetails(_logController))
            {
                MessageBox.Show((string)Application.Current.FindResource("ExportedAllData"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the working set of processes should be cleared
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ClearWorkingSetsDropDownMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ClearMemory(1);
        }

        /// <summary>
        /// Method that is called when the FileSystem cache should be cleared
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ClearFileSystemCacheDropDownMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ClearMemory(2);
        }

        /// <summary>
        /// Method that is called when the RAM should be filled
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void FillRamDropDownMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ClearMemory(3);
        }

        /// <summary>
        /// Clear the memory
        /// </summary>
        /// <param name="index">The type of memory that needs to be cleared</param>
        private async void ClearMemory(int index)
        {
            if (_clearingMemory) return;
            if (!_ramController.EmptyWorkingSets && !_ramController.ClearFileSystemCache && !_ramController.ClearClipboard && !_ramController.FillRam) return;

            _logController.AddLog(new ApplicationLog("Clearing RAM Memory"));
            _clearingMemory = true;

            try
            {
                BtnClearMemory.IsEnabled = false;

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (index)
                {
                    case 0:
                        await _ramController.ClearMemory();
                        break;
                    case 1:
                        await _ramController.ClearWorkingSets();
                        break;
                    case 2:
                        await _ramController.ClearFileSystemCaches();
                        break;
                    case 3:
                        await _ramController.FillRamData();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            BtnClearMemory.IsEnabled = true;
            _clearingMemory = false;

            _logController.AddLog(new ApplicationLog("Done clearing RAM memory"));
        }

        /// <summary>
        /// Method that is called when the MainWindow is closing
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The CancelEventArgs</param>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            try
            {
                if (Properties.Settings.Default.HideOnClose && Visibility == Visibility.Visible)
                {
                    Hide();
                    e.Cancel = true;
                }
                else
                {
                    // Save properties
                    Properties.Settings.Default.Save();
                    // De-register any hotkeys, if applicable
                    _hotKeyController?.Dispose();
                    // Dispose the RamController object gracefully
                    _ramController?.Dispose();
                    TbiIcon?.Dispose();
                    // Dispose of the LogController object gracefully
                    _logController.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when a file is dropped on the MainWindow object
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The System.Windows.DragEventArgs</param>
        private void MainWindow_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null) return;

            foreach (string s in files)
            {
                if (!System.IO.File.Exists(s)) continue;
                List<Process> processes = Utils.GetProcessesForFile(s);
                try
                {
                    if (processes.Count == 0) return;
                    foreach (Process p in processes)
                    {
                        _logController.AddLog(new RamLog("Emptying working set for process: " + p.ProcessName));
                        NativeMethods.EmptyWorkingSet(p.Handle);
                        _logController.AddLog(new RamLog("Successfully emptied working set for process " + p.ProcessName));
                    }
                }
                catch (Exception ex)
                {
                    _logController.AddLog(new ErrorLog(ex.Message));
                    MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
