using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace MultithreadedDirectoryCopier {
    public partial class MainWindow : Window {
        private MainWindowViewModel _viewModel;

        public MainWindow() {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            _viewModel.Owner = this;
            Closing += _viewModel.OnWindowClosing;
        }
    }
}
