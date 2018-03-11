using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MemPlus.Business.EXPORT;
using MemPlus.Business.GUI;
using MemPlus.Business.LOG;
using Microsoft.Win32;

namespace MemPlus.Views.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow
    {
        #region Variables
        /// <summary>
        /// The LogController object that can be used to add logs
        /// </summary>
        private readonly LogController _logController;
        /// <summary>
        /// The LogType that is currently being monitored
        /// </summary>
        private readonly LogType _logType;
        /// <summary>
        /// A boolean to indicate whether automatic scrolling is enabled or not
        /// </summary>
        private bool _autoScroll;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new LogWindow object
        /// </summary>
        /// <param name="logController">The LogController object that can be used to add and view logs</param>
        /// <param name="logType">The LogType that is currently being monitored</param>
        public LogWindow(LogController logController, LogType logType)
        {
            _logController = logController;
            _logController.AddLog(new ApplicationLog("Initializing LogWindow"));

            _logType = logType;

            InitializeComponent();
            ChangeVisualStyle();
            LoadProperties();

            FillLogView();

            _logController.LogAddedEvent += LogAddedEvent;
            _logController.LogsClearedEvent += LogsClearedEvent;
            _logController.LogDeletedEvent += LogDeletedEvent;
            _logController.LogTypeClearedEvent += LogTypeClearedEvent;

            _autoScroll = true;

            _logController.AddLog(new ApplicationLog("Done initializing LogWindow"));
        }

        /// <summary>
        /// Load the current properties into the GUI
        /// </summary>
        private void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading LogWindow properties"));
            try
            {
                Topmost = Properties.Settings.Default.Topmost;
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
            _logController.AddLog(new ApplicationLog("Done loading LogWindow properties"));
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
        /// Method that will be called when all logs of a certain type have been cleared
        /// </summary>
        /// <param name="clearedList">The list of Log objects that were removed</param>
        private void LogTypeClearedEvent(List<Log> clearedList)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (Log l in clearedList)
                {
                    LsvLogs.Items.Remove(l);
                }
            });
        }

        /// <summary>
        /// Fill the ListView will all current Log objects
        /// </summary>
        private void FillLogView()
        {
            foreach (Log l in _logController.GetLogs(_logType))
            {
                LsvLogs.Items.Add(l);
            }
        }

        /// <summary>
        /// Method that will be called when a Log object was removed
        /// </summary>
        /// <param name="log">The Log object that was removed</param>
        private void LogDeletedEvent(Log log)
        {
            if (log.LogType != _logType) return;
            Dispatcher.Invoke(() =>
            {
                LsvLogs.Items.Remove(log);
            });
        }

        /// <summary>
        /// Method that will be called when all logs were removed
        /// </summary>
        private void LogsClearedEvent()
        {
            Dispatcher.Invoke(() =>
            {
                LsvLogs.Items.Clear();
            });
        }

        /// <summary>
        /// Method that will be called when a Log object was added
        /// </summary>
        /// <param name="log">The Log object that was added</param>
        private void LogAddedEvent(Log log)
        {
            if (log.LogType != _logType) return;
            Dispatcher.Invoke(() =>
            {
                LsvLogs.Items.Add(log);

                if (_autoScroll)
                {
                    LsvLogs.ScrollIntoView(LsvLogs.Items[LsvLogs.Items.Count - 1]);
                }
            });
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        private void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing LogWindow theme style"));
            StyleManager.ChangeStyle(this);
            _logController.AddLog(new ApplicationLog("Done changing LogWindow theme style"));
        }

        /// <summary>
        /// Method that is called when a scoll action happened in the ListView
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The ScrollEventArgs</param>
        private void LsvLogs_OnScroll(object sender, ScrollEventArgs e)
        {
            if (!(e.OriginalSource is ScrollBar sb)) return;

            if (sb.Orientation == Orientation.Horizontal)
                return;
            _autoScroll = Math.Abs(sb.Value - sb.Maximum) < 1;
        }

        /// <summary>
        /// Method that will be called when all logs of a certain type should be cleared
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnClear_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.ClearLogs(_logType);
        }

        /// <summary>
        /// Method that will be called when all Logs of a certain type should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|HTML file (*.html)|*.html|CSV file (*.csv)|*.csv|Excel file (*.csv)|*.csv"
            };

            if (sfd.ShowDialog() != true) return;
            _logController.AddLog(new ApplicationLog("Exporting logs"));
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
                _logController.Export(sfd.FileName, _logType, type);

                MessageBox.Show("All logs have been exported!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                _logController.AddLog(new ApplicationLog("Done exporting logs"));
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that will be called when a Log object should be removed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void DeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvLogs.SelectedItems.Count == 0) return;
            _logController.RemoveLog(LsvLogs.SelectedItem as Log);
        }

        /// <summary>
        /// Method that will be called when a Log object should be copied to the clipboard
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void CopyMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvLogs.SelectedItems.Count == 0) return;
            if (!(LsvLogs.SelectedItem is Log selectedLog)) return;
            try
            {
                Clipboard.SetText(selectedLog.Time + "\t" + selectedLog.Data);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
