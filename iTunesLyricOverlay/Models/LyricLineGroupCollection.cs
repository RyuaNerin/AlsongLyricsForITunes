using System.Collections.Generic;
using System.Text;

namespace iTunesLyricOverlay.Models
{
    public class LyricLineGroupCollection : List<LyricLineGroupModel>
    {
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
