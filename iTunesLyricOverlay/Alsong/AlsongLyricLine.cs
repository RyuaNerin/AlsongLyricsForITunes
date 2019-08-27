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
        public static TimeSpan ParseTime(string time)
        {
            if (TimeSpan.TryParseExact(time, @"mm\:ss\.ff", CultureInfo.CurrentCulture, out var result))
                return result;

            return TimeSpan.ParseExact(time, @"\[mm\:ss\.ff\]", CultureInfo.CurrentCulture);
        }

        public AlsongLyricLine(string data)
        {
            var m = ReLine.Match(data);

            try
            {
                this.Time = ParseTime(m.Groups[1].Value);
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
