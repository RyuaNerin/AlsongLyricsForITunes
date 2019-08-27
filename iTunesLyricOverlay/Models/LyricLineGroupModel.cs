using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using iTunesLyricOverlay.Alsong;

namespace iTunesLyricOverlay.Models
{
    [DebuggerDisplay("{this}")]
    public class LyricLineGroupModel : List<LyricLineModel>
    {
        public LyricLineGroupModel(TimeSpan time)
        {
            this.Time = time;
        }
        public LyricLineGroupModel(AlsongLyricLine line)
        {
            this.Time = line.Time;

            this.Add(new LyricLineModel(line));
        }
        public LyricLineGroupModel(IEnumerable<AlsongLyricLine> lines)
        {
            this.Time = lines.First().Time;

            this.AddRange(lines.Select(e => new LyricLineModel(e)));
        }

        public override string ToString()
            => $"[{this.Time.ToString(@"mm\:ss\.ff")}] Lines = {this.Count}";

        public TimeSpan Time { get; set; }

        public string TimeStr => this.Time.ToString(@"mm\:ss\.ff");

        private bool m_focused;
        public bool Focused
        {
            get => this.m_focused;
            set
            {
                this.m_focused = value;

                foreach (var line in this)
                    line.Focused = value;
            }
        }
    }
}
