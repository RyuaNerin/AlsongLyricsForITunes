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
            => $"{track.Artist}|{track.Title}|{track.Album}|{track.Duration}|{track.TrackID}";

        public LyricArchive()
        {
        }
        public LyricArchive(IITTrackWrapper track)
        {
            this.TrackID = GetID(track);
        }

        [BsonId]
        [BsonField("id")]
        public string               TrackID { get; set; }

        [BsonField("lyric_id")]
        public string               LyricID { get; set; }

        [BsonIgnore]
        public AlsongLyricLine[]    Lyric   { get; set; }

        [BsonField("lyric")]
        public byte[] LyricCompressed
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
