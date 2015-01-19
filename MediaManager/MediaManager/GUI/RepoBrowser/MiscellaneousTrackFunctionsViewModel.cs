namespace MediaManager.GUI.RepoBrowser
{
    using System.IO;
    using System.Linq;
    using MediaManager.Data;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.GUI.StaticObjects;
    public class MiscellaneousTrackFunctionsViewModel : BaseScreen
    {
        MusicVideo _vid;
        Track _track;
        public bool IsAddEnabled
        {
            get
            {
                return _vid == null && _track != null;
            }
        }

        public bool IsRemoveEnabled
        {
            get
            {
                return _vid != null && _track != null;
            }
        }

        public string HasMusicVid
        {
            get { return _vid != null ? "Yes" : "No"; }
        }

        public MusicVideo Vid
        {
            get { return _vid; }
            set
            {
                _vid = value;
                NotifyOfPropertyChange(() => IsAddEnabled);
                NotifyOfPropertyChange(() => IsRemoveEnabled);
                NotifyOfPropertyChange(() => HasMusicVid);
            }
        }

        public void LoadMiscFunctions(int trackId)
        {
            _track = DAL.AdHocData.Tracks.FirstOrDefault(x => x.Id == trackId);
            if (_track != null)
                Vid = DAL.AdHocData.MusicVideos.FirstOrDefault(x => x.TrackId == trackId);
        }

        public void AddVid()
        {
            var fileBrowser = new System.Windows.Forms.OpenFileDialog();
            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            var file = new FileInfo(fileBrowser.FileName);

            var repoDir = Global.AppParameters.FirstOrDefault(x => x.Name == "MusicRepo").Value;

            var copyToDir = string.Format("{0}\\{1}\\{2}", repoDir, _track.Album.Band.Name, _track.Album.Name);
            if (!System.IO.Directory.Exists(copyToDir))
                System.IO.Directory.CreateDirectory(copyToDir);
            var fullDir = string.Format("{0}\\{1}", copyToDir, fileBrowser.SafeFileName);

            if (!System.IO.File.Exists(fullDir))
            {
                file.CopyTo(fullDir);
            }

            AddVideoToDB(_track.Album.Band.Name, _track.Album.Name, _track.Album.Genre.Name, _track.Name, fullDir);
            ShowMessageBox("Successfully Saved Musicvideo", "", Framework.Enumerators.MessageBoxOptions.Ok);
        }

        public void AddVideoToDB(string band, string album, string genre, string track, string filePath)
        {
            var _band = DAL.GetBand(band, true);
            var _genre = DAL.GetGenre(genre, true);
            var _album = DAL.GetAlbum(album, true, _band.Id, _genre.Id);
            var vidTrack = DAL.Data.Tracks.Add(new Track() { AlbumID = _album.Id, Name = track, FilePath = filePath });
            DAL.SaveData();
            Vid = DAL.Data.MusicVideos.Add(new MusicVideo() { MusicVideoId = vidTrack.Id, TrackId = _track.Id });
            DAL.SaveData();
        }
    }
}
