namespace MediaManager.GUI.RepoBrowser
{
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using MediaManager.Data;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Interfaces;
    using Caliburn.Micro;

    [Export(typeof(IStartupScreen))]
    public class TracksViewModel : BaseScreen, IStartupScreen
    {
        List<TreeViewItem> _tracks;
        readonly IEventAggregator _eventAggregator;
        TrackPropertiesViewModel _trackProperties;

        public TrackPropertiesViewModel TrackProperties
        {
            get
            {
                if (_trackProperties == null)
                    _trackProperties = new TrackPropertiesViewModel() { Parent = this };
                return _trackProperties;
            }
            set
            {
                _trackProperties = value;
                NotifyOfPropertyChange(() => TrackProperties);
            }
        }

        public List<TreeViewItem> Tracks
        {
            get
            {
                if (_tracks == null) _tracks = new List<TreeViewItem>();
                return _tracks;
            }
            set
            {
                _tracks = value;
                NotifyOfPropertyChange(() => Tracks);

            }
        }

        [ImportingConstructor]
        public TracksViewModel(IDialogManager dialogs, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            Dialogs = dialogs;
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            Refresh();
        }


        void tvi_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            var parent = e.Source as TreeViewItem;

            if ((parent.Items.Count == 1) && (parent.Items[0] is string))
            {
                parent.Items.Clear();

                if (parent.Tag is Band)
                {
                    var id = ((Band)parent.Tag).Id;
                    var albums = DAL.Data.Albums.Where(x => x.BandID == id).ToList();

                    foreach (var a in albums)
                        parent.Items.Add(CreateNode(a.Name, a));
                }
                else if (parent.Tag is Album)
                {
                    var id = ((Album)parent.Tag).Id;

                    var videos = from t in DAL.Data.Tracks
                                 join mv in DAL.Data.MusicVideos on t.Id equals mv.MusicVideoId
                                 where t.AlbumID == id
                                 select t;

                    var tracks = DAL.Data.Tracks.Where(x => x.AlbumID == id).Except(videos).ToList();

                    foreach (var t in tracks)
                        parent.Items.Add(CreateNode(t.Name, t, false));
                }
            }
        }

        private TreeViewItem CreateNode(string name, object o, bool addChild = true)
        {
            var tvi = new TreeViewItem() { Header = name, Tag = o };
            if (addChild)
                tvi.Items.Add("Loading...");
            tvi.Expanded += tvi_Expanded;
            return tvi;
        }

        public void Refresh()
        {
            Tracks = new List<TreeViewItem>();
            var bands = DAL.Data.Bands.ToList();
            foreach (var b in bands)
                Tracks.Add(CreateNode(b.Name, b));

            //Force the treeview to update;
            var view = (TracksView)GetView();
            view.Tracks.Items.Refresh();
        }

        public void SelectedItem(TreeViewItem tvi)
        {
            if (tvi != null && tvi.Tag is Track)
                TrackProperties.TrackId = ((Track)tvi.Tag).Id;
        }

        public string MenuTitle
        {
            get { return "Repository"; }
        }

        public int Sequence
        {
            get
            {
                return 20;
            }
        }

        public string Icon
        { get { return "..\\Images\\Repo.png"; } }
    }
}
