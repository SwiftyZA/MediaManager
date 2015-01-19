namespace MediaManager.Framework.BaseClasses
{
    using Caliburn.Micro;
    using System;
    using MediaManager.Framework.Enumerators;
    using MediaManager.Framework.Interfaces;
    public abstract class BaseScreen : Screen
    {
        IDialogManager dialogs;

        public IDialogManager Dialogs
        {
            get { return dialogs; }
            protected set { dialogs = value; }
        }

        public void ShowDialog(IScreen screen)
        {
            if (Dialogs != null)
                Dialogs.ShowDialog(screen);
            else if (Parent != null)
                ((BaseScreen)Parent).ShowDialog(screen);
            else
            {
                throw new Exception("Screen has no parent or dialog interface, you missed something neil");
            }
        }

        public void ShowMessageBox(string message, string title, MessageBoxOptions options)
        {
            if (Dialogs != null)
                Dialogs.ShowMessageBox(message, title, options);
            else if (Parent != null)
                ((BaseScreen)Parent).ShowMessageBox(message, title, options);
            else
            {
                throw new Exception("Screen has no parent or dialog interface, you missed something neil");
            }
        }

        protected IConductor Conductor
        {
            get { return (IConductor)Parent; }
        }

        public void Show()
        {
            Conductor.ActivateItem(this);
        }

        bool isDirty;

        public bool IsDirty
        {
            get { return isDirty; }
            set
            {
                isDirty = value;
                NotifyOfPropertyChange(() => IsDirty);
            }
        }

        protected virtual void DoCloseCheck(IDialogManager dialogs, Action<bool> callback)
        {
            dialogs.ShowMessageBox(
                "You have unsaved data. Are you sure you want to close this document? All changes will be lost.",
                "Unsaved Data",
                MessageBoxOptions.YesNo,
                box => callback(box.WasSelected(MessageBoxOptions.Yes))
                );
        }

    }
}
