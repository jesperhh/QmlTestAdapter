using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using OktetNET.OQmlTestAdapter;
using Moq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace OktetNET.QmlTestAdapterTest
{
    [TestClass]
    public class QmlTestDiscovererTest: TestBase
    {
        [TestMethod]
        public void TestDiscoverTestsSimple()
        {
            QmlTestRunnerMock.Setup(m => m.Execute(It.IsRegex("-functions"), It.IsAny<IDiscoveryContext>())).Returns(
                new QmlTestRunnerResult("", FunctionsSimple, 0));

            TestDiscoverer.DiscoverTests(new string[] { Path }, null, FrameworkHandleMock.Object, DiscoverySinkMock.Object);

            DiscoverySinkMock.Verify(m => m.SendTestCase(It.Is<TestCase>(tc =>
                    tc.FullyQualifiedName == "simpletest::test_simple" &&
                    tc.CodeFilePath == Path &&
                    tc.ExecutorUri == QmlTestExecutor.ExecutorUri &&
                    tc.Source == Path 
            )), Times.Once);
        }

        [TestMethod]
        public void TestDiscoverTestsComplex()
        {
            QmlTestRunnerMock.Setup(m => m.Execute(It.IsRegex("-functions"), It.IsAny<IDiscoveryContext>())).Returns(
                new QmlTestRunnerResult("", FunctionsComplex, 0));

            TestDiscoverer.DiscoverTests(new string[] { Path }, null, FrameworkHandleMock.Object, DiscoverySinkMock.Object);

            DiscoverySinkMock.Verify(m => m.SendTestCase(It.IsAny<TestCase>()), Times.Exactly(5));
        }

        [TestMethod]
        public void TestDiscoverTestsError()
        {
            QmlTestRunnerMock.Setup(m => m.Execute(It.IsRegex("-functions"), It.IsAny<IDiscoveryContext>())).Returns(
                new QmlTestRunnerResult("", FunctionsError, 1));

            TestDiscoverer.DiscoverTests(new string[] { Path }, null, FrameworkHandleMock.Object, DiscoverySinkMock.Object);

            DiscoverySinkMock.Verify(m => m.SendTestCase(It.IsAny<TestCase>()), Times.Never());

            FrameworkHandleMock.Verify(m => m.SendMessage(TestMessageLevel.Error, It.IsAny<string>()),
                Times.Once());
        }
    }
}
