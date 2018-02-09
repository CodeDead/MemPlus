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
    /// Interaction logic for ApplicationLogWindow.xaml
    /// </summary>
    public partial class ApplicationLogWindow
    {

        private readonly LogController _logController;
        private bool _autoScroll;

        public ApplicationLogWindow(LogController logController)
        {
            _logController = logController;

            _logController.AddLog(new ApplicationLog("Initializing Application Log Window"));

            InitializeComponent();
            ChangeVisualStyle();

            _logController.AddLog(new ApplicationLog("Done initializing Application Log Window"));

            FillLogView();

            _logController.LogAddedEvent += LogAddedEvent;
            _logController.LogClearedEvent += LogClearedEvent;
            _logController.LogDeletedEvent += LogDeletedEvent;
            _logController.LogTypeClearedEvent += LogTypeClearedEvent;

            _autoScroll = true;
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
            foreach (Log l in _logController.GetLogs().Where(l => l.LogType == LogType.Application))
            {
                LsvLogs.Items.Add(l);
            }
        }

        private void LogDeletedEvent(Log log)
        {
            if (log.LogType != LogType.Application) return;
            LsvLogs.Items.Remove(log);
        }

        private void LogClearedEvent()
        {
            LsvLogs.Items.Clear();
        }

        private void LogAddedEvent(Log log)
        {
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
            _logController.AddLog(new ApplicationLog("Changing MainWindow theme style"));
            StyleManager.ChangeStyle(this);
            _logController.AddLog(new ApplicationLog("Done changing MainWindow theme style"));
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
            _logController.ClearLogs(LogType.Application);
        }
    }
}
