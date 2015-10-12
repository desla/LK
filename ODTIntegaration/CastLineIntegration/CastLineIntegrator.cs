namespace Alvasoft.ODTIntegration.CastLineIntegration
{
    using System;
    using System.Windows.Forms;
    using ITS;
    using Buffer;    
    using ConnectionHolders;
    using Structures;
    using CastLineConnector;
    using Configuration;    
    using Alvasoft.Utils.Activity;
    using log4net;
    using OPCAutomation;
    using Oracle.ManagedDataAccess.Client;

    /// <summary>
    /// Мост для взаимодействия между заводской ИТС и ЛК.
    /// </summary>
    public class CastLineIntegrator : 
        InitializableImpl, 
        ICastLineCallback, 
        ICurrentValueReaderCallback,
        IConnectionHolderCallback<OracleConnection>,
        IConnectionHolderCallback<OPCServer>
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

        /// <summary>
        /// Для чтения текущих показателей.
        /// </summary>
        private ICurrentValueReader currentValuesReader;

        private ICurrentValuesPreparer currentValuesPreparer;

        private OracleConnectionHolder oracleConnectionHolder;
        private OpcConnectionHolder opcConnectionHolder;

        private CastLinesConfiguration castLinesConfig;
        private ConnectionsConfiguration connectionsConfig;
        private CurrentValuesConfiguration currentValuesConfig;

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
                currentValuesConfig = new CurrentValuesConfiguration();
                currentValuesConfig.LoadConfiguration(appPath + "Settings/CurrentValues.xml");

                // Инициализация ConnectionHolder'ов.
                oracleConnectionHolder = new OracleConnectionHolder(
                    connectionsConfig.Its.Host,
                    connectionsConfig.Its.User, 
                    connectionsConfig.Its.Password);
                oracleConnectionHolder.SetReconnectionInterval(connectionsConfig.Its.ReconnectionInterval);
                oracleConnectionHolder.SetHolderName("Oracle");
                oracleConnectionHolder.Subscribe(this);

                opcConnectionHolder = new OpcConnectionHolder(
                    connectionsConfig.CastLine.ServerName, 
                    connectionsConfig.CastLine.Host);
                opcConnectionHolder.SetHolderName("OPC");
                opcConnectionHolder.Subscribe(this);
                opcConnectionHolder.SetReconnectionInterval(connectionsConfig.CastLine.ReconnectionInterval);                

                // Создание читателя текущих значений.
                currentValuesReader = new CurrentValueReaderImpl();
                currentValuesReader.SetCallback(this);
                currentValuesReader.SetConfiguration(currentValuesConfig);
                currentValuesReader.SetConnectionHolder(opcConnectionHolder);                

                // Создание подготовителя текущих данных.
                currentValuesPreparer = new CurrentValuesPreparerImpl();
                currentValuesPreparer.SetConnectionHoder(oracleConnectionHolder);
                currentValuesPreparer.SetCurrentValues(currentValuesConfig.GetCurrentValues());

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
                opcConnectionHolder.TryConnect();
                oracleConnectionHolder.TryConnect();
                opcConnectionHolder.Start();
                oracleConnectionHolder.Start();                
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
                opcConnectionHolder.Stop();
                oracleConnectionHolder.Stop();
                opcConnectionHolder.Dispose();
                oracleConnectionHolder.Dispose();
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
        /// Реализация ICurrentValueReaderCallback.
        /// Возникает при появлении новых данных.
        /// </summary>
        /// <param name="aReader">Текущий читатель.</param>
        /// <param name="aCurrentValues">Данные.</param>
        public void OnCurrentValueReceived(ICurrentValueReader aReader, CurrentValue[] aCurrentValues)
        {
            logger.Info("Получены текущие параметры. Всего значений: " + aCurrentValues.Length);
            logger.Info("Подготавливаем данные для передачи в ИТС...");
            if (currentValuesPreparer.TryPrepareValues(aCurrentValues)) {
                logger.Info("Данные подготовлены успешно.");
                logger.Info("Передача данных в ИТС...");
                if (its.TryAddCurrentValues(aCurrentValues)) {
                    logger.Info("Данные успешно переданы в ИТС.");
                }
                else {
                    logger.Info("Добавление данных в ИТС не удалось. Сохраняем в буфер.");
                    dataBuffer.AddCurrentValues(aCurrentValues);
                }
            }
        }  

        /// <summary>
        /// Реализует интерфейс IConnectionHolderCallback.
        /// Возникает при подключении к БД ИТС.
        /// </summary>
        /// <param name="aConnectionHolder"></param>
        public void OnConnected(IConnectionHolder<OracleConnection> aConnectionHolder)
        {
            logger.Info(string.Format("Подключен {0}", aConnectionHolder.GetHolderName()));
            if (!its.IsInitialized()) {
                its.Initialize();
            }
            if (!currentValuesPreparer.IsInitialized()) {
                currentValuesPreparer.Initialize();
            }
            TrySentBufferedData();
        }

        /// <summary>
        /// Реализует интерфейс IConnectionHolderCallback.
        /// Возникает при отключении от БД ИТС.
        /// </summary>
        /// <param name="aConnectionHolder"></param>
        public void OnDisconnected(IConnectionHolder<OracleConnection> aConnectionHolder)
        {
            logger.Info(string.Format("Отключен {0}", aConnectionHolder.GetHolderName()));
        }

        /// <summary>
        /// Реализует интерфейс IConnectionHolderCallback.
        /// Возникает при подключении к ЛК.
        /// </summary>
        /// <param name="aConnectionHolder"></param>
        public void OnConnected(IConnectionHolder<OPCServer> aConnectionHolder)
        {
            logger.Info(string.Format("Подключен {0}", aConnectionHolder.GetHolderName()));
            try {
                for (var i = 0; i < castLines.Length; ++i) {
                    if (!castLines[i].IsInitialized()) {
                        castLines[i].Initialize();
                    }
                }
                if (!currentValuesReader.IsInitialized()) {
                    currentValuesReader.Initialize();
                }                
                currentValuesReader.Start();
            }
            catch (Exception ex) {
                logger.Error("Ошибка opc onConnected: " + ex.Message);
            }
        }

        /// <summary>
        /// Реализует интерфейс IConnectionHolderCallback.
        /// Возникает при отключении от ЛК.
        /// </summary>
        /// <param name="aConnectionHolder"></param>
        public void OnDisconnected(IConnectionHolder<OPCServer> aConnectionHolder)
        {
            logger.Info(string.Format("Отключен {0}", aConnectionHolder.GetHolderName()));
            try {
                currentValuesReader.Stop();
                //currentValuesReader.Uninitialize();
                //foreach (var castLine in castLines) {
                //    castLine.Uninitialize();
                //}                
                try {
                    var server = opcConnectionHolder.WaitConnection();
                    server.Disconnect();
                }
                finally {
                    opcConnectionHolder.ReleaseConnection();
                }
            }
            catch (Exception ex) {
                logger.Error("Ошибка opc onDisconnected: " + ex.Message);
            }
        }

        private void TrySentBufferedData()
        {
            try {
                if (dataBuffer.IsEmpty()) {
                    return;
                }

                var finishedProducts = dataBuffer.GetStoredProductsOrDefault();
                if (finishedProducts != null) {
                    logger.Info("Передача данных ЕГП из буфера в ИТС... Данных для передачи: " + finishedProducts.Length);
                    if (its.TryAddFinishedProducts(finishedProducts)) {
                        logger.Info("Данные успешно переданы в ИТС.");
                        dataBuffer.ClearFinishedProducts();
                    }
                    else {
                        logger.Info("Не удалось передать данные в ИТС.");
                    }
                }

                var currentValues = dataBuffer.GetStoredCurrentValuesOrDefault();
                if (currentValues != null) {
                    logger.Info("Передача данных теущих параметров из буфера в ИТС... Данных для передачи: " + currentValues.Length);
                    if (its.TryAddCurrentValues(currentValues)) {
                        logger.Info("Данные успешно переданы в ИТС.");
                        dataBuffer.ClearCurrentValues();
                    }
                    else {
                        logger.Info("Не удалось передать данные в ИТС.");
                    }
                }
            }
            catch (Exception ex) {
                logger.Error("Ошибка при сохранении в ИТС: " + ex.Message);
            }
        }
    }
}
