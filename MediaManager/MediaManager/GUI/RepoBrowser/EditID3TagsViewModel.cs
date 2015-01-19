namespace MediaManager.GUI.RepoBrowser
{
    using TagLib;
    using System.Linq;
    using System.Collections.Generic;
    using MediaManager.Data;
    using MediaManager.Framework.BaseClasses;
    using System;
    using MediaManager.GUI.StaticObjects;
    public class EditID3TagsViewModel : BaseScreen
    {
        #region FIELDS & PROPERTIES
        File _iD3Tags;
        Track _track;
        string _fileName;

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                NotifyOfPropertyChange(() => FileName);
            }
        }

        public string Artist
        {
            get
            {
                if (_iD3Tags == null) return "";

                return _iD3Tags.Tag.FirstAlbumArtist != null ? _iD3Tags.Tag.FirstAlbumArtist : (_iD3Tags.Tag.FirstArtist != null ? _iD3Tags.Tag.FirstArtist : "Unknown");
            }
            set
            {
                _iD3Tags.Tag.AlbumArtists = new string[] { value };
                NotifyOfPropertyChange(() => Artist);
            }
        }

        public string Album
        {
            get
            {
                if (_iD3Tags == null) return "";

                return _iD3Tags.Tag.Album != null ? _iD3Tags.Tag.Album : "Unknown";
            }
            set
            {
                _iD3Tags.Tag.Album = value;
                NotifyOfPropertyChange(() => Album);
            }
        }

        public string Title
        {
            get
            {
                if (_iD3Tags == null) return "";

                return _iD3Tags.Tag.Title != null ? _iD3Tags.Tag.Title : FileName;
            }
            set
            {
                _iD3Tags.Tag.Title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        public string Genre
        {
            get
            {
                if (_iD3Tags == null) return "";

                return _iD3Tags.Tag.FirstGenre != null ? _iD3Tags.Tag.FirstGenre : "Unknown";
            }
            set
            {
                _iD3Tags.Tag.Genres = new string[] { value };
                NotifyOfPropertyChange(() => Genre);
            }
        }
        public string TrackNr
        {
            get
            {
                if (_iD3Tags == null) return "";

                return _iD3Tags.Tag.Track.ToString();
            }
            set
            {
                try
                {
                    _iD3Tags.Tag.Track = Convert.ToUInt16(value); ;
                }
                catch (Exception ex) { }//no point in handling this
                NotifyOfPropertyChange(() => TrackNr);
            }
        }

        public bool Enabled
        {
            get { return _iD3Tags != null; }
        }

        public File ID3Tags
        {
            get { return _iD3Tags; }
            set
            {
                _iD3Tags = value;
                NotifyOfPropertyChange(() => Enabled);
                NotifyOfPropertyChange(() => Artist);
                NotifyOfPropertyChange(() => Album);
                NotifyOfPropertyChange(() => Genre);
                NotifyOfPropertyChange(() => Title);
                NotifyOfPropertyChange(() => TrackNr);
            }
        }
        #endregion FIELDS & PROPERTIES

        public EditID3TagsViewModel()
        {
        }

        public void LoadID3Tags(int trackId)
        {
            _track = DAL.AdHocData.Tracks.FirstOrDefault(x => x.Id == trackId);
            if (_track == null) return;
            FileName = _track.FilePath.Substring(_track.FilePath.LastIndexOf('\\') + 1);

            ID3Tags = TagLib.File.Create(_track.FilePath);
        }

        public void Save()
        {
            _iD3Tags.Save();
            UpdateTrackDBAndRepo();
            ShowMessageBox("Save completed successfully", "", Framework.Enumerators.MessageBoxOptions.Ok);
        }

        public void UpdateTrackDBAndRepo()
        {
            var repoDir = Global.AppParameters.FirstOrDefault(x => x.Name == "MusicRepo").Value;

            var copyToDir = string.Format("{0}\\{1}\\{2}", repoDir, Artist, Album);
            if (!System.IO.Directory.Exists(copyToDir))
                System.IO.Directory.CreateDirectory(copyToDir);
            var fullDir = string.Format("{0}\\{1}", copyToDir, FileName);

            System.IO.File.Move(_track.FilePath, fullDir);

            var _band = DAL.GetBand(Artist, true);
            var _genre = DAL.GetGenre(Genre, true);
            var _album = DAL.GetAlbum(Album, true, _band.Id, _genre.Id);
            DAL.MoveTrack(_track.Id, _album.Id, Title, fullDir);
        }
    }
}
