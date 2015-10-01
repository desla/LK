namespace Alvasoft.ODTIntegration.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using log4net;

    /// <summary>
    /// Конфигурация литейных конвейеров.
    /// </summary>
    public class CastLinesConfiguration
    {
        private static readonly ILog logger = LogManager.GetLogger("CastLinesConfiguration");

        private const string NODE_CAST_LINE = "cast_line";
        private const string ATTR_CAST_LINE_ID = "number";

        private Dictionary<int, Dictionary<string, string>> castLines 
            = new Dictionary<int, Dictionary<string, string>>();

        /// <summary>
        /// Возвращает все загруженные номеров ЛК.
        /// </summary>
        /// <returns>Массив номеров ЛК.</returns>
        public int[] GetCastLineNumbers()
        {
            return castLines.Keys.ToArray();
        }

        /// <summary>
        /// Возвращает все OPC-теги для указанного номера ЛК.
        /// </summary>
        /// <param name="aCastLineNumber">Номер ЛК.</param>
        /// <returns>Описание тегов.</returns>
        public Dictionary<string, string> GetOpcItems(int aCastLineNumber)
        {
            if (!castLines.ContainsKey(aCastLineNumber)) {
                throw new ArgumentException("Конфигурация не содержит " +
                                            "данных для ЛК с номером " + aCastLineNumber);
            }

            return new Dictionary<string, string>(castLines[aCastLineNumber]);
        }

        /// <summary>
        /// Загружает конфигурацию из XML-файла.
        /// </summary>
        /// <param name="aFileName">Имя файла.</param>
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
                    case NODE_CAST_LINE:
                        LoadCastLine(nodes[i]);
                        break;
                    default:
                        logger.Info("Во время загруги конфигурации ЛК " +
                                    "обнаружен неизвестный тег: " + nodes[i].Name);
                        break;
                }
            }
        }

        /// <summary>
        /// Загружает описание ОРС тегов для конкретного ЛК.
        /// </summary>
        /// <param name="aXmlNode">Нод в XML.</param>
        private void LoadCastLine(XmlNode aXmlNode)
        {
            var castLineNumber = Convert.ToInt32(aXmlNode.Attributes[ATTR_CAST_LINE_ID].Value);

            var castLine = CreateCastLine(castLineNumber);

            var nodes = aXmlNode.ChildNodes;
            for (var i = 0; i < nodes.Count; ++i) {
                castLine[nodes[i].Name] = nodes[i].InnerText;
            }
        }

        /// <summary>
        /// Подготавливает место для описание ОРС тегов конкретного ЛК.
        /// </summary>
        /// <param name="aCastLineNumber">Номер ЛК.</param>
        /// <returns>Словарь с описанием.</returns>
        private Dictionary<string, string> CreateCastLine(int aCastLineNumber)
        {
            if (castLines.ContainsKey(aCastLineNumber)) {
                throw new ArgumentException("Несколько ЛК имеют одинаковые идентификаторы.");
            }

            castLines[aCastLineNumber] = new Dictionary<string, string>();
            return castLines[aCastLineNumber];
        }
    }
}
