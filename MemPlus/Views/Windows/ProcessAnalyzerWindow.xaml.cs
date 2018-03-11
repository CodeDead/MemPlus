using System;
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
            StyleManager.ChangeStyle(this);
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
        /// Method that will be called when all ProcessDetail objects should be cleared
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnClear_OnClick(object sender, RoutedEventArgs e)
        {
            LsvProcessList.Items.Clear();
        }

        /// <summary>
        /// Method that will be called when all ProcessDetail objects should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            Utils.ExportProcessDetails(_logController);
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
        private void RefreshProcessDetails()
        {
            _logController.AddLog(new ProcessLog("Refreshing process details"));
            LsvProcessList.Items.Clear();
            foreach (ProcessDetail pd in Utils.GetProcessDetails(_logController))
            {
                LsvProcessList.Items.Add(pd);
            }
            _logController.AddLog(new ProcessLog("Done refreshing process details"));
        }

        /// <summary>
        /// Method that will be called when a ProcessDetail object should be copied to the clipboard
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
    }
}
