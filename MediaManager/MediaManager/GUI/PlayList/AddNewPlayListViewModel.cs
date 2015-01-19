namespace MediaManager.GUI.PlayList
{
    using MediaManager.Data;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Interfaces;
    using MediaManager.GUI.StaticObjects;
    public class AddNewPlayListViewModel : BaseScreen
    {
        string _name;
        string _notes;

        public string Notes
        {
            get { return _notes; }
            set { _notes = value; NotifyOfPropertyChange(() => Notes); }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyOfPropertyChange(() => Name); }
        }

        public void Add()
        {
            DAL.Data.PlayLists.Add(new PlayList() { Name = Name, Notes = Notes, UserId = Global.Currentuser.Id });
            DAL.SaveData();
            Close();
        }

        public void Close()
        {
            TryClose();
        }
    }
}
