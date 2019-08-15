using iTunesLyricOverlay.Alsong;

namespace iTunesLyricOverlay.Wrapper
{
    public class AlsongLyricWrapper
    {
        public AlsongLyricWrapper(IITTrackWrapper track, bool foundByFile, AlsongLyric lyric)
        {
            this.CurrentTrack = track;
            this.FoundByFile  = foundByFile;
            this.AlsongLyric  = lyric;
        }

        public IITTrackWrapper  CurrentTrack { get; }
        public bool             FoundByFile  { get; }
        public AlsongLyric      AlsongLyric  { get; }
    }
}
