using System.Windows;
using MemPlus.Classes.GUI;
using MemPlus.Classes.LOG;

namespace MemPlus.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        private readonly MainWindow _mainWindow;
        private readonly LogController _logController;

        public SettingsWindow(MainWindow mainWindow, LogController logController)
        {
            _logController = logController;
            _logController.AddLog(new ApplicationLog("Initializing SettingsWindow"));

            InitializeComponent();
            ChangeVisualStyle();
            LoadProperties();

            _mainWindow = mainWindow;

            _logController.AddLog(new ApplicationLog("Done initializing SettingsWindow"));
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        private void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing SettingsWindow theme style"));

            StyleManager.ChangeStyle(this);

            _logController.AddLog(new ApplicationLog("Done changing SettingsWindow theme style"));
        }

        private void LoadProperties()
        {
            _logController.AddLog(new ApplicationLog("Loading SettingsWindow properties"));

            //TODO
            //General
            ChbAutoUpdate.IsChecked = Properties.Settings.Default.AutoUpdate;
            if (Properties.Settings.Default.Topmost)
            {
                ChbTopmost.IsChecked = Properties.Settings.Default.Topmost;
                Topmost = true;
            }
            else
            {
                Topmost = false;
            }

            ChbRamMonitor.IsChecked = Properties.Settings.Default.RamMonitor;
            ChbDisableInactive.IsChecked = Properties.Settings.Default.DisableOnInactive;
            ItbRamMonitorTimeout.Value = Properties.Settings.Default.RamMonitorInterval;

            _logController.AddLog(new ApplicationLog("Done loading SettingsWindow properties"));
        }

        private void SaveProperties()
        {
            _logController.AddLog(new ApplicationLog("Saving properties"));

            //TODO
            //General
            if (ChbAutoUpdate.IsChecked != null) Properties.Settings.Default.AutoUpdate = ChbAutoUpdate.IsChecked.Value;
            if (ChbTopmost.IsChecked != null) Properties.Settings.Default.Topmost = ChbTopmost.IsChecked.Value;
            if (ChbRamMonitor.IsChecked != null) Properties.Settings.Default.RamMonitor = ChbRamMonitor.IsChecked.Value;
            if (ChbDisableInactive.IsChecked != null) Properties.Settings.Default.DisableOnInactive = ChbDisableInactive.IsChecked.Value;
            if (ItbRamMonitorTimeout.Value != null) Properties.Settings.Default.RamMonitorInterval = (int) ItbRamMonitorTimeout.Value;

            Properties.Settings.Default.Save();

            _mainWindow.ChangeVisualStyle();
            _mainWindow.LoadProperties();

            _logController.AddLog(new ApplicationLog("Properties have been saved"));

            MessageBox.Show("All settings have been saved!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetSettings()
        {
            _logController.AddLog(new ApplicationLog("Resetting properties"));

            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();

            _mainWindow.ChangeVisualStyle();
            _mainWindow.LoadProperties();
            LoadProperties();

            _logController.AddLog(new ApplicationLog("Properties have been reset"));

            MessageBox.Show("All settings have been reset!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            ResetSettings();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveProperties();
        }
    }
}
