using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OktetNET.OQmlTestAdapter
{
    public class QmlTestRunnerResult
    {
        public int ExitCode { get; private set; }
        public string StandardOutput { get; private set; }
        public string StandardError { get; private set; }

        public QmlTestRunnerResult(string standardOutput, string standardError, int exitCode)
        {
            ExitCode = exitCode;
            StandardError = standardError;
            StandardOutput = standardOutput;
        }
    }

    public interface IQmlTestRunner
    {
        QmlTestRunnerResult Execute(string parameters, IDiscoveryContext runContext);
    }
}
