using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace iTunesLyricOverlay.Windows
{
    public partial class LyricCachedWindow : Window
    {
        public LyricCachedWindow()
        {
            this.InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void ctlRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.ctlList.ItemsSource = App.LyricCollection.FindAll().ToArray();
        }
    }
}
