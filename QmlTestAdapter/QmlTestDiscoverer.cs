using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OktetNET.OQmlTestAdapter
{
	[DefaultExecutorUri(QmlTestExecutor.ExecutorUriString)]
    [FileExtension(".qml")]
    public class QmlTestDiscoverer: ITestDiscoverer
    {
        IQmlTestRunner qmlTestRunner;

        public QmlTestDiscoverer()
        {
            qmlTestRunner = new QmlTestRunner();
        }

        public QmlTestDiscoverer(IQmlTestRunner qmlTestRunner)
        {
            this.qmlTestRunner = qmlTestRunner;
        }

        public void DiscoverTests(
            IEnumerable<string> sources, 
            IDiscoveryContext discoveryContext, 
            IMessageLogger logger, 
            ITestCaseDiscoverySink discoverySink)
        {
            foreach (TestCase test in GetTests(qmlTestRunner, sources, discoveryContext, logger))
                discoverySink.SendTestCase(test);
        }

        internal static IEnumerable<TestCase> GetTests(
            IQmlTestRunner qmlTestRunner,
            IEnumerable<string> sourceFiles, 
            IDiscoveryContext discoveryContext,
            IMessageLogger logger)
        {
            ConcurrentBag<TestCase> tests = new ConcurrentBag<TestCase>();

            Parallel.ForEach(sourceFiles, s =>
            {
                foreach (TestCase testCase in QmlTestRunnerWrapper.GetTests(qmlTestRunner, s, logger, discoveryContext))
                {
                    tests.Add(testCase);
                }
            });

            return tests;
        }
    }
}
