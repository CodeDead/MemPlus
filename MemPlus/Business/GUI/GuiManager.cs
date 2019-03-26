using System;
using System.Windows;
using System.Windows.Media;
using MemPlus.Business.LOG;
using Syncfusion.Windows.Shared;

namespace MemPlus.Business.GUI
{
    /// <summary>
    /// Static class to change the style of an object
    /// </summary>
    internal static class GuiManager
    {
        /// <summary>
        /// Change the visual style of an object
        /// </summary>
        /// <param name="o">The object that needs to have a style overhaul</param>
        internal static void ChangeStyle(DependencyObject o)
        {
            try
            {
                SkinStorage.SetVisualStyle(o, Properties.Settings.Default.VisualStyle);
                SkinStorage.SetMetroBrush(o, new SolidColorBrush(Properties.Settings.Default.MetroColor));
                if (!(o is ChromelessWindow window)) return;
                window.BorderThickness = new Thickness(Properties.Settings.Default.BorderThickness);
                window.CornerRadius = new CornerRadius(0, 0, 0, 0);
                window.Opacity = Properties.Settings.Default.WindowOpacity / 100;
                window.ResizeBorderThickness = new Thickness(Properties.Settings.Default.WindowResizeBorder);
            }
            catch (Exception ex)
            {
                SkinStorage.SetVisualStyle(o, "Metro");
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Change the language of the application, depending on the settings
        /// </summary>
        /// <param name="logController">The LogController object that can be used to add logs</param>
        internal static void ChangeLanguage(LogController logController)
        {
            logController.AddLog(new ApplicationLog("Changing language"));
            ResourceDictionary dict = new ResourceDictionary();
            Uri langUri;
            try
            {
                switch (Properties.Settings.Default.SelectedLanguage)
                {
                    default:
                        langUri = new Uri("..\\Resources\\Languages\\en_US.xaml", UriKind.Relative);
                        break;
                    case 0:
                        langUri = new Uri("..\\Resources\\Languages\\de_DE.xaml", UriKind.Relative);
                        break;
                    case 2:
                        langUri = new Uri("..\\Resources\\Languages\\es_ES.xaml", UriKind.Relative);
                        break;
                    case 3:
                        langUri = new Uri("..\\Resources\\Languages\\fr_FR.xaml", UriKind.Relative);
                        break;
                    case 4:
                        langUri = new Uri("..\\Resources\\Languages\\gl_ES.xaml", UriKind.Relative);
                        break;
                    case 5:
                        langUri = new Uri("..\\Resources\\Languages\\it_IT.xaml", UriKind.Relative);
                        break;
                    case 6:
                        langUri = new Uri("..\\Resources\\Languages\\nl_BE.xaml", UriKind.Relative);
                        break;
                    case 7:
                        langUri = new Uri("..\\Resources\\Languages\\nl_NL.xaml", UriKind.Relative);
                        break;
                }
            }
            catch (Exception ex)
            {
                langUri = new Uri("..\\Resources\\Languages\\en.xaml", UriKind.Relative);
                logController.AddLog(new ApplicationLog(ex.Message));
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            dict.Source = langUri;
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);

            logController.AddLog(new ApplicationLog("Done changing language"));
        }
    }
}
