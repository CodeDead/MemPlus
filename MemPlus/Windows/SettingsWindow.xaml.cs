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
            LoadSettings();

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

        private void LoadSettings()
        {
            //TODO
        }

        private void SaveSettings()
        {
            //TODO
            Properties.Settings.Default.Save();

            _mainWindow.ChangeVisualStyle();
            _mainWindow.LoadProperties();

            _logController.AddLog(new ApplicationLog("Settings have been saved"));

            MessageBox.Show("All settings have been saved!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetSettings()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();

            _mainWindow.ChangeVisualStyle();
            _mainWindow.LoadProperties();
        }

        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            ResetSettings();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
    }
}
