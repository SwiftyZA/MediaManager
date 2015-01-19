namespace MediaManager.GUI.Play
{
    using Caliburn.Micro;
    using CommonNetworkObjects;
    using System.ComponentModel.Composition;
    using MediaManager.Data.Models;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Interfaces;
    using MediaManager.Framework.Misc;
    using MediaManager.Networking;

    [Export(typeof(IStartupScreen))]
    public class NowPlayingViewModel : BaseScreen, IStartupScreen, IHandle<WinampCommand>
    {
        readonly IEventAggregator _eventAggregator;
        ConnectionMonger DeepThroat = new ConnectionMonger();//This is a reference to watergate, get you mind out of the gutter!
        int _volume;

        public int Volume
        {
            get { return _volume; }
            set { _volume = value; NotifyOfPropertyChange(() => Volume); }
        }

        public string MenuTitle
        {
            get { return "Now Playing"; }
        }

        public int Sequence
        {
            get { return 1; }
        }

        public string Icon
        { get { return "..\\Images\\Play.png"; } }

        public string Title
        {
            get
            {
                return WinampComs.GetCurrentSongTitle();
            }
        }

        public void Play()
        {
            WinampComs.Play();
        }

        public void Pause()
        {
            WinampComs.Pause();
        }

        public void Next()
        {
            WinampComs.NextTrack();
        }
        public void Previous()
        {
            WinampComs.PrevTrack();
        }
        public void Stop()
        {
            WinampComs.Stop();
        }
        public void VolumeUp()
        {
            WinampComs.VolumeUp();
        }

        public void VolumeDown()
        {
            WinampComs.VolumeDown();
        }

        public void SetVolume()
        {
            WinampComs.SetVolume(Volume);
        }


        [ImportingConstructor]
        public NowPlayingViewModel(IDialogManager dialogs, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            Dialogs = dialogs;

            UDPBroadcaster.StartUdpBroadcast(2211);
            DeepThroat.StartServer();

        }

        public void Handle(WinampCommand message)
        {
            switch (message.Opperation)
            {
                case WinampOpperations.NextTrack:
                    Next();
                    break;
                case WinampOpperations.Pause:
                    Pause();
                    break;
                case WinampOpperations.Play:
                    Play();
                    break;
                case WinampOpperations.PreviousTrack:
                    Previous();
                    break;
                case WinampOpperations.SetVolume:
                    int vol;
                    if (int.TryParse(message.Value, out vol))
                        Volume = vol;
                    SetVolume();
                    break;
                case WinampOpperations.Stop:
                    Stop();
                    break;
                case WinampOpperations.VolumeDown:
                    VolumeDown();
                    break;
                case WinampOpperations.VolumeUp:
                    VolumeUp();
                    break;
            }
        }
    }
}
