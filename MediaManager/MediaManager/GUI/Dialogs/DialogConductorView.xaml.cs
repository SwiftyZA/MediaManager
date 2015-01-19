namespace MediaManager.GUI.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for DialogConductorView.xaml
    /// </summary>
    public partial class DialogConductorView : UserControl
    {
        bool disabled;

        public DialogConductorView()
        {
            InitializeComponent();
            ActiveItem.ContentChanged += OnTransitionCompleted;
            Loaded += OnLoad;
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            if (disabled) DisableBackground();
        }

        void OnTransitionCompleted(object sender, EventArgs e)
        {
            if (ActiveItem.Content == null)
                EnableBackground();
            else
            {
                DisableBackground();

                var control = ActiveItem.Content as Control;
                if (control != null)
                    control.Focus();
            }
        }

        public void EnableBackground()
        {
            disabled = false;
            ChangeEnabledState(GetBackground(), true);
        }

        public void DisableBackground()
        {
            disabled = true;
            ChangeEnabledState(GetBackground(), false);
        }

        List<UIElement> GetBackground()
        {
            List<UIElement> children = new List<UIElement>();
            var contentControl = (ContentControl)Parent;
            var container = (Panel)contentControl.Parent;
            foreach (var child in container.Children)
            {
                if (child != contentControl)
                    children.Add((UIElement)child);
            }
            return children;
            //container.Children.Where(child => child != contentControl);
        }

        void ChangeEnabledState(IEnumerable<UIElement> background, bool state)
        {
            foreach (var uiElement in background)
            {
                var control = uiElement as Control;
                if (control != null)
                    control.IsEnabled = state;
                else
                {
                    var panel = uiElement as Panel;
                    if (panel != null)
                    {
                        foreach (var child in panel.Children)
                        {
                            ChangeEnabledState(new[] { (UIElement)child }, state);
                        }
                    }
                }
            }
        }
    }
}
