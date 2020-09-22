using FileTreeGrids.Extensions;
using FileTreeGrids.Models.FileSystemItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;

namespace FileTreeGrids.Models.FileSystemTrees
{
    public class FileSystemTree
    {
        //Events
        public event EventHandler RootChanged;
        public event EventHandler RootFullPathChanged;

        //Fields
        private FileSystemItem root;
        private string rootFullPath;
        private Type itemType;
        private FileSystemWatcher watcher;

        //Properties
        public FileSystemItem Root
        {
            get => root;
            private set
            {
                root = value;
                RootChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public string RootFullPath
        {
            get => rootFullPath;
            set
            {
                rootFullPath = value;
                OnRootFullPathChanged();
            }
        }
        public Type ItemType
        {
            get => itemType;
            set
            {
                itemType = value;
                OnItemTypeChanged();
            }
        }

        //Constructors
        public FileSystemTree()
        {
            ItemType = typeof(FileSystemItem);

            watcher = new FileSystemWatcher();
            watcher.Changed += Watcher_Changed;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.FileName
                                | NotifyFilters.DirectoryName;
        }

        //Methods
        private void OnRootFullPathChanged()
        {
            ReloadRoot();

            if (!string.IsNullOrWhiteSpace(RootFullPath) &&
                Directory.Exists(RootFullPath) &&
                !string.IsNullOrWhiteSpace(Path.GetDirectoryName(RootFullPath)))
            {
                watcher.Path = Path.GetDirectoryName(RootFullPath);
                watcher.EnableRaisingEvents = true;
            }
            else
                watcher.EnableRaisingEvents = false;


            RootFullPathChanged?.Invoke(this, EventArgs.Empty);
        }
        private void OnItemTypeChanged()
        {
            ReloadRoot();
        }
        private void ReloadRoot()
        {
            Root = null;

            if (string.IsNullOrWhiteSpace(RootFullPath) ||
                !Directory.Exists(RootFullPath))
                return;

            FileSystemItem item;
            try
            {
                item = FileSystemItem.Create(ItemType, RootFullPath);
            }
            catch
            {
                return;
            }
            Root = item;
        }
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (Root != null && e.Name == Root.Name)
                Application.Current.Dispatcher.Invoke(() => {
                    ReloadRoot();
                });
        }
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (Root != null && e.Name == Root.Name)
                Application.Current.Dispatcher.Invoke(() => ReloadRoot());

        }
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Root != null && e.Name == Root.Name)
                Application.Current.Dispatcher.Invoke(() => Root.OnInfoChanged());

        }

        ~FileSystemTree()
        {
            watcher.Dispose();
        }
    }
}
