namespace Alvasoft.ODTIntegration.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Structures;
    using log4net;

    /// <summary>
    /// Конфигурация параметров, передаваемых постоянно.
    /// </summary>
    public class CurrentValuesConfiguration
    {
        private static readonly ILog logger = LogManager.GetLogger("CastLinesConfiguration");

        private const string NODE_READ_INTERVAL = "readInterval";        
        private const string NODE_ITEM = "item";
        private const string NODE_OPC_ITEM_NAME = "opcItemName";
        private const string NODE_TYPE_NAME = "typeName";
        private const string NODE_OBJECT_NAME = "objectName";
        private const string NODE_DATA_NAME = "dataName";

        private List<CurrentValueInfo> currentValues = new List<CurrentValueInfo>();

        /// <summary>
        /// Интервал времени для чтения данных и передачи в ИТС.
        /// </summary>
        public double ReadInterval { get; private set; }

        /// <summary>
        /// Возвращает список всех описаний текущих значений.
        /// </summary>
        /// <returns>Список описаний.</returns>
        public CurrentValueInfo[] GetCurrentValues()
        {
            return currentValues.ToArray();
        }

        /// <summary>
        /// Возвращает количество описаний данных.
        /// </summary>
        /// <returns>Количество описаний.</returns>
        public int GetCurrentValuesCount()
        {
            return currentValues.Count;
        }

        /// <summary>
        /// Загружает конфигурацию в память.
        /// </summary>
        /// <param name="aFileName">Имя файла конфигурации.</param>
        public void LoadConfiguration(string aFileName)
        {
            if (string.IsNullOrEmpty(aFileName)) {
                throw new ArgumentNullException("aFileName");
            }

            var document = new XmlDocument();
            document.Load(aFileName);

            var root = document.DocumentElement;
            var nodes = root.ChildNodes;
            for (var i = 0; i < nodes.Count; ++i) {
                switch (nodes[i].Name) {
                    case NODE_READ_INTERVAL:
                        ReadInterval = Convert.ToDouble(nodes[i].InnerText);
                        break;
                    case NODE_ITEM:
                        LoadCurrentValueDescription(nodes[i]);
                        break;
                }
            }
        }

        private void LoadCurrentValueDescription(XmlNode aNode)
        {
            var currentValue = new CurrentValueInfo();
            var nodes = aNode.ChildNodes;
            for (var i = 0; i < nodes.Count; ++i) {
                switch (nodes[i].Name) {
                    case NODE_OPC_ITEM_NAME:
                        currentValue.OpcItemName = nodes[i].InnerText;
                        break;
                    case NODE_DATA_NAME:
                        currentValue.DataName = nodes[i].InnerText;
                        break;
                    case NODE_OBJECT_NAME:
                        currentValue.ObjectName = nodes[i].InnerText;
                        break;
                    case NODE_TYPE_NAME:
                        currentValue.TypeName = nodes[i].InnerText;
                        break;
                }
            }

            currentValues.Add(currentValue);
        }
    }
}
