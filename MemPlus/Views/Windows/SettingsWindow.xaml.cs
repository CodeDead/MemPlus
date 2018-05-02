using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using MemPlus.Business.GUI;
using MemPlus.Business.LOG;
using MemPlus.Business.UTILS;
using Microsoft.Win32;

namespace MemPlus.Views.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        #region Variables
        /// <summary>
        /// The MainWindow object that can be used to change the properties
        /// </summary>
        private readonly MainWindow _mainWindow;
        /// <summary>
        /// The LogController object that can be used to add logs
        /// </summary>
        private readonly LogController _logController;
        /// <summary>
        /// The Key that is being used as a hotkey for clearing the memory
        /// </summary>
        private Key _hotKey;
        /// <summary>
        /// The modifier keys that are used to control the hotkey
        /// </summary>
        private string _hotKeyModifiers;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new SettingsWindow object
        /// </summary>
        /// <param name="mainWindow">The MainWindow object that can be used to change the properties</param>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        public SettingsWindow(MainWindow mainWindow, LogController logController)
        {
            _logController = logController;
            _logController.AddLog(new ApplicationLog("Initializing SettingsWindow"));

            _mainWindow = mainWindow;

            InitializeComponent();
            ChangeVisualStyle();
            LoadProperties();

            _logController.AddLog(new ApplicationLog("Done initializing SettingsWindow"));
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        private void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing SettingsWindow theme style"));
            StyleManager.ChangeStyle(this);
            _logController.AddLog(new ApplicationLog("Done changing SettingsWindow theme style"));
        }

        /// <summary>
        /// Load the appropriate properties into the GUI
        /// </summary>
        private void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading SettingsWindow properties"));

            try
            {
                //General
                ChbAutoStart.IsChecked = Utils.AutoStartUp();
                ChbAutoUpdate.IsChecked = Properties.Settings.Default.AutoUpdate;
                ChbStartHidden.IsChecked = Properties.Settings.Default.HideOnStart;
                ChbHideOnClose.IsChecked = Properties.Settings.Default.HideOnClose;
                ChbRunAsAdmin.IsChecked = Properties.Settings.Default.RunAsAdministrator;
                ChbStartMinimized.IsChecked = Properties.Settings.Default.StartMinimized;
                if (Properties.Settings.Default.Topmost)
                {
                    ChbTopmost.IsChecked = Properties.Settings.Default.Topmost;
                    Topmost = true;
                }
                else
                {
                    Topmost = false;
                }

                ChbNotifyIcon.IsChecked = Properties.Settings.Default.NotifyIcon;
                if (Properties.Settings.Default.WindowDragging)
                {
                    // Remove previously added event handler if applicable
                    MouseDown -= OnMouseDown;
                    MouseDown += OnMouseDown;
                    ChbWindowDraggable.IsChecked = true;
                }
                else
                {
                    MouseDown -= OnMouseDown;
                    ChbWindowDraggable.IsChecked = false;
                }
                ChbAdminWarning.IsChecked = Properties.Settings.Default.AdministrativeWarning;
                ChbRamClearingMessage.IsChecked = Properties.Settings.Default.RamCleaningMessage;
                ChbNotifyIconStatistics.IsChecked = Properties.Settings.Default.NotifyIconStatistics;
                ChbDisplayGauge.IsChecked = Properties.Settings.Default.DisplayGauge;
                ChbWindowRamStatistics.IsChecked = Properties.Settings.Default.WindowRamStatistics;

                CboLanguage.SelectedIndex = Properties.Settings.Default.SelectedLanguage;

                //RAM Monitor
                ChbRamMonitor.IsChecked = Properties.Settings.Default.RamMonitor;
                ChbDisableInactive.IsChecked = Properties.Settings.Default.DisableOnInactive;

                int ramInterval = Properties.Settings.Default.RamMonitorInterval;
                switch (Properties.Settings.Default.RamMonitorIntervalIndex)
                {
                    case 0:
                        ItbRamMonitorTimeout.Value = ramInterval;
                        break;
                    case 1:
                        ItbRamMonitorTimeout.Value = ramInterval / 1000;
                        break;
                    case 2:
                        ItbRamMonitorTimeout.Value = ramInterval / 1000 / 60;
                        break;
                    case 3:
                        ItbRamMonitorTimeout.Value = ramInterval / 1000 / 60 / 60;
                        break;
                }
                CboRamMonitorInterval.SelectedIndex = Properties.Settings.Default.RamMonitorIntervalIndex;

                ChbAutoOptimizePercentage.IsChecked = Properties.Settings.Default.AutoOptimizePercentage;
                ItbAutoOptimizePercentage.Value = Properties.Settings.Default.AutoOptimizePercentageThreshold;

                ChbAutoOptimizeTimed.IsChecked = Properties.Settings.Default.AutoOptimizeTimed;

                CboAutoOptimizeTimedIndex.SelectedIndex = Properties.Settings.Default.AutoOptimizeTimedIntervalIndex;
                switch (Properties.Settings.Default.AutoOptimizeTimedIntervalIndex)
                {
                    case 0:
                        ItbAutoOptimizeTimed.Value = Properties.Settings.Default.AutoOptimizeTimedInterval / 1000 / 60;
                        break;
                    case 1:
                        ItbAutoOptimizeTimed.Value = Properties.Settings.Default.AutoOptimizeTimedInterval / 1000 / 60 / 60;
                        break;
                }

                ChbStartupMemoryClear.IsChecked = Properties.Settings.Default.StartupMemoryClear;
                ChbEmptyWorkingSet.IsChecked = Properties.Settings.Default.EmptyWorkingSet;
                ChbFileSystemCache.IsChecked = Properties.Settings.Default.FileSystemCache;
                ChbStandByCache.IsChecked = Properties.Settings.Default.StandByCache;
                ChbClearClipboard.IsChecked = Properties.Settings.Default.ClearClipboard;
                if (Properties.Settings.Default.ProcessExceptions != null)
                {
                    foreach (string s in Properties.Settings.Default.ProcessExceptions)
                    {
                        LsvExclusions.Items.Add(s);
                    }
                }
                else
                {
                    LsvExclusions.Items.Clear();
                }

                ChbHotKey.IsChecked = Properties.Settings.Default.UseHotKey;
                _hotKey = Properties.Settings.Default.HotKey;
                _hotKeyModifiers = Properties.Settings.Default.HotKeyModifiers;
                TxtHotKey.Text = _hotKeyModifiers + _hotKey;

                //Theme
                CboStyle.Text = Properties.Settings.Default.VisualStyle;
                CpMetroBrush.Color = Properties.Settings.Default.MetroColor;
                SldBorderThickness.Value = Properties.Settings.Default.BorderThickness;
                SldOpacity.Value = Properties.Settings.Default.WindowOpacity * 100;
                SldWindowResize.Value = Properties.Settings.Default.WindowResizeBorder;
                ItbWarningLevel.Value = Properties.Settings.Default.WarningLevel;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _logController.AddLog(new ApplicationLog("Done loading SettingsWindow properties"));
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
        /// Save all properties
        /// </summary>
        private void SaveProperties()
        {
            _logController.AddLog(new ApplicationLog("Saving properties"));
            try
            {
                //General
                if (ChbAutoStart.IsChecked != null && ChbAutoStart.IsChecked.Value)
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "MemPlus", System.Reflection.Assembly.GetExecutingAssembly().Location);
                } else if (ChbAutoStart.IsChecked != null && !ChbAutoStart.IsChecked.Value)
                {
                    if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "MemPlus","").ToString() != "")
                    {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                        {
                            key?.DeleteValue("MemPlus");
                        }
                    }
                }
                if (ChbAutoUpdate.IsChecked != null) Properties.Settings.Default.AutoUpdate = ChbAutoUpdate.IsChecked.Value;
                if (ChbTopmost.IsChecked != null) Properties.Settings.Default.Topmost = ChbTopmost.IsChecked.Value;
                if (ChbNotifyIcon.IsChecked != null) Properties.Settings.Default.NotifyIcon = ChbNotifyIcon.IsChecked.Value;
                if (ChbWindowDraggable.IsChecked != null) Properties.Settings.Default.WindowDragging = ChbWindowDraggable.IsChecked.Value;
                if (ChbAdminWarning.IsChecked != null) Properties.Settings.Default.AdministrativeWarning = ChbAdminWarning.IsChecked.Value;
                if (ChbRamClearingMessage.IsChecked != null) Properties.Settings.Default.RamCleaningMessage = ChbRamClearingMessage.IsChecked.Value;
                if (ChbNotifyIconStatistics.IsChecked != null) Properties.Settings.Default.NotifyIconStatistics = ChbNotifyIconStatistics.IsChecked.Value;
                if (ChbDisplayGauge.IsChecked != null) Properties.Settings.Default.DisplayGauge = ChbDisplayGauge.IsChecked.Value;
                if (ChbWindowRamStatistics.IsChecked != null) Properties.Settings.Default.WindowRamStatistics = ChbWindowRamStatistics.IsChecked.Value;
                if (ChbStartHidden.IsChecked != null) Properties.Settings.Default.HideOnStart = ChbStartHidden.IsChecked.Value;
                if (ChbHideOnClose.IsChecked != null) Properties.Settings.Default.HideOnClose = ChbHideOnClose.IsChecked.Value;
                if (ChbRunAsAdmin.IsChecked != null) Properties.Settings.Default.RunAsAdministrator = ChbRunAsAdmin.IsChecked.Value;
                if (ChbStartMinimized.IsChecked != null) Properties.Settings.Default.StartMinimized = ChbStartMinimized.IsChecked.Value;
                Properties.Settings.Default.SelectedLanguage = CboLanguage.SelectedIndex;

                //RAM Monitor
                if (ChbRamMonitor.IsChecked != null) Properties.Settings.Default.RamMonitor = ChbRamMonitor.IsChecked.Value;
                if (ChbDisableInactive.IsChecked != null) Properties.Settings.Default.DisableOnInactive = ChbDisableInactive.IsChecked.Value;

                Properties.Settings.Default.RamMonitorIntervalIndex = CboRamMonitorInterval.SelectedIndex;
                if (ItbRamMonitorTimeout.Value != null)
                {
                    int ramInterval = (int)ItbRamMonitorTimeout.Value;
                    switch (CboRamMonitorInterval.SelectedIndex)
                    {
                        case 1:
                            ramInterval = ramInterval * 1000;
                            break;
                        case 2:
                            ramInterval = ramInterval * 1000 * 60;
                            break;
                        case 3:
                            ramInterval = ramInterval * 1000 * 60 * 60;
                            break;
                    }

                    Properties.Settings.Default.RamMonitorInterval = ramInterval;
                }

                if (ChbAutoOptimizePercentage.IsChecked != null) Properties.Settings.Default.AutoOptimizePercentage = ChbAutoOptimizePercentage.IsChecked.Value;
                if (ItbAutoOptimizePercentage.Value != null) Properties.Settings.Default.AutoOptimizePercentageThreshold = (int)ItbAutoOptimizePercentage.Value;

                if (ChbAutoOptimizeTimed.IsChecked != null) Properties.Settings.Default.AutoOptimizeTimed = ChbAutoOptimizeTimed.IsChecked.Value;

                Properties.Settings.Default.AutoOptimizeTimedIntervalIndex = CboAutoOptimizeTimedIndex.SelectedIndex;
                if (ItbAutoOptimizeTimed.Value != null)
                {
                    switch (CboAutoOptimizeTimedIndex.SelectedIndex)
                    {
                        case 0:
                            Properties.Settings.Default.AutoOptimizeTimedInterval = (int)ItbAutoOptimizeTimed.Value * 1000 * 60;
                            break;
                        case 1:
                            Properties.Settings.Default.AutoOptimizeTimedInterval = (int)ItbAutoOptimizeTimed.Value * 1000 * 60 * 60;
                            break;
                    }
                }

                //RAM Optimizer
                if (ChbStartupMemoryClear.IsChecked != null) Properties.Settings.Default.StartupMemoryClear = ChbStartupMemoryClear.IsChecked.Value;
                if (ChbEmptyWorkingSet.IsChecked != null) Properties.Settings.Default.EmptyWorkingSet = ChbEmptyWorkingSet.IsChecked.Value;
                if (ChbFileSystemCache.IsChecked != null) Properties.Settings.Default.FileSystemCache = ChbFileSystemCache.IsChecked.Value;
                if (ChbStandByCache.IsChecked != null) Properties.Settings.Default.StandByCache = ChbStandByCache.IsChecked.Value;
                if (ChbClearClipboard.IsChecked != null) Properties.Settings.Default.ClearClipboard = ChbClearClipboard.IsChecked.Value;
                List<string> exclusionList = LsvExclusions.Items.Cast<string>().ToList();
                Properties.Settings.Default.ProcessExceptions = exclusionList;
                if (ChbHotKey.IsChecked != null) Properties.Settings.Default.UseHotKey = ChbHotKey.IsChecked.Value;
                Properties.Settings.Default.HotKey = _hotKey;
                Properties.Settings.Default.HotKeyModifiers = _hotKeyModifiers;
                

                //Theme
                Properties.Settings.Default.VisualStyle = CboStyle.Text;
                Properties.Settings.Default.MetroColor = CpMetroBrush.Color;
                Properties.Settings.Default.BorderThickness = SldBorderThickness.Value;
                Properties.Settings.Default.WindowOpacity = SldOpacity.Value / 100;
                Properties.Settings.Default.WindowResizeBorder = SldWindowResize.Value;
                if (ItbWarningLevel.Value != null) Properties.Settings.Default.WarningLevel = ItbWarningLevel.Value.Value;

                Properties.Settings.Default.Save();

                _mainWindow.ChangeVisualStyle();
                _mainWindow.LoadProperties();
                _mainWindow.LoadLanguage();
                _mainWindow.HotKeyModifier(new WindowInteropHelper(_mainWindow));
                ChangeVisualStyle();
                LoadProperties();

                _logController.AddLog(new ApplicationLog("Properties have been saved"));


                MessageBox.Show((string)Application.Current.FindResource("SavedAllSettings"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Reset all properties to their default values
        /// </summary>
        private void ResetSettings()
        {
            _logController.AddLog(new ApplicationLog("Resetting properties"));

            try
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "MemPlus", "").ToString() != "")
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        key?.DeleteValue("MemPlus");
                    }
                }

                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();

                _mainWindow.ChangeVisualStyle();
                _mainWindow.LoadProperties();
                _mainWindow.LoadLanguage();
                _mainWindow.HotKeyModifier(new WindowInteropHelper(_mainWindow));

                ChangeVisualStyle();
                LoadProperties();

                _logController.AddLog(new ApplicationLog("Properties have been reset"));

                MessageBox.Show((string)Application.Current.FindResource("ResetAllSettings"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when all properties should be reset to their default values
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            ResetSettings();
        }

        /// <summary>
        /// Method that is called when all properties should be saved
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveProperties();
        }

        /// <summary>
        /// Method that is called when an OpenFileDialog should be shown
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnFileView_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*" };

            if (ofd.ShowDialog() == true)
            {
                TxtExclusion.Text = ofd.FileName;
            }
        }

        /// <summary>
        /// Method that is called when an exclusion should be added to the list
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnAddExclusion_OnClick(object sender, RoutedEventArgs e)
        {
            if (TxtExclusion.Text.Length == 0) return;

            if (System.IO.File.Exists(TxtExclusion.Text))
            {
                if (LsvExclusions.Items.Contains(TxtExclusion.Text)) return;
                LsvExclusions.Items.Add(TxtExclusion.Text);
                TxtExclusion.Text = "";
            }
            else
            {
                MessageBox.Show((string)Application.Current.FindResource("FileDoesNotExist"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when an exclusion should be copied to the clipboard
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void CopyExclusionMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvExclusions.SelectedItems.Count == 0) return;
            try
            {
                Clipboard.SetText(LsvExclusions.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when an exclusion should be deleted from the list
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void DeleteExclusionMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvExclusions.SelectedItems.Count == 0) return;
            LsvExclusions.Items.Remove(LsvExclusions.SelectedItem);
        }

        /// <summary>
        /// Method that is called when all exclusions should be cleared
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ClearExclusionsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            LsvExclusions.Items.Clear();
        }

        /// <summary>
        /// Method that is called when ChbAutoOptimizePercentage is checked
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ChbAutoOptimizePercentage_OnChecked(object sender, RoutedEventArgs e)
        {
            if (ChbRamMonitor.IsChecked == null || ChbRamMonitor.IsChecked.Value) return;
            MessageBox.Show((string)Application.Current.FindResource("AutoOptimizeWarning"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            ChbRamMonitor.IsChecked = true;
        }

        /// <summary>
        /// Method that is called when an object is dropped on the ListView
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The DragEventArgs</param>
        private void LsvExclusions_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null) return;
            foreach (string s in files)
            {
                if (System.IO.File.Exists(s))
                {
                    LsvExclusions.Items.Add(s);
                }
            }
        }

        /// <summary>
        /// Method that is called when the border thickness should change
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedPropertyChangedEventArgs</param>
        private void SldBorderThickness_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BorderThickness = new Thickness(SldBorderThickness.Value);
        }

        /// <summary>
        /// Method that is called when the opacity should change dynamically
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedPropertyChangedEventArgs</param>
        private void SldOpacity_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Opacity = SldOpacity.Value / 100;
        }

        /// <summary>
        /// Method  that is called when the ResizeBorderThickness should change dynamically
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedPropertyChangedEventArgs</param>
        private void SldWindowResize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ResizeBorderThickness = new Thickness(SldWindowResize.Value);
        }

        /// <summary>
        /// Method that is called when the user is pressing a key on the textbox
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The KeyEventArgs</param>
        private void TxtHotKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == Key.Back || key == Key.LeftShift || key == Key.RightShift || key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftAlt || key == Key.RightAlt || key == Key.LWin || key == Key.RWin)
            {
                _hotKey = Key.None;
                _hotKeyModifiers = "";
                TxtHotKey.Text = "";
                return;
            }

            StringBuilder shortcutText = new StringBuilder();
            if ((Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
            {
                shortcutText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) != 0)
            {
                shortcutText.Append("Shift+");
            }
            if ((Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) != 0)
            {
                shortcutText.Append("Alt+");
            }

            _hotKeyModifiers = shortcutText.ToString();
            _hotKey = key;

            shortcutText.Append(key);

            TxtHotKey.Text = shortcutText.ToString();
        }
    }
}
