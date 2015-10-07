namespace Alvasoft.ODTIntegration.CastLineIntegration
{
    using System;
    using System.Windows.Forms;
    using ODTIntegaration.ITS;
    using Buffer;    
    using ODTIntegaration.ConnectionHolder;
    using ODTIntegaration.Structures;
    using CastLineConnector;
    using Configuration;
    using ITS;    
    using Alvasoft.Utils.Activity;
    using log4net;

    /// <summary>
    /// Мост для взаимодействия между заводской ИТС и ЛК.
    /// </summary>
    public class CastLineIntegrator : 
        InitializableImpl, 
        ICastLineCallback, 
        IConnectionHolderCallback
    {
        private static readonly ILog logger = LogManager.GetLogger("CastLineIntegrator");

        /// <summary>
        /// Интерфейс для связи с ИТС.
        /// </summary>
        private IIts its;

        /// <summary>
        /// Буфер для временного хранения данных при обрыве связи с ИТС.
        /// </summary>
        private IDataBuffer dataBuffer;

        /// <summary>
        /// Массив коннекторов к ЛК.
        /// </summary>
        private ICastLineConnector[] castLines;

        private OracleConnectionHolder oracleConnectionHolder;
        private OpcConnectionHolder opcConnectionHolder;

        private CastLinesConfiguration castLinesConfig;
        private ConnectionsConfiguration connectionsConfig;

        protected override void DoInitialize()
        {
            try {
                logger.Info("Инициализация...");
                // загрузка коонфигураций.
                var appPath = Application.StartupPath + "/";
                connectionsConfig = new ConnectionsConfiguration();
                connectionsConfig.LoadFromFile(appPath + "Settings/Network.xml");
                castLinesConfig = new CastLinesConfiguration();
                castLinesConfig.LoadConfiguration(appPath + "Settings/CastLines.xml");

                // Инициализация ConnectionHolder'ов.
                oracleConnectionHolder = new OracleConnectionHolder(connectionsConfig.Its);
                oracleConnectionHolder.SetHolderName(ConnectionHolderBase.Oracleholder);
                oracleConnectionHolder.SetCallback(this);
                opcConnectionHolder = new OpcConnectionHolder(connectionsConfig.CastLine);
                opcConnectionHolder.SetHolderName(ConnectionHolderBase.Opcholder);
                opcConnectionHolder.SetCallback(this);

                // Инициализация ИТС.
                its = new ItsImpl();
                its.SetConnectionHoder(oracleConnectionHolder);
                its.Initialize();

                // Инициализация списка ЛК.
                var castLineNumbers = castLinesConfig.GetCastLineNumbers();
                if (castLineNumbers != null && castLineNumbers.Length > 0) {
                    castLines = new ICastLineConnector[castLineNumbers.Length];
                    for (var i = 0; i < castLineNumbers.Length; ++i) {                        
                        castLines[i] = new CastLineConnectorImpl();
                        var opcItems = castLinesConfig.GetOpcItems(castLineNumbers[i]);
                        castLines[i].SetCastLineNumber(castLineNumbers[i]);
                        castLines[i].SetOpcTagsList(opcItems);                        
                        castLines[i].SetOpcConnectionHolder(opcConnectionHolder);
                        castLines[i].SetCastLineCallback(this);
                    }
                }

                // Инициализация буфера.
                dataBuffer = new MemoryBufferImpl();                
        
                // Открытие соединений.
                opcConnectionHolder.OpenConnection();
                oracleConnectionHolder.OpenConnection();                
                logger.Info("Инициализация завершена.");
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время инициализации: " + ex.Message);
            }
        }

        protected override void DoUninitialize()
        {
            logger.Info("Деинициализация...");
            try {
                opcConnectionHolder.TryCloseConnection();
                oracleConnectionHolder.TryCloseConnection();
                foreach (var castLine in castLines) {
                    castLine.Uninitialize();
                }
            }
            catch (Exception ex) {
                logger.Error("Ошибка при деинициализации: " + ex.Message);
            }
            finally {
                logger.Info("Деинициализация завершена.");
            }
        }

        /// <summary>
        /// Реалилзация интерфейса IcastLineCallback.
        /// Происходит при получении запроса на новую карту плавки.
        /// </summary>
        /// <param name="aConnector">ЛК.</param>
        /// <param name="aFurnaceNumber">Номер миксера для передачи в ИТС.</param>
        public void OnCastRequest(ICastLineConnector aConnector, int aFurnaceNumber)
        {
            var castLineNumber = aConnector.GetCastLineNumber();
            logger.Info(string.Format("Получен запрос от ЛК №{0} на карту плавки.", castLineNumber));

            CastPlan castPlan = null;
            try {
                logger.Info(string.Format("Выполняем запрос у ИТС карты плавки для ЛК №{0}.", castLineNumber));
                castPlan = its.GetCastPlat(aFurnaceNumber);
                if (castPlan == null) {
                    logger.Info("Карта плавки не получена.");
                    return;
                }

                logger.Info(string.Format("Карта плавки получена для ЛК №{0}.", castLineNumber.ToString()));
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время запроса карты плавки у ИТС: " + ex.Message);
                logger.Info(string.Format("Ошибка получения карты плавки для ЛК №{0}.", castLineNumber));
                return;
            }

            logger.Info(string.Format("Записываем карту плавки в ЛК №{0}...", castLineNumber));
            logger.Info(aConnector.TryWriteCastPlan(castPlan)
                ? string.Format("Карта плавки записана успешно в ЛК №{0}.", castLineNumber)
                : string.Format("Запись карты плавки в ЛК №{0} не удалась.", castLineNumber));
        }

        /// <summary>
        /// Реализация ICastLineCallback.
        /// Происходит при получении информации о единице готовой продукции.
        /// </summary>
        /// <param name="aConnector">ЛК.</param>
        /// <param name="aPocket">Единица готовой продукции.</param>
        public void OnFinishedProductAppeared(ICastLineConnector aConnector, FinishedProduct aPocket)
        {
            var castLineNumber = aConnector.GetCastLineNumber();
            logger.Info(string.Format("Получены данные о новой ЕГП от ЛК № {0}. {1}.",
                castLineNumber, aPocket.ToString()));
            logger.Info("Передача данный ЕГП в ИТС...");
            if (its.TryAddFinishedProduct(aPocket)) {
                logger.Info(string.Format("Данные о ЕГП от ЛК №{0} успешно переданы в ИТС.", castLineNumber));
            }
            else {
                logger.Info(string.Format("Не удалось переданные ЕГП от ЛК №{0} в ИТС. Сохраняем в буфер.", castLineNumber));
                dataBuffer.AddFinishedProduct(aPocket);
            }            
        }

        /// <summary>
        /// Реализация IConnectionHolderCallback.
        /// Возникает во время подключения.
        /// </summary>
        /// <param name="aConnection">ConnectinHolder.</param>
        public void OnConnected(ConnectionHolderBase aConnection)
        {
            logger.Info(string.Format("Подключен {0}", aConnection.GetHolderName()));
            switch (aConnection.GetHolderName()) {
                case ConnectionHolderBase.Opcholder:
                    for (var i = 0; i < castLines.Length; ++i) {
                        if (!castLines[i].IsInitialized()) {
                            castLines[i].Initialize();
                        }                        
                    }
                    break;
                case ConnectionHolderBase.Oracleholder:
                    if (!its.IsInitialized()) {
                        its.Initialize();
                    }
                    if (!dataBuffer.IsEmpty()) {
                        TrySentBufferedData();
                    }
                    break;
            }
        }        

        /// <summary>
        /// Реализация IConnectionHolderCallback.
        /// Возникает во время отключения.
        /// </summary>
        /// <param name="aConnection">ConnectionHolder.</param>
        public void OnDisconnected(ConnectionHolderBase aConnection)
        {         
            logger.Info(string.Format("Отключен {0}", aConnection.GetHolderName()));
        }

        /// <summary>
        /// Реализация IConnectionHolderCallback.
        /// Возникает во время ошибка.
        /// </summary>
        /// <param name="aConnection">ConnectionHolder.</param>
        /// <param name="aError">Ошибка.</param>
        public void OnError(ConnectionHolderBase aConnection, Exception aError)
        {         
            logger.Info(string.Format("Ошибка в {0}: {1}.", aConnection.GetHolderName(), aError.Message));
            aConnection.TryCloseConnection();
        }

        private void TrySentBufferedData()
        {
            try {
                var storedData = dataBuffer.GetStoredProductsOrDefault();
                logger.Info("Передача данных из буфера в ИТС... Данных для передачи: " + storedData.Length);
                if (its.TryAddFinishedProducts(storedData)) {
                    logger.Info("Данные успешно переданы в ИТС.");
                    dataBuffer.Clear();
                }
                else {
                    logger.Info("Не удалось передать данные в ИТС.");
                }
            }
            catch (Exception ex) {
                logger.Error("Ошибка при сохранении в ИТС: " + ex.Message);
            }
        }
    }
}
