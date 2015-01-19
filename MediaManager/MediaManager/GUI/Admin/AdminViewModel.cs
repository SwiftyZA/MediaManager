namespace MediaManager.GUI.Admin
{
    using System.Linq;
    using Caliburn.Micro;
    using MediaManager.Data;
    using System.ComponentModel.Composition;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Interfaces;
    using System.Collections.Generic;
    using System.Data.Objects;

    [Export(typeof(IStartupScreen))]
    public class AdminViewModel : BaseScreen, IStartupScreen
    {
        readonly IEventAggregator _eventAggregator;
        List<SystemParameter> _systemParameters;

        public List<SystemParameter> SystemParameters
        {
            get { return _systemParameters; }
            set { _systemParameters = value; NotifyOfPropertyChange(() => SystemParameters); }
        }


        [ImportingConstructor]
        public AdminViewModel(IDialogManager dialogs, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            Dialogs = dialogs;


            _systemParameters = DAL.Data.SystemParameters.ToList();

        }

        public void SaveParams()
        {
            DAL.SaveData();
        }

        public string MenuTitle
        {
            get { return "Admin"; }
        }

        public int Sequence
        {
            get
            {
                return 998;
            }
        }

        public string Icon
        { get { return "..\\Images\\Settings.png"; } }
    }
}
