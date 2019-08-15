using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using iTunesLib;

namespace iTunesLyricOverlay.Wrapper
{
    public class ITunesWrapper
    {
        public void Start()
        {
            new Thread(this.MonitorITunes) { Priority = ThreadPriority.Lowest, IsBackground = true }.Start();
        }

        public void Stop()
        {
            this.m_running = false;
            this.m_threadMonitor.WaitOne();

            this.DeinitITunes();
        }

        public event Action ITunesAttached;
        public event Action ITunesDeattached;

        public event _IiTunesEvents_OnDatabaseChangedEventEventHandler           OnDatabaseChangedEvent;
        public event _IiTunesEvents_OnPlayerPlayEventEventHandler                OnPlayerPlayEvent;
        public event _IiTunesEvents_OnPlayerStopEventEventHandler                OnPlayerStopEvent;
        public event _IiTunesEvents_OnPlayerPlayingTrackChangedEventEventHandler OnPlayerPlayingTrackChangedEvent;
        public event _IiTunesEvents_OnUserInterfaceEnabledEventEventHandler      OnUserInterfaceEnabledEvent;
        public event _IiTunesEvents_OnCOMCallsDisabledEventEventHandler          OnCOMCallsDisabledEvent;
        public event _IiTunesEvents_OnCOMCallsEnabledEventEventHandler           OnCOMCallsEnabledEvent;
        public event _IiTunesEvents_OnQuittingEventEventHandler                  OnQuittingEvent;
        public event _IiTunesEvents_OnAboutToPromptUserToQuitEventEventHandler   OnAboutToPromptUserToQuitEvent;
        public event _IiTunesEvents_OnSoundVolumeChangedEventEventHandler        OnSoundVolumeChangedEvent;

        private volatile bool m_running = true;

        private readonly ManualResetEvent m_threadMonitor = new ManualResetEvent(false);

        private readonly object m_itunesLock = new object();
        private int m_itunesPidof = -1;
        private iTunesApp m_itunes;

        private void MonitorITunes()
        {
            while (this.m_running)
            {
                var process = Process.GetProcessesByName("iTunes");
                if (process == null || process.Length == 0)
                {
                    this.DeinitITunes();
                }
                else
                {
                    using (process[0])
                    {
                        if (this.m_itunesPidof != process[0].Id)
                        {
                            this.m_itunesPidof = process[0].Id;

                            this.DeinitITunes();
                            this.InitITunes();
                        }
                    }
                }

                Thread.Sleep(1000);
            }

            this.m_threadMonitor.Set();
        }

        private void InitITunes()
        {
            lock (this.m_itunesLock)
            {
                this.m_itunes = new iTunesAppClass();

                this.m_itunes.OnAboutToPromptUserToQuitEvent   += this.ITunes_OnAboutToPromptUserToQuitEvent;
                this.m_itunes.OnCOMCallsDisabledEvent          += this.ITunes_OnCOMCallsDisabledEvent;
                this.m_itunes.OnCOMCallsEnabledEvent           += this.ITunes_OnCOMCallsEnabledEvent;
                this.m_itunes.OnDatabaseChangedEvent           += this.ITunes_OnDatabaseChangedEvent;
                this.m_itunes.OnPlayerPlayEvent                += this.ITunes_OnPlayerPlayEvent;
                this.m_itunes.OnPlayerPlayingTrackChangedEvent += this.ITunes_OnPlayerPlayingTrackChangedEvent;
                this.m_itunes.OnPlayerStopEvent                += this.ITunes_OnPlayerStopEvent;
                this.m_itunes.OnQuittingEvent                  += this.ITunes_OnQuittingEvent;
                this.m_itunes.OnSoundVolumeChangedEvent        += this.ITunes_OnSoundVolumeChangedEvent;
                this.m_itunes.OnUserInterfaceEnabledEvent      += this.ITunes_OnUserInterfaceEnabledEvent;

                this.ITunesAttached?.Invoke();
            }
        }

