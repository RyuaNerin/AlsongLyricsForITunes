using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using iTunesLyricOverlay.Models;
using iTunesLyricOverlay.Utilities;

namespace iTunesLyricOverlay.Windows
{
    public partial class MainWindow : Window
    {
        private readonly SearchWindow  m_searchWindow  = new SearchWindow();
        private readonly OverlayWindow m_overlayWindow = new OverlayWindow();
        private readonly ConfigWindow  m_configWindow = new ConfigWindow();
        private readonly LyricArchiveWindow m_lyricCachedWindow = new LyricArchiveWindow();

        public MainWindow()
        {
            this.InitializeComponent();

            this.DataContext = MainModel.Instance;

            MainModel.Instance.OnLyricsFocusChanged += this.LyricViewerModel_LyricsFocusChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainModel.Instance.Init();

            this.m_overlayWindow.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainModel.Instance.Deinit();

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
            if (Config.Instance.MainWindow_AutoScroll == false)
                return;
            
            try
            {
                this.Dispatcher.BeginInvoke(new Action<object>(this.ctlItemsControl.ScrollToCenterOfView), item);
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
            new SetTrackLyricWindow(MainModel.Instance.CurrentTrack, MainModel.Instance.Lyric.LinesGroup).ShowDialog();
        }

        private void ctlOpenOverlay_Click(object sender, RoutedEventArgs e)
        {
            this.m_overlayWindow.Show();
        }

        private void ctlOpenConfig_Click(object sender, RoutedEventArgs e)
        {
            this.m_configWindow.Show();
        }

        private void ctlOpenLyricCache_Click(object sender, RoutedEventArgs e)
        {
            this.m_lyricCachedWindow.Show();
        }
    }
}
