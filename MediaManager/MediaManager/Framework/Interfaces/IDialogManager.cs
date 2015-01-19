namespace MediaManager.Framework.Interfaces
{
    using Caliburn.Micro;
    using System;
    using MediaManager.Framework.Enumerators;
    public interface IDialogManager
    {
        void ShowDialog(IScreen dialogModel);
        void ShowMessageBox(string message, string title = null, MessageBoxOptions options = MessageBoxOptions.Ok, Action<IMessageBox> callback = null);
    }
}
