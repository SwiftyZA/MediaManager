namespace MediaManager.GUI.Import
{
    using System.Linq;
    using Caliburn.Micro;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using MediaManager.Data;
    using MediaManager.Framework.BaseClasses;
    using MediaManager.Framework.Interfaces;
    using System;
    using MediaManager.GUI.AddEditDialog;
    using MediaManager.Framework.Enumerators;
    using System.IO;
    using System.Text;
    using MediaManager.GUI.StaticObjects;
    using TagLib;
    using MediaManager.GUI.WarningDialog;
    using System.Threading;
    using System.Threading.Tasks;

    [Export(typeof(IStartupScreen))]
    public class ImportViewModel : BaseScreen, IStartupScreen, IHandle<String>
    {
        readonly IEventAggregator _eventAggregator;
        List<Band> _bands;
        Band _selectedBand;
        List<Album> _albums;
        Album _selectedAlbum;
        String _directory;
        StringBuilder _info;
        List<FileInfo> _files;
        WarningResponse _warningResponse;
        ImportType _importType;
        bool _moveFiles;
        bool _abort;
        private static EventWaitHandle _stopper;
        private WarningDialogViewModel _warning;

        public bool MoveFiles
        {
            get { return _moveFiles; }
            set { _moveFiles = value; NotifyOfPropertyChange(() => MoveFiles); }
        }

        public bool SingleChecked
        {
            get
            {
                if (_importType == ImportType.SingleAlbum) return true;
                return false;
            }
            set
            {
                if (value)
                    Type = ImportType.SingleAlbum;
            }
        }

        public bool AllChecked
        {
            get
            {
                if (_importType == ImportType.All) return true;
                return false;
            }
            set
            {
                if (value)
                    Type = ImportType.All;
            }
        }

        public ImportType Type
        {
            get { return _importType; }
            set
            {
                _importType = value;
                NotifyOfPropertyChange(() => AllChecked);
                NotifyOfPropertyChange(() => SingleChecked);
                GetFilesInDir();
            }
        }

        public List<FileInfo> Files
        {
            get
            {
                if (_files == null)
                    _files = new List<FileInfo>();
                return _files;
            }
            set { _files = value; }
        }

        public string Info
        {
            get
            {
                return _info.ToString();
            }
            set
            {
                _info.AppendLine(string.Format("{0} {1}", DateTime.Now.ToString("[yyyy:MM:dd HH:mm:ss]"), value));
                NotifyOfPropertyChange(() => Info);
                _eventAggregator.PublishOnUIThread("Info");
            }
        }

        public String Directory
        {
            get
            {
                if (_directory == null) _directory = string.Empty;
                return _directory;
            }
            set { _directory = value; NotifyOfPropertyChange(() => Directory); }
        }

        public Album SelectedAlbum
        {
            get { return _selectedAlbum; }
            set { _selectedAlbum = value; NotifyOfPropertyChange(() => SelectedAlbum); }
        }

        public List<Album> Albums
        {
            get { return _albums; }
            set { _albums = value; NotifyOfPropertyChange(() => Albums); }
        }

        public Band SelectedBand
        {
            get { return _selectedBand; }
            set
            {
                _selectedBand = value;
                NotifyOfPropertyChange(() => SelectedBand);
                LoadAlbums();
            }
        }

        public List<Band> Bands
        {
            get { return _bands; }
            set
            {
                _bands = value;
                NotifyOfPropertyChange(() => Bands);
            }
        }

        [ImportingConstructor]
        public ImportViewModel(IDialogManager dialogs, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            LoadBands();

            Type = ImportType.SingleAlbum;

            Dialogs = dialogs;
            if (_info == null)
                _info = new StringBuilder();

            _abort = false;
            _stopper = new EventWaitHandle(false, EventResetMode.AutoReset);
            _warning = new WarningDialogViewModel();

#if DEBUG
            Directory = "E:\\Personal\\Music";
            //Browse();
#endif
        }

        public void Browse()
        {
            var fileBrowser = new System.Windows.Forms.FolderBrowserDialog();
            fileBrowser.SelectedPath = Directory;
            var result = fileBrowser.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel) return;

            Directory = fileBrowser.SelectedPath;

            if (!System.IO.Directory.Exists(Directory)) return;

            GetFilesInDir();
        }

        private void GetFilesInDir()
        {
            Files = new List<FileInfo>();

            if (Directory == String.Empty) return;

            var paths = System.IO.Directory.EnumerateFiles(Directory, "*.mp3", (Type == ImportType.All ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            foreach (var p in paths)
                Files.Add(new FileInfo(p));

            //TODO: Replace with regex
            Files = Files.Where(x => x.Extension.ToLower() == ".mp3").ToList(); //only mp3s for now, got another plans for music videos
            //|| x.Extension == ".avi"
            //|| x.Extension == ".mp4"
            //|| x.Extension == ".mpg"
            //|| x.Extension == ".mpeg").ToList();

            Info = string.Format("{0} media file(s) have been located at the selected directory", Files.Count);
        }

        public void AddBand()
        {
            var addEdit = new AddEditDialogViewModel(AddEditType.Band);
            addEdit.Deactivated += delegate { LoadBands(); };
            Dialogs.ShowDialog(addEdit);
        }

        public void AddAlbum()
        {
            var addEdit = new AddEditDialogViewModel(AddEditType.Album, SelectedBand.Id);
            addEdit.Deactivated += delegate { LoadAlbums(); };
            Dialogs.ShowDialog(addEdit);
        }

        public void LoadBands()
        {
            Bands = DAL.Data.Bands.ToList();
            SelectedBand = Bands.FirstOrDefault();
            LoadAlbums();
        }

        public void LoadAlbums()
        {
            if (SelectedBand != null)
                Albums = DAL.Data.Albums.Where(x => x.BandID == SelectedBand.Id).ToList();

            if (Albums.Count > 0)
                SelectedAlbum = Albums.FirstOrDefault();
        }

        public void Import()
        {
            Task.Run(async () =>
            {
                switch (Type)
                {
                    case ImportType.SingleAlbum:
                        ImportSingleAlbum();
                        break;
                    case ImportType.All:
                        ImportAll();
                        break;
                }
            });
        }

        private void ImportAll()
        {
            var repoDir = Global.AppParameters.FirstOrDefault(x => x.Name == "MusicRepo").Value;
            int count = 0;
            foreach (var f in Files)
            {
                if (_abort)
                {
                    Info = "Import Aborted";
                    _abort = false;
                    return;
                }

                count++;
                var tags = TagLib.File.Create(f.FullName);

                var band = tags.Tag.FirstAlbumArtist != null ? tags.Tag.FirstAlbumArtist : (tags.Tag.FirstArtist != null ? tags.Tag.FirstArtist : "Unknown");
                var album = tags.Tag.Album != null ? tags.Tag.Album : "Unknown";
                var genre = tags.Tag.FirstGenre != null ? tags.Tag.FirstGenre : "Unknown";
                var title = tags.Tag.Title != null ? tags.Tag.Title : f.Name;
                var duration = tags.Properties.Duration != null ? tags.Properties.Duration.Seconds : 0;
                var trackNr = tags.Tag.Track != null ? tags.Tag.Track : 0;

                var copyToDir = string.Format("{0}\\{1}\\{2}", repoDir, band, album);
                if (!System.IO.Directory.Exists(copyToDir))
                    System.IO.Directory.CreateDirectory(copyToDir);
                var fullDir = string.Format("{0}\\{1}", copyToDir, f.Name);

                if (!System.IO.File.Exists(fullDir))
                {
                    if (MoveFiles)
                        f.MoveTo(fullDir);
                    else
                        f.CopyTo(fullDir);


                    Info = string.Format("Imported [{3} of {4}] {0} - {1} - {2}", band, album, title, count, Files.Count);
                }
                else
                    Info = string.Format("Skipped [{3} of {4}] {0} - {1} - {2} -> Track already imported", band, album, title, count, Files.Count);
                AddTrackToDB(band, album, genre, title, fullDir, duration, (int)trackNr);
            }

            LoadBands();
            Info = "Import complete";
        }

        private void ImportSingleAlbum()
        {
            var repoDir = Global.AppParameters.FirstOrDefault(x => x.Name == "MusicRepo").Value;
            _warningResponse = WarningResponse.None;
            foreach (var f in Files)
            {
                var band = _selectedBand.Name;
                var album = _selectedAlbum.Name;
                var genre = _selectedAlbum.Genre.Name;
                var tags = TagLib.File.Create(f.FullName);
                var duration = tags.Properties.Duration != null ? tags.Properties.Duration.Seconds : 0;
                var trackNr = tags.Tag.Track != null ? tags.Tag.Track : 0;

                if (tags.Tag.Album != _selectedAlbum.Name)
                    album = HandleAlbumMismatch(_selectedAlbum.Name, tags.Tag.Album, f.Name);
                if (tags.Tag.FirstAlbumArtist != band)
                    band = HandleBandMismatch(_selectedBand.Name,
                        tags.Tag.FirstAlbumArtist != null ? tags.Tag.FirstAlbumArtist : (tags.Tag.FirstArtist != null ? tags.Tag.FirstArtist : "Unknown"),
                        f.Name);

                var copyToDir = string.Format("{0}\\{1}\\{2}", repoDir, band, album);
                if (!System.IO.Directory.Exists(copyToDir))
                    System.IO.Directory.CreateDirectory(copyToDir);
                var fullDir = string.Format("{0}\\{1}", copyToDir, f.Name);

                if (!System.IO.File.Exists(fullDir))
                {
                    if (MoveFiles)
                        f.MoveTo(fullDir);
                    else
                        f.CopyTo(fullDir);

                    Info = string.Format("Imported {0} - {1} - {2}", band, album, f.Name);
                }
                else
                    Info = string.Format("Skipped {0} - {1} - {2} -> Track already imported", band, album, f.Name);

                AddTrackToDB(band, album, genre, f.Name, fullDir, duration, (int)trackNr);
            }

            LoadBands();
            Info = "Import complete";
        }

        private string HandleAlbumMismatch(string userInput, string metaData, string fileName)
        {
            string albumTitle = userInput;
            if (_warningResponse.HasFlag(WarningResponse.Album_Meta))
            {
                albumTitle = metaData;
            }
            else if (!_warningResponse.HasFlag(WarningResponse.Album_User))
            {


                _warning.Message = string.Format("The selected album ({0}) does not match the album name in the metadata ({1}) for the following track:\n{2}", userInput, metaData, fileName);
                _warning.Deactivated += warning_Deactivated;
                _eventAggregator.PublishOnUIThread("warning");
                _stopper.WaitOne();

                switch (_warning.Feedback)
                {
                    case WarningFeedback.Abort:
                        //TODO: Figure out a way (an efficient one) to roleback stuff already copied/moved
                        return "-1";
                    case WarningFeedback.UseMetadata:
                        albumTitle = metaData;
                        break;
                    case WarningFeedback.UseMetadata | WarningFeedback.ApplyToAll:
                        albumTitle = metaData;
                        if (_warningResponse == WarningResponse.None)
                            _warningResponse = WarningResponse.Album_Meta;
                        else
                            _warningResponse |= WarningResponse.Album_Meta;
                        break;
                    case WarningFeedback.UseUserInput:
                        albumTitle = userInput;
                        break;
                    case WarningFeedback.UseUserInput | WarningFeedback.ApplyToAll:
                        albumTitle = userInput;
                        if (_warningResponse == WarningResponse.None)
                            _warningResponse = WarningResponse.Album_User;
                        else
                            _warningResponse |= WarningResponse.Album_User;
                        break;
                }
            }

            return albumTitle;
        }

        private string HandleBandMismatch(string userInput, string metaData, string fileName)
        {
            string band = userInput;
            if (_warningResponse.HasFlag(WarningResponse.Artist_Meta))
            {
                band = metaData;
            }
            else if (!_warningResponse.HasFlag(WarningResponse.Artist_User))
            {
                _warning.Message = string.Format("The selected artist ({0}) does not match the artist name in the metadata ({1}) for the following track:\n{2}", userInput, metaData, fileName);
                _warning.Deactivated += warning_Deactivated;
                _eventAggregator.PublishOnUIThread("warning");
                _stopper.WaitOne();

                switch (_warning.Feedback)
                {
                    case WarningFeedback.Abort:
                        //TODO: Figure out a way (an efficient one) to roleback stuff already copied/moved
                        return "-1";
                    case WarningFeedback.UseMetadata:
                        band = metaData;
                        break;
                    case WarningFeedback.UseMetadata | WarningFeedback.ApplyToAll:
                        band = metaData;
                        if (_warningResponse == WarningResponse.None)
                            _warningResponse = WarningResponse.Artist_Meta;
                        else
                            _warningResponse |= WarningResponse.Artist_Meta;
                        break;
                    case WarningFeedback.UseUserInput:
                        band = userInput;
                        break;
                    case WarningFeedback.UseUserInput | WarningFeedback.ApplyToAll:
                        band = userInput;
                        if (_warningResponse == WarningResponse.None)
                            _warningResponse = WarningResponse.Artist_User;
                        else
                            _warningResponse |= WarningResponse.Artist_User;
                        break;
                }
            }

            return band;
        }

        void warning_Deactivated(object sender, DeactivationEventArgs e)
        {
            _stopper.Set();
        }

        public void AddTrackToDB(string band, string album, string genre, string track, string filePath, int duration, int trackNr)
        {
            var _band = DAL.GetBand(band, true);
            var _genre = DAL.GetGenre(genre, true);
            var _album = DAL.GetAlbum(album, true, _band.Id, _genre.Id);
            DAL.AddTrack(track, _album.Id, filePath, duration, trackNr);
        }

        private void ScrollInfoBoxDown()
        {
            //I hate having to break convention, so to maintain my own santiy, I'll close my eyes while writing this method :(

            //var view = (ImportView)FetVCiew();
            //view.indo.scrollroend();

            //On second thoughts, lets rather look...
            var view = (ImportView)GetView();
            if (view != null)
                view.Info.ScrollToEnd();
        }

        public void Handle(string message)
        {
            switch (message)
            {
                case "Info":
                    ScrollInfoBoxDown();
                    break;
                case "warning":
                    var winMan = new WindowManager();
                    winMan.ShowDialog(_warning);
                    break;
            }
        }

        public void Abort()
        {
            _abort = true;
        }

        public string MenuTitle
        {
            get { return "Import"; }
        }

        public int Sequence
        {
            get
            {
                return 100;
            }
        }

        public string Icon
        { get { return "..\\Images\\Import.png"; } }
    }


}
