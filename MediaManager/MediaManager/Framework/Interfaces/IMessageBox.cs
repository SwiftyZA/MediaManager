namespace MediaManager.Framework.Interfaces
{
    using Caliburn.Micro;
    using MediaManager.Framework.Enumerators;

    public interface IMessageBox : IScreen
    {
        string Message { get; set; }
        MessageBoxOptions Options { get; set; }

        void Ok();
        void Cancel();
        void Yes();
        void No();

        bool WasSelected(MessageBoxOptions option);
    }
}
