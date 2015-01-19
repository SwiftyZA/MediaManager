namespace MediaManager.GUI.Common
{
    using System.Linq;
    using System.Collections.Generic;
    using MediaManager.Data;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.GUI.StaticObjects;
    using MediaManager.Framework.Enumerators;
    using System;
    using System.Text;
    using System.ComponentModel.Composition;

    public class AddPropertyViewModel : BaseScreen
    {
        #region FIELDS & PROPERTIES
        List<Property> _properties;
        Property _selectedProperty;
        int _recordId;
        AddEditPropertyType _type;
        string _direction;
        PropertyControlType _controlType;
        List<PropertyOption> _options;
        PropertyOption _selectedOption;
        string _textValue;
        bool _boolValue;
        List<PropertyOption> _list;
        List<PropertyOption> _selectedList;

        public List<PropertyOption> SelectedList
        {
            get { return _selectedList; }
            set { _selectedList = value; NotifyOfPropertyChange(() => SelectedList); }
        }

        public List<PropertyOption> List
        {
            get { return _list; }
            set { _list = value; NotifyOfPropertyChange(() => List); }
        }

        public bool BoolValue
        {
            get { return _boolValue; }
            set { _boolValue = value; NotifyOfPropertyChange(() => BoolValue); }
        }

        public string TextValue
        {
            get { return _textValue; }
            set { _textValue = value; NotifyOfPropertyChange(() => TextValue); }
        }

        public PropertyOption SelectedOption
        {
            get { return _selectedOption; }
            set { _selectedOption = value; NotifyOfPropertyChange(() => SelectedOption); }
        }

        public List<PropertyOption> Options
        {
            get { return _options; }
            set { _options = value; NotifyOfPropertyChange(() => Options); }
        }

        public bool SelectOneVisibility
        {
            get
            {
                return _controlType == PropertyControlType.SelectOne;
            }
        }

        public bool TextValueVisibility
        {
            get
            {
                return _controlType == PropertyControlType.Text;
            }
        }

        public bool BoolVisibility
        {
            get
            {
                return _controlType == PropertyControlType.Bool;
            }
        }

        public bool SelectMultiVisibility
        {
            get
            {
                return _controlType == PropertyControlType.SelectMultiple;
            }
        }

        public PropertyControlType ControlType
        {
            get { return _controlType; }
            set
            {
                _controlType = value;
                NotifyOfPropertyChange(() => SelectOneVisibility);
                NotifyOfPropertyChange(() => TextValueVisibility);
                NotifyOfPropertyChange(() => BoolVisibility);
                NotifyOfPropertyChange(() => SelectMultiVisibility);
            }
        }

        public bool DirectionVisibility
        {
            get
            {
                return _type == AddEditPropertyType.SearchCriteria && _selectedProperty.PropertyType.DataType == "int";
            }
        }

        public List<string> Directions
        {
            get { return Enum.GetNames(typeof(SearchDirection)).ToList(); }
        }

        public string Direction
        {
            get { return _direction; }
            set { _direction = value; NotifyOfPropertyChange(() => Direction); }
        }


        public Property SelectedProperty
        {
            get { return _selectedProperty; }
            set
            {
                _selectedProperty = value;
                if (value == null) return;

                switch (_selectedProperty.PropertyType.Name)
                {
                    case "Select One":
                        ControlType = PropertyControlType.SelectOne;
                        Options = DAL.AdHocData.PropertyOptions.Where(x => x.PropertyId == _selectedProperty.Id).ToList();
                        SelectedOption = Options.FirstOrDefault();
                        break;
                    case "Text Value":
                        ControlType = PropertyControlType.Text;
                        TextValue = "";
                        break;
                    case "Yes / No":
                        ControlType = PropertyControlType.Bool;
                        _boolValue = true;
                        NotifyOfPropertyChange(() => BoolValue);
                        break;
                    case "Select Multiple":
                        ControlType = PropertyControlType.SelectMultiple;
                        List = DAL.AdHocData.PropertyOptions.Where(x => x.PropertyId == _selectedProperty.Id).ToList();
                        break;
                }

                NotifyOfPropertyChange(() => SelectedProperty);
                NotifyOfPropertyChange(() => DirectionVisibility);
            }
        }

        public List<Property> Properties
        {
            get { return _properties; }
            set { _properties = value; NotifyOfPropertyChange(() => Properties); }
        }
        #endregion FIELDS & PROPERTIES

        public AddPropertyViewModel(int id, AddEditPropertyType type)
        {
            _type = type;
            _recordId = id;
            LoadProperties();

            SelectedProperty = _properties.FirstOrDefault();
            _direction = Directions.FirstOrDefault();
        }

        public void LoadProperties()
        {
            switch (_type)
            {
                case AddEditPropertyType.Track:
                    var currentProps = (from p in DAL.Data.Properties
                                        join tp in DAL.Data.TrackProperties on p.Id equals tp.PropertyId
                                        where tp.TrackId == _recordId
                                        select p);

                    _properties = DAL.Data.Properties.Where(x => !x.SearchCriteriaOnly).Except(currentProps).ToList();
                    break;
                case AddEditPropertyType.SearchCriteria:
                    var currentCrit = (from p in DAL.Data.Properties
                                       join sc in DAL.Data.SearchCritrias on p.Id equals sc.PropertyId
                                       where sc.PlayListId == _recordId
                                       select p);

                    _properties = DAL.Data.Properties.Except(currentCrit).ToList();
                    break;
            }
        }

        public void Add()
        {
            switch (_type)
            {
                case AddEditPropertyType.Track:
                    var track = DAL.AdHocData.Tracks.FirstOrDefault(x => x.Id == _recordId);

                    DAL.Data.TrackProperties.Add(new TrackProperty()
                    {
                        PropertyId = _selectedProperty.Id,
                        TrackId = track.Id,
                        UserId = Global.Currentuser.Id,
                        Value = GetValue()
                    });
                    break;
                case AddEditPropertyType.SearchCriteria:
                    var playlist = DAL.AdHocData.PlayLists.FirstOrDefault(x => x.Id == _recordId);

                    DAL.Data.SearchCritrias.Add(new SearchCritria()
                    {
                        Direction = Direction,
                        PropertyId = _selectedProperty.Id,
                        PlayListId = playlist.Id,
                        Value = GetValue()
                    });
                    break;
            }

            DAL.SaveData();
            TryClose();
        }

        public void Close()
        {
            TryClose();
        }

        string GetValue()
        {
            switch (ControlType)
            {
                case PropertyControlType.SelectOne:
                    return _selectedOption.Value;
                case PropertyControlType.Text:
                    return TextValue;
                case PropertyControlType.Bool:
                    return Convert.ToInt16(BoolValue).ToString();//I did it like this in order to simplify the SQL SP
                case PropertyControlType.SelectMultiple:
                    var sb = new StringBuilder();
                    foreach (var i in _selectedList)
                        sb.Append(i.Value + ",");
                    return sb.ToString().Substring(0, sb.ToString().LastIndexOf(','));
                default:
                    return "1";
            }
        }
    }
}
