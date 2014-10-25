using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace OktetNET.OQmlTestAdapter
{
    [Export(typeof(ISettingsProvider))]
    [Export(typeof(IRunSettingsService))]
    [SettingsName(QmlTestAdapterSettings.SettingsName)]
    public class QmlTestAdapterSettingsService : IRunSettingsService, ISettingsProvider
    {
        private readonly XmlSerializer serializer;
        public QmlTestAdapterSettings Settings { get; private set; }

        public QmlTestAdapterSettingsService()
        {
            Settings = new QmlTestAdapterSettings();
            serializer = new XmlSerializer(typeof(QmlTestAdapterSettings));
        }


        public IXPathNavigable AddRunSettings(
            IXPathNavigable inputRunSettingDocument,
            IRunSettingsConfigurationInfo configurationInfo,
            ILogger log)
        {
            ValidateArg.NotNull(inputRunSettingDocument, "inputRunSettingDocument");

            XPathNavigator navigator = inputRunSettingDocument.CreateNavigator();

            if (navigator.MoveToChild("RunSettings", ""))
            {
                if (!navigator.MoveToChild(QmlTestAdapterSettings.SettingsName, ""))
                {
                    navigator.AppendChild(Settings.ToXml().OuterXml);
                }
            }

            navigator.MoveToRoot();
            return navigator;
        }

        public string Name
        {
            get
            {
                return QmlTestAdapterSettings.SettingsName;
            }
        }

        public void Load(XmlReader reader)
        {
            ValidateArg.NotNull(reader, "reader");

            if (reader.Read() && reader.Name.Equals(Name))
            {
                Settings = serializer.Deserialize(reader) as QmlTestAdapterSettings;
            }
        }
    }
}
