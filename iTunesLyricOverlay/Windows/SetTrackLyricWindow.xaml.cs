using System.Collections.Generic;
using System.Text;
using System.Windows;
using iTunesLyricOverlay.Models;
using iTunesLyricOverlay.Wrapper;

namespace iTunesLyricOverlay.Windows
{
    public partial class SetTrackLyricWindow : Window
    {
        private readonly LyricLineGroupModel[]  m_lyricGroup;
        private readonly IITTrackWrapper        m_track;

        private string m_lyric;

        public SetTrackLyricWindow(IITTrackWrapper track, LyricLineGroupModel[] lyric)
        {
            this.InitializeComponent();

            this.m_track = track;
            this.m_lyricGroup = lyric;

            this.ctlNowPlaying.Text = track.NowPlaying;

            this.CheckBox_IsCheckedChanged(null, null);

            this.ctlLyricShowTime .Checked   += this.CheckBox_IsCheckedChanged;
            this.ctlLyricShowTime .Unchecked += this.CheckBox_IsCheckedChanged;
            this.ctlLyricBlankLine.Checked   += this.CheckBox_IsCheckedChanged;
            this.ctlLyricBlankLine.Unchecked += this.CheckBox_IsCheckedChanged;
        }

        private void CheckBox_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            var showTime     = this.ctlLyricShowTime.IsChecked ?? false;
            var addBlankLine = this.ctlLyricBlankLine.IsChecked ?? false;

            var sb = new StringBuilder(4096);
            var lst = new List<string>();

            for (int i = 0; i < this.m_lyricGroup.Length; ++i)
            {
                var isMultiLine = this.m_lyricGroup[i].Count > 1;

                if (showTime)
                {
                    lst.Add($"[{this.m_lyricGroup[i].TimeStr}]");
                    sb.AppendLine($"[{this.m_lyricGroup[i].TimeStr}]");
                }

                foreach (var line in this.m_lyricGroup[i])
                {
                    if (!isMultiLine || !string.IsNullOrWhiteSpace(line.Line.Text))
                    {
                        lst.Add(line.Line.Text);
                        sb.AppendLine(line.Line.Text);
                    }
                }

                if (addBlankLine)
                {
                    if (i + 1 < this.m_lyricGroup.Length)
                    {
                        lst.Add("");
                        sb.AppendLine();
                    }
                }
            }

            this.ctlPreview.ItemsSource = lst;
            this.m_lyric = sb.ToString();
        }

        private void ctlApply_Click(object sender, RoutedEventArgs e)
        {
            this.ctlApply.IsEnabled = false;

            this.m_track.SetLyrics(this.m_lyric);

            this.ctlApply.IsEnabled = true;

            MessageBox.Show(this, "아이튠즈 보관함에 적용하였습니다.");
            this.Close();
        }
    }
}
