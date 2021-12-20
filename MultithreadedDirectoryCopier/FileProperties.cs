using System.ComponentModel;
using System.IO;


namespace MultithreadedDirectoryCopier {
    class FileProperties : INotifyPropertyChanged {
        private long _copiedDataSize;

        public string Name {
            get;
        }
        public long Size {
            get;
        }

        public long CopiedDataSize {
            get => _copiedDataSize;
            set {
                if (value == _copiedDataSize) return;
                _copiedDataSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CopiedDataSize)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public string Path { get; }
        
        public FileProperties(string path) {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            Size = new FileInfo(path).Length;
            CopiedDataSize = 0;
        }
    }
}
