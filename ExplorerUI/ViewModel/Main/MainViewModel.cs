using ExplorerUI.View.Main;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        
        //Properties
        public string RootFullPath {
            get => rootFullPath;
            set
            {
                rootFullPath = value;
                OnPropertyChanged();
            }
        }

        //Constructors
        static MainViewModel()
        {
            SelectRoot = new RoutedCommand(nameof(SelectRoot), typeof(MainWindow));
        }

        //Methods
        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
