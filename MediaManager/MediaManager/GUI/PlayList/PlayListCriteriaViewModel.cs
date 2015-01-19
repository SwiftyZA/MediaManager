namespace MediaManager.GUI.PlayList
{
    using System.Linq;
    using MediaManager.Data;
    using System.Collections.Generic;
    using MediaManager.Data.Models;
    using MediaManager.Framework.BaseClasses;
    using System;
    using MediaManager.Framework.Interfaces;
    using MediaManager.GUI.RepoBrowser;
    using MediaManager.GUI.Common;
    using MediaManager.Framework.Enumerators;
    using System.Text;
    using MediaManager.GUI.StaticObjects;
    using System.Diagnostics;
    public class PlayListCriteriaViewModel : BaseScreen, IParentList
    {
        List<CheckBoxListItem> _artists;
        List<CheckBoxListItem> _genres;
        PlayList _playList;
        List<SearchCritria> _criteria;
        EditPropertyViewModel _property;
        SearchCritria _selectedCriteria;
        AddPropertyViewModel _addCriteria;

        public AddPropertyViewModel AddCriteria
        {
            get { return _addCriteria; }
            set { _addCriteria = value; NotifyOfPropertyChange(() => AddCriteria); }
        }

        public SearchCritria SelectedCriteria
        {
            get { return _selectedCriteria; }
            set
            {
                _selectedCriteria = value;
                NotifyOfPropertyChange(() => SelectedCriteria);
                if (_selectedCriteria != null)
                    Property.LoadProperty(_selectedCriteria.Id);
            }
        }

        public EditPropertyViewModel Property
        {
            get { return _property; }
            set { _property = value; NotifyOfPropertyChange(() => Property); }
        }

        public List<SearchCritria> Criteria
        {
            get { return _criteria; }
            set
            {
                _criteria = value;
                NotifyOfPropertyChange(() => Criteria);
            }
        }

        public bool PreferMusicVideos
        {
            get
            {
                if (_playList == null || _playList.SearchCritrias.Count == 0) return false;
                {
                    var MVid = _playList.SearchCritrias.FirstOrDefault(x => x.Property.Name == "Perfer Music Videos");
                    if (MVid != null)
                        return MVid.Value == "1";
                }
                return false;
            }
            set
            {
                if (_playList == null || _playList.SearchCritrias.Count == 0) return;
                var MVid = _playList.SearchCritrias.FirstOrDefault(x => x.Property.Name == "Perfer Music Videos");
                if (MVid != null)
                    MVid.Value = value ? "1" : "0";
            }
        }

        public bool IncludePropertyless
        {
            get
            {
                if (_playList == null || _playList.SearchCritrias.Count == 0) return false;
                {
                    var Propless = _playList.SearchCritrias.FirstOrDefault(x => x.Property.Name == "Include Propertyless Tracks");
                    if (Propless != null)
                        return Propless.Value == "1";
                }
                return false;
            }
            set
            {
                if (_playList == null || _playList.SearchCritrias.Count == 0) return;
                var Propless = _playList.SearchCritrias.FirstOrDefault(x => x.Property.Name == "Include Propertyless Tracks");
                if (Propless != null)
                    Propless.Value = value ? "1" : "0";
            }
        }
        public string Notes
        {
            get
            {
                if (_playList == null) return "";
                return _playList.Notes;
            }
            set
            {
                if (_playList != null)
                    _playList.Notes = value;
                NotifyOfPropertyChange(() => Notes);
            }
        }

        public string Name
        {
            get
            {
                if (_playList == null) return "";
                return _playList.Name;
            }
            set
            {
                if (_playList != null)
                    _playList.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public List<CheckBoxListItem> Genres
        {
            get { return _genres; }
            set { _genres = value; NotifyOfPropertyChange(() => Genres); }
        }

        public List<CheckBoxListItem> Artists
        {
            get { return _artists; }
            set { _artists = value; NotifyOfPropertyChange(() => Artists); }
        }

        public PlayListCriteriaViewModel()
        {
            ReloadCheckListBoxes();
            _property = new EditPropertyViewModel(AddEditPropertyType.SearchCriteria) { Parent = this };
        }

        void ReloadCheckListBoxes()
        {
            _genres = DAL.AdHocData.Genres.Select(x => new CheckBoxListItem()
            {
                Id = x.Id,
                Name = x.Name,
                Selected = false
            }).ToList();

            _artists = DAL.AdHocData.Bands.Select(x => new CheckBoxListItem()
            {
                Id = x.Id,
                Name = x.Name,
                Selected = false
            }).ToList();
        }

        public void LoadPlayList(int id)
        {
            ReloadCheckListBoxes();
            _playList = DAL.Data.PlayLists.FirstOrDefault(x => x.Id == id);
            if (_playList == null) return;

            foreach (var a in _playList.PlayListArtists)
            {
                var art = _artists.FirstOrDefault(x => x.Id == a.ArtistId);
                if (art != null) art.Selected = true;
            }

            foreach (var g in _playList.PlayListGenres)
            {
                var gen = _artists.FirstOrDefault(x => x.Id == g.GenreId);
                if (gen != null) gen.Selected = true;
            }

            NotifyOfPropertyChange(() => PreferMusicVideos);
            NotifyOfPropertyChange(() => Artists);
            NotifyOfPropertyChange(() => Genres);
            NotifyOfPropertyChange(() => Name);
            NotifyOfPropertyChange(() => Notes);

            ReloadList();
        }

        public void Save()
        {
            DAL.Data.PlayListArtists.RemoveRange(_playList.PlayListArtists);
            DAL.Data.PlayListGenres.RemoveRange(_playList.PlayListGenres);

            _playList.PlayListArtists = Artists.Where(x => x.Selected).Select(x => new PlayListArtist()
            {
                ArtistId = x.Id,
                PlayListId = _playList.Id
            }).ToList();

            _playList.PlayListGenres = Genres.Where(x => x.Selected).Select(x => new PlayListGenre()
            {
                GenreId = x.Id,
                PlayListId = _playList.Id
            }).ToList();

            DAL.Data.PlayLists.Attach(_playList);
            DAL.SaveData();
        }

        public void ReloadList()
        {
            if (_playList != null)
                Criteria = DAL.AdHocData.SearchCritrias.Where(x => x.PlayListId == _playList.Id).ToList();
        }

        public void Add()
        {
            _addCriteria = new AddPropertyViewModel(_playList.Id, AddEditPropertyType.SearchCriteria);
            if (_addCriteria.Properties.Count == 0)
            {
                ShowMessageBox("No more available criteria, please edit the existing ones", "Cannot add criteria", MessageBoxOptions.Ok);
                _addCriteria.TryClose();
            }
            else
            {
                _addCriteria.Deactivated += _addCriteria_Deactivated;
                ShowDialog(_addCriteria);
            }
        }

        void _addCriteria_Deactivated(object sender, Caliburn.Micro.DeactivationEventArgs e)
        {
            ReloadList();
        }


    }
}
