using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MemPlus.Business.GUI;
using MemPlus.Business.LOG;
using MemPlus.Business.PROCESS;
using MemPlus.Business.UTILS;

namespace MemPlus.Views.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for ProcessAnalyzerWindow.xaml
    /// </summary>
    public partial class ProcessAnalyzerWindow
    {
        #region Variables
        /// <summary>
        /// The LogController object that can be used to add logs
        /// </summary>
        private readonly LogController _logController;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new ProcessAnalyzerWindow object
        /// </summary>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        public ProcessAnalyzerWindow(LogController logController)
        {
            _logController = logController;
            _logController.AddLog(new ApplicationLog("Initializing ProcessAnalyzerWindow"));

            InitializeComponent();
            ChangeVisualStyle();
            LoadProperties();
            RefreshProcessDetails();

            _logController.AddLog(new ApplicationLog("Done initializing ProcessAnalyzerWindow"));
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        private void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing ProcessAnalyzerWindow theme style"));
            GuiManager.ChangeStyle(this);
            _logController.AddLog(new ApplicationLog("Done changing ProcessAnalyzerWindow theme style"));
        }

        /// <summary>
        /// Load the properties of the application
        /// </summary>
        private void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading ProcessAnalyzerWindow properties"));
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
            _logController.AddLog(new ApplicationLog("Done loading ProcessAnalyzerWindow properties"));
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
        /// Method that is called when all ProcessDetail objects should be cleared
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnClear_OnClick(object sender, RoutedEventArgs e)
        {
            LsvProcessList.Items.Clear();
        }

        /// <summary>
        /// Method that is called when all ProcessDetail objects should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private async void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            if (await Utils.ExportProcessDetails(_logController))
            {
                MessageBox.Show((string)Application.Current.FindResource("ExportedAllData"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Method that is called when the ProcessDetail objects should be refreshed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshProcessDetails();
        }

        /// <summary>
        /// Refresh all ProcessDetail objects
        /// </summary>
        private async void RefreshProcessDetails()
        {
            _logController.AddLog(new ProcessLog("Refreshing process details"));
            LsvProcessList.Items.Clear();
            foreach (ProcessDetail pd in await Utils.GetProcessDetails(_logController))
            {
                LsvProcessList.Items.Add(pd);
            }
            _logController.AddLog(new ProcessLog("Done refreshing process details"));
        }

        /// <summary>
        /// Method that is called when a ProcessDetail object should be copied to the clipboard
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void CopyMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvProcessList.SelectedItems.Count == 0) return;
            if (!(LsvProcessList.SelectedItem is ProcessDetail selectedProcess)) return;
            try
            {
                Clipboard.SetText(selectedProcess.ProcessId + "\t" + selectedProcess.ProcessName + "\t" + selectedProcess.ProcessLocation + "\t" + selectedProcess.MemoryUsage);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the working set of a process should be emptied
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void EmptyWorkingSetMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvProcessList.SelectedItems.Count == 0) return;
            if (!(LsvProcessList.SelectedItem is ProcessDetail detail)) return;
            try
            {
                _logController.AddLog(new RamLog("Emptying working set for process: " + detail.ProcessName));
                // Empty the working set of the process
                NativeMethods.EmptyWorkingSet(Process.GetProcessById(detail.ProcessId).Handle);

                _logController.AddLog(new RamLog("Successfully emptied working set for process " + detail.ProcessName));
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when a Process should be killed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void KillMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (LsvProcessList.SelectedItems.Count == 0) return;
            if (!(LsvProcessList.SelectedItem is ProcessDetail detail)) return;

            try
            {
                _logController.AddLog(new ApplicationLog("Killing " + detail.ProcessName + " (" + detail.ProcessId + ")"));
                Process.GetProcessById(detail.ProcessId).Kill();
                _logController.AddLog(new ApplicationLog("Done killing " + detail.ProcessName + " (" + detail.ProcessId + ")"));
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
