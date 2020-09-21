using FileTreeGrids.Models.FileSystemItems;
using FileTreeGrids.Models.FileSystemTrees;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections;

namespace FileTreeGrids.Models.Converters
{
    public class FileSystemTreeToCollectionConverter
    {
        //Fields
        private ObservableCollection<FileSystemItem> collection;
        private FileSystemTree tree;
        private FileSystemItem bindedRoot;

        //Properties
        public ReadOnlyObservableCollection<FileSystemItem> Collection
        {
            get;
        } 
        public FileSystemTree Tree
        {
            get => tree;
            set
            {
                Unbind(tree);
                tree = value;
                Bind(tree);
            }
        }

        //Constructors
        public FileSystemTreeToCollectionConverter()
        {
            collection = new ObservableCollection<FileSystemItem>();
            Collection = new ReadOnlyObservableCollection<FileSystemItem>(collection);
        }

        //Methods
        private void Unbind(FileSystemTree tree)
        {
            if (tree == null)
                return;

            collection.Clear();
            tree.RootChanged -= Tree_RootChanged;
        }
        private void Bind(FileSystemTree tree)
        {
            if (tree == null)
                return;

            collection.Clear();
            tree.RootChanged += Tree_RootChanged;
            OnRootChanged();
        }
        private void Tree_RootChanged(object sender, EventArgs e)
        {
            OnRootChanged();
        }
        private void OnRootChanged()
        {
            Unbind(bindedRoot);
            bindedRoot = Tree.Root;
            Bind(bindedRoot);
        }
        private void Unbind(FileSystemItem item)
        {
            if (item == null)
                return;

            collection.Remove(item);

            item.ChildsChanged -= Item_ChildsChanged;
            if (item.Childs != null)
                Item_ChildsChanged(item, new NotifyCollectionChangedEventArgs(
                   NotifyCollectionChangedAction.Remove,
                   new List<FileSystemItem>(item.Childs)));
        }
        private void Bind(FileSystemItem item, FileSystemItem parent = null)
        {
            if (item == null)
                return;

            if (parent != null)
            {
                int parentIndex = collection.IndexOf(parent);
                collection.Insert(parentIndex + 1, item);
            }
            else
                collection.Add(item);


            item.ChildsChanged += Item_ChildsChanged;
            if (item.Childs != null)
                Item_ChildsChanged(item, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, 
                    new List<FileSystemItem>(item.Childs)));
        }
        private void Item_ChildsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var itemSender = sender as FileSystemItem;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        Bind(item as FileSystemItem, itemSender);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        Unbind(item as FileSystemItem);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var child in itemSender.Childs)
                        Unbind(child);
                    break;
            }
        }

        //Destructor
        ~FileSystemTreeToCollectionConverter()
        {
            Unbind(Tree);
        }
    }
}
