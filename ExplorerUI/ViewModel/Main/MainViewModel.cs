using ExplorerUI.View.Main;
using FileTreeGrids.Models.FileSystemItems;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace ExplorerUI.ViewModel.Main
{
    public class MainViewModel : INotifyPropertyChanged
    {
        //Commands
        public static RoutedCommand SelectRoot;

        //Events
        public event PropertyChangedEventHandler PropertyChanged;

        //Fields
        private string rootFullPath;
        private Type itemType;
        
        //Properties
        public string RootFullPath {
            get => rootFullPath;
            set
            {
                rootFullPath = value;
                OnPropertyChanged();
            }
        }
        public Type ItemType
        {
            get => itemType;
            set
            {
                itemType = value;
                OnPropertyChanged();
            }
        }

        //Constructors
        static MainViewModel()
        {
            SelectRoot = new RoutedCommand(nameof(SelectRoot), typeof(MainWindow));
        }
        public MainViewModel()
        {
            ItemType = typeof(FileSystemItem);
        }

        //Methods
        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
