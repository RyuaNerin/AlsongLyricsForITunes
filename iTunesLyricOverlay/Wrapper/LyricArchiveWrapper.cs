using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using iTunesLyricOverlay.Alsong;
using iTunesLyricOverlay.Database;
using iTunesLyricOverlay.Models;

namespace iTunesLyricOverlay.Wrapper
{
    public class LyricArchiveWrapper : INotifyPropertyChanged
    {
        public static LyricArchiveWrapper GetLyrics(IITTrackWrapper track)
        {
            var arcCached = App.LyricCollection.FindById(LyricArchive.GetID(track));
            if (arcCached != null && arcCached.Lyric != null)
                return new LyricArchiveWrapper(track, arcCached, true);

            AlsongLyric[] lyrics;
            LyricArchiveWrapper archive;

            lyrics = AlsongAPI.SearchByFile(track.Location);
            if (lyrics?.Length > 0 && TryWrap(track, lyrics[0], out archive))
                return archive;

            lyrics = AlsongAPI.SearchByText(track.Artist, track.Title, 0);
            if (lyrics?.Length > 0 && TryWrap(track, lyrics[0], out archive))
                return archive;

            return null;
        }

        public static bool TryWrap(IITTrackWrapper track, AlsongLyric lyric, out LyricArchiveWrapper archive)
        {
            archive = null;

            if (!lyric.GetLyrics())
                return false;

            var arc = new LyricArchive(track)
            {
                LyricID = lyric.LyricID,
                Lyric   = lyric.Lyric,
            };

            archive = new LyricArchiveWrapper(track, arc, false);
            archive.Save();

            return true;
        }

        private readonly LyricArchive m_archive;

        private LyricArchiveWrapper(IITTrackWrapper track, LyricArchive archive, bool isArchived)
        {
            this.m_archive  = archive;
            this.IsArchived = isArchived;
            this.Track      = track;

            this.LinesGroup = new LyricLineGroupCollection(archive.Lyric);
        }

        public void Save()
        {
            App.LyricCollection.Upsert(this.m_archive);
        }

        public IITTrackWrapper Track { get; }

        public LyricLineGroupCollection LinesGroup { get; }

        public bool IsArchived { get; }

        public string LyricID => this.m_archive.LyricID;

        public TimeSpan Sync
        {
            get => this.m_archive.Sync;
            set
            {
                this.m_archive.Sync = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("SyncStr");
            }
        }

        public string SyncStr
        {
            get
            {
                var v = this.Sync;
                if (v == TimeSpan.Zero)
                    return null;

                if (v < TimeSpan.Zero)
                    return v.ToString(@"\<\ mm\:ss\.ff");
                else
                    return v.ToString(@"mm\:ss\.ff\ \>");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
