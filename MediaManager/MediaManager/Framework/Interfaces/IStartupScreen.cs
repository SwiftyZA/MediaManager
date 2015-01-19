//This interface is just to differentiate between screens that should be activated at app start and all others
namespace MediaManager.Framework.Interfaces
{
    using Caliburn.Micro;
    public interface IStartupScreen : IGuardClose
    {
        string MenuTitle { get; }
        int Sequence { get; }
        string Icon { get; }
    }
}
