namespace MediaManager.GUI.ClientDetails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MediaManager.Data;
    using MediaManager.Framework.BaseClasses;
    public class ClientDetailsViewModel : BaseScreen
    {
        Guid _userId;
        String _message;
        String _username;

        public String Username
        {
            get { return _username; }
            set
            {
                _username = value;
                NotifyOfPropertyChange(() => Username);
            }
        }

        public String Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }
    }
}
