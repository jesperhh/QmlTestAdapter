using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OktetNET.OQmlTestAdapter
{
    public class QmlTestAdapterSettings : TestRunSettings
    {
        public const string SettingsName = "QmlTestAdapterSettings";

        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(QmlTestAdapterSettings));

        public int TimeoutMilliseconds { get; set; }

        public string QmlTestRunnerPath { get; set; }

        public QmlTestAdapterSettings()
            : base(SettingsName)
        {
            TimeoutMilliseconds = Int32.MaxValue;
            QmlTestRunnerPath = FindQmlTestRunnerPath();
        }

        private string FindQmlTestRunnerPath()
        {
            return Directory.EnumerateFiles(@"C:\Qt\", "qmltestrunner.exe", SearchOption.AllDirectories).First();
        }

        public override XmlElement ToXml()
        {
            StringWriter stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, this);
            string xml = stringWriter.ToString();
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            return document.DocumentElement;
        }

        public static QmlTestAdapterSettings GetSettings(IDiscoveryContext context)
        {
            if (context == null)
                return new QmlTestAdapterSettings();

            QmlTestAdapterSettings settings = context.RunSettings.GetSettings(
                QmlTestAdapterSettings.SettingsName) as QmlTestAdapterSettings;

            return settings ?? new QmlTestAdapterSettings();
        }
    }
}
