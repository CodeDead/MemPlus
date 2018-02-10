using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MemPlus.Classes;
using MemPlus.Classes.LOG;

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

            FillLogView();

            _logController.LogAddedEvent += LogAddedEvent;
            _logController.LogsClearedEvent += LogsClearedEvent;
            _logController.LogDeletedEvent += LogDeletedEvent;
            _logController.LogTypeClearedEvent += LogTypeClearedEvent;

            _autoScroll = true;

            _logController.AddLog(new ApplicationLog("Done initializing LogWindow"));
        }

        private void LogTypeClearedEvent(List<Log> clearedList)
        {
            foreach (Log l in clearedList)
            {
                LsvLogs.Items.Remove(l);
            }
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
            LsvLogs.Items.Remove(log);
        }

        private void LogsClearedEvent()
        {
            LsvLogs.Items.Clear();
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
            throw new NotImplementedException();
        }
    }
}
