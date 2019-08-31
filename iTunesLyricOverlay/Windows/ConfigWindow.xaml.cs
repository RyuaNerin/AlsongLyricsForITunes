using System.ComponentModel;
using System.Windows;
using iTunesLyricOverlay.Alsong;
using iTunesLyricOverlay.Models;

namespace iTunesLyricOverlay.Windows
{
    public partial class ConfigWindow : Window
    {
        private static readonly LyricLineGroupCollection MainWindowLyricExample = new LyricLineGroupCollection
        {
            new LyricLineGroupModel(AlsongLyricLine.ParseTime("[00:18.73]"))
            {
                new LyricLineModel(new AlsongLyricLine("[00:18.73]絡まった蜘蛛の巣があたしを指差して")),
                new LyricLineModel(new AlsongLyricLine("[00:18.73]카라맛타 쿠모노 스가 아타시오 유비사시테")),
                new LyricLineModel(new AlsongLyricLine("[00:18.73]얽힌 거미집이 나를 손가락질하고")),
            },
            new LyricLineGroupModel(AlsongLyricLine.ParseTime("[00:18.73]"))
            {
                new LyricLineModel(new AlsongLyricLine("[00:23.36]浮ついた胸の奥に皮肉を投げる")),
                new LyricLineModel(new AlsongLyricLine("[00:23.36]우와츠이타 무네노 오쿠니 히니쿠오 나게루")),
                new LyricLineModel(new AlsongLyricLine("[00:23.36]들뜬 가슴에 야유를 던져")),
            }
        };

        private readonly Config m_config;

        public ConfigWindow()
        {
            this.InitializeComponent();

            this.m_config = Config.Instance.Clone();
            this.DataContext = this.m_config;
        }

        public new void Show()
        {
            this.m_config.CopyFrom(Config.Instance);

            this.ctlMainWindowLyricsOption_IsCheckedChanged(null, null);

            base.Show();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void ctlMainWindowLyricsOption_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            this.ctlMainWindowLyricPreview.Text = MainWindowLyricExample.Format(this.ctlApplyLyricsToITunes_WithTime.IsChecked ?? false, this.ctlApplyLyricsToITunes_WithBlankLine.IsChecked ?? false).TrimEnd('\r', '\n');
        }

        private void CtlRealtimeUpdate_Checked(object sender, RoutedEventArgs e)
        {
            this.DataContext = Config.Instance;
        }

        private void CtlRealtimeUpdate_Unchecked(object sender, RoutedEventArgs e)
        {
            this.m_config.CopyFrom(Config.Instance);
            this.DataContext = this.m_config;
        }

        private void ctlOk_Click(object sender, RoutedEventArgs e)
        {
            Config.Instance.CopyFrom(this.m_config);
            this.Close();
        }

        private void ctlApply_Click(object sender, RoutedEventArgs e)
        {
            Config.Instance.CopyFrom(this.m_config);
        }

        private void ctlReset_Click(object sender, RoutedEventArgs e)
        {
            this.m_config.CopyFrom(Config.Instance);
        }

        private void ctlClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {

        }
    }
}
