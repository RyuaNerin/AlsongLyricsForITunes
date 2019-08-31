using System.Windows;
using iTunesLyricOverlay.Database;
using iTunesLyricOverlay.Models;

namespace iTunesLyricOverlay.Windows
{
    public partial class LyricArchiveViewWindow : Window
    {
        public LyricArchiveViewWindow(LyricArchive archive)
        {
            this.InitializeComponent();

            this.ctlArtist  .Text = archive.Artist;
            this.ctlTitle   .Text = archive.Title;
            this.ctlAlbum   .Text = archive.Album;
            this.ctlDuration.Text = archive.DurationStr;

            this.ctlItemsControl.ItemsSource = new LyricLineGroupCollection(archive.Lyric);
        }
    }
}
