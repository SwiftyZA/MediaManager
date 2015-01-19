namespace MediaManager.GUI.Dashboard
{
    using Caliburn.Micro;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Interfaces;
    using MediaManager.GUI.ClientDetails;

    public class DashboardViewModel : BaseScreen, IHandle<DashboardViewModel>
    {
        readonly IEventAggregator _eventAggregator;
        ClientDetailsViewModel _clientDetails;

        public ClientDetailsViewModel ClientDetails
        {
            get
            {
                if (_clientDetails == null)
                    _clientDetails = new ClientDetailsViewModel();
                return _clientDetails;
            }
            set
            {
                _clientDetails = value;
                NotifyOfPropertyChange(() => ClientDetails);
            }
        }

        [ImportingConstructor]
        public DashboardViewModel(IDialogManager dialogs, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            Dialogs = dialogs;
        }

        public void Handle(DashboardViewModel message)
        {
            throw new NotImplementedException();
        }

        public string MenuTitle
        {
            get
            {
                return "Dashboard";
            }
        }

        public int Sequence
        {
            get
            {
                return 999;
            }
        }
    }
}
