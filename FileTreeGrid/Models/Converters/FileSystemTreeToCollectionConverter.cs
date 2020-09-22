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
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Immutable;

namespace FileTreeGrids.Models.Converters
{
    public class FileSystemTreeToCollectionConverter
    {
        //Fields
        private ObservableCollection<FileSystemItem> collection;
        private FileSystemTree tree;
        private FileSystemItem bindedRoot;
        private Task refreshTask;
        private CancellationTokenSource refreshTokenSource;

        //Properties
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
        public CollectionViewSource CollectionSource
        {
            get;
        }

        //Constructors
        public FileSystemTreeToCollectionConverter()
        {
            collection = new ObservableCollection<FileSystemItem>();

            CollectionSource = new CollectionViewSource() { Source = collection };
            CollectionSource.Filter += new FilterEventHandler(CollectionSource_Filter);
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

            item.PropertyChanged -= Item_PropertyChanged;
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
                int index = collection.IndexOf(parent);
                foreach (var child in parent.Childs)
                    if (child != item)
                    {
                        if (collection.Contains(child))
                            index++;
                    }
                    else
                        break;

                collection.Insert(index + 1, item);
            }
            else
                collection.Add(item);

            item.PropertyChanged += Item_PropertyChanged;
            item.ChildsChanged += Item_ChildsChanged;
            if (item.Childs != null)
                Item_ChildsChanged(item, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    new List<FileSystemItem>(item.Childs)));
        }
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileSystemItem.IsHidden) &&
                sender is FileSystemItem)
            {
                GrowUpRefreshTask();
            }
        }
        private void GrowUpRefreshTask()
        {
            if (refreshTask == null || refreshTask.IsCompleted)
            {
                refreshTokenSource = new CancellationTokenSource();
                var token = refreshTokenSource.Token;
                refreshTask = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(50, token);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (!token.IsCancellationRequested)
                                CollectionSource.View.Refresh();
                        });
                    }
                    catch
                    {

                    }
                });
            }
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
                case NotifyCollectionChangedAction.Move:
                    MoveItems(e.OldItems, e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var item in e.OldItems)
                        Unbind(item as FileSystemItem);
                    foreach (var item in e.NewItems)
                        Bind(item as FileSystemItem, itemSender);
                    break;
            }
        }
        private void CollectionSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is FileSystemItem item)
            {
                e.Accepted = !item.IsHidden;
            }
        }
        private void MoveItems(IList items, int oldIndex, int newIndex)
        {
            if (items == null || items.Count == 0)
                return;

            int offset = collection.IndexOf(items[0] as FileSystemItem);
            for (int i = 0; i < items.Count; i++)
            {
                collection.Move(offset + i, offset + i + newIndex - oldIndex);
            }
        }

        //Destructor
        ~FileSystemTreeToCollectionConverter()
        {
            Unbind(Tree);

            if (refreshTokenSource != null && 
                !refreshTokenSource.IsCancellationRequested)
                refreshTokenSource.Cancel();
        }
    }
}
