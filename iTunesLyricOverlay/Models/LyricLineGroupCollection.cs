using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTunesLyricOverlay.Alsong;

namespace iTunesLyricOverlay.Models
{
    public class LyricLineGroupCollection : List<LyricLineGroupModel>
    {
        public LyricLineGroupCollection()
        {
        }
        public LyricLineGroupCollection(AlsongLyricLine[] lyric)
        {
            var groups = new LyricLineGroupCollection();
            LyricLineGroupModel g = null;

            foreach (var line in lyric)
            {
                if (g == null || g.Time != line.Time)
                {
                    if (g != null)
                        groups.Add(g);

                    g = new LyricLineGroupModel(line.Time);
                }

                g.Add(new LyricLineModel(line));
            }
            if (g.Count > 0)
                groups.Add(g);

            // 모두 공란인 그룹 비우는 작업
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].All(e => string.IsNullOrWhiteSpace(e.Line.Text)))
                {
                    groups[i].RemoveRange(1, groups[i].Count - 1);
                }
            }

            if (groups.Count == 1)
            {
                groups.Clear();
                groups.AddRange(lyric.Select(e => new LyricLineGroupModel(e)).ToArray());
            }
            else
            {
                for (int i = 1; i < groups.Count - 1; ++i)
                {
                    if (groups[i].Time == TimeSpan.Zero)
                    {
                        groups[i].Time = groups[i + 1].Time;
                    }
                }
            }

            this.AddRange(groups);
        }

        public string Format(bool showTime, bool addBlankLine)
        {
            return this.Format(showTime, addBlankLine, out var lines);
        }
        public string Format(bool showTime, bool addBlankLine, out string[] lines)
        {
            var sb = new StringBuilder(4096);
            var lst = new List<string>();

            for (int i = 0; i < this.Count; ++i)
            {
                var isMultiLine = this[i].Count > 1;

                if (showTime)
                {
                    lst.Add($"[{this[i].TimeStr}]");
                    sb.AppendLine($"[{this[i].TimeStr}]");
                }

                foreach (var line in this[i])
                {
                    if (!isMultiLine || !string.IsNullOrWhiteSpace(line.Line.Text))
                    {
                        lst.Add(line.Line.Text);
                        sb.AppendLine(line.Line.Text);
                    }
                }

                if (addBlankLine)
                {
                    if (i + 1 < this.Count)
                    {
                        lst.Add("");
                        sb.AppendLine();
                    }
                }
            }

            lines = lst.ToArray();
            return sb.ToString();
        }
    }
}
