using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using MemPlus.Business.GUI;
using MemPlus.Business.LOG;
using MemPlus.Business.UTILS;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

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
        internal SettingsWindow(MainWindow mainWindow, LogController logController)
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
            GuiManager.ChangeStyle(this);
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
                // General
                ChbAutoStart.IsChecked = Utils.AutoStartUp();

                if (Properties.Settings.Default.WindowDragging)
                {
                    // Remove previously added event handler if applicable
                    MouseDown -= OnMouseDown;
                    MouseDown += OnMouseDown;
                }
                else
                {
                    MouseDown -= OnMouseDown;
                }

                // Logging
                int clearInterval = Properties.Settings.Default.LogClearInterval;
                switch (Properties.Settings.Default.LogClearIntervalIndex)
                {
                    case 0:
                        ItbAutoClearLogsInterval.Value = clearInterval;
                        break;
                    default:
                        ItbAutoClearLogsInterval.Value = clearInterval / 1000;
                        break;
                    case 2:
                        ItbAutoClearLogsInterval.Value = clearInterval / 1000 / 60;
                        break;
                    case 3:
                        ItbAutoClearLogsInterval.Value = clearInterval / 1000 / 60 / 60;
                        break;
                }
                CboLogClearInterval.SelectedIndex = Properties.Settings.Default.LogClearIntervalIndex;

                // RAM Monitor

                int ramInterval = Properties.Settings.Default.RamMonitorInterval;
                switch (Properties.Settings.Default.RamMonitorIntervalIndex)
                {
                    case 0:
                        ItbRamMonitorTimeout.Value = ramInterval;
                        break;
                    default:
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

                CboAutoOptimizeTimedIndex.SelectedIndex = Properties.Settings.Default.AutoOptimizeTimedIntervalIndex;
                switch (Properties.Settings.Default.AutoOptimizeTimedIntervalIndex)
                {
                    default:
                        ItbAutoOptimizeTimed.Value = Properties.Settings.Default.AutoOptimizeTimedInterval / 1000 / 60;
                        break;
                    case 1:
                        ItbAutoOptimizeTimed.Value = Properties.Settings.Default.AutoOptimizeTimedInterval / 1000 / 60 / 60;
                        break;
                }

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

                _hotKey = Properties.Settings.Default.HotKey;
                _hotKeyModifiers = Properties.Settings.Default.HotKeyModifiers;
                TxtHotKey.Text = _hotKeyModifiers + _hotKey;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
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
                // General
                if (ChbAutoStart.IsChecked != null && ChbAutoStart.IsChecked.Value)
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "MemPlus", System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                else if (ChbAutoStart.IsChecked != null && !ChbAutoStart.IsChecked.Value)
                {
                    if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", "MemPlus", "").ToString() != "")
                    {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                        {
                            key?.DeleteValue("MemPlus");
                        }
                    }
                }

                // Logging
                _logController.SetLoggingEnabled(Properties.Settings.Default.LoggingEnabled);
                _logController.SetAutoClear(Properties.Settings.Default.LogClearAuto);

                Properties.Settings.Default.LogClearIntervalIndex = CboLogClearInterval.SelectedIndex;

                if (ItbAutoClearLogsInterval.Value != null)
                {
                    int logInterval = (int) ItbAutoClearLogsInterval.Value;
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (CboLogClearInterval.SelectedIndex)
                    {
                        case 1:
                            logInterval = logInterval * 1000;
                            break;
                        case 2:
                            logInterval = logInterval * 1000 * 60;
                            break;
                        case 3:
                            logInterval = logInterval * 1000 * 60 * 60;
                            break;
                    }
                    Properties.Settings.Default.LogClearInterval = logInterval;
                    _logController.SetAutoClearInterval(logInterval);
                }

                if (Properties.Settings.Default.SaveLogsToFile && Properties.Settings.Default.LogPath.Length == 0)
                {
                    Properties.Settings.Default.SaveLogsToFile = false;
                }

                if (Properties.Settings.Default.LogPath.Length > 0)
                {
                    _logController.SetSaveDirectory(Properties.Settings.Default.LogPath);
                }

                /*
                 * Make sure this is the last LogController method that is called when saving the settings
                 * because this will only work properly when all other settings (especially the directory)
                 * have been set correctly
                 */
                _logController.SetSaveToFile(Properties.Settings.Default.SaveLogsToFile);

                // RAM Monitor
                Properties.Settings.Default.RamMonitorIntervalIndex = CboRamMonitorInterval.SelectedIndex;
                if (ItbRamMonitorTimeout.Value != null)
                {
                    int ramInterval = (int)ItbRamMonitorTimeout.Value;
                    // ReSharper disable once SwitchStatementMissingSomeCases
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

                Properties.Settings.Default.AutoOptimizeTimedIntervalIndex = CboAutoOptimizeTimedIndex.SelectedIndex;
                if (ItbAutoOptimizeTimed.Value != null)
                {
                    switch (CboAutoOptimizeTimedIndex.SelectedIndex)
                    {
                        default:
                            Properties.Settings.Default.AutoOptimizeTimedInterval = (int)ItbAutoOptimizeTimed.Value * 1000 * 60;
                            break;
                        case 1:
                            Properties.Settings.Default.AutoOptimizeTimedInterval = (int)ItbAutoOptimizeTimed.Value * 1000 * 60 * 60;
                            break;
                    }
                }

                // RAM Optimizer
                List<string> exclusionList = LsvExclusions.Items.Cast<string>().ToList();
                Properties.Settings.Default.ProcessExceptions = exclusionList;
                Properties.Settings.Default.HotKey = _hotKey;
                Properties.Settings.Default.HotKeyModifiers = _hotKeyModifiers;

                // Theme
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
                _logController.AddLog(new ErrorLog(ex.Message));
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

                _logController.SetAutoClear(Properties.Settings.Default.LogClearAuto);
                _logController.SetAutoClearInterval(Properties.Settings.Default.LogClearInterval);
                _logController.SetSaveDirectory(Properties.Settings.Default.LogPath);
                _logController.SetSaveToFile(Properties.Settings.Default.SaveLogsToFile);

                ChangeVisualStyle();
                LoadProperties();

                _logController.AddLog(new ApplicationLog("Properties have been reset"));

                MessageBox.Show((string)Application.Current.FindResource("ResetAllSettings"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
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
            if (TxtExclusion.Text.Length == 0 || LsvExclusions.Items.Contains(TxtExclusion.Text)) return;

            if (System.IO.File.Exists(TxtExclusion.Text))
            {
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
                _logController.AddLog(new ErrorLog(ex.Message));
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
            try
            {
                if (Properties.Settings.Default.RamMonitor) return;
                MessageBox.Show((string)Application.Current.FindResource("AutoOptimizeWarning"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                Properties.Settings.Default.RamMonitor = true;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            try
            {
                BorderThickness = new Thickness(Properties.Settings.Default.BorderThickness);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the opacity should change dynamically
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedPropertyChangedEventArgs</param>
        private void SldOpacity_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                Opacity = Properties.Settings.Default.WindowOpacity / 100;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method  that is called when the ResizeBorderThickness should change dynamically
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedPropertyChangedEventArgs</param>
        private void SldWindowResize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                ResizeBorderThickness = new Thickness(Properties.Settings.Default.WindowResizeBorder);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the user is pressing a key on the TextBox object
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

        /// <summary>
        /// Method that is called when the user clicks the button to select the path for saving log files
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnSelectLogFilePath(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            TxtLogFilePath.Text = fbd.SelectedPath;

            try
            {
                Properties.Settings.Default.LogPath = TxtLogFilePath.Text;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the Window is closing
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The CancelEventArgs</param>
        private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            try
            {
                Properties.Settings.Default.Reload();
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the Theme has changed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The SelectionChangedEventArgs</param>
        private void ThemeSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeVisualStyle();
        }
    }
}
