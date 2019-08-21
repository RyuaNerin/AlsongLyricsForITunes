using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace iTunesLyricOverlay.Alsong
{
    [DebuggerDisplay("{this}")]
    public class AlsongLyricLine
    {
        public AlsongLyricLine()
        {
        }

        private static readonly Regex ReLine = new Regex(@"^\[(\d\d:\d\d.\d\d)\](.*)$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        public AlsongLyricLine(string data)
        {
            var m = ReLine.Match(data);

            try
            {
                this.Time = TimeSpan.ParseExact(m.Groups[1].Value, @"mm\:ss\.ff", CultureInfo.CurrentCulture);
                this.Text = m.Groups[2].Value.Trim();
            }
            catch
            {
            }
        }

        public AlsongLyricLine(TimeSpan time, string text)
        {
            this.Time = time;
            this.Text = text;
        }

        public override string ToString()
            => $"[{this.Time.ToString(@"mm\:ss\.ff")}] {this.Text}";

        public TimeSpan Time    { get; set; }
        public string   Text    { get; set; }
    }
}
