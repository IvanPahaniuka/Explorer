using FileTreeGrids.Extensions;
using FileTreeGrids.Models.Converters;
using FileTreeGrids.Models.FileSystemItems;
using FileTreeGrids.Models.FileSystemTrees;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileTreeGrids
{
    public partial class FileTreeGrid : UserControl
    {
        //RoutedEvents
        public static readonly RoutedEvent RootChangedEvent;
        public static readonly RoutedEvent ItemTypeChangedEvent;

        //DependencyProperties
        public static readonly DependencyProperty RootProperty;
        public static readonly DependencyProperty ItemTypeProperty;

        //Events
        public event RoutedPropertyChangedEventHandler<string> RootChanged
        {
            add { AddHandler(RootChangedEvent, value); }
            remove { RemoveHandler(RootChangedEvent, value); }
        }
        public event RoutedPropertyChangedEventHandler<Type> ItemTypeChanged
        {
            add { AddHandler(ItemTypeChangedEvent, value); }
            remove { RemoveHandler(ItemTypeChangedEvent, value); }
        }

        //Fields
        private FileSystemTree itemsSource;
        private FileSystemTreeToCollectionConverter converter;

        //Properties
        public string Root
        {
            get { return (string)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }
        public Type ItemType
        {
            get { return (Type)GetValue(ItemTypeProperty); }
            set { SetValue(ItemTypeProperty, value); }
        }
        public ObservableCollection<DataGridColumn> Columns
        {
            get => grid.Columns;
        }

        internal FileSystemTree ItemsSource
        {
            get => itemsSource;
            set
            {
                itemsSource = value;
                converter.Tree = value;
                grid.ItemsSource = converter.CollectionSource.View;
            }
        }


        //Constructors
        static FileTreeGrid()
        {
            RootProperty = DependencyProperty.Register(
                nameof(Root), typeof(string), typeof(FileTreeGrid),
                new FrameworkPropertyMetadata(string.Empty,
                    new PropertyChangedCallback(OnRootChanged)));
            ItemTypeProperty = DependencyProperty.Register(
                nameof(ItemType), typeof(Type), typeof(FileTreeGrid),
                new FrameworkPropertyMetadata(typeof(FileSystemItem),
                    new PropertyChangedCallback(OnItemTypeChanged)),
                new ValidateValueCallback(ValidateItemTypeValue));

            RootChangedEvent = EventManager.RegisterRoutedEvent(
                nameof(RootProperty), RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<string>),
                typeof(FileTreeGrid));
            ItemTypeChangedEvent = EventManager.RegisterRoutedEvent(
                nameof(ItemTypeProperty), RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Type>),
                typeof(FileTreeGrid));
        }
        public FileTreeGrid()
        {
            InitializeComponent();
            
            converter = new FileSystemTreeToCollectionConverter();
            ItemsSource = new FileSystemTree();
        }

        //Static methods
        private static void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FileTreeGrid)d;

            var args = new RoutedPropertyChangedEventArgs<string>(
                (string)e.OldValue, (string)e.NewValue, RootChangedEvent);
            control.OnRootChanged(args);
        }
        private static bool ValidateItemTypeValue(object value)
        {
            var type = value as Type;

            if (value == null)
                return false;

            if (!type.IsCompatible<FileSystemItem>())
                return false;

            Type[] types = { typeof(FileSystemInfo) };
            if (type.GetConstructor(types) == null)
                return false;

            return true;
        }
        private static void OnItemTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FileTreeGrid)d;

            var args = new RoutedPropertyChangedEventArgs<Type>(
                (Type)e.OldValue, (Type)e.NewValue, ItemTypeChangedEvent);
            control.OnItemTypeChanged(args);
        }

        //Methods
        protected virtual void OnRootChanged(RoutedPropertyChangedEventArgs<string> args)
        {
            ItemsSource.RootFullPath = Root;
            RaiseEvent(args);
        }
        protected virtual void OnItemTypeChanged(RoutedPropertyChangedEventArgs<Type> args)
        {
            ItemsSource.ItemType = ItemType;
            RaiseEvent(args);
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row &&
                row.Item is FileSystemItem item)
            {
                item.IsActive = !item.IsActive;
            }
        }
    }
}