using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MemPlus.Business.GUI;
using MemPlus.Business.LOG;
using MemPlus.Business.RAM;
using MemPlus.Business.UTILS;

namespace MemPlus.Views.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for RamAnalyzerWindow.xaml
    /// </summary>
    public partial class RamAnalyzerWindow
    {
        #region Variables
        /// <summary>
        /// The LogController object that can be used to add logs
        /// </summary>
        private readonly LogController _logController;
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new AnalyzerWindow object
        /// </summary>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        internal RamAnalyzerWindow(LogController logController)
        {
            _logController = logController;
            _logController.AddLog(new ApplicationLog("Initializing AnalyzerWindow"));

            InitializeComponent();
            ChangeVisualStyle();
            LoadProperties();

            _logController.AddLog(new ApplicationLog("Done initializing AnalyzerWindow"));
        }

        /// <summary>
        /// Refresh RAM data
        /// </summary>
        private void RefreshRamData()
        {
            _logController.AddLog(new RamLog("Refreshing RAM data"));
            TrvRam.Items.Clear();
            try
            {
                List<RamStick> ramSticks = Utils.GetRamSticks();

                if (ramSticks == null || ramSticks.Count == 0)
                {
                    MessageBox.Show((string)Application.Current.FindResource("CouldNotRetrieveInformation"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
                    _logController.AddLog(new RamLog("Could not retrieve RAM Analyzer information"));
                    Close();
                    return;
                }

                foreach (RamStick s in ramSticks)
                {
                    TreeViewItem treeItem = new TreeViewItem {Header = s.GetValue("BankLabel")};

                    foreach (RamData data in s.GetRamData())
                    {
                        TreeViewItem headerItem = new TreeViewItem {Header = data.Key};
                        headerItem.Items.Add(new TreeViewItem {Header = data.Value});
                        treeItem.Items.Add(headerItem);
                    }

                    TrvRam.Items.Add(treeItem);
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _logController.AddLog(new RamLog("Done refreshing RAM data"));
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        private void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing AnalyzerWindow theme style"));
            GuiManager.ChangeStyle(this);
            _logController.AddLog(new ApplicationLog("Done changing AnalyzerWindow theme style"));
        }

        /// <summary>
        /// Load the properties of the application
        /// </summary>
        private void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading AnalyzerWindow properties"));
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
            _logController.AddLog(new ApplicationLog("Done loading AnalyzerWindow properties"));
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
        /// Method that is called when the RAM data should be refreshed
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshRamData();
        }

        /// <summary>
        /// Method that is called when RamStick information should be exported
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            if (Utils.ExportRamSticks(_logController))
            {
                MessageBox.Show((string)Application.Current.FindResource("ExportedAllData"), "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Method that is called when the selected item should be copied to the clipboard
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void CopyMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(TrvRam.SelectedItem is TreeViewItem selectedItem)) return;
            try
            {
                Clipboard.SetText(selectedItem.Header.ToString());
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ErrorLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method that is called when the window has loaded
        /// </summary>
        /// <param name="sender">The object that called this method</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void AnalyzerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshRamData();
        }
    }
}
