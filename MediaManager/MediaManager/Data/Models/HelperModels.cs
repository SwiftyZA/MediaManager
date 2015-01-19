namespace MediaManager.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public class AddEditObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OriginalValue { get; set; }
    }

    public class CheckBoxListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }


}
