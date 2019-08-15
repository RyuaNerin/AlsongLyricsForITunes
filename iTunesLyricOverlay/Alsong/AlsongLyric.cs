using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using iTunesLyricOverlay.Utilities;

namespace iTunesLyricOverlay.Alsong
{
    [DebuggerDisplay("Title = {Title} / Artist = {Artist}")]
    public class AlsongLyric
    {
        public AlsongLyric()
        {
        }
        public static AlsongLyric Parse(XmlNode xmlNode)
        {
            AlsongLyric lyric = null;
            if (xmlNode.Name == "ST_SEARCHLYRIC_LIST")
            {
                lyric = new AlsongLyric
                {
                    LyricID = xmlNode["lyricID"].InnerText,
                    Title   = xmlNode["title"  ].InnerText,
                    Artist  = xmlNode["artist" ].InnerText,
                    Album   = xmlNode["album"  ].InnerText,
                };
            }
            else if (xmlNode.Name == "GetLyric7Result")
            {
                lyric = new AlsongLyric
                {
                    LyricID = xmlNode["strInfoID"].InnerText,
                    Title   = xmlNode["strTitle" ].InnerText,
                    Artist  = xmlNode["strArtist"].InnerText,
                    Album   = xmlNode["strAlbum" ].InnerText,

                    Lyric   = ParseLyric(xmlNode["strLyric"].InnerText),
                };
            }

            return string.IsNullOrWhiteSpace(lyric?.Title) ? null : lyric;
        }

        public string LyricID   { get; set; }
        public string Title     { get; set; }
        public string Artist    { get; set; }
        public string Album     { get; set; }
        
        public AlsongLyricLine[]    Lyric    { get; set; }

        public static AlsongLyricLine[] ParseLyric(string lyricRawString)
        {
            using (var reader = new StringReader(lyricRawString))
                return ParseLyric(reader);
        }
        public static AlsongLyricLine[] ParseLyric(TextReader reader)
        {
            string line;

            var lreader = new LineReader(reader);

            var lyric = new List<AlsongLyricLine>();
            while (!string.IsNullOrWhiteSpace(line = lreader.ReadLine()))
                lyric.Add(new AlsongLyricLine(line));

            return lyric.ToArray();
        }

        public bool GetLyrics()
        {
            if (this.Lyric != null)
                return true;

            var rawStr = AlsongAPI.GetRawLyric(this.LyricID);
            if (rawStr == null)
                return true;

            this.Lyric = ParseLyric(rawStr);
            return true;
        }
    }
}
