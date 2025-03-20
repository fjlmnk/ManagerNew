using System;
using System.Windows;

namespace FileManager3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileManagerViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new FileManagerViewModel();
            DataContext = viewModel;
        }
    }
}
