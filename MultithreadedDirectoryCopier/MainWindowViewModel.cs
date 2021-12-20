using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MultithreadedDirectoryCopier {
    class MainWindowViewModel : INotifyPropertyChanged {

        private object _key = new object();
        private Task[] _tasks;
        private ConcurrentQueue<string> _pathsFiles;
        private string _copyFrom;
        private string _copyTo;
        private string _nameCopiedDirectory;
        private long _directorySize;
        private long _copiedDataSize;
        private bool _isCopying;
        private CancellationTokenSource _cancel;
        private bool _isOverallProgressBarVisible;

        public event PropertyChangedEventHandler PropertyChanged;
        public IList<FileProperties> Files { get; }
        public IList<bool> IsElementsVisible { get; }
        public Window Owner { get; set; }


        public string CopyFrom {
            get => _copyFrom;
            set {
                if (_copyFrom == value) return;
                _copyFrom = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CopyFrom)));
            }
        }
        public string CopyTo {
            get => _copyTo;
            set {
                if (_copyTo == value) return;
                _copyTo = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CopyTo)));
            }
        }
        public long DirectorySize {
            get => _directorySize;
            set {
                if (_directorySize == value) return;
                _directorySize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DirectorySize)));
            }
        }
        public long CopiedDataSize {
            get => _copiedDataSize;
            set {
                if (_copiedDataSize == value) return;
                _copiedDataSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CopiedDataSize)));
            }
        }
        public bool IsOverallProgressBarVisible {
            get => _isOverallProgressBarVisible;
            set {
                if (_isOverallProgressBarVisible == value) return;
                _isOverallProgressBarVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOverallProgressBarVisible)));
            }
        }


        public DelegateCommand CopyCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand CopyDirectoryFromCommand { get; }
        public DelegateCommand СopyDirectoryToCommand { get; }



        public MainWindowViewModel() {
            Files = new ObservableCollection<FileProperties>(new FileProperties[4]);
            IsElementsVisible = new ObservableCollection<bool> {false,false,false,false};
            IsOverallProgressBarVisible = false;
            _isCopying = false;
            CopyCommand = new DelegateCommand(Copy);
            CancelCommand = new DelegateCommand(Cancel);
            CopyDirectoryFromCommand = new DelegateCommand(() => CopyFrom = WorkWithDirectory.DirectorySelection());
            СopyDirectoryToCommand = new DelegateCommand(() =>  CopyTo = WorkWithDirectory.DirectorySelection());
        }


        private async void Copy() {
            if (_isCopying)
                return;
            if (!Directory.Exists(CopyFrom)) {
                MessageBox.Show(
                    $"Не удалось найти путь Откуда копировать - '{CopyFrom}'",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
                return;
            }
            if (!Directory.Exists(CopyTo)) {
                MessageBox.Show($"Не удалось найти путь Куда копировать - '{CopyTo}'",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
                return;
            }
            if (CopyTo.Contains(CopyFrom)) {
                MessageBox.Show($"Конечная папка, в которую следует поместить файлы, является дочерней для папки, в которой она находится",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
                return;
            }
            
            _cancel = new CancellationTokenSource();
            CancellationToken token = _cancel.Token;
            _nameCopiedDirectory = GetNewNameIfDirectoryExists(CopyFrom);
            _pathsFiles = new ConcurrentQueue<string>(WorkWithDirectory.RecursiveDirectoryTraversal(CopyFrom));
            CopiedDataSize = 0;
            DirectorySize = WorkWithDirectory.GetDirectorySize(CopyFrom);
            _isCopying = true;
            IsOverallProgressBarVisible = true;

            _tasks = Enumerable.Range(0, 4)
                .Select(index => Task.Run(() => CopyFile(index, token)).ContinueWith(
                    prevTask => {
                        if (prevTask.IsFaulted) {
                            _cancel.Cancel();
                            throw prevTask.Exception.InnerException;
                        }
                    }))
                .ToArray();
            try {
                await Task.WhenAll(_tasks);
            }
            catch (Exception ex) {
                MessageBox.Show(
                    $"Произошла ошибка!\n\n{ex.Message}\n\nВсе скопированные файлы будут удалены",
                    "Ошибка!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                DeleteCodiedFile();
            }

            if (!token.IsCancellationRequested)
                MessageBox.Show("Все файлы скопированы!");
            else
                for (int i = 0; i < IsElementsVisible.Count; i++)
                    IsElementsVisible[i] = false;
            
            IsOverallProgressBarVisible = false;
            _isCopying = false;
        }

        private void Cancel() {
            if (!_isCopying)
                return;
            _cancel.Cancel();
            _isCopying = false;
            Task.WaitAll(_tasks);
            DeleteCodiedFile();
        }

        private void CopyFile(int indexTask, CancellationToken token) {
            
            while (_pathsFiles.TryDequeue(out string file)) {
                if (token.IsCancellationRequested)
                    return;
                
                Files[indexTask] = new FileProperties(file);
                IsElementsVisible[indexTask] = true;
                string from = file;
                string to;
                to = from.Replace(CopyFrom, Path.Combine(CopyTo, _nameCopiedDirectory));
                if (!Directory.Exists(Path.GetDirectoryName(to)))
                    Directory.CreateDirectory(Path.GetDirectoryName(to));
                byte[] buffer = new byte[4096];
                int total = 0;
                using (Stream readStream = File.OpenRead(from)) {
                    if (token.IsCancellationRequested)
                        return;
                    using (Stream writeStream = File.Create(to)) {
                        while (readStream.Position < readStream.Length) {
                            if (token.IsCancellationRequested)
                                return;
                            int read = readStream.Read(buffer, 0, buffer.Length);
                            writeStream.Write(buffer, 0, read);
                            total += read;
                            lock (_key) {
                                CopiedDataSize += read;
                            }
                            Files[indexTask].CopiedDataSize = total;
                        }
                    }
                }
                IsElementsVisible[indexTask] = false;
            }
        }


        public void OnWindowClosing(object sender, CancelEventArgs e) {
            if (!_isCopying)
                return;
            MessageBoxResult result = MessageBox.Show(
                "Закрыть программу и прервать процесс копирования файлов?\n\nВсе скопированные файлы будут удалены!",
                "Закрыть программу",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
                );
            if (result == MessageBoxResult.No) {
                e.Cancel = true;
                return;
            }
            Cancel();
        }



        private string GetNewNameIfDirectoryExists(string path) {
            string nameCopiedDirectory = new DirectoryInfo(CopyFrom).Name;
            string[] directories = Directory.GetDirectories(CopyTo);
            int count = Directory.GetDirectories(CopyTo)
                .Count(directory => new DirectoryInfo(directory).Name.Contains(nameCopiedDirectory));
            if (count > 1)
                return $"{nameCopiedDirectory} — копия ({count})";
            else if (count == 1)
               return $"{nameCopiedDirectory} — копия";
            else 
                return nameCopiedDirectory;
        }


        private void DeleteCodiedFile() {
            CleaningProcessWindow cleaningWindow = new CleaningProcessWindow(Path.Combine(CopyTo, _nameCopiedDirectory));
            cleaningWindow.Owner = Owner;
            cleaningWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner.Opacity = 0.8;
            cleaningWindow.ShowDialog();
            Owner.Opacity = 1;
        }
    }
}
