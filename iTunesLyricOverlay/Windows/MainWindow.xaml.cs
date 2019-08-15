using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using iTunesLyricOverlay.Models;

namespace iTunesLyricOverlay.Windows
{
    public partial class MainWindow : Window
    {
        private readonly SearchWindow m_searchWindow = new SearchWindow();

        public MainWindow()
        {
            this.InitializeComponent();

            this.DataContext = MainModel.Instance;

            MainModel.Instance.OnLyricsFocusChanged += this.LyricViewerModel_LyricsFocusChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainModel.Instance.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainModel.Instance.Stop();

            this.m_searchWindow.Close();

            Application.Current.Shutdown();
        }

        private void LyricLine_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
                return;

            MainModel.Instance.SetPlayerPosition(((LyricLineModel)((TextBlock)sender).Tag).Line.Time);
        }

        private void LyricViewerModel_LyricsFocusChanged(LyricLineGroupModel item)
        {
            try
            {
                var cp = this.ctlItemsControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;

                if (cp != null)
                    this.Dispatcher.InvokeAsync(() => cp.BringIntoView(), DispatcherPriority.Background);
            }
            catch
            {
            }
        }

        private void CtlSearchLyrics_Click(object sender, RoutedEventArgs e)
        {
            this.m_searchWindow.Owner = this;
            this.m_searchWindow.Show();
        }

        private void ctlMenuSetTrackLyric_Click(object sender, RoutedEventArgs e)
        {
            new SetTrackLyricWindow(MainModel.Instance.CurrentTrack, MainModel.Instance.LinesGroup).ShowDialog();
        }
    }
}
