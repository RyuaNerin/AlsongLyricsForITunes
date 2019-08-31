using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using iTunesLib;
using iTunesLyricOverlay.Alsong;
using iTunesLyricOverlay.Database;
using iTunesLyricOverlay.Models;
using iTunesLyricOverlay.Wrapper;

namespace iTunesLyricOverlay
{
    public enum LyricState
    {
        NotPlaying  = 0,

        Searching   = 1,
        NotFound    = 2,
        Success     = 3,

        AlsongError = -1,
    }

    public class MainModel : INotifyPropertyChanged
    {
        public static MainModel Instance { get; }
        static MainModel()
        {
            Instance = new MainModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public event Action<IITTrackWrapper> OnPlayerTrackChanged;
        public event Action<LyricLineGroupModel> OnLyricsFocusChanged;

        private volatile bool m_running = true;

        private readonly ManualResetEvent m_threadUpdatePos = new ManualResetEvent(false);

        private readonly object m_currentTrackLock = new object();
        private IITTrackWrapper m_currentTrack;

        public ITunesWrapper ITunes { get; } = new ITunesWrapper();

        private MainModel()
        {
        }

        public void Init()
        {
            new Thread(this.UpdatePlayingPos) { Priority = ThreadPriority.Lowest, IsBackground = true }.Start();

            this.ITunes.ITunesAttached += this.ITunes_ITunesAttached;
            this.ITunes.ITunesDeattached += this.ITunes_ITunesDeattached;

            this.ITunes.OnPlayerPlayEvent += this.ITunes_OnPlayerPlayEvent;
            this.ITunes.OnPlayerPlayingTrackChangedEvent += this.ITunes_OnPlayerPlayingTrackChangedEvent;
            this.ITunes.OnPlayerStopEvent += this.ITunes_OnPlayerStopEvent;

            this.ITunes.Init();
        }

        public void Deinit()
        {
            this.ITunes.Deinit();

            this.m_running = false;
        }

        private LyricState m_state;
        public LyricState State
        {
            get => this.m_state;
            private set
            {
                this.m_state = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("StateString");
            }
        }
        public string StateString
        {
            get
            {
                switch (this.State)
                {
                    case LyricState.NotPlaying : return "By RyuaNerin";
                    case LyricState.Searching  : return "가사를 검색중입니다";
                    case LyricState.NotFound   : return "가사를 찾을 수 없습니다";
                    case LyricState.AlsongError: return "오류가 발생하였습니다";
                }

                return null;
            }
        }

        private int m_musicPos;
        public int MusicPos
        {
            get => this.m_musicPos;
            private set
            {
                this.m_musicPos = value;
                this.OnPropertyChanged();
            }
        }
        private int m_musicPosMax;
        public int MusicPosMax
        {
            get => this.m_musicPosMax;
            private set
            {
                this.m_musicPosMax = value;
                this.OnPropertyChanged();
            }
        }

        private LyricLineGroupCollection m_linesGroup;
        public LyricLineGroupCollection LinesGroup
        {
            get => this.m_linesGroup;
            private set
            {
                this.m_linesGroup = value;
                this.OnPropertyChanged();
            }
        }

        private string m_currentLyricID;
        public string CurrentLyricID
        {
            get => this.m_currentLyricID;
            private set
            {
                this.m_currentLyricID = value;
                this.OnPropertyChanged();
            }
        }

        public IITTrackWrapper CurrentTrack
        {
            get
            {
                lock (this.m_currentTrackLock)
                    return this.m_currentTrack;
            }
            set
            {
                this.m_currentTrack = value;
                this.OnPropertyChanged();
            }
        }

        private void ITunes_ITunesDeattached()
        {
            this.State = LyricState.NotPlaying;

            this.m_updatePos.Reset();
        }

        private void ITunes_ITunesAttached()
        {
            if (this.ITunes.PlayerState == ITPlayerState.ITPlayerStatePlaying)
            {
                this.TrackChanged(this.ITunes.CurrentTrack);
            }
        }

        private void ITunes_OnPlayerPlayEvent(object iTrack)
        {
            this.TrackChanged((IITTrackWrapper)iTrack);
        }

        private void ITunes_OnPlayerPlayingTrackChangedEvent(object iTrack)
        {
            this.TrackChanged((IITTrackWrapper)iTrack);
        }

        private void ITunes_OnPlayerStopEvent(object iTrack)
        {
            var track = (IITTrackWrapper)iTrack;

            lock (this.m_currentTrackLock)
            {
                if (this.m_currentTrack == track)
                {
                    this.m_updatePos.Reset();
                }
            }
        }

        private readonly ManualResetEvent m_updatePos = new ManualResetEvent(false);
        private void UpdatePlayingPos()
        {
            while (this.m_running)
            {
                this.m_updatePos.WaitOne();

                try
                {
                    this.SetTime(TimeSpan.FromMilliseconds(this.ITunes.PlayerPositionMS));
                }
                catch
                {
                }

                Thread.Sleep(200);
            }

            this.m_threadUpdatePos.Set();
        }

        private void TrackChanged(IITTrackWrapper track)
        {
            this.OnPlayerTrackChanged?.Invoke(track);

            this.m_updatePos.Set();

            lock (this.m_currentTrackLock)
            {
                this.CurrentTrack = track;

                this.MusicPos = this.ITunes.PlayerPositionMS;
                this.MusicPosMax = track.Duration * 1000;

                try
                {
                    this.State = LyricState.Searching;

                    var arcCached = App.LyricCollection.FindById(LyricArchive.GetID(track));
                    if (arcCached != null && arcCached.Lyric != null)
                    {
                        this.CurrentLyricID = arcCached.LyricID;

                        this.SetLyrics(arcCached.Lyric);
                        return;
                    }

                    AlsongLyric[] lyrics;
                    
                    lyrics = AlsongAPI.SearchByFile(track.Location);
                    if (lyrics != null)
                    {
                        this.SetAlsongLyrics(this.m_currentTrack, lyrics[0]);
                        return;
                    }
                    
                    lyrics = AlsongAPI.SearchByText(track.Artist, track.Title, 0);
                    if (lyrics?.Length > 0)
                    {
                        this.SetAlsongLyrics(this.m_currentTrack, lyrics[0]);
                        return;
                    }

                    this.SetLyrics(null);
                }
                catch
                {
                }
            }
        }

        public void SetAlsongLyrics(AlsongLyricWrapper lyricWrapper)
        {
            this.SetAlsongLyrics(lyricWrapper.CurrentTrack, lyricWrapper.AlsongLyric);
        }

        private void SetAlsongLyrics(IITTrackWrapper track, AlsongLyric lyric)
        {
            lock (this.m_currentTrackLock)
            {
                if (this.m_currentTrack != track)
                    return;

                if (lyric.GetLyrics())
                {
                    this.CurrentLyricID = lyric.LyricID;

                    var arc = new LyricArchive(track)
                    {
                        Lyric   = lyric.Lyric,
                        LyricID = lyric.LyricID,
                    };

                    App.LyricCollection.Upsert(arc);

                    this.SetLyrics(lyric.Lyric);
                    return;
                }

                this.State = LyricState.AlsongError;
            }
        }

        private int m_lastFocusedIndex = -1;
        private void SetLyrics(AlsongLyricLine[] lyric)
        {
            lock (this.m_currentTrackLock)
            {
                if (lyric == null)
                {
                    this.State = LyricState.NotFound;
                    this.LinesGroup = null;
                    return;
                }

                this.State = LyricState.Success;

                //////////////////////////////////////////////////
                
                this.LinesGroup = new LyricLineGroupCollection(lyric);

                //////////////////////////////////////////////////

                if (this.LinesGroup.Count > 0)
                {
                    this.LinesGroup[0].Focused = true;
                    this.m_lastFocusedIndex = 0;

                    this.OnLyricsFocusChanged?.Invoke(this.LinesGroup[0]);
                }
                else
                {
                    this.m_lastFocusedIndex = -1;
                }

                if (Config.Instance.ApplyLyricsToITunes)
                {
                    var lyricStr = this.LinesGroup.Format(Config.Instance.ApplyLyricsToITunes_WithTime, Config.Instance.ApplyLyricsToITunes_WithBlankLine);

                    this.m_currentTrack.SetLyrics(lyricStr);
                }
            }
        }

        public void SetPlayerPosition(TimeSpan ts)
        {
            this.ITunes.PlayerPositionMS = (int)ts.TotalMilliseconds;
            this.SetTime(ts);
        }

        private void SetTime(TimeSpan pos)
        {
            this.MusicPos = (int)pos.TotalMilliseconds;

            if (this.m_linesGroup == null)
                return;

            int index = 0;
            while (index + 1 < this.LinesGroup.Count)
            {
                if (pos < this.LinesGroup[index + 1].Time)
                    break;
                index++;
            }

            if (this.m_lastFocusedIndex != index)
            {
                this.LinesGroup[this.m_lastFocusedIndex].Focused = false;
                this.LinesGroup[index].Focused = true;

                this.m_lastFocusedIndex = index;

                this.OnLyricsFocusChanged?.Invoke(this.LinesGroup[index]);
            }
        }
    }
}
