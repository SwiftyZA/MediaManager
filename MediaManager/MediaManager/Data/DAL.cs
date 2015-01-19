using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Data
{
    public static class DAL
    {
        static DataEntities _data;

        public static DataEntities Data
        {
            get
            {
                if (_data == null)
                    _data = new DataEntities();
                return DAL._data;
            }
            set { DAL._data = value; }
        }

        public static DataEntities AdHocData
        {
            get
            {
                return new DataEntities();
            }
        }

        public static bool SaveData()
        {
            try
            {
                _data.SaveChanges();
            }
            catch (Exception ex)
            {
                _data.Dispose();
                _data = new DataEntities();
                return false;

            }
            return true;
        }

        public static Band GetBand(string name, bool AddIfNull)
        {
            var band = _data.Bands.FirstOrDefault(x => x.Name == name);
            if (band == null && AddIfNull)
            {
                band = _data.Bands.Add(new Band() { Name = name });
                SaveData();
            }
            return band;
        }

        public static Album GetAlbum(string name, bool AddIfNull, int bandId, int genreId)
        {
            var album = _data.Albums.FirstOrDefault(x => x.Name == name && x.BandID == bandId);
            if (album == null && AddIfNull)
            {
                album = _data.Albums.Add(new Album() { Name = name, BandID = bandId, GenreID = genreId });
                SaveData();
            }
            return album;
        }

        public static Genre GetGenre(string name, bool AddIfNull)
        {
            var genre = _data.Genres.FirstOrDefault(x => x.Name == name);
            if (genre == null && AddIfNull)
            {
                genre = _data.Genres.Add(new Genre() { Name = name });
                SaveData();
            }
            return genre;
        }

        public static void AddTrack(string title, int albumId, string filePath, int duration, int trackNr)
        {
            var album = _data.Albums.FirstOrDefault(x => x.Id == albumId);
            if (album.Tracks.FirstOrDefault(x => x.Name == title) == null)
                album.Tracks.Add(new Track() { Name = title, FilePath = filePath, Duration = duration, TrackNr = trackNr });
            SaveData();
        }

        public static void MoveTrack(int trackId, int albumId, string title, string path)
        {
            var track = _data.Tracks.FirstOrDefault(x => x.Id == trackId);
            track.AlbumID = albumId;
            track.Name = title;
            track.FilePath = path;

            SaveData();
        }
    }
}
