namespace MediaManager.Framework.Enumerators
{
    using System;
    using System.ComponentModel.Composition;

    [Flags]
    public enum MessageBoxOptions
    {
        Ok = 2,
        Cancel = 4,
        Yes = 8,
        No = 16,

        OkCancel = Ok | Cancel,
        YesNo = Yes | No,
        YesNoCancel = Yes | No | Cancel
    }

    public enum ConnectionStatus
    {
        Available = 1,
        Away = 2,
        Disconnected = 3,
        Offline = 4
    }

    public enum AddEditType
    {
        Genre = 1,
        Band = 2,
        Album = 3,
        Track = 4
    }

    public enum AddEditState
    {
        Add = 1,
        Edit = 2
    }

    [Flags]
    public enum WarningFeedback
    {
        Abort = 2,
        UseMetadata = 4,
        UseUserInput = 8,
        ApplyToAll = 16
    }

    [Flags]
    public enum WarningResponse
    {
        None = 1,
        Artist_Meta = 2,
        Artist_User = 4,
        Album_Meta = 8,
        Album_User = 16
    }

    public enum ImportType
    {
        All = 1,
        SingleAlbum = 2
    }

    public enum PropertyControlType
    {
        NoneSelected = 0,
        SelectOne = 1,
        Text = 2,
        Bool = 3,
        SelectMultiple = 4

    }

    public enum AddEditPropertyType
    {
        Track = 1,
        SearchCriteria = 2
    }

    public enum SearchDirection
    {
        exact = 1,
        more = 2,
        less = 3
    }
}
