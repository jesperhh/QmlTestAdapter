using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.TestWindow.Extensibility.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OktetNET.OQmlTestAdapter
{
    public class QmlTestContainer: ITestContainer
    {
        private readonly ITestContainerDiscoverer containerDiscoverer;
        private readonly DateTime timeStamp;

        public QmlTestContainer(ITestContainerDiscoverer containerDiscoverer, string source)
        {
            this.containerDiscoverer = containerDiscoverer;
            this.Source = source;
            this.timeStamp = GetTimeStamp();
        }

        private QmlTestContainer(QmlTestContainer original)
            : this(original.containerDiscoverer, original.Source)
        {

        }

        private DateTime GetTimeStamp()
        {
            if (!String.IsNullOrEmpty(this.Source) && File.Exists(this.Source))
            {
                return File.GetLastWriteTime(this.Source);
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public int CompareTo(ITestContainer other)
        {
            QmlTestContainer testContainer = other as QmlTestContainer;
            if (testContainer == null)
                return -1;

            int result = String.Compare(this.Source, testContainer.Source, StringComparison.OrdinalIgnoreCase);
            if (result != 0)
                return result;

            return this.timeStamp.CompareTo(testContainer.timeStamp);
        }

        public IEnumerable<Guid> DebugEngines
        {
            get { return Enumerable.Empty<Guid>(); }
        }

        /// <summary>
        /// Only relevant for WinRT tests
        /// </summary>
        /// <returns>null always</returns>
        public IDeploymentData DeployAppContainer()
        {
            return null;
        }

        public ITestContainerDiscoverer Discoverer
        {
            get { return containerDiscoverer; }
        }

        /// <summary>
        /// Only true if this is a test container for WinRT tests
        /// </summary>
        public bool IsAppContainerTestContainer
        {
            get { return false; }
        }

        public ITestContainer Snapshot()
        {
            return new QmlTestContainer(this);
        }

        public string Source { get; private set; }

        public FrameworkVersion TargetFramework
        {
            get { return FrameworkVersion.None; }
        }

        public Architecture TargetPlatform
        {
            get { return Architecture.AnyCPU; }
        }
    }
}
