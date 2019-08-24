using System;
using System.IO;

namespace iTunesLyricOverlay.Utilities
{
    public class LineReader
    {
        public LineReader(TextReader reader)
        {
            this.m_reader = reader;
        }

        private readonly TextReader m_reader;

        private string m_string;

        public string ReadLine()
        {
            if (string.IsNullOrWhiteSpace(this.m_string))
                this.m_string = this.m_reader.ReadLine();

            if (string.IsNullOrWhiteSpace(this.m_string))
                return null;

            string part;

            if ((part = this.CutString("<br>"  )) != null) return part;
            if ((part = this.CutString("<br/>" )) != null) return part;
            if ((part = this.CutString("<br />")) != null) return part;

            part = this.m_string;
            this.m_string = null;
            return part;
        }

        private string CutString(string delim)
        {
            var br = this.m_string.IndexOf(delim, 0, StringComparison.CurrentCultureIgnoreCase);
            if (br != -1)
            {
                var part = this.m_string.Substring(0, br);

                if (br + delim.Length < this.m_string.Length)
                    this.m_string = this.m_string.Substring(br + delim.Length);
                else
                    this.m_string = null;

                return part;
            }
            return null;
        }
    }
}
