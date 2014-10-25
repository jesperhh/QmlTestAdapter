using System;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using OktetNET.OQmlTestAdapter;

namespace OktetNET.QmlTestAdapterTest
{
    [TestClass]
    public class TestBase
    {
        protected Mock<IFrameworkHandle> FrameworkHandleMock { get; private set; }
        protected Mock<IQmlTestRunner> QmlTestRunnerMock { get; private set; } 
        protected Mock<ITestCaseDiscoverySink> DiscoverySinkMock { get; private set; }
        protected QmlTestExecutor TestExecutor { get; private set; }
        protected QmlTestDiscoverer TestDiscoverer { get; private set; }

        [TestInitialize()]
        public void TestInitialize()
        {
            FrameworkHandleMock = new Mock<IFrameworkHandle>(MockBehavior.Strict);
            QmlTestRunnerMock = new Mock<IQmlTestRunner>(MockBehavior.Strict);
            DiscoverySinkMock = new Mock<ITestCaseDiscoverySink>(MockBehavior.Strict);

            TestExecutor = new QmlTestExecutor(QmlTestRunnerMock.Object);
            TestDiscoverer = new QmlTestDiscoverer(QmlTestRunnerMock.Object);

            FrameworkHandleMock.Setup(m => m.RecordStart(It.IsAny<TestCase>()));
            FrameworkHandleMock.Setup(m => m.RecordResult(It.IsAny<TestResult>()));
            FrameworkHandleMock.Setup(m => m.RecordEnd(It.IsAny<TestCase>(), It.IsAny<TestOutcome>()));
            FrameworkHandleMock.Setup(m => m.SendMessage(It.IsAny<TestMessageLevel>(), It.IsAny<string>()));
            DiscoverySinkMock.Setup(m => m.SendTestCase(It.IsAny<TestCase>()));
        }

        protected const string Path = 
@"C:\Users\jesper\projects\QmlTestAdapter\tst_test.qml";

        protected const string FunctionsSimple =
@"qml: simpletest::test_simple()";

        protected const string XmlSimplePass =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestCase name=""qmltestrunner"">
<Environment>
    <QtVersion>5.3.2</QtVersion>
    <QTestVersion>5.3.2</QTestVersion>
</Environment>
<TestFunction name=""simpletest::test_simple"">
<Incident type=""pass"" file="""" line=""0"" />
    <Duration msecs=""0.016133""/>
</TestFunction>
<Duration msecs=""77.636613""/>
</TestCase>";

        protected const string XmlSimpleFail =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestCase name=""qmltestrunner"">
<Environment>
    <QtVersion>5.3.2</QtVersion>
    <QTestVersion>5.3.2</QTestVersion>
</Environment>
<TestFunction name=""simpletest::test_simple"">
<Incident type=""fail"" file=""C:\Users\jesper\projects\QmlTestAdapter\tst_test.qml"" line=""22"">
    <Description><![CDATA[2 + 2 = 5
   Actual   (): 4
   Expected (): 5]]></Description>
</Incident>
    <Duration msecs=""15.339868""/>
</TestFunction>
<Duration msecs=""77.636613""/>
</TestCase>";


        protected const string XmlError =
@"file:///C:/Users/jesper/projects/QmlTestAdapter/tst_test.qml:36:5: Expected token `)' 
         }
 
         ^
";

        protected const string FunctionsError =
@"'tst_test.qm' does not exist under 'C:/Users/jesper/projects/QmlTestAdapter'.";


        protected const string FunctionsComplex =
@"qml: tabletest::test_table()
qml: MathTests2::test_fail()
qml: MathTests2::test_math()
qml: MathTests::test_fail()
qml: MathTests::test_math()
";

        protected const string XmlComplex =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestCase name=""qmltestrunner"">
<Environment>
    <QtVersion>5.3.2</QtVersion>
    <QTestVersion>5.3.2</QTestVersion>
</Environment>
<TestFunction name=""tabletest::initTestCase"">
<Incident type=""pass"" file="""" line=""0"" />
    <Duration msecs=""0.016133""/>
</TestFunction>
<TestFunction name=""tabletest::test_table"">
<Incident type=""fail"" file=""C:\Users\jesper\projects\QmlTestAdapter\tst_test.qml"" line=""35"">
    <DataTag><![CDATA[2 + 2 != 3]]></DataTag>
    <Description><![CDATA[Compared values are not the same
   Actual   (): 4
   Expected (): 3]]></Description>
</Incident>
<Incident type=""fail"" file=""C:\Users\jesper\projects\QmlTestAdapter\tst_test.qml"" line=""35"">
    <DataTag><![CDATA[2 + 6 != 7]]></DataTag>
    <Description><![CDATA[Compared values are not the same
   Actual   (): 8
   Expected (): 7]]></Description>
</Incident>
    <Duration msecs=""14.656192""/>
</TestFunction>
<TestFunction name=""tabletest::cleanupTestCase"">
<Incident type=""pass"" file="""" line=""0"" />
    <Duration msecs=""0.014177""/>
</TestFunction>
<TestFunction name=""MathTests2::initTestCase"">
<Incident type=""pass"" file="""" line=""0"" />
    <Duration msecs=""0.010685""/>
</TestFunction>
<TestFunction name=""MathTests2::test_fail"">
<Incident type=""fail"" file=""C:\Users\jesper\projects\QmlTestAdapter\tst_test.qml"" line=""22"">
    <Description><![CDATA[2 + 2 = 5
   Actual   (): 4
   Expected (): 5]]></Description>
</Incident>
    <Duration msecs=""15.339868""/>
</TestFunction>
<TestFunction name=""MathTests2::test_math"">
<Incident type=""pass"" file="""" line=""0"" />
    <Duration msecs=""15.564617""/>
</TestFunction>
<TestFunction name=""MathTests2::cleanupTestCase"">
<Incident type=""pass"" file="""" line=""0"" />
    <Duration msecs=""0.019136""/>
</TestFunction>
<TestFunction name=""MathTests::initTestCase"">
<Incident type=""pass"" file="""" line=""0"" />
    <Duration msecs=""0.011174""/>
</TestFunction>
<TestFunction name=""MathTests::test_fail"">
<Incident type=""fail"" file=""C:\Users\jesper\projects\QmlTestAdapter\tst_test.qml"" line=""12"">
    <Description><![CDATA[2 + 2 = 5
   Actual   (): 4
   Expected (): 5]]></Description>
</Incident>
    <Duration msecs=""15.298592""/>
</TestFunction>
<TestFunction name=""MathTests::test_math"">
<Incident type=""pass"" file="""" line=""0"" />
    <Duration msecs=""15.528509""/>
</TestFunction>
<TestFunction name=""MathTests::cleanupTestCase"">
<Incident type=""pass"" file="""" line=""0"" />
    <Duration msecs=""0.013200""/>
</TestFunction>
<Duration msecs=""77.636613""/>
</TestCase>";
    }
}
