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
        private string fullPath;
        private bool isDirectory;
        private FileSystemWatcher watcher;
        private FileSystemInfo info;

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
        public string FullPath
        {
            get => fullPath;
            private set
            {
                fullPath = value;
                OnPathChanged();
            }
        }
        public IEnumerable<FileSystemItem> Childs
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
            get; set;
        }

        //Constructors
        public FileSystemItem(FileSystemInfo info)
        {
            IsHidden = false;
            IsActive = false;

            Info = info;

            watcher = new FileSystemWatcher();
            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Created;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.FileName
                                | NotifyFilters.DirectoryName;
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

            IsDirectory = PathExtensions.IsDirectory(FullPath);
            if (IsDirectory)
            {
                watcher.Path = FullPath;
                watcher.EnableRaisingEvents = true;
            }
            else
                watcher.EnableRaisingEvents = false;

            OnPropertyChanged(nameof(FullPath));
        }
        private void OnHiddenChanged()
        {
            foreach (var child in Childs)
                child.IsHidden = IsHidden && !IsActive;

            OnPropertyChanged(nameof(IsHidden));
        }
        private void OnActiveChanged()
        {
            if (IsDirectory && Childs == null)
            {
                LoadChilds();
            }

            foreach (var child in Childs)
                child.IsHidden = IsHidden && !IsActive;

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
                        ChildsList.Add(item);
                    }
                    catch
                    {
                    }
                }
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
            var item = ChildsList.Find(i => i.FullPath == e.FullPath);
            item.OnInfoChanged();
        }
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            RemoveChild(e.FullPath);
        }
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            AddChild(e.FullPath);
        }
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            RemoveChild(e.OldFullPath);
            AddChild(e.FullPath);
        }
        private void RemoveChild(string fullPath)
        {
            var item = ChildsList.Find(i => i.FullPath == fullPath);
            ChildsList.Remove(item);

            ChildsChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }
        private void AddChild(string fullPath)
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

            ChildsList.Add(item);

            ChildsChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }


        ~FileSystemItem()
        {
            watcher.Dispose();
        }
    }
}
