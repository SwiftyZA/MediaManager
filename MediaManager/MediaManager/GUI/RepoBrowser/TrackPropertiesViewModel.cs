namespace MediaManager.GUI.RepoBrowser
{
    using System.Linq;
    using System.Collections.Generic;
    using MediaManager.Data;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Interfaces;
    using Caliburn.Micro;
    using MediaManager.GUI.Common;
    using MediaManager.Framework.Enumerators;
    public class TrackPropertiesViewModel : BaseScreen, IParentList
    {
        #region FIELDS & PROPERTIES
        int _trackId;
        List<TrackProperty> _properties;
        TrackProperty _selectedProperty;
        EditPropertyViewModel _editTrackPropertyViewModel;
        AddPropertyViewModel _addTrackProperty;
        EditID3TagsViewModel _iD3Tags;
        MiscellaneousTrackFunctionsViewModel _miscellaneous;

        public MiscellaneousTrackFunctionsViewModel Miscellaneous
        {
            get { return _miscellaneous; }
            set { _miscellaneous = value; NotifyOfPropertyChange(() => Miscellaneous); }
        }


        public EditID3TagsViewModel ID3Tags
        {
            get { return _iD3Tags; }
            set { _iD3Tags = value; NotifyOfPropertyChange(() => ID3Tags); }
        }

        public EditPropertyViewModel Property
        {
            get { return _editTrackPropertyViewModel; }
            set { _editTrackPropertyViewModel = value; NotifyOfPropertyChange(() => Property); }
        }

        public TrackProperty SelectedProperty
        {
            get { return _selectedProperty; }
            set
            {
                _selectedProperty = value;
                NotifyOfPropertyChange(() => SelectedProperty);
                if (_selectedProperty != null)
                    Property.LoadProperty(SelectedProperty.Id);
                else
                    Property.ClearControl();


            }
        }

        public List<TrackProperty> Properties
        {
            get { return _properties; }
            set { _properties = value; NotifyOfPropertyChange(() => Properties); }
        }

        public int TrackId
        {
            get { return _trackId; }
            set
            {
                _trackId = value;
                NotifyOfPropertyChange(() => TrackId);
                ReloadList();
                ID3Tags.LoadID3Tags(TrackId);
                Miscellaneous.LoadMiscFunctions(TrackId);
            }
        }
        #endregion FIELDS & PROPERTIES
        public TrackPropertiesViewModel()
        {
            _iD3Tags = new EditID3TagsViewModel() { Parent = this };
            _editTrackPropertyViewModel = new EditPropertyViewModel(AddEditPropertyType.Track) { ControlType = PropertyControlType.NoneSelected, Parent = this };
            _miscellaneous = new MiscellaneousTrackFunctionsViewModel() { Parent = this };
        }

        public void Add(int trackId)
        {
            _addTrackProperty = new AddPropertyViewModel(trackId, AddEditPropertyType.Track);
            if (_addTrackProperty.Properties.Count == 0)
            {
                ShowMessageBox("No more available properties, please edit the existing ones", "Cannot add property", MessageBoxOptions.Ok);
                _addTrackProperty.TryClose();
            }
            else
            {
                _addTrackProperty.Deactivated += _addTrackProperty_Deactivated;
                ShowDialog(_addTrackProperty);
            }
        }

        void _addTrackProperty_Deactivated(object sender, Caliburn.Micro.DeactivationEventArgs e)
        {
            ReloadList();
        }

        public bool CanAdd(int trackId)
        {
            return trackId > 0;
        }

        public void ReloadList()
        {
            Properties = DAL.AdHocData.TrackProperties.Where(x => x.TrackId == _trackId).ToList();
        }
    }
}
