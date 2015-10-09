namespace Alvasoft.ODTIntegration.ITS
{
    using System;
    using System.Collections.Generic;
    using Oracle.ManagedDataAccess.Client;
    using log4net;
    using ConnectionHolder;
    using Structures;
    using Alvasoft.Utils.Activity;

    /// <summary>
    /// Реализация подготовителя данных.
    /// </summary>
    public class CurrentValuesPreparerImpl : 
        InitializableImpl, 
        ICurrentValuesPreparer
    {
        private static readonly ILog logger = LogManager.GetLogger("CurrentValuesPreparerImpl");

        private OracleConnectionHolder connectionHolder;
        private CurrentValueInfo[] currentValueInfos;
        private Dictionary<CurrentValueInfo, CurrentValueIdentifiers> 
            currentValueIds = new Dictionary<CurrentValueInfo, CurrentValueIdentifiers>(); 

        public void SetConnectionHoder(OracleConnectionHolder aConnectionHolder)
        {
            connectionHolder = aConnectionHolder;
        }

        public void SetCurrentValues(CurrentValueInfo[] aCurrentValueInfos)
        {
            currentValueInfos = aCurrentValueInfos;
        }

        public bool TryPrepareValues(CurrentValue[] aCurrentValues)
        {
            logger.Info("Подготовка данных...");
            if (!IsInitialized()) {
                logger.Info("Нельзя подготовить данные без инициализации.");
                return false;
            }

            try {
                lock (currentValueIds) {
                    foreach (var currentValue in aCurrentValues) {
                        var isFind = false;
                        foreach (var currentValueInfo in currentValueInfos) {
                            if (currentValue.Info.Equals(currentValueInfo)) {
                                currentValue.Ids = currentValueIds[currentValueInfo].GetCopy();
                                isFind = true;
                                break;
                            }
                        }

                        if (!isFind) {
                            logger.Info("Для текущего показания не обнаружены идентификаторы: " +
                                        currentValue.Info.DataName);
                        }
                    }
                }
            }
            catch (Exception ex) {
                logger.Error("Ошибка при подготовке данных: " + ex.Message);
                return false;
            }

            logger.Info("Данные подготовлены успешно.");
            return true;
        }

        protected override void DoInitialize()
        {
            logger.Info("Инициализация...");
            try {
                connectionHolder.LockConnection();

                if (!connectionHolder.IsConnected()) {
                    logger.Info("Отсутствует подключение к БД ИТС.");
                    return;
                }

                var properties = OracleCommands.Default;
                var connection = connectionHolder.GetOracleConnection();
                using (var command = new OracleCommand(properties.GetIdentifiersCommand, connection)) {
                    command.Parameters.Add(properties.pTypeName, OracleDbType.Varchar2);
                    command.Parameters.Add(properties.pObjectName, OracleDbType.Varchar2);
                    command.Parameters.Add(properties.pDataName, OracleDbType.Varchar2);

                    lock (currentValueIds) {
                        foreach (var valueInfo in currentValueInfos) {
                            command.Parameters[properties.pTypeName].Value = valueInfo.TypeName;
                            command.Parameters[properties.pObjectName].Value = valueInfo.ObjectName;
                            command.Parameters[properties.pDataName].Value = valueInfo.DataName;

                            using (var reader = command.ExecuteReader()) {
                                if (reader.Read()) {
                                    var ids = new CurrentValueIdentifiers();
                                    ids.TypeId = Convert.ToInt32(reader.GetValue(0));
                                    ids.ObjectId = Convert.ToInt32(reader.GetValue(1));
                                    ids.DataId = Convert.ToInt32(reader.GetValue(2));

                                    currentValueIds[valueInfo] = ids;
                                }
                                else {
                                    logger.Info("Не удалось получить идентификаторы для показателя: " +
                                                valueInfo.DataName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                logger.Error("Ошибка при инициализации: " + ex.Message);
                connectionHolder.ProcessError(ex);
            }
            finally {
                connectionHolder.ReleaseConnection();
                logger.Info("Инициализация завершена.");
            }            
        }

        protected override void DoUninitialize()
        {
            lock (currentValueIds) {
                currentValueIds.Clear();
            }
        }
    }
}
