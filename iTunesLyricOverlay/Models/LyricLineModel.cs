using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using iTunesLyricOverlay.Alsong;

namespace iTunesLyricOverlay.Models
{
    [DebuggerDisplay("{this}")]
    public class LyricLineModel : INotifyPropertyChanged
    {
        public LyricLineModel(AlsongLyricLine line)
        {
            this.Line = line;
        }

        public AlsongLyricLine Line { get; }

        private bool m_focused = false;
        public bool Focused
        {
            get => this.m_focused;
            set
            {
                this.m_focused = value;
                this.OnPropertyChanged();
            }
        }

        public override string ToString()
            => $"[{this.Line.Time.ToString(@"mm\:ss\.ff")}] {this.Line.Text}";

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
