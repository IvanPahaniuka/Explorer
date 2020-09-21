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

namespace FileTreeGrids.Converters
{
    public class FileSystemTreeToCollectionConverter : IValueConverter
    {
        //Fields
        private ObservableCollection<FileSystemItem> collection;
        private FileSystemTree tree;
        private FileSystemItem bindedRoot;

        //Properties
        private FileSystemTree Tree
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
        }

        //Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FileSystemTree tree)
            {
                Tree = tree;
                return collection;
            }

            return DependencyProperty.UnsetValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

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
            Item_ChildsChanged(item, new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item.Childs));
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
                    foreach (var item in e.NewItems)
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
