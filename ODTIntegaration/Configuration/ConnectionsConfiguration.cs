namespace Alvasoft.ODTIntegration.Configuration
{
    using System;
    using System.Xml;
    using log4net;

    public class ConnectionsConfiguration
    {
        private static readonly ILog logger = LogManager.GetLogger("ConnectionsConfiguration");

        private const string NODE_ITS = "ITS_Oracle";
        private const string NODE_OPC = "Cast_Lines_Opc";

        public ConnectionConfiguration Its { get; private set; }

        public ConnectionConfiguration CastLine { get; private set; }

        public void LoadFromFile(string aFileName)
        {
            if (string.IsNullOrEmpty(aFileName)) {
                throw new ArgumentNullException("aFileName");
            }

            var document = new XmlDocument();
            document.Load(aFileName);

            Its = new ConnectionConfiguration();
            CastLine = new ConnectionConfiguration();

            var root = document.DocumentElement;            
            var nodes = root.ChildNodes;
            for (var i = 0; i < nodes.Count; ++i) {
                switch (nodes[i].Name) {
                    case NODE_ITS:
                        Its.LoadFromXmlNode(nodes[i]);
                        break;
                    case NODE_OPC:
                        CastLine.LoadFromXmlNode(nodes[i]);
                        break;
                    default:
                        logger.Info("При загрузке сетевой конфигурации " +
                                    "найден неизвестный тег: " + nodes[i].Name);
                        break;
                }
            }
        }
    }
}
