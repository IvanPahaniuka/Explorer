using FileTreeGrid.Models.Comparers.FileSystemItems;
using FileTreeGrids.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace FileTreeGrids.Models.FileSystemItems
{
    public partial class FileSystemItem : INotifyPropertyChanged
    {
        //Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler ChildsChanged;

        //Fields
        private bool isActive;
        private bool isHidden;
        private int level;
        private string fullPath;
        private string name;
        private bool isDirectory;
        private FileSystemWatcher watcher;
        private FileSystemInfo info;
        private List<FileSystemItem> childsList;

        //Properties
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                OnActiveChanged();
            }
        }
        public bool IsHidden
        {
            get => isHidden;
            set
            {
                isHidden = value;
                OnHiddenChanged();
            }
        }
        public int Level
        {
            get => level;
            set
            {
                level = value;
                OnPropertyChanged();
            }

        }
        public string FullPath
        {
            get => fullPath;
            private set
            {
                fullPath = value;
                OnPathChanged();
            }
        }
        public string Name
        {
            get => name;
            private set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public IReadOnlyCollection<FileSystemItem> Childs
        {
            get => ChildsList;
        }
        public bool IsDirectory
        {
            get => isDirectory;
            private set
            {
                isDirectory = value;
                OnPropertyChanged();
            }
        }

        protected FileSystemInfo Info
        {
            get => info;
            private set
            {
                if (value == null)
                    throw new ArgumentNullException();

                info = value;
                FullPath = info.FullName;
            }
        }

        private List<FileSystemItem> ChildsList
        {
            get => childsList;
            set
            {
                childsList = value;
                UpdateWatcherEvents();
            }
        }

        //Constructors
        public FileSystemItem(FileSystemInfo info)
        {
            watcher = new FileSystemWatcher();
            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Created;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.FileName
                                | NotifyFilters.DirectoryName;

            IsHidden = false;
            IsActive = false;
            Level = 0;

            Info = info;
        }

        //Methods
        protected internal virtual void OnInfoChanged() 
        {
            try
            {
                Info.Refresh();
            }
            catch
            {
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void OnPathChanged()
        {
            ClearChilds();
            IsActive = false;

            Name = Path.GetFileName(FullPath);
            if (string.IsNullOrWhiteSpace(Name))
                Name = FullPath;

            IsDirectory = PathExtensions.IsDirectory(FullPath);
            if (IsDirectory)
            {
                watcher.Path = FullPath;
                UpdateWatcherEvents();
            }
            
            

            OnPropertyChanged(nameof(FullPath));
        }
        private void OnHiddenChanged()
        {
            if (Childs != null)
                foreach (var child in Childs)
                    child.IsHidden = IsHidden || !IsActive;

            OnPropertyChanged(nameof(IsHidden));
        }
        private void OnActiveChanged()
        {
            if (IsDirectory && Childs == null)
            {
                LoadChilds();
            }

            if (Childs != null)
                foreach (var child in Childs)
                    child.IsHidden = IsHidden || !IsActive;

            OnPropertyChanged(nameof(IsActive));
        }
        private void LoadChilds()
        {
            try
            {
                ChildsList = new List<FileSystemItem>();
                var dirInfo = Info as DirectoryInfo;
                var files = dirInfo.GetFileSystemInfos();
                foreach (var file in files)
                {
                    try
                    {
                        var item = Activator.CreateInstance(GetType(), file) as FileSystemItem;
                        FixItemState(item, this);
                        ChildsList.Add(item);
                    }
                    catch
                    {
                    }
                }
                ChildsList.Sort(FileSystemItemsComparers.NameComparer);
            }
            catch
            {
                return;
            }

            ChildsChanged?.Invoke(this, 
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    ChildsList));
        }
        private void ClearChilds()
        {
            if (Childs != null)
            {
                ChildsChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset));
                ChildsList = null;
            }
        }
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                var item = ChildsList?.Find(i => i.FullPath == e.FullPath);
                item?.OnInfoChanged();
            });
        }
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => RemoveChild(e.FullPath));
        }
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => AddChild(e.FullPath));
        }
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                RemoveChild(e.OldFullPath);
                AddChild(e.FullPath);
            });
        }
        private void RemoveChild(string fullPath)
        {
            if (ChildsList != null)
            {
                var item = ChildsList.Find(i => i.FullPath == fullPath);
                if (item != null)
                {
                    ChildsList.Remove(item);

                    ChildsChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }
            }
        }
        private void AddChild(string fullPath)
        {
            if (ChildsList != null)
            {
                FileSystemItem item;
                try
                {
                    item = Create(GetType(), fullPath);
                }
                catch
                {
                    return;
                }
                FixItemState(item, this);
                ChildsList.Add(item);
                ChildsList.Sort(FileSystemItemsComparers.NameComparer);

                ChildsChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
        }
        private void UpdateWatcherEvents()
        {
            try
            {
                watcher.EnableRaisingEvents = IsDirectory && ChildsList != null;
            }
            catch
            {
                watcher.EnableRaisingEvents = false;
            }
        }


        ~FileSystemItem()
        {
            watcher.Dispose();
        }
    }
}
