using FileTreeGrids.Extensions;
using FileTreeGrids.Models.FileSystemItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
            watcher.Created += Watcher_Created;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;
            watcher.IncludeSubdirectories = true;
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
                Directory.Exists(RootFullPath))
            {
                try
                {
                    watcher.Path = RootFullPath;
                    watcher.EnableRaisingEvents = true;
                }
                catch
                {

                }
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
            if (Root != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var parentPath = Path.GetDirectoryName(e.FullPath);
                    var parentItem = FindItem(parentPath);
                    if (parentItem != null && parentItem.Childs != null)
                    {
                        parentItem.RemoveChild(e.OldFullPath);
                        parentItem.AddChild(e.FullPath);
                    }
                });
            }
        }
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (Root != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var parentPath = Path.GetDirectoryName(e.FullPath);
                    var parentItem = FindItem(parentPath);
                    if (parentItem != null && parentItem.Childs != null)
                        parentItem.RemoveChild(e.FullPath);
                });
            }

        }
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Root != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var item = FindItem(e.FullPath);
                    if (item != null)
                        item.OnInfoChanged();
                });
            }

        }
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (Root != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var parentPath = Path.GetDirectoryName(e.FullPath);
                    var parentItem = FindItem(parentPath);
                    if (parentItem != null && parentItem.Childs != null)
                        parentItem.AddChild(e.FullPath);
                });
            }
        }
        private FileSystemItem FindItem(string path)
        {
            var dirBuilder = new StringBuilder(256);
            path = path.Substring(RootFullPath.Length).Trim('/', '\\');
            var current = Root;

            while (!string.IsNullOrWhiteSpace(path) && current != null)
            {
                dirBuilder.Clear();
                for (int i = 0; i < path.Length && path[i] != '/' && path[i] != '\\'; i++)
                    dirBuilder.Append(path[i]);

                var dir = dirBuilder.ToString();
                path = path.Substring(dir.Length).Trim('/', '\\');

                if (current.Childs != null)
                    current = current.Childs.First(i => i.Name == dir);
                else
                    current = null;
            }

            return current;
        }

        ~FileSystemTree()
        {
            watcher.Dispose();
        }
    }
}
