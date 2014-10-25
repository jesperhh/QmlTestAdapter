using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace OktetNET.OQmlTestAdapter.Helpers
{
    [Export(typeof(ITestFilesUpdateWatcher))]
    public class TestFilesUpdateWatcher : IDisposable, ITestFilesUpdateWatcher
    {
        private class FileWatcherInfo
        {
            public FileWatcherInfo(FileSystemWatcher watcher)
            {
                Watcher = watcher;
                LastEventTime = DateTime.MinValue;
            }

            public FileSystemWatcher Watcher { get; set; }
            public DateTime LastEventTime { get; set; }
        }

        private IDictionary<string, FileWatcherInfo> fileWatchers;
        public event EventHandler<TestFileChangedEventArgs> FileChangedEvent;

        public TestFilesUpdateWatcher()
        {
            fileWatchers = new Dictionary<string, FileWatcherInfo>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddWatch(string path)
        {
            ValidateArg.NotNullOrEmpty(path, "path");
            if (!fileWatchers.ContainsKey(path))
            {
                string directoryName = Path.GetDirectoryName(path);
                string fileName = Path.GetFileName(path);

                FileWatcherInfo watcherInfo = new FileWatcherInfo(new FileSystemWatcher(directoryName, fileName));
                fileWatchers.Add(path, watcherInfo);

                watcherInfo.Watcher.Changed += OnChanged;
                watcherInfo.Watcher.Renamed += OnRenamed;
                watcherInfo.Watcher.EnableRaisingEvents = true;
            }
        }

        public void RemoveWatch(string path)
        {
            ValidateArg.NotNullOrEmpty(path, "path");
            FileWatcherInfo watcherInfo;
            if (fileWatchers.TryGetValue(path, out watcherInfo))
            {
                watcherInfo.Watcher.EnableRaisingEvents = false;

                fileWatchers.Remove(path);

                watcherInfo.Watcher.Changed -= OnChanged;
                watcherInfo.Watcher.Renamed -= OnRenamed;
                watcherInfo.Watcher.Dispose();
                watcherInfo.Watcher = null;
            }
        }

        private void OnRenamed(object sender, RenamedEventArgs renamedEventArgs)
        {
            OnChanged(sender, renamedEventArgs);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            FileWatcherInfo watcherInfo;
            if (FileChangedEvent != null && fileWatchers.TryGetValue(e.FullPath, out watcherInfo))
            {
                DateTime writeTime = File.GetLastWriteTime(e.FullPath);
                if (writeTime.Subtract(watcherInfo.LastEventTime).TotalMilliseconds > 500)
                {
                    watcherInfo.LastEventTime = writeTime;
                    FileChangedEvent(sender, new TestFileChangedEventArgs(
                        e.FullPath, TestFileChangedReason.Changed));
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && fileWatchers != null)
            {
                foreach (FileWatcherInfo fileWatcher in fileWatchers.Values)
                {
                    if (fileWatcher != null && fileWatcher.Watcher != null)
                    {
                        fileWatcher.Watcher.Changed -= OnChanged;
                        fileWatcher.Watcher.Renamed -= OnRenamed;
                        fileWatcher.Watcher.Dispose();
                    }
                }

                fileWatchers.Clear();
                fileWatchers = null;
            }
        }
    }
}