using ExplorerUI.Models.FileSystemItems;
using ExplorerUI.ViewModel.Main;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
