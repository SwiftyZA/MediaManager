namespace MediaManager.GUI.WarningDialog
{
    using System.Collections.Generic;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Enumerators;

    public class WarningDialogViewModel : BaseScreen
    {
        string _message;
        public WarningFeedback Feedback;
        private bool _applyToAll;

        public bool ApplyToAll
        {
            get { return _applyToAll; }
            set { _applyToAll = value; NotifyOfPropertyChange(() => ApplyToAll); }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; NotifyOfPropertyChange(() => Message); }
        }
        public WarningDialogViewModel()
        {

        }

        public void Abort()
        {
            Feedback = WarningFeedback.Abort;
            TryClose();
        }

        public void UseUser()
        {
            Feedback = WarningFeedback.UseUserInput | (ApplyToAll ? WarningFeedback.ApplyToAll : 0);
            TryClose();
        }

        public void UseMeta()
        {
            Feedback = WarningFeedback.UseMetadata | (ApplyToAll ? WarningFeedback.ApplyToAll : 0);
            TryClose();
        }
    }
}
