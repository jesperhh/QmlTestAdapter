using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using OktetNET.OQmlTestAdapter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OktetNET.OQmlTestAdapter
{
    public class QmlTestRunner : IQmlTestRunner
    {
        public QmlTestRunnerResult Execute(string parameters, IDiscoveryContext context)
        {
            using (Process p = new Process())
            {
                QmlTestAdapterSettings settings = QmlTestAdapterSettings.GetSettings(context);

                ProcessStartInfo psi = new ProcessStartInfo(settings.QmlTestRunnerPath, parameters);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardError = true;

                p.StartInfo = psi;
                p.Start();
                p.WaitForExit(settings.TimeoutMilliseconds);

                return new QmlTestRunnerResult(p.StandardOutput.ReadToEnd(), p.StandardError.ReadToEnd(), p.ExitCode);
            }
        }
    }
}