        private void DeinitITunes()
        {
            lock (this.m_itunesLock)
            {
                if (this.m_itunes == null)
                    return;

                /*
                try
                {
                    this.m_itunes.OnAboutToPromptUserToQuitEvent   -= this.ITunes_OnAboutToPromptUserToQuitEvent;
                    this.m_itunes.OnCOMCallsDisabledEvent          -= this.ITunes_OnCOMCallsDisabledEvent;
                    this.m_itunes.OnCOMCallsEnabledEvent           -= this.ITunes_OnCOMCallsEnabledEvent;
                    this.m_itunes.OnDatabaseChangedEvent           -= this.ITunes_OnDatabaseChangedEvent;
                    this.m_itunes.OnPlayerPlayEvent                -= this.ITunes_OnPlayerPlayEvent;
                    this.m_itunes.OnPlayerPlayingTrackChangedEvent -= this.ITunes_OnPlayerPlayingTrackChangedEvent;
                    this.m_itunes.OnPlayerStopEvent                -= this.ITunes_OnPlayerStopEvent;
                    this.m_itunes.OnQuittingEvent                  -= this.ITunes_OnQuittingEvent;
                    this.m_itunes.OnSoundVolumeChangedEvent        -= this.ITunes_OnSoundVolumeChangedEvent;
                    this.m_itunes.OnUserInterfaceEnabledEvent      -= this.ITunes_OnUserInterfaceEnabledEvent;
                }
                catch
                {
                }
                */

                try
                {
                    Marshal.ReleaseComObject(this.m_itunes);
                }
                catch
                {
                }

                this.m_itunes = null;
                this.ITunesDeattached?.Invoke();
            }
        }

        private void ITunes_OnUserInterfaceEnabledEvent()
            => this.OnUserInterfaceEnabledEvent?.Invoke();

        private void ITunes_OnSoundVolumeChangedEvent(int newVolume)
            => this.OnSoundVolumeChangedEvent?.Invoke(newVolume);

        private void ITunes_OnQuittingEvent()
        {
            this.DeinitITunes();
            this.OnQuittingEvent?.Invoke();
        }

        private void ITunes_OnPlayerStopEvent(object iTrack)
            => this.OnPlayerStopEvent?.Invoke(IITTrackWrapper.Convert((IITTrack)iTrack));

        private void ITunes_OnPlayerPlayingTrackChangedEvent(object iTrack)
            => this.OnPlayerPlayingTrackChangedEvent?.Invoke(IITTrackWrapper.Convert((IITTrack)iTrack));

        private void ITunes_OnPlayerPlayEvent(object iTrack)
            => this.OnPlayerPlayEvent?.Invoke(IITTrackWrapper.Convert((IITTrack)iTrack));

        private void ITunes_OnDatabaseChangedEvent(object deletedObjectIDs, object changedObjectIDs)
            => this.OnDatabaseChangedEvent?.Invoke(deletedObjectIDs, changedObjectIDs);

        private void ITunes_OnCOMCallsEnabledEvent()
            => this.OnCOMCallsEnabledEvent?.Invoke();

        private void ITunes_OnCOMCallsDisabledEvent([ComAliasName("iTunesLib.ITCOMDisabledReason")] ITCOMDisabledReason reason)
            => this.OnCOMCallsDisabledEvent?.Invoke(reason);

        private void ITunes_OnAboutToPromptUserToQuitEvent()
        {
            this.DeinitITunes();
            this.OnAboutToPromptUserToQuitEvent?.Invoke();
        }

        private void iTunes_OnAboutToPromptUserToQuitEvent()
        {
            this.DeinitITunes();
        }

        //////////////////////////////////////////////////

        public ITPlayerState? PlayerState
        {
            get
            {
                lock (this.m_itunesLock)
                    return this.m_itunes?.PlayerState;
            }
        }

        public IITTrackWrapper CurrentTrack
        {
            get
            {
                lock (this.m_itunesLock)
                    return IITTrackWrapper.Convert(this.m_itunes?.CurrentTrack);
            }
        }

        public int PlayerPositionMS
        {
            get
            {
                lock (this.m_itunesLock)
                {
                    return this.m_itunes?.PlayerPositionMS ?? 0;
                }
            }
            set
            {
                lock (this.m_itunesLock)
                {
                    if (this.m_itunes != null)
                    {
                        this.m_itunes.PlayerPositionMS = value;
                    }
                }
            }
        }
    }
}
