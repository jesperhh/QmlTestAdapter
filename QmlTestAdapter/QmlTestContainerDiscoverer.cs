using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using OktetNET.OQmlTestAdapter.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OktetNET.OQmlTestAdapter
{
    [Export(typeof(ITestContainerDiscoverer))]
    public class QmlTestContainerDiscoverer : ITestContainerDiscoverer
    {
        public event EventHandler TestContainersUpdated;
        private readonly QmlTestContainerCache cachedContainers;
        
        [ImportingConstructor]
        public QmlTestContainerDiscoverer(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            ISolutionEventsListener solutionListener,
            ITestFilesUpdateWatcher testFilesUpdateWatcher,
            ITestFileAddRemoveListener testFilesAddRemoveListener)
        {
            cachedContainers = new QmlTestContainerCache(
                this, serviceProvider, solutionListener, testFilesUpdateWatcher, testFilesAddRemoveListener);

            cachedContainers.CacheUpdated += (source, e) => {
                if (TestContainersUpdated != null)
                    TestContainersUpdated(this, EventArgs.Empty);
                };
        }

        public Uri ExecutorUri
        {
            get { return QmlTestExecutor.ExecutorUri; }
        }

        public IEnumerable<ITestContainer> TestContainers
        {
            get { return cachedContainers.Get(); }
        }
    }
}