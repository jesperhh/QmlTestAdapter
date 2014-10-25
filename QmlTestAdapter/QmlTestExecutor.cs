using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OktetNET.OQmlTestAdapter
{
    [ExtensionUri(QmlTestExecutor.ExecutorUriString)]
    public class QmlTestExecutor: ITestExecutor
    {
        public const string ExecutorUriString = "executor://qmlexecutor/v1";
        public static readonly Uri ExecutorUri = new Uri(QmlTestExecutor.ExecutorUriString);

        private bool cancel = false;

        IQmlTestRunner qmlTestRunner;

        public QmlTestExecutor()
        {
            qmlTestRunner = new QmlTestRunner();
        }

        public QmlTestExecutor(IQmlTestRunner qmlTestRunner)
        {
            this.qmlTestRunner = qmlTestRunner;
        }

        public void Cancel()
        {
            cancel = true;
        }

        public void RunTests(
            IEnumerable<string> sources,
            IRunContext context,
            IFrameworkHandle frameworkHandle)
        {
            IEnumerable<TestCase> testCases = QmlTestDiscoverer.GetTests(qmlTestRunner, sources, context, frameworkHandle);
            RunTests(testCases, context, frameworkHandle);
        }

        public void RunTests(
            IEnumerable<TestCase> tests,
            IRunContext context,
            IFrameworkHandle frameworkHandle)
        {
            Parallel.ForEach(tests.GroupBy(t => t.Source), group =>
                {
                    if (cancel)
                        return;

                    QmlTestRunnerWrapper.RunTests(qmlTestRunner, group.Key, group, frameworkHandle, context);
                }
            );
        }
    }
}
