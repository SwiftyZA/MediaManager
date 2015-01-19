namespace MediaManager.Framework.Interfaces
{
    using Caliburn.Micro;
    public interface IShell : IConductor, IGuardClose
    {
        IDialogManager Dialogs { get; }
    }
}
