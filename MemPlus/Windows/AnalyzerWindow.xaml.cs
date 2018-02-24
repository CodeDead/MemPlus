using System;
using System.Windows;
using System.Windows.Controls;
using MemPlus.Classes.GUI;
using MemPlus.Classes.LOG;
using MemPlus.Classes.RAM;
using MemPlus.Classes.RAM.ViewModels;
using Microsoft.Win32;

namespace MemPlus.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for AnalyzerWindow.xaml
    /// </summary>
    public partial class AnalyzerWindow
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
        public AnalyzerWindow(LogController logController)
        {
            _logController = logController;
            _logController.AddLog(new ApplicationLog("Initializing AnalyzerWindow"));

            InitializeComponent();
            ChangeVisualStyle();
            LoadProperties();

            RefreshRamData();

            _logController.AddLog(new ApplicationLog("Done initializing AnalyzerWindow"));
        }

        /// <summary>
        /// Refresh RAM data
        /// </summary>
        private void RefreshRamData()
        {
            _logController.AddLog(new RamLog("Refreshing RAM data"));
            try
            {
                TrvRam.Items.Clear();
                foreach (RamStick s in RamAnalyzer.GetRamSticks())
                {
                    TreeViewItem treeItem = new TreeViewItem {Header = s.GetValue("BankLabel")};

                    foreach (RamData data in s.GetRamData())
                    {
                        treeItem.Items.Add(new TreeViewItem() { Header = data.Key + ": " + data.Value });
                    }

                    TrvRam.Items.Add(treeItem);
                }
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _logController.AddLog(new RamLog("Done refreshing RAM data"));
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
        /// Load the properties of the application
        /// </summary>
        private void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading AnalyzerWindow properties"));
            try
            {
                Topmost = Properties.Settings.Default.Topmost;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _logController.AddLog(new ApplicationLog("Done loading AnalyzerWindow properties"));
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
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter =
                    "Text file (*.txt)|*.txt|HTML file (*.html)|*.html|CSV file (*.csv)|*.csv|Excel file (*.csv)|*.csv"
            };
            if (sfd.ShowDialog() != true) return;
            try
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (sfd.FilterIndex)
                {
                    //Filterindex starts at 1
                    case 1:
                        RamDataExporter.ExportText(sfd.FileName, RamAnalyzer.GetRamSticks());
                        break;
                    case 2:
                        RamDataExporter.ExportHtml(sfd.FileName, RamAnalyzer.GetRamSticks());
                        break;
                    case 3:
                        RamDataExporter.ExportCsv(sfd.FileName, RamAnalyzer.GetRamSticks());
                        break;
                    case 4:
                        RamDataExporter.ExportExcel(sfd.FileName, RamAnalyzer.GetRamSticks());
                        break;
                }
                MessageBox.Show("Exported all data!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
