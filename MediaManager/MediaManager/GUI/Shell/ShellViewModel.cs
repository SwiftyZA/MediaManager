namespace MediaManager.GUI.Shell
{
    using System.Linq;
    using Caliburn.Micro;
    using System.ComponentModel.Composition;
    using MediaManager.Framework.Interfaces;
    using System.Collections.Generic;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Enumerators;

    [Export(typeof(IShell))]
    public class ShellViewModel : Conductor<IStartupScreen>.Collection.OneActive, IShell
    {
        readonly IDialogManager dialogs;

        public IDialogManager Dialogs
        {
            get { return dialogs; }
        }

        [ImportingConstructor]
        public ShellViewModel(IDialogManager dialogs, [ImportMany]IEnumerable<IStartupScreen> workspaces)
        {
            this.dialogs = dialogs;
            Items.AddRange(workspaces.OrderBy(x => x.Sequence).ToList());

            ActivateItem(Items.FirstOrDefault());
        }
    }
}
