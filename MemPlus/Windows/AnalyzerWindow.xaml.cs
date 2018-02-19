using System;
using System.Windows;
using MemPlus.Classes.GUI;
using MemPlus.Classes.LOG;
using MemPlus.Classes.RAM;
using MemPlus.Classes.RAM.ViewModels;

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

        public AnalyzerWindow(LogController logController)
        {
            _logController = logController;
            _logController.AddLog(new ApplicationLog("Initializing AnalyzerWindow"));

            InitializeComponent();
            ChangeVisualStyle();
            LoadProperties();

            new RamAnalyzer(5000, ProcessListUpdatedEvent, ProcessRemovedEvent);

            _logController.AddLog(new ApplicationLog("Done initializing AnalyzerWindow"));
        }

        private void ProcessRemovedEvent(ProcessData processData)
        {
            Dispatcher.Invoke(() =>
            {
                LsvProcesses.Items.Remove(processData);
            });
        }

        private void ProcessListUpdatedEvent(ProcessData processData)
        {
            Dispatcher.Invoke(() =>
            {
                LsvProcesses.Items.Add(processData);
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
        /// Load the properties of the application
        /// </summary>
        private void LoadProperties()
        {
            try
            {
                Topmost = Properties.Settings.Default.Topmost;
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
