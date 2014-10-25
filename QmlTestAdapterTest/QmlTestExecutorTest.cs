using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OktetNET.OQmlTestAdapter;
using Moq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace OktetNET.QmlTestAdapterTest
{
    [TestClass]
    public class QmlTestExecutorTest: TestBase
    {
        private void Execute(string functions, int functionsExitCode, string xml, int xmlExitCode)
        {
            QmlTestRunnerMock.Setup(m => m.Execute(It.IsRegex("-functions"), It.IsAny<IDiscoveryContext>())).Returns(
                new QmlTestRunnerResult("", functions, functionsExitCode));

            QmlTestRunnerMock.Setup(m => m.Execute(It.IsRegex("-xml"), It.IsAny<IDiscoveryContext>())).Returns(
                new QmlTestRunnerResult(xml, "", xmlExitCode));

            TestExecutor.RunTests(new string[] { Path }, null, FrameworkHandleMock.Object);
        }

        [TestMethod]
        public void TestRunTestsSimplePass()
        {
            Execute(FunctionsSimple, 0, XmlSimplePass, 0);

            FrameworkHandleMock.Verify(m => m.RecordResult(It.Is<TestResult>(
                tr =>
                tr.Duration == TimeSpan.FromMilliseconds(0.016133) &&
                tr.Outcome == TestOutcome.Passed
                )), Times.Once);

            FrameworkHandleMock.Verify(m => m.RecordEnd(It.Is<TestCase>(tc =>
                    tc.FullyQualifiedName == "simpletest::test_simple" &&
                    tc.CodeFilePath == Path &&
                    tc.ExecutorUri == QmlTestExecutor.ExecutorUri &&
                    tc.Source == Path &&
                    tc.LineNumber == 0
            ), TestOutcome.Passed), Times.Once);

            FrameworkHandleMock.Verify(m => m.RecordStart(It.Is<TestCase>(tc =>
                    tc.FullyQualifiedName == "simpletest::test_simple" &&
                    tc.CodeFilePath == Path &&
                    tc.ExecutorUri == QmlTestExecutor.ExecutorUri &&
                    tc.Source == Path &&
                    tc.LineNumber == 0
            )), Times.Once);

            FrameworkHandleMock.Verify(m => m.SendMessage(It.IsAny<TestMessageLevel>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public void TestRunTestsSimpleFail()
        {
            Execute(FunctionsSimple, 0, XmlSimpleFail, 0);

            FrameworkHandleMock.Verify(m => m.RecordResult(It.Is<TestResult>(
                tr =>
                tr.Duration == TimeSpan.FromMilliseconds(15.339868) &&
                tr.Outcome == TestOutcome.Failed &&
                tr.ErrorMessage.Equals("2 + 2 = 5" + Environment.NewLine +
                "   Actual   (): 4" + Environment.NewLine +
                "   Expected (): 5" + Environment.NewLine)
                )), Times.Once);

            FrameworkHandleMock.Verify(m => m.RecordStart(It.Is<TestCase>(tc =>
                    tc.FullyQualifiedName == "simpletest::test_simple" &&
                    tc.CodeFilePath == Path &&
                    tc.ExecutorUri == QmlTestExecutor.ExecutorUri &&
                    tc.Source == Path &&
                    tc.LineNumber == 22
            )), Times.Once);

            FrameworkHandleMock.Verify(m => m.RecordEnd(It.Is<TestCase>(tc =>
                    tc.FullyQualifiedName == "simpletest::test_simple" &&
                    tc.CodeFilePath == Path &&
                    tc.ExecutorUri == QmlTestExecutor.ExecutorUri &&
                    tc.Source == Path &&
                    tc.LineNumber == 22
            ), TestOutcome.Failed), Times.Once);

            FrameworkHandleMock.Verify(m => m.SendMessage(It.IsAny<TestMessageLevel>(), It.IsAny<string>()),
                Times.Never());
        }

        [TestMethod]
        public void TestRunTestsErrorFunctions()
        {
            Execute(FunctionsError, 1, XmlError, 0);

            FrameworkHandleMock.Verify(m => m.SendMessage(TestMessageLevel.Error, It.IsAny<string>()),
                Times.Once);
            FrameworkHandleMock.Verify(m => m.RecordStart(It.IsAny<TestCase>()),
                Times.Never);
            FrameworkHandleMock.Verify(m => m.RecordResult(It.IsAny<TestResult>()),
                Times.Never);
            FrameworkHandleMock.Verify(m => m.RecordEnd(It.IsAny<TestCase>(), It.IsAny<TestOutcome>()),
                Times.Never);
        }

        [TestMethod]
        public void TestRunTestsErrorXml()
        {
            Execute(FunctionsSimple, 0, XmlError, 0);

            FrameworkHandleMock.Verify(m => m.RecordStart(It.IsAny<TestCase>()),
                Times.Once);
            FrameworkHandleMock.Verify(m => m.RecordResult(It.IsAny<TestResult>()),
                Times.Never);
            FrameworkHandleMock.Verify(m => m.RecordEnd(It.IsAny<TestCase>(), It.IsAny<TestOutcome>()),
                Times.Never);
            FrameworkHandleMock.Verify(m => m.SendMessage(TestMessageLevel.Error, It.IsAny<string>()),
                Times.Once);
        }

        [TestMethod]
        public void TestRunTestsComplex()
        {
            Execute(FunctionsComplex, 0, XmlComplex, 0);

            FrameworkHandleMock.Verify(m => m.RecordResult(It.IsAny<TestResult>()), 
                Times.Exactly(5));
            FrameworkHandleMock.Verify(m => m.RecordEnd(It.IsAny<TestCase>(), TestOutcome.Failed), 
                Times.Exactly(3));
            FrameworkHandleMock.Verify(m => m.RecordEnd(It.IsAny<TestCase>(), TestOutcome.Passed), 
                Times.Exactly(2));
        }
    }
}
