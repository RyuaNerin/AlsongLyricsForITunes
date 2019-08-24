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
        private static readonly TimeSpan WaitAfterScroll = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan UserScrollEventExpire = TimeSpan.FromMilliseconds(100);

        private readonly SearchWindow  m_searchWindow  = new SearchWindow();
        private readonly OverlayWindow m_overlayWindow = new OverlayWindow();

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

        private volatile bool m_userScroll = false;
        private DateTime m_userScrollExpired = DateTime.MinValue;
        private DateTime m_nextAutoScroll = DateTime.MaxValue;
        private void ctlItemsControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.m_userScroll = true;
            this.m_userScrollExpired = DateTime.Now + UserScrollEventExpire;
        }

        private void ctlItemsControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this.m_userScroll = true;
            this.m_userScrollExpired = DateTime.Now + UserScrollEventExpire;
        }

        private void CtlItemsControl_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.m_userScroll)
            {
                this.m_userScroll = false;

                if (DateTime.Now < this.m_userScrollExpired)
                    return;
            }
            
            this.m_nextAutoScroll = DateTime.Now + WaitAfterScroll;
        }

        private void LyricViewerModel_LyricsFocusChanged(LyricLineGroupModel item)
        {
            if (Config.Instance.MainWindow_AutoScroll == false)
                return;

            if (DateTime.Now >= this.m_nextAutoScroll)
            {
                try
                {
                    this.Dispatcher.BeginInvoke(new Action<object>(this.ctlItemsControl.ScrollToCenterOfView), item);
                }
                catch
                {
                }
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

        private void ctlOpenOverlay_Click(object sender, RoutedEventArgs e)
        {
            this.m_overlayWindow.Show();
        }
    }
}
