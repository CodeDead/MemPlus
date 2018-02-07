using System;
using System.Windows;

namespace MemPlus.Windows
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnClearMemory_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //Clear working set of all processes that the user has access to
                Classes.MemPlus.EmptyWorkingSetFunction();
                //Clear file system cache
                Classes.MemPlus.ClearFileSystemCache(ChbFileSystemCache.IsChecked != null && ChbFileSystemCache.IsChecked.Value);

                MessageBox.Show("Your memory has now been cleared of any non-essential data.", "MemPlus",MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
