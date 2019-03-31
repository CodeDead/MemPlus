using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MemPlus.Business.GUI;
using MemPlus.Business.LOG;
using MemPlus.Business.RAM;
using MemPlus.Business.UTILS;

namespace MemPlus.Views.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for RamStatisticsWindow.xaml
    /// </summary>
    public partial class RamStatisticsWindow
    {
        #region Variables
        /// <summary>
        /// The LogController object that can be used to add logs
        /// </summary>
        private readonly LogController _logController;
        /// <summary>
        /// The RamController object that can be used to display RAM usage statistics
        /// </summary>
        private readonly RamController _ramController;
        /// <summary>
        /// A boolean to indicate whether automatic scrolling is enabled or not
        /// </summary>
        private bool _autoScroll;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new LogWindow object
        /// </summary>
        /// /// <param name="ramController">The RamController object that can be used to view RamUsage objects</param>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        internal RamStatisticsWindow(RamController ramController, LogController logController)
        {
            _logController = logController;
            _logController.AddLog(new ApplicationLog("Initializing RamStatisticsWindow"));

            InitializeComponent();
            ChangeVisualStyle();
            LoadProperties();

            _ramController = ramController;

            FillLogView();

            _ramController.RamUsageAddedEvent += RamUsageAddedEvent;
            _ramController.RamUsageRemovedEvent += RamUsageRemovedEvent;
            _ramController.RamUsageClearedEvent += RamUsageClearedEvent;

            _autoScroll = true;

            _logController.AddLog(new ApplicationLog("Done initializing RamStatisticsWindow"));
        }

        /// <summary>
        /// Load the current properties into the GUI
        /// </summary>
        private void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading RamStatisticsWindow properties"));
            try
            {
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
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _logController.AddLog(new ApplicationLog("Done loading RamStatisticsWindow properties"));
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
        /// Method that is called when all RamUsage objects were cleared
        /// </summary>
        private void RamUsageClearedEvent()
        {
            Dispatcher.Invoke(() =>
            {
                LsvStatistics.Items.Clear();
            });
        }

        /// <summary>
        /// Fill the ListView with all current RamUsage objects
        /// </summary>
        private void FillLogView()
        {
            foreach (RamUsage usage in _ramController.GetRamUsageHistory())
            {
                LsvStatistics.Items.Add(usage);
            }
        }

        /// <summary>
        /// Method that is called when a RamUsage object was removed
        /// </summary>
        /// <param name="ramUsage">The RamUsage object that was removed</param>
        private void RamUsageRemovedEvent(RamUsage ramUsage)
        {
            Dispatcher.Invoke(() =>
            {
                LsvStatistics.Items.Remove(ramUsage);
            });
        }

        /// <summary>
        /// Method that is called when a RamUsage object was added
        /// </summary>
        /// <param name="ramUsage">The RamUsage object that was added</param>
        private void RamUsageAddedEvent(RamUsage ramUsage)
        {
            Dispatcher.Invoke(() =>
            {
                LsvStatistics.Items.Add(ramUsage);

                if (!_autoScroll) return;
                LsvStatistics.ScrollIntoView(LsvStatistics.Items[LsvStatistics.Items.Count - 1]);
            });
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        private void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing RamStatisticsWindow theme style"));
            GuiManager.ChangeStyle(this);
            _logController.AddLog(new ApplicationLog("Done changing RamStatisticsWindow theme style"));
        }

        /// <summary>
        /// Method that is called when a scroll action happened in the ListView
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The ScrollEventArgs</param>
        private void LsvStatistics_OnScroll(object sender, ScrollEventArgs e)
        {
            if (!(e.OriginalSource is ScrollBar sb)) return;
            if (sb.Orientation == Orientation.Horizontal) return;

            _autoScroll = Math.Abs(sb.Value - sb.Maximum) < 1;
        }

        /// <summary>
        /// Method that is called when all RamUsage objects should be cleared
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnClear_OnClick(object sender, RoutedEventArgs e)
        {
            _ramController.ClearRamUsageHistory();
        }

        /// <summary>
        /// Method that is called when all RamUsage objects should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            if (Utils.ExportRamUsage(_ramController, _logController))
            {
                MessageBox.Show((string)Application.Current.FindResource("ExportedAllData"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Method that is called when a RamUsage object should be removed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void DeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvStatistics.SelectedItems.Count == 0) return;
            _ramController.RemoveRamUsage(LsvStatistics.SelectedItem as RamUsage);
        }

        /// <summary>
        /// Method that is called when a Log object should be copied to the clipboard
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void CopyMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvStatistics.SelectedItems.Count == 0) return;
            if (!(LsvStatistics.SelectedItem is RamUsage selectedUsage)) return;

            try
            {
                Clipboard.SetText(selectedUsage.RecordedDate + "\t" + selectedUsage.TotalUsed + "\t" + selectedUsage.RamTotal + "\t" + selectedUsage.UsagePercentage);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the mouse wheel is used
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The MouseWheelEventArgs</param>
        private void LsvStatistics_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _autoScroll = false;
        }
    }
}
