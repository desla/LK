namespace Alvasoft.ODTIntegration.ITS
{
    using System;
    using System.Data;
    using Oracle.ManagedDataAccess.Client;     
    using Structures;
    using Utils.Extensions;
    using Alvasoft.Utils.Activity;
    using ConnectionHolders;
    using log4net;

    /// <summary>
    /// Реализация ИТС через БД Oracle.
    /// </summary>
    public class ItsImpl : 
        InitializableImpl, 
        IIts
    {
        private static readonly ILog logger = LogManager.GetLogger("ItsImpl");

        /// <summary>
        /// Держатель соединения с ИТС.
        /// </summary>
        private OracleConnectionHolder connectionHolder;

        public void SetConnectionHoder(OracleConnectionHolder aConnectionHolder)
        {
            connectionHolder = aConnectionHolder;
        }

        public CastPlan GetCastPlat(int aFurnaceNumber)
        {
            logger.Info("Запрос карты плавки для для миксера №" + aFurnaceNumber);

            try {
                var connection = connectionHolder.WaitConnection();                

                var properties = OracleCommands.Default;
                using (var command = 
                    new OracleCommand(properties.GetCastPlanProcedure, connection)) {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(properties.aCursor, OracleDbType.RefCursor, ParameterDirection.Output);
                    command.Parameters.Add(properties.aFurnaceNumber, OracleDbType.Int32, ParameterDirection.Input);
                    command.Parameters[properties.aFurnaceNumber].Value = aFurnaceNumber;

                    using (var reader = command.ExecuteReader()) {                        
                        if (reader.Read()) {
                            var castPlan = reader.ReadCastPlan();
                            return castPlan;
                        }

                        return null;
                    }
                }

            }
            catch (Exception ex) {
                logger.Error("Ошибка при обращении к БД ИТС: " + ex.Message);                
                return null;
            }
            finally {
                connectionHolder.ReleaseConnection();
            }
        }

        public bool TryAddFinishedProduct(FinishedProduct aPocket)
        {
            return TryAddFinishedProducts(new[] {aPocket});
        }

        public bool TryAddFinishedProducts(FinishedProduct[] aPockets)
        {
            logger.Info("Добавление информации о ЕГП.");
            try {
                var connection = connectionHolder.WaitConnection();

                var properties = OracleCommands.Default;                
                using (var transaction = connection.BeginTransaction())
                using (var command = new OracleCommand(properties.InsertFinishedProductCommand)) {
                    command.Connection = connection;
                    command.Transaction = transaction;                    
                    command.Parameters.Add(properties.pMeltId, OracleDbType.Int32);
                    command.Parameters.Add(properties.pFurnaceNumber, OracleDbType.Int32);
                    command.Parameters.Add(properties.pMeltNumber, OracleDbType.Int32);
                    command.Parameters.Add(properties.pStackNumber, OracleDbType.Int32);
                    command.Parameters.Add(properties.pWeight, OracleDbType.Int32);
                    command.Parameters.Add(properties.pReceiptTime, OracleDbType.Date);

                    foreach (var aPocket in aPockets) {
                        command.Parameters[properties.pMeltId].Value = aPocket.MeltId;
                        command.Parameters[properties.pFurnaceNumber].Value = aPocket.FurnaceNumber;
                        command.Parameters[properties.pMeltNumber].Value = aPocket.CastNumber;
                        command.Parameters[properties.pStackNumber].Value = aPocket.StackNumber;
                        command.Parameters[properties.pWeight].Value = aPocket.Weight;
                        command.Parameters[properties.pReceiptTime].Value = aPocket.ReceiveTime;
                        command.ExecuteNonQuery();
                    }                    

                    transaction.Commit();                    
                }

                return true;
            }
            catch (Exception ex) {
                logger.Error("Ошибка при добавлении ЕГП: " + ex.Message);                
                return false;
            }
            finally {
                connectionHolder.ReleaseConnection();
            }
        }

        public bool TryAddCurrentValue(CurrentValue aCurrentValue)
        {
            return TryAddCurrentValues(new [] { aCurrentValue });
        }

        public bool TryAddCurrentValues(CurrentValue[] aCurrentValues)
        {
            logger.Info("Добавление информации текущих параметров...");
            try {
                var connection = connectionHolder.WaitConnection();

                var properties = OracleCommands.Default;                
                using (var transaction = connection.BeginTransaction())
                using (var command = new OracleCommand(properties.InsertCurrentValueCommand)) {
                    command.Connection = connection;
                    command.Transaction = transaction;
                    command.Parameters.Add(properties.pDataInfoId, OracleDbType.Int32);
                    command.Parameters.Add(properties.pObjectInfoId, OracleDbType.Int32);
                    command.Parameters.Add(properties.pValueTime, OracleDbType.Date);
                    command.Parameters.Add(properties.pValue, OracleDbType.Double);
                    if (aCurrentValues != null) {
                        foreach (var currentValue in aCurrentValues) {
                            command.Parameters[properties.pDataInfoId].Value = currentValue.Ids.DataId;
                            command.Parameters[properties.pObjectInfoId].Value = currentValue.Ids.ObjectId;
                            command.Parameters[properties.pValueTime].Value = currentValue.ValueTime;
                            command.Parameters[properties.pValue].Value = currentValue.Value;
                            command.ExecuteNonQuery();
                        }
                    }                    

                    transaction.Commit();                 
                }

                logger.Info("Текущие параметры успешно добавлены в ИТС.");
                return true;
            }
            catch (Exception ex) {
                logger.Error("Ошибка при добавлении текущих параметров: " + ex.Message);                
                return false;
            }
            finally {
                connectionHolder.ReleaseConnection();
            }
        }
    }
}
