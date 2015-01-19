namespace MediaManager.GUI.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MediaManager.Data;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Enumerators;
    using MediaManager.Framework.Interfaces;
    public class EditPropertyViewModel : BaseScreen
    {
        #region FIELDS & PROPERTIES
        PropertyControlType _controlType;
        string _propertyName;
        List<PropertyOption> _options;
        PropertyOption _selectedOption;
        string _textValue;
        bool _boolValue;
        List<PropertyOption> _list;
        List<PropertyOption> _selectedList;
        TrackProperty _tProperty;
        SearchCritria _sProperty;
        AddEditPropertyType _editType;
        Property _property;

        private string _value
        {
            get
            {
                switch (_editType)
                {
                    case AddEditPropertyType.Track:
                        return _tProperty.Value;
                    case AddEditPropertyType.SearchCriteria:
                        return _sProperty.Value;
                    default:
                        return "0";
                }
            }
        }

        public bool SaveEnabled
        {
            get
            {
                return _controlType != PropertyControlType.NoneSelected;
            }
        }
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

        public string PropertyName
        {
            get { return _propertyName; }
            set
            {
                _propertyName = value;
                NotifyOfPropertyChange(() => PropertyName);
            }
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
                NotifyOfPropertyChange(() => SaveEnabled);
            }
        }
        #endregion FIELDS & PROPERTIES

        public EditPropertyViewModel(AddEditPropertyType type)
        {
            _editType = type;
            ClearControl();
        }

        public void LoadProperty(int id)
        {
            switch (_editType)
            {
                case AddEditPropertyType.Track:
                    _tProperty = DAL.AdHocData.TrackProperties.FirstOrDefault(x => x.Id == id);
                    if (_tProperty != null)
                        _property = _tProperty.Property;
                    break;
                case AddEditPropertyType.SearchCriteria:
                    _sProperty = DAL.AdHocData.SearchCritrias.FirstOrDefault(x => x.Id == id);
                    if (_sProperty != null)
                        _property = _sProperty.Property;
                    break;
            }

            if (_property == null) return;

            PropertyName = _property.Name;

            switch (_property.PropertyType.Name)
            {
                case "Select One":
                    ControlType = PropertyControlType.SelectOne;
                    Options = DAL.AdHocData.PropertyOptions.Where(x => x.PropertyId == _property.Id).ToList();
                    SelectedOption = Options.FirstOrDefault(x => x.Value == _value);
                    break;
                case "Text Value":
                    ControlType = PropertyControlType.Text;
                    TextValue = _value;
                    break;
                case "Yes / No":
                    ControlType = PropertyControlType.Bool;
                    _boolValue = _value == "1";
                    NotifyOfPropertyChange(() => BoolValue);
                    break;
                case "Select Multiple":
                    ControlType = PropertyControlType.SelectMultiple;
                    List = DAL.AdHocData.PropertyOptions.Where(x => x.PropertyId == _property.Id).ToList();
                    break;
            }
        }

        public void Save()
        {
            switch (_editType)
            {
                case AddEditPropertyType.Track:
                    var tp = DAL.Data.TrackProperties.FirstOrDefault(x => x.Id == _tProperty.Id);
                    if (tp == null) return;
                    tp.Value = GetValue();
                    break;
                case AddEditPropertyType.SearchCriteria:
                    var sc = DAL.Data.SearchCritrias.FirstOrDefault(x => x.Id == _sProperty.Id);
                    if (sc == null) return;
                    sc.Value = GetValue();
                    break;
            }

            DAL.SaveData();

            ShowMessageBox("Save completed successfully", "", Framework.Enumerators.MessageBoxOptions.Ok);
        }

        public void Remove()
        {
            var tp = DAL.Data.TrackProperties.FirstOrDefault(x => x.Id == _property.Id);
            if (tp == null) return;

            DAL.Data.TrackProperties.Remove(tp);
            DAL.SaveData();
            ((IParentList)Parent).ReloadList();
            ShowMessageBox("Property successfully removed", "", Framework.Enumerators.MessageBoxOptions.Ok);
        }

        public string GetValue()
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

        public void ClearControl()
        {
            ControlType = PropertyControlType.NoneSelected;
            PropertyName = "No property selected";
        }
    }
}
