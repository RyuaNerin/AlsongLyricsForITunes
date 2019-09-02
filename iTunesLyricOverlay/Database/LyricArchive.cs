using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using iTunesLyricOverlay.Alsong;
using iTunesLyricOverlay.Wrapper;
using LiteDB;

namespace iTunesLyricOverlay.Database
{
    public class LyricArchive
    {
        public static string GetID(IITTrackWrapper track)
            => $"{track.Artist}|{track.Title}|{track.Album}|{track.Duration}|{track.BitRate}";

        public LyricArchive()
        {
        }
        public LyricArchive(IITTrackWrapper track)
        {
            this.LyricArchiveId  = GetID(track);

            this.Artist   = track.Artist;
            this.Title    = track.Title;
            this.Album    = track.Album;
            this.Duration = track.Duration;

            this.Cached   = DateTime.Now;
        }

        public override string ToString()
            => this.LyricArchiveId;

        [BsonIgnore]
        public bool IsInvalid
            => string.IsNullOrWhiteSpace(this.LyricArchiveId) ||
               string.IsNullOrWhiteSpace(this.Artist) ||
               string.IsNullOrWhiteSpace(this.Title) ||
               string.IsNullOrWhiteSpace(this.Album) ||
               this.Duration == 0 ||
               this.Cached == default(DateTime) ||
               this.Lyric == null || this.Lyric.Length == 0;

        [BsonId]
        public string   LyricArchiveId  { get; set; }

        public string   Artist          { get; set; }
        public string   Title           { get; set; }
        public string   Album           { get; set; }
        public int      Duration        { get; set; }
        public DateTime Cached          { get; set; }

        public string   LyricID         { get; set; }

        [BsonIgnore] public string DurationStr  => string.Format("{0:#0}:{1:00}", this.Duration / 60, this.Duration % 60);
        [BsonIgnore] public string CachedStr    => this.Cached.ToString("yyyy-MM-dd HH:mm:ss");

        [BsonIgnore]
        public AlsongLyricLine[]    Lyric { get; set; }
        public byte[]               LyricCompressed
        {
            get
            {
                using (var mem = new MemoryStream(4096))
                {
                    mem.Position = 0;

                    using (var gzip = new GZipStream(mem, CompressionLevel.Optimal, true))
                    {
                        var writer = new StreamWriter(gzip, Encoding.UTF8);

                        foreach (var line in this.Lyric)
                            writer.WriteLine(line.ToString());

                        writer.Flush();
                    }

                    var ff = mem.ToArray();
                    return ff;
                }
            }
            set
            {
                using (var mem = new MemoryStream(value))
                using (var gzip = new GZipStream(mem, CompressionMode.Decompress, true))
                {
                    mem.Position = 0;

                    var reader = new StreamReader(gzip, Encoding.UTF8);
                    this.Lyric = AlsongLyric.ParseLyric(reader);
                }
            }
        }
    }
}
