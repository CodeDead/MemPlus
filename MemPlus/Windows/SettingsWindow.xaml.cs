using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
            InitializeComponent();
            ChangeVisualStyle();

            _mainWindow = mainWindow;
            _logController = logController;
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
            _logController.AddLog(new ApplicationLog("Settings have been saved"));
        }
    }
}
