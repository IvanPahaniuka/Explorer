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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FileTreeGrids.Models.FileSystemItems
{
    public partial class FileSystemItem : INotifyPropertyChanged
    {
        //Struct
        private struct FileSystemItemMemento
        {
            public bool isActive;
            public bool isHidden;
            public int level;

            public static FileSystemItemMemento Create(FileSystemItem item)
            {
                return new FileSystemItemMemento
                {
                    isActive = item.isActive,
                    isHidden = item.isHidden,
                    level = item.level
                };
            }
        }

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
        private FileSystemInfo info;
        private CancellationTokenSource loadingTokenSource;
        private Task loadingTask;

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
                if (info.FullName != FullPath)
                    FullPath = info.FullName;

                OnInfoChanged();
            }
        }

        internal List<FileSystemItem> ChildsList
        {
            get;
            set;
        }

        //Constructors
        public FileSystemItem(FileSystemInfo info)
        {
            IsHidden = false;
            IsActive = false;
            Level = 0;

            Info = info;
        }

        //Methods
        internal void RemoveChild(string fullPath)
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
        internal void AddChild(string fullPath)
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
        internal void UpdateInfo()
        {
            try
            {
                Info.Refresh();
            }
            catch
            {
                return;
            }


            OnInfoChanged();
        }

        protected virtual void OnInfoChanged()
        {
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

            if (Info.FullName != FullPath)
                Info = FileSystemInfoExtensions.GetInfo(FullPath);

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
            StopLoadingTask();

            loadingTokenSource = new CancellationTokenSource();
            var token = loadingTokenSource.Token;
            var memento = FileSystemItemMemento.Create(this);
            loadingTask = Task.Run(() =>
            {
                try
                {
                    var newChilds = new List<FileSystemItem>();
                    var dirInfo = Info as DirectoryInfo;
                    var files = dirInfo.GetFileSystemInfos();
                    foreach (var file in files)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        try
                        {
                            var item = Activator.CreateInstance(GetType(), file) as FileSystemItem;
                            FixItemState(item, memento);
                            newChilds.Add(item);
                        }
                        catch
                        {
                        }
                    }
                    newChilds.Sort(FileSystemItemsComparers.NameComparer);

                    if (token.IsCancellationRequested)
                        return;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ChildsList = newChilds;
                        ChildsChanged?.Invoke(this,
                           new NotifyCollectionChangedEventArgs(
                               NotifyCollectionChangedAction.Add,
                               ChildsList));
                    }, DispatcherPriority.Background);
                }
                catch { }
            });


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
                item?.UpdateInfo();
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
        private void StopLoadingTask()
        {
            if (loadingTask != null &&
                !loadingTask.IsCompleted)
            {
                if (!loadingTokenSource.IsCancellationRequested)
                    loadingTokenSource.Cancel();

                loadingTask.Wait();
            }
        }


        ~FileSystemItem()
        {
            StopLoadingTask();
        }
    }
}
