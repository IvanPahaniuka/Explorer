using ExplorerUI.ViewModel.Main;
using Ookii.Dialogs.Wpf;
using System.Windows;
using System.Windows.Input;

namespace ExplorerUI.View.Main
{
    public partial class MainWindow : Window
    {
        //Fields
        private MainViewModel model;

        //Constructors
        public MainWindow()
        {
            InitializeComponent();

            model = new MainViewModel();
            DataContext = model;
        }
        
        //Methods
        private void CommandBinding_SelectRoot(object sender, ExecutedRoutedEventArgs e)
        {
            var OF = new VistaFolderBrowserDialog();
            if (OF.ShowDialog() == true)
            {
                model.RootFullPath = OF.SelectedPath;
            }
        }
    }
}
