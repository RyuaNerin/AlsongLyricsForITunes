using iTunesLib;

namespace iTunesLyricOverlay.Wrapper
{
    public class IITTrackWrapper
    {
        public static IITTrackWrapper Convert(IITTrack track)
            => track == null ? null : new IITTrackWrapper(track);

        public IITTrackWrapper(IITTrack track)
        {
            this.m_track = track;

            this.Duration = track.Duration;
            this.Artist   = track.Artist ?? track.Composer;
            this.Title    = track.Name;
            this.Album    = track.Album;
            this.Location = ((dynamic)track).Location as string;
            this.TrackID  = track.trackID;
        }

        private readonly IITTrack m_track;

        public int      Duration  { get; }
        public string   Artist    { get; }
        public string   Title     { get; }
        public string   Album     { get; }
        public string   Location  { get; }
        public int      TrackID   { get; }

        public void SetLyrics(string lyrics)
        {
            try
            {
                ((dynamic)this.m_track).Lyrics = lyrics;
            }
            catch
            {
            }
        }
    }
}
