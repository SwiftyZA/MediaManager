namespace MediaManager.GUI.AddEditDialog
{
    using System.Linq;
    using System.Collections.Generic;
    using MediaManager.Data;
    using MediaManager.Data.Models;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Enumerators;

    public class AddEditDialogViewModel : BaseScreen
    {
        AddEditState _state;
        int _parentRecordId;
        AddEditType _type;
        List<AddEditObject> _items;
        AddEditObject _selectedItem;
        string _name;
        List<Genre> _genres;
        Genre _selectedGenre;

        public Genre SelectedGenre
        {
            get
            {
                if (_selectedGenre == null && DisplayGenre)
                    _selectedGenre = Genres.FirstOrDefault();
                return _selectedGenre;
            }
            set { _selectedGenre = value; NotifyOfPropertyChange(() => SelectedGenre); }
        }

        public List<Genre> Genres
        {
            get { return _genres; }
            set { _genres = value; }
        }

        public AddEditState State
        {
            get { return _state; }
            set
            {
                _state = value;
                NotifyOfPropertyChange(() => State);
                NotifyOfPropertyChange(() => AddEditText);
                NotifyOfPropertyChange(() => AddChecked);
                NotifyOfPropertyChange(() => EditChecked);
                NotifyOfPropertyChange(() => Name);
            }
        }

        public bool AddChecked
        {
            get
            {
                return _state == AddEditState.Add;
            }
            set
            {
                if (value)
                {
                    State = AddEditState.Add;
                    Name = string.Empty;
                }
            }
        }

        public bool EditChecked
        {
            get
            {
                return _state == AddEditState.Edit;
            }
            set
            {
                if (value)
                    State = AddEditState.Edit;
            }
        }

        public bool DisplayGenre
        {
            get { return _type == AddEditType.Album; }
        }

        public bool HasChanges
        {
            get
            {
                foreach (var o in _items)
                    if (o.Name != o.OriginalValue)
                        return true;
                return false;
            }
        }

        public AddEditObject SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                if (_selectedItem != null)
                {
                    State = AddEditState.Edit;
                    Name = _selectedItem.Name;
                    if (DisplayGenre)
                        SelectedGenre = Genres.FirstOrDefault(x => x.Id == DAL.Data.Albums.FirstOrDefault(y => y.Id == SelectedItem.Id).GenreID);
                }
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyOfPropertyChange(() => Name); }
        }

        public string AddEditText
        {
            get { return State.ToString(); }
        }

        public List<AddEditObject> Items
        {
            get { return _items; }
            set { _items = value; NotifyOfPropertyChange(() => Items); }
        }

        public AddEditDialogViewModel(AddEditType type, int parentRecId = 0)
        {
            _state = AddEditState.Add;
            _parentRecordId = parentRecId;
            _type = type;
            LoadObjects();
        }

        void LoadObjects()
        {
            switch (_type)
            {
                case AddEditType.Album:
                    Items = DAL.Data.Albums.Where(x => x.BandID == _parentRecordId).Select(x => new AddEditObject()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        OriginalValue = x.Name
                    }).ToList();

                    Genres = DAL.Data.Genres.ToList();

                    break;
                case AddEditType.Band:
                    Items = DAL.Data.Bands.Select(x => new AddEditObject()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        OriginalValue = x.Name
                    }).ToList();
                    break;
                case AddEditType.Genre:
                    Items = DAL.Data.Genres.Select(x => new AddEditObject()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        OriginalValue = x.Name
                    }).ToList();
                    break;
            }
        }

        public void Add()
        {
            switch (_type)
            {
                case AddEditType.Album:
                    if (_state == AddEditState.Add)
                        DAL.Data.Albums.Add(new Album() { Name = Name, BandID = _parentRecordId, GenreID = SelectedGenre.Id });
                    else
                    {
                        var item = DAL.Data.Albums.FirstOrDefault(x => x.Id == _selectedItem.Id);
                        if (item != null)
                        {
                            item.Name = Name;
                            item.GenreID = _selectedGenre.Id;
                        }
                    }
                    break;
                case AddEditType.Band:
                    if (_state == AddEditState.Add)
                        DAL.Data.Bands.Add(new Band() { Name = Name });
                    else
                    {
                        var item = DAL.Data.Bands.FirstOrDefault(x => x.Id == _selectedItem.Id);
                        if (item != null)
                            item.Name = Name;
                    }
                    break;
                case AddEditType.Genre:
                    if (_state == AddEditState.Add)
                        DAL.Data.Genres.Add(new Genre() { Name = Name });
                    else
                    {
                        var item = DAL.Data.Genres.FirstOrDefault(x => x.Id == _selectedItem.Id);
                        if (item != null)
                            item.Name = Name;
                    }
                    break;
            }

            if (DAL.SaveData())
            {
                Name = string.Empty;
                LoadObjects();
            }
        }

        public void AddClicked()
        {
            SelectedItem = null;
            State = AddEditState.Add;
        }

        public void Delete(AddEditObject obj)
        {
            switch (_type)
            {
                case AddEditType.Album:
                    //TODO: Check tracks
                    if (true)
                    {
                        var album = DAL.Data.Albums.FirstOrDefault(x => x.Id == obj.Id);
                        if (album != null)
                            DAL.Data.Albums.Remove(album);
                    }
                    break;
                case AddEditType.Band:
                    var band = DAL.Data.Bands.FirstOrDefault(x => x.Id == obj.Id);
                    if (band != null && band.Albums.Count == 0)
                        DAL.Data.Bands.Remove(band);
                    else
                        System.Windows.Forms.MessageBox.Show(string.Format("Band cannot be removed, they have {0} album(s) loaded.\nPlease remove the album(s) before trying to remove the band", band.Albums.Count)
                            , "Failed to remove band", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    break;
                case AddEditType.Genre:
                    var genre = DAL.Data.Genres.FirstOrDefault(x => x.Id == obj.Id);
                    if (genre != null && genre.Albums.Count == 0)
                        DAL.Data.Genres.Remove(genre);
                    else
                        System.Windows.Forms.MessageBox.Show(string.Format("Genre cannot be removed, it has {0} album(s) loaded.\nPlease remove the album(s) or change its genre before trying to remove the genre", genre.Albums.Count)
                            , "Failed to remove genre", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    break;
            }
            DAL.SaveData();
            LoadObjects();
        }

        public void Save()
        {
            foreach (var o in _items)
                if (o.Name != o.OriginalValue)
                {
                    switch (_type)
                    {
                        case AddEditType.Album:
                            var a = DAL.Data.Albums.FirstOrDefault(x => x.Id == o.Id);
                            a.Name = o.Name;
                            break;
                        case AddEditType.Band:
                            var b = DAL.Data.Bands.FirstOrDefault(x => x.Id == o.Id);
                            b.Name = o.Name;
                            break;
                        case AddEditType.Genre:
                            var g = DAL.Data.Genres.FirstOrDefault(x => x.Id == o.Id);
                            g.Name = o.Name;
                            break;
                    }
                    DAL.SaveData();
                }

            TryClose();
        }

        public void Close()
        {
            if (HasChanges)
            {
                var result = System.Windows.Forms.MessageBox.Show("You have some unsaved changes, do you want to discard them and close the dialog?"
                    , "Discard Changes?", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning);
                if (result == System.Windows.Forms.DialogResult.OK)
                    TryClose();
            }
            else
                TryClose();
        }
    }
}
