using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using iTunesLyricOverlay.Alsong;
using iTunesLyricOverlay.Wrapper;

namespace iTunesLyricOverlay.Windows
{
    public partial class SearchWindow : Window
    {
        private readonly ObservableCollection<AlsongLyricWrapper> m_searchResults = new ObservableCollection<AlsongLyricWrapper>();

        public SearchWindow()
        {
            this.InitializeComponent();

            this.DataContext = MainModel.Instance;

            this.ctlSearchResults.ItemsSource = this.m_searchResults;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
                this.CtlClear_Click(null, null);
        }

        private IITTrackWrapper m_currentTrack;
        private void CtlClear_Click(object sender, RoutedEventArgs e)
        {
            var track = MainModel.Instance.CurrentTrack;

            this.StopSearch();
            
            this.ctlSearchResults  .Visibility = Visibility.Visible;
            this.ctlSearchNoResults.Visibility = Visibility.Hidden;

            this.m_searchResults.Clear();
            this.m_currentTrack = track;
            this.ctlNowPlaying.Text = MainModel.Instance.PlayingTitle;

            if (track == null)
            {
                this.ctlSearch.IsEnabled = false;
                return;
            }

            this.ctlSearchTitle .Text = track.Title;
            this.ctlSearchArtist.Text = track.Artist;

            this.ctlSearch.IsEnabled = true;
        }
        
        private void CtlSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.CtlSearch_Click(null, null);
        }

        private void CtlSearch_Click(object sender, RoutedEventArgs e)
        {
            if (this.ctlSearch.Tag == null)
            {
                this.StartSearch();
            }
            else
            {
                this.StopSearch();
            }
        }

        private readonly ManualResetEventSlim m_searchLock = new ManualResetEventSlim(true);
        private CancellationTokenSource m_searchCancelToken;
        private Task m_searchTask;

        private void StartSearch()
        {
            if (this.m_searchLock.IsSet)
                return;
            this.m_searchLock.Wait();

            this.ctlSearch.IsEnabled = false;
            this.m_searchResults.Clear();

            this.m_searchCancelToken = new CancellationTokenSource();

            var args = new SearchTaskArgs
            {
                Track  = this.m_currentTrack,
                Token  = this.m_searchCancelToken.Token,

                Artist = this.ctlSearchArtist.Text,
                Title  = this.ctlSearchTitle.Text,
            };

            this.m_searchTask = Task.Factory.StartNew(this.SearchTask, args, this.m_searchCancelToken.Token);

            this.ctlSearch.Content = "검색 중지";
            this.ctlSearch.IsEnabled = true;

            this.ctlSearch.Tag = 0;

            this.m_searchLock.Set();
        }
        private void StopSearch(bool wait = true)
        {
            if (this.m_searchLock.IsSet)
                return;
            this.m_searchLock.Wait();

            this.ctlSearch.IsEnabled = false;

            if (wait)
            {
                this.m_searchCancelToken?.Cancel();
                this.m_searchTask?.Wait();
                this.m_searchCancelToken?.Dispose();
            }

            this.ctlSearch.Content = "검색";
            this.ctlSearch.IsEnabled = true;

            this.ctlSearch.Tag = null;

            this.m_searchLock.Set();
        }

        class SearchTaskArgs
        {
            public IITTrackWrapper      Track { get; set; }
            public CancellationToken    Token { get; set; }

            public string Artist { get; set; }
            public string Title { get; set; }

        }
        private void SearchTask(object oargs)
        {
            var args = (SearchTaskArgs)oargs;

            AlsongLyric[] lyrics = null;
            var totalCount = 0;

            ////////////////////////////////////////////////////////////
            // Search By File
            lyrics = AlsongAPI.SearchByFile(args.Track.Location);
            if (lyrics != null)
            {
                totalCount += 1;

                this.Dispatcher.Invoke(new Action<IEnumerable<AlsongLyricWrapper>>(this.AddToResults), lyrics.Select(e => new AlsongLyricWrapper(args.Track, true, e)));
            }

            ////////////////////////////////////////////////////////////
            // Search By Text
            var page = 0;
            do
            {
                lyrics = AlsongAPI.SearchByText(args.Artist, args.Title, page++);

                if (lyrics == null)
                    break;

                totalCount += lyrics.Length;
                this.Dispatcher.Invoke(new Action<IEnumerable<AlsongLyricWrapper>>(this.AddToResults), lyrics.Select(e => new AlsongLyricWrapper(args.Track, false, e)));
            } while (args.Token.IsCancellationRequested || lyrics == null);

            ////////////////////////////////////////////////////////////
            
            if (totalCount == 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.ctlSearchResults  .Visibility = Visibility.Hidden;
                    this.ctlSearchNoResults.Visibility = Visibility.Visible;
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.ctlSearchResults  .Visibility = Visibility.Visible;
                    this.ctlSearchNoResults.Visibility = Visibility.Hidden;
                });
            }

            this.Dispatcher.Invoke(new Action<bool>(this.StopSearch), false);
        }
        private void AddToResults(IEnumerable<AlsongLyricWrapper> items)
        {
            foreach (var item in items)
                this.m_searchResults.Add(item);
        }

        private void LyricLine_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MainModel.Instance.SetAlsongLyrics((AlsongLyricWrapper)((ListViewItem)sender).Content);
        }
    }
}
