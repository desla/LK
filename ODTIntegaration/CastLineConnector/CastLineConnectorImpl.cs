namespace Alvasoft.ODTIntegration.CastLineConnector
{
    using System;    
    using System.Collections.Generic;
    using ODTIntegaration.ConnectionHolder;
    using ODTIntegaration.Structures;
    using OpcTagAccessProvider;
    using Alvasoft.Utils.Activity;
    using Utils.Extensions;    
    using log4net;

    /// <summary>
    /// Реализация конектора к ЛК.
    /// </summary>
    public class CastLineConnectorImpl : 
        InitializableImpl, 
        ICastLineConnector, 
        IOpcValueListener
    {
        private static readonly ILog logger = LogManager.GetLogger("CastLineConnectorImpl");

        private const string DB601_NEW_BATCH_REQUEST = "DB601_NEW_BATCH_REQUEST";
        private const string DB601_FURNACE_NUMBER = "DB601_FURNACE_NUMBER";

        private const string DB600_NEW_BATCH_RECEIVED = "DB600_NEW_BATCH_RECEIVED";
        private const string DB600_FURNACE_NUM = "DB600_FURNACE_NUM";
        private const string DB600_CAST_NUM = "DB600_CAST_NUM";
        private const string DB600_MELT_ID = "DB600_MELT_ID";
        private const string DB600_PRODUCT_NAME = "DB600_PRODUCT_NAME";

        private const string DB620_DATA_READY = "DB620_DATA_READY";
        private const string DB620_WEIGHT = "DB620_WEIGHT";
        private const string DB620_ITEM_NO = "DB620_ITEM_NO";
        private const string DB620_CAST_NUM = "DB620_CAST_NUM";
        private const string DB620_FURNACE_NUM = "DB620_FURNACE_NUM";
        private const string DB620_MELT_ID = "DB620_MELT_ID";

        private Dictionary<string, OpcValueImpl> tag = new Dictionary<string, OpcValueImpl>();

        /// <summary>
        /// Обратная связь для оповещения о новых данных.
        /// </summary>
        private ICastLineCallback callback;

        /// <summary>
        /// Список OPC-тегов.
        /// </summary>
        private Dictionary<string, string> opcTagsList;

        /// <summary>
        /// Держатель соединения ОРС.
        /// </summary>
        private OpcConnectionHolder connectionHolder;

        /// <summary>
        /// Номер ЛК.
        /// </summary>
        private int castLineNumber = -1;

        public void SetCastLineCallback(ICastLineCallback aCallback)
        {
            callback = aCallback;
        }

        public void SetOpcTagsList(Dictionary<string, string> aOpcTagsList)
        {
            opcTagsList = aOpcTagsList;
        }

        public void SetOpcConnectionHolder(OpcConnectionHolder aConnectionHolder)
        {
            connectionHolder = aConnectionHolder;
        }

        public int GetCastLineNumber()
        {
            return castLineNumber; 
        }

        public void SetCastLineNumber(int aCastLineNumber)
        {
            castLineNumber = aCastLineNumber;
        }

        public bool TryWriteCastPlan(CastPlan aCastPlan)
        {
            logger.Info(string.Format("Запись карты плавки в контроллер ЛК №{0}: {1}...", 
                castLineNumber, aCastPlan));
            try {
                tag[DB600_CAST_NUM].WriteValue(aCastPlan.CastNumber);
                tag[DB600_FURNACE_NUM].WriteValue(aCastPlan.FurnaceNumber);
                tag[DB600_MELT_ID].WriteValue(aCastPlan.MeltId);
                tag[DB600_PRODUCT_NAME].WriteValue(aCastPlan.ProductName.ToArialCyrilic());
                tag[DB600_NEW_BATCH_RECEIVED].WriteValue(true);

                logger.Info("Запись карты плавки завершена.");

                connectionHolder.UpdateLastOperationTime();
                return true;
            }
            catch (Exception ex) {
                logger.Error(string.Format("Ошибка при записи карты плавки " +
                                           "в ЛК №{0}: {1}", castLineNumber, ex.Message));
                return false;
            }
        }
        
        protected override void DoInitialize()
        {
            logger.Info("Инициализация...");
            if (!connectionHolder.IsConnected()) {
                throw new ArgumentException("ОРС соединение еще не установлено.");
            }

            var opcServer = connectionHolder.GetOpcServer();

            tag[DB601_NEW_BATCH_REQUEST] = new OpcValueImpl(opcServer, opcTagsList[DB601_NEW_BATCH_REQUEST]);
            tag[DB601_FURNACE_NUMBER] = new OpcValueImpl(opcServer, opcTagsList[DB601_FURNACE_NUMBER]);

            tag[DB600_NEW_BATCH_RECEIVED] = new OpcValueImpl(opcServer, opcTagsList[DB600_NEW_BATCH_RECEIVED]);
            tag[DB600_FURNACE_NUM] = new OpcValueImpl(opcServer, opcTagsList[DB600_FURNACE_NUM]);
            tag[DB600_CAST_NUM] = new OpcValueImpl(opcServer, opcTagsList[DB600_CAST_NUM]);
            tag[DB600_MELT_ID] = new OpcValueImpl(opcServer, opcTagsList[DB600_MELT_ID]);
            tag[DB600_PRODUCT_NAME] = new OpcValueImpl(opcServer, opcTagsList[DB600_PRODUCT_NAME]);

            tag[DB620_DATA_READY] = new OpcValueImpl(opcServer, opcTagsList[DB620_DATA_READY]);
            tag[DB620_WEIGHT] = new OpcValueImpl(opcServer, opcTagsList[DB620_WEIGHT]);
            tag[DB620_ITEM_NO] = new OpcValueImpl(opcServer, opcTagsList[DB620_ITEM_NO]);
            tag[DB620_CAST_NUM] = new OpcValueImpl(opcServer, opcTagsList[DB620_CAST_NUM]);
            tag[DB620_FURNACE_NUM] = new OpcValueImpl(opcServer, opcTagsList[DB620_FURNACE_NUM]);
            tag[DB620_MELT_ID] = new OpcValueImpl(opcServer, opcTagsList[DB620_MELT_ID]);            
            
            foreach (var opcValueImpl in tag.Values) {
                // два тега активируем после всех, чтобы не пропустить данные,
                // так как будет ошибка, если они будут активированы впред всех.
                if (!opcValueImpl.Name.Equals(opcTagsList[DB601_NEW_BATCH_REQUEST]) &&
                    !opcValueImpl.Name.Equals(opcTagsList[DB620_DATA_READY])) {
                        opcValueImpl.Activate();
                }                
            }

            tag[DB601_NEW_BATCH_REQUEST].IsListenValueChanging = true;            
            tag[DB601_NEW_BATCH_REQUEST].SubscribeToValueChange(this);
            tag[DB620_DATA_READY].IsListenValueChanging = true;
            tag[DB620_DATA_READY].SubscribeToValueChange(this);

            tag[DB601_NEW_BATCH_REQUEST].Activate();
            tag[DB620_DATA_READY].Activate();

            logger.Info("Инициализация завершена.");
        }

        protected override void DoUninitialize()
        {
            logger.Info("Деинициализация...");
            try {
                foreach (var opcTag in tag.Values) {
                    opcTag.Deactivate();
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
        /// Срабатывает при изменении значения тега.
        /// </summary>
        /// <param name="aOpcValue">Тег ОРС.</param>
        /// <param name="aValueChangedEventArgs">Параметры.</param>
        public void OnValueChanged(IOpcValue aOpcValue, OpcValueChangedEventArgs aValueChangedEventArgs)
        {
            connectionHolder.UpdateLastOperationTime();
            if (aOpcValue.Name.Equals(opcTagsList[DB601_NEW_BATCH_REQUEST])) {
                if (Convert.ToBoolean(aValueChangedEventArgs.Value)) {
                    CastPlanRequest();
                    TryResetCastPlanRequest();
                }
            }
            else if (aOpcValue.Name.Equals(opcTagsList[DB620_DATA_READY])) {
                if (Convert.ToBoolean(aValueChangedEventArgs.Value)) {
                    FinishedProductIsReady();
                    TryResetFinishedProduct();
                }
            }
            else {
                logger.Error("Неизвестный ОРС-тег: " + aOpcValue.Name);
            }
        }

        /// <summary>
        /// Обнуляет информацию о ЕГП.
        /// </summary>
        private void TryResetFinishedProduct()
        {
            try {
                tag[DB620_DATA_READY].WriteValue(false);
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время обнуления информации ЕГП: " + ex.Message);
            }
        }

        /// <summary>
        /// Отправляет в callbcak информацию о ЕГП.
        /// </summary>
        private void FinishedProductIsReady()
        {
            logger.Info("Появились данные ЕГП.");
            FinishedProduct pocket;
            if (!TryGetFinishedProduct(out pocket)) {
                logger.Info("Во время получения данных ЕГП произошла ошибка.");
                return;
            }

            logger.Info("Конечный продукт: " + pocket.ToString());
            try {
                if (callback != null) {
                    callback.OnFinishedProductAppeared(this, pocket);
                }
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время передачи данных ЕГП в Callback: " + ex.Message);
            }
        }

        /// <summary>
        /// Пытается получить информацию о ЕГП.
        /// </summary>
        /// <param name="aPocket">ЕГП.</param>
        /// <returns>True - если получение успешно, false - иначе.</returns>
        private bool TryGetFinishedProduct(out FinishedProduct aPocket)
        {
            try {
                aPocket = new FinishedProduct();
                aPocket.FurnaceNumber = Convert.ToInt32(tag[DB620_FURNACE_NUM].ReadCurrentValue());
                aPocket.CastNumber = Convert.ToInt32(tag[DB620_CAST_NUM].ReadCurrentValue());
                aPocket.MeltId = Convert.ToInt32(tag[DB620_MELT_ID].ReadCurrentValue());
                aPocket.StackNumber = Convert.ToInt32(tag[DB620_ITEM_NO].ReadCurrentValue());
                aPocket.Weight = Convert.ToInt32(tag[DB620_WEIGHT].ReadCurrentValue());

                return true;
            }
            catch (Exception ex) {
                aPocket = null;
                logger.Error("Ошибка во время получения данных ЕГП: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Обнуляет запрос новой карты плавки.
        /// </summary>
        private void TryResetCastPlanRequest()
        {
            try {
                tag[DB601_NEW_BATCH_REQUEST].WriteValue(false);
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время обнуления запроса карты плавки: " + ex.Message);
            }
        }

        /// <summary>
        /// Выполняет запрос новой карты плавки.
        /// Передает данные в Callback.
        /// </summary>
        private void CastPlanRequest()
        {
            logger.Info("Запрос на новую карту плавки.");
            int furnaceNumber;
            if (!TryGetFurnaceNumber(out furnaceNumber)) {
                logger.Info("Ошибка во время получения значения миксера.");
                return;
            }

            logger.Info(string.Format("Номер миксера {0}", furnaceNumber));
            try {
                if (callback != null) {
                    callback.OnCastRequest(this, furnaceNumber);
                }
            }
            catch (Exception ex) {
                logger.Error("Ошибка во время передачи данных в Callback: " + ex.Message);
            }
        }

        /// <summary>
        /// Пытается получить номер миксера.
        /// </summary>
        /// <param name="aFurnaceNumber">Номер миксера.</param>
        /// <returns>True, если получить удалось, fakse - иначе.</returns>
        private bool TryGetFurnaceNumber(out int aFurnaceNumber)
        {
            try {
                aFurnaceNumber = Convert.ToInt32(tag[DB600_FURNACE_NUM].ReadCurrentValue());

                return true;
            }
            catch (Exception ex) {
                aFurnaceNumber = -1;
                logger.Error("Ошибка при получении номера миксера: " + ex.Message);                
                return false;
            }            
        }
    }
}
