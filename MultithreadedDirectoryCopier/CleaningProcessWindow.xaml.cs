using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MultithreadedDirectoryCopier {
    public partial class CleaningProcessWindow : Window {
        private string _path;
        public CleaningProcessWindow(string path) {
            InitializeComponent();
            _path = path;
            Loaded += CleaningProcessWindow_Loaded;
        }

        private void CleaningProcessWindow_Loaded(object sender, RoutedEventArgs e) {
            DeleteFile();
        }

        private void DeleteFile() {
            Task.Run(() => Directory.Delete(_path, true))
                .ContinueWith(prevTask => Dispatcher.Invoke(() => Close()));
        }
    }
}
