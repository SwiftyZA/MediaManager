namespace MediaManager.GUI.PlayList
{
    using System.Linq;
    using MoreLinq;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using MediaManager.Data;
    using MediaManager.Framework.BaseClasses;
    public class PlaylistTracksViewModel : BaseScreen
    {
        List<TreeViewItem> _tracks;
        List<sp_GetPlayListTracks_Result> _playlist;

        public List<TreeViewItem> Tracks
        {
            get { return _tracks; }
            set
            {
                _tracks = value;
                NotifyOfPropertyChange(() => Tracks);
            }
        }

        public PlaylistTracksViewModel(int playListId, bool PreferMVids, bool includePropLess)
        {
            _tracks = new List<TreeViewItem>();
            _playlist = DAL.AdHocData.sp_GetPlayListTracks(playListId, PreferMVids, includePropLess).ToList();

            var artists = _playlist.DistinctBy(x => x.Artist).Select(x => x.Artist).ToList();
            foreach (var a in artists)
                Tracks.Add(AddArtist(a));

        }

        private TreeViewItem AddArtist(string artist)
        {
            var tvi = new TreeViewItem() { Header = artist };
            var albums = _playlist.Where(x => x.Artist == artist).DistinctBy(x => x.AlbumID).Select(x => new { x.AlbumID, x.Album }).ToList();
            foreach (var a in albums)
                tvi.Items.Add(AddAlbum(a.AlbumID, a.Album));
            return tvi;
        }

        private TreeViewItem AddAlbum(int albumId, string album)
        {
            var tvi = new TreeViewItem() { Header = album };
            var tracks = _playlist.Where(x => x.AlbumID == albumId).ToList();
            foreach (var t in tracks)
                tvi.Items.Add(new TreeViewItem() { Header = t.Title });
            return tvi;

        }
    }
}
