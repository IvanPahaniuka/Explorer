using FileTreeGrids.Extensions;
using FileTreeGrids.Models.FileSystemItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

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
        }

        //Methods
        private void OnRootFullPathChanged()
        {
            ReloadRoot();
            RootFullPathChanged?.Invoke(this, EventArgs.Empty);
        }
        private void OnItemTypeChanged()
        {
            ReloadRoot();
        }
        private void ReloadRoot()
        {
            Root = null;

            if (string.IsNullOrWhiteSpace(RootFullPath))
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
    }
}
