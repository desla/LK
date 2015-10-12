namespace Alvasoft.ODTIntegration.CastLineConnector
{
    using System;
    using System.Collections.Generic;    
    using Structures;
    using OpcTagAccessProvider;
    using System.Timers;
    using log4net;
    using Configuration;
    using ConnectionHolders;
    using Alvasoft.Utils.Activity;

    /// <summary>
    /// Реализация читателя текущих значений.
    /// </summary>
    public class CurrentValueReaderImpl : 
        InitializableImpl, 
        ICurrentValueReader
    {
        private static readonly ILog logger = LogManager.GetLogger("CurrentValueReaderImpl");

        private OpcConnectionHolder connectionHolder;
        private ICurrentValueReaderCallback callback;
        
        private CurrentValuesConfiguration configuration;
        private Timer readerTimer;
        private Dictionary<CurrentValueInfo, OpcValueImpl> opcTag = 
            new Dictionary<CurrentValueInfo, OpcValueImpl>();

        private bool isActive = false;
        private object isActiveLock = new object();

        public void SetConnectionHolder(OpcConnectionHolder aConnectionHolder)
        {
            connectionHolder = aConnectionHolder;
        }

        public void SetConfiguration(CurrentValuesConfiguration aConfiguration)
        {
            configuration = aConfiguration;
        }

        public void SetCallback(ICurrentValueReaderCallback aCallback)
        {
            callback = aCallback;
        }

        public void Start()
        {
            if (!IsInitialized()) {
                logger.Info("Нельзя запутить читатель текущих параметров без инициализации.");
            }           

            readerTimer.Start();
        }

        public void Stop()
        {
            if (!IsInitialized()) {
                return;
            }

            readerTimer.Stop();
        }

        protected override void DoInitialize()
        {
            try {
                var opcServer = connectionHolder.WaitConnection();
                var currentValues = configuration.GetCurrentValues();
                foreach (var currentValue in currentValues) {
                    opcTag[currentValue] = new OpcValueImpl(opcServer, currentValue.OpcItemName);
                    opcTag[currentValue].Activate();
                }

                readerTimer = new Timer();
                readerTimer.Elapsed += ReadCurrentValues;
                readerTimer.Interval = configuration.ReadInterval*1000;
            } 
            finally {
                connectionHolder.ReleaseConnection();
            }
        }        

        protected override void DoUninitialize()
        {            
            logger.Info("Деинициализация...");
            foreach (var opcValue in opcTag.Values) {
                try {
                    opcValue.Deactivate();
                }
                catch (Exception ex) {
                    logger.Error("Ошибка при деинициализации: " + ex.Message);
                }
            }                
            opcTag.Clear();
            logger.Info("Деинициализация произведена.");
        }

        private void ReadCurrentValues(object sender, ElapsedEventArgs e)
        {
            lock (isActiveLock) {
                if (isActive) {
                    return;
                }

                isActive = true;
            }

            try {
                var results = new List<CurrentValue>();
                var currentValueInfos = opcTag.Keys;
                foreach (var currentValueInfo in currentValueInfos) {
                    var value = opcTag[currentValueInfo].ReadCurrentValue();
                    var result = new CurrentValue();
                    result.Info = currentValueInfo;
                    result.ValueTime = DateTime.Now;
                    result.Value = Convert.ToDouble(value);
                    results.Add(result);
                }

                if (results.Count > 0) {
                    if (callback != null) {
                        callback.OnCurrentValueReceived(this, results.ToArray());
                    }
                }
            }
            catch (Exception ex) {
                logger.Error("Ошибка при чтении текущего значения: " + ex.Message);
            }
            finally {
                lock (isActiveLock) {
                    isActive = false;
                }
            }            
        }
    }
}
