using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.IO;


namespace MultithreadedDirectoryCopier {
    class WorkWithDirectory {

        public static string DirectorySelection() {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                return dialog.FileName;

            return null;
        }

        public static IEnumerable<string> RecursiveDirectoryTraversal(string path) {
            List<string> paths = new List<string>();
            foreach (string sub in Directory.GetDirectories(path))
                paths.AddRange(RecursiveDirectoryTraversal(sub));
            foreach (string file in Directory.GetFiles(path))
                paths.Add(file);
            return paths;
        }

        public static long GetDirectorySize(string path) {
            long size = 0;
            foreach (string sub in Directory.GetDirectories(path))
                size += GetDirectorySize(sub);
            foreach (string file in Directory.GetFiles(path))
                size += new FileInfo(file).Length;
            return size;
        }
    }
}
