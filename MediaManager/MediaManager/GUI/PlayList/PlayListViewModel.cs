namespace MediaManager.GUI.PlayList
{
    using System.Linq;
    using MediaManager.Data;
    using Caliburn.Micro;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Interfaces;
    using System.Text;
    using MediaManager.GUI.StaticObjects;
    using System.Diagnostics;

    [Export(typeof(IStartupScreen))]
    public class PlayListViewModel : BaseScreen, IStartupScreen
    {
        #region FIELDS & PROPERTIES
        readonly IEventAggregator _eventAggregator;
        List<PlayList> _playLists;
        PlayList _selectedPlayList;
        PlayListCriteriaViewModel _criteria;
        AddNewPlayListViewModel _addNew;
        PlaylistTracksViewModel _viewPlaylist;

        public PlayListCriteriaViewModel Criteria
        {
            get { return _criteria; }
            set { _criteria = value; NotifyOfPropertyChange(() => Criteria); }
        }

        public PlayList SelectedPlayList
        {
            get { return _selectedPlayList; }
            set
            {
                _selectedPlayList = value;
                NotifyOfPropertyChange(() => SelectedPlayList);
                LoadSelectedPlayList();
            }
        }

        public List<PlayList> PlayLists
        {
            get { return _playLists; }
            set { _playLists = value; NotifyOfPropertyChange(() => PlayLists); }
        }
        #endregion FIELDS & PROPERTIES

        [ImportingConstructor]
        public PlayListViewModel(IDialogManager dialogs, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            _criteria = new PlayListCriteriaViewModel() { Parent = this };
            Dialogs = dialogs;
            LoadPlayLists();
        }

        public void LoadSelectedPlayList()
        {
            if (_selectedPlayList != null)
                Criteria.LoadPlayList(_selectedPlayList.Id);
        }
        public void LoadPlayLists()
        {
            PlayLists = DAL.AdHocData.PlayLists.ToList();
            SelectedPlayList = PlayLists.FirstOrDefault();
        }

        public void Add()
        {
            _addNew = new AddNewPlayListViewModel() { Parent = this };
            _addNew.Deactivated += _addNew_Deactivated;
            ShowDialog(_addNew);
        }

        void _addNew_Deactivated(object sender, DeactivationEventArgs e)
        {
            LoadPlayLists();
        }

        public void Play()
        {
            _criteria.Save();

            var sb = new StringBuilder();
            var repo = Global.AppParameters.FirstOrDefault(x => x.Name == "MusicRepo").Value;

            sb.AppendLine("#EXTM3U");

            var playlist = DAL.AdHocData.sp_GetPlayListTracks(_selectedPlayList.Id, _criteria.PreferMusicVideos, _criteria.IncludePropertyless);
            foreach (var f in playlist)
            {
                sb.AppendLine(string.Format("#EXTINF:{0},{1} - {2} {3}", f.Duration, f.Artist, f.TrackNr, f.Title));
                sb.AppendLine(f.FilePath.Replace(string.Format("{0}\\", repo), ""));
            }

            System.IO.File.WriteAllText(string.Format("{0}\\playlist.m3u", repo), sb.ToString());

            Process.Start(string.Format("{0}\\playlist.m3u", repo));
        }

        public void ShowPlayList()
        {
            _viewPlaylist = new PlaylistTracksViewModel(SelectedPlayList.Id, _criteria.PreferMusicVideos, _criteria.IncludePropertyless);
            ShowDialog(_viewPlaylist);
        }

        public string MenuTitle
        {
            get { return "Playlists"; }
        }

        public int Sequence
        {
            get
            {
                return 10;
            }
        }

        public string Icon
        { get { return "..\\Images\\Playlist.png"; } }
    }
}
