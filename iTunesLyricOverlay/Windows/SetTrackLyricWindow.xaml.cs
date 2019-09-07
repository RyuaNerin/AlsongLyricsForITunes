using System.Windows;
using iTunesLyricOverlay.Models;
using iTunesLyricOverlay.Wrapper;

namespace iTunesLyricOverlay.Windows
{
    public partial class SetTrackLyricWindow : Window
    {
        private readonly LyricLineGroupCollection   m_lyricGroup;
        private readonly IITTrackWrapper            m_track;

        private string m_lyric;

        public SetTrackLyricWindow(IITTrackWrapper track, LyricLineGroupCollection lyric)
        {
            this.InitializeComponent();

            this.m_track = track;
            this.m_lyricGroup = lyric;

            this.ctlNowPlayingTitle.Text = track.Title;
            this.ctlNowPlayingAlbum.Text = track.ArtistAndAlbum;

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

            var lyric = this.m_lyricGroup.Format(showTime, addBlankLine, out var lines);

            this.ctlPreview.ItemsSource = lines;
            this.m_lyric = lyric; 
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
