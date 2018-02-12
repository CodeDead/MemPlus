using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MemPlus.Classes.GUI;
using MemPlus.Classes.LOG;
using Microsoft.Win32;

namespace MemPlus.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow
    {
        private readonly LogController _logController;
        private readonly LogType _logType;
        private bool _autoScroll;

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

        private void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading LogWindow properties"));
            Topmost = Properties.Settings.Default.Topmost;
            _logController.AddLog(new ApplicationLog("Done loading LogWindow properties"));
        }

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

        private void FillLogView()
        {
            foreach (Log l in _logController.GetLogs().Where(l => l.LogType == _logType))
            {
                LsvLogs.Items.Add(l);
            }
        }

        private void LogDeletedEvent(Log log)
        {
            if (log.LogType != _logType) return;
            Dispatcher.Invoke(() =>
            {
                LsvLogs.Items.Remove(log);
            });
        }

        private void LogsClearedEvent()
        {
            Dispatcher.Invoke(() =>
            {
                LsvLogs.Items.Clear();
            });
        }

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

        private void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing LogWindow theme style"));
            StyleManager.ChangeStyle(this);
            _logController.AddLog(new ApplicationLog("Done changing LogWindow theme style"));
        }

        private void LsvLogs_OnScroll(object sender, ScrollEventArgs e)
        {
            if (!(e.OriginalSource is ScrollBar sb)) return;

            if (sb.Orientation == Orientation.Horizontal)
                return;
            _autoScroll = Math.Abs(sb.Value - sb.Maximum) < 1;
        }

        private void BtnClear_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.ClearLogs(_logType);
        }

        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|HTML file (*.html)|*.html|CSV file (*.csv)|*.csv|Excel file (*.csv)|*.csv"
            };

            if (sfd.ShowDialog() != true) return;
            _logController.AddLog(new ApplicationLog("Exporting logs"));
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

        private void DeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvLogs.SelectedItems.Count == 0) return;
            _logController.RemoveLog(LsvLogs.SelectedItem as Log);
        }
    }
}
