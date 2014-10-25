using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using OktetNET.OQmlTestAdapter.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OktetNET.OQmlTestAdapter
{
    public class QmlTestContainerCache: IDisposable
    {
        public event EventHandler CacheUpdated;
        private readonly List<ITestContainer> cache = new List<ITestContainer>();

        private ISolutionEventsListener solutionListener;
        private ITestFilesUpdateWatcher testFilesUpdateWatcher;
        private ITestFileAddRemoveListener testFilesAddRemoveListener;
        private ITestContainerDiscoverer testContainerDiscoverer;
        private readonly IServiceProvider serviceProvider;
        private bool initialContainerSearch = true;

        public QmlTestContainerCache(
            ITestContainerDiscoverer testContainerDiscoverer,
            IServiceProvider serviceProvider,
            ISolutionEventsListener solutionListener, 
            ITestFilesUpdateWatcher testFilesUpdateWatcher, 
            ITestFileAddRemoveListener testFilesAddRemoveListener)
        {
            this.solutionListener = solutionListener;
            this.serviceProvider = serviceProvider;
            this.testFilesUpdateWatcher = testFilesUpdateWatcher;
            this.testFilesAddRemoveListener = testFilesAddRemoveListener;
            this.testContainerDiscoverer = testContainerDiscoverer;
            
            this.solutionListener.SolutionUnloaded += OnSolutionUnloaded;
            this.solutionListener.SolutionProjectChanged += OnSolutionProjectChanged;
            this.testFilesUpdateWatcher.FileChangedEvent += OnProjectItemChanged;
            this.testFilesAddRemoveListener.TestFileChanged += OnProjectItemChanged;

            this.solutionListener.StartListeningForChanges();
            this.testFilesAddRemoveListener.StartListeningForTestFileChanges();
        }

        public void Add(ITestContainerDiscoverer containerDiscoverer, string path)
        {
            Remove(path);
            if (IsTestFile(path))
            {
                QmlTestContainer container = new QmlTestContainer(containerDiscoverer,
                    path.ToLowerInvariant());

                cache.Add(container);
            }
        }

        public void Remove(string path)
        {
            cache.RemoveAll(container => container.Source.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        private void AddAll(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                testFilesUpdateWatcher.AddWatch(file);
                Add(this.testContainerDiscoverer, file);
            }
        }

        private void RemoveAll(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                testFilesUpdateWatcher.RemoveWatch(file);
                Remove(file);
            }
        }

        private void OnCacheUpdated()
        {
            if (CacheUpdated != null && !initialContainerSearch)
            {
                CacheUpdated(this, EventArgs.Empty);
            }
        }

        private void OnSolutionUnloaded(object sender, EventArgs eventArgs)
        {
            initialContainerSearch = true;
        }

        private void OnSolutionProjectChanged(object sender, SolutionEventsListenerEventArgs e)
        {
            if (e != null)
            {
                switch (e.ChangedReason)
                {
                    case SolutionChangedReason.Load:
                        AddAll(GetTestFiles(e.Project));
                        break;
                    case SolutionChangedReason.Unload:
                        RemoveAll(GetTestFiles(e.Project));
                        break;
                }
            }
        }

        private void OnProjectItemChanged(object sender, TestFileChangedEventArgs e)
        {
            if (e != null)
            {
                switch (e.ChangedReason)
                {
                    case TestFileChangedReason.Added:
                        testFilesUpdateWatcher.AddWatch(e.File);
                        Add(testContainerDiscoverer, e.File);
                        break;
                    case TestFileChangedReason.Removed:
                        testFilesUpdateWatcher.RemoveWatch(e.File);
                        Remove(e.File);
                        break;
                    case TestFileChangedReason.Changed:
                        Add(testContainerDiscoverer, e.File);
                        break;
                }

                OnCacheUpdated();
            }
        }

        public IEnumerable<ITestContainer> Get()
        {
            if (initialContainerSearch)
            {
                cache.Clear();
                AddAll(GetTestFiles());
                initialContainerSearch = false;
            }

            return cache;
        }

        private IEnumerable<string> GetTestFiles()
        {
            var solution = (IVsSolution)serviceProvider.GetService(typeof(SVsSolution));
            var loadedProjects = solution.EnumerateLoadedProjects(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION).OfType<IVsProject>();

            return loadedProjects.SelectMany(GetTestFiles).ToList();
        }

        private IEnumerable<string> GetTestFiles(IVsProject project)
        {
            return VsSolutionHelper.GetProjectItems(project).Where(
                o => IsTestFile(o));
        }

        private static readonly Regex matcher = new Regex("\\\\tst_.*\\.qml$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static bool IsTestFile(string filePath)
        {
            return matcher.IsMatch(filePath);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (testFilesUpdateWatcher != null)
                {
                    testFilesUpdateWatcher.FileChangedEvent -= OnProjectItemChanged;
                    ((IDisposable)testFilesUpdateWatcher).Dispose();
                    testFilesUpdateWatcher = null;
                }

                if (testFilesAddRemoveListener != null)
                {
                    testFilesAddRemoveListener.TestFileChanged -= OnProjectItemChanged;
                    testFilesAddRemoveListener.StopListeningForTestFileChanges();
                    testFilesAddRemoveListener = null;
                }

                if (solutionListener != null)
                {
                    solutionListener.SolutionProjectChanged -= OnSolutionProjectChanged;
                    solutionListener.StopListeningForChanges();
                    solutionListener = null;
                }
            }
        }
    }
}
