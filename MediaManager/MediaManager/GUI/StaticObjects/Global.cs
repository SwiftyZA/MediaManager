namespace MediaManager.GUI.StaticObjects
{
    using System.Linq;
    using System.Collections.Generic;
    using MediaManager.Data;

    public static class Global
    {
        private static List<SystemParameter> _appParameters;
        private static User _currentuser;

        public static User Currentuser
        {
            get
            {
                //TODO: Add user selection on app startup
                if (_currentuser == null)
                    _currentuser = DAL.AdHocData.Users.FirstOrDefault();
                return Global._currentuser;
            }
            set { Global._currentuser = value; }
        }

        public static List<SystemParameter> AppParameters
        {
            get
            {
                if (Global._appParameters == null || Global._appParameters.Count == 0)
                    _appParameters = DAL.Data.SystemParameters.ToList();
                return Global._appParameters;
            }
        }

    }
}
