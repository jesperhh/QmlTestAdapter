using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace OktetNET.OQmlTestAdapter
{
    internal class QmlTestRunnerWrapper
    {
        internal static IEnumerable<TestCase> GetTests(
            IQmlTestRunner qmlTestRunner, 
            string filePath,
            IMessageLogger logger,
            IDiscoveryContext context)
        {
            try
            {
                QmlTestRunnerResult result = qmlTestRunner.Execute("-functions -input " + filePath, 
                    context);

                if (result.ExitCode != 0)
                    logger.SendMessage(TestMessageLevel.Error, result.StandardError);

                return ParseQmlTestRunnerFunctionsOutput(filePath, result.StandardError);
            }
            catch (Exception ex)
            {
                logger.SendMessage(TestMessageLevel.Error, ex.StackTrace);
            }

            return new List<TestCase>();
        }

        internal static void RunTests(
            IQmlTestRunner qmlTestRunner, 
            string source, 
            IEnumerable<TestCase> testCases,
            IFrameworkHandle frameworkHandle,
            IDiscoveryContext context)
        {
            try
            {
                foreach (TestCase testCase in testCases)
                {
                    frameworkHandle.RecordStart(testCase);
                }

                Dictionary<string, TestCase> dict = testCases.ToDictionary(tc => tc.FullyQualifiedName);

                string functions = String.Join(" ", testCases.Select(tc => "\"" + tc.FullyQualifiedName + "\""));
                string arguments = "-xml -input " + source + " " + functions;

                QmlTestRunnerResult result = qmlTestRunner.Execute(arguments, context);
                ParseQmlTestRunnerXmlOutput(frameworkHandle, dict, result.StandardOutput);
            }
            catch (Exception ex)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error, ex.StackTrace);
            }
        }

        internal static void ParseQmlTestRunnerXmlOutput(IFrameworkHandle frameworkHandle, Dictionary<string, TestCase> dict, string stdout)
        {
            using (StringReader reader = new StringReader(stdout))
            {
                XPathDocument xml = new XPathDocument(reader);
                foreach (XPathNavigator testFunction in xml.CreateNavigator().Select("/TestCase/TestFunction"))
                {
                    string name = testFunction.GetAttribute("name", "");
                    if (!dict.ContainsKey(name))
                        continue;

                    TestResult testResult = new TestResult(dict[name]);
                    testResult.Duration = TimeSpan.FromMilliseconds(testFunction.SelectSingleNode("Duration/@msecs").ValueAsDouble);
                    testResult.Outcome = TestOutcome.Passed;
                    foreach (XPathNavigator incident in testFunction.Select("Incident"))
                    {
                        string outcome = incident.GetAttribute("type", "");
                        testResult.TestCase.LineNumber = int.Parse(incident.GetAttribute("line", ""));
                        if (!outcome.Equals("pass", StringComparison.InvariantCulture))
                        {
                            testResult.Outcome = TestOutcome.Failed;
                            XPathNavigator description = incident.SelectSingleNode("Description/text()");
                            if (description != null)
                            {
                                testResult.ErrorMessage += description.ToString() + Environment.NewLine;
                            }
                        }
                    }

                    frameworkHandle.RecordResult(testResult);
                    frameworkHandle.RecordEnd(testResult.TestCase, testResult.Outcome);
                }
            }
        }

        internal static IEnumerable<TestCase> ParseQmlTestRunnerFunctionsOutput(string filePath, string stderr)
        {
            return Regex.Matches(stderr, "^qml: (.*)::(.*)\\(\\)\r?$", RegexOptions.Multiline).Cast<Match>().Select(
                match =>
                {
                    string name = match.Groups[1] + "::" + match.Groups[2];
                    TestCase testCase = new TestCase(name, QmlTestExecutor.ExecutorUri, filePath);
                    testCase.CodeFilePath = filePath;
                    return testCase;
                }
                );
        }
    }
}
