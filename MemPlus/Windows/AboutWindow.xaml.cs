using System;
using System.Diagnostics;
using System.Windows;
using MemPlus.Classes.GUI;
using MemPlus.Classes.LOG;

namespace MemPlus.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        private readonly LogController _logController;

        /// <inheritdoc />
        /// <summary>
        /// Initialize a new AboutWindow
        /// </summary>
        public AboutWindow(LogController logController)
        {
            _logController = logController;
            _logController.AddLog(new ApplicationLog("Initializing AboutWindow"));

            InitializeComponent();
            ChangeVisualStyle();

            _logController.AddLog(new ApplicationLog("Done initializing AboutWindow"));
        }

        /// <summary>
        /// Change the visual style of the controls, depending on the settings.
        /// </summary>
        private void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing AboutWindow theme style"));

            StyleManager.ChangeStyle(this);

            _logController.AddLog(new ApplicationLog("Done changing AboutWindow theme style"));
        }

        /// <summary>
        /// Close the current window
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.AddLog(new ApplicationLog("Closing AboutWindow"));
            Close();
        }

        /// <summary>
        /// Open the file containing the license for MemPlus
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnLicense_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _logController.AddLog(new ApplicationLog("Opening MemPlus license file"));
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\gpl.pdf");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open the CodeDead website
        /// </summary>
        /// <param name="sender">The object that has initialized the method</param>
        /// <param name="e">The routed event arguments</param>
        private void BtnCodeDead_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _logController.AddLog(new ApplicationLog("Opening CodeDead website"));
                Process.Start("https://codedead.com/");
            }
            catch (Exception ex)
            {
                _logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
