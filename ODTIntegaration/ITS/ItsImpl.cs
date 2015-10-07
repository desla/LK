namespace Alvasoft.ODTIntegration.ITS
{
    using System;
    using System.Data;
    using Oracle.ManagedDataAccess.Client; 
    using ODTIntegaration.ConnectionHolder;
    using ODTIntegaration.Structures;
    using ODTIntegaration.ITS;
    using Utils.Extensions;
    using Alvasoft.Utils.Activity;    
    using log4net;

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
                connectionHolder.LockConnection();

                if (!connectionHolder.IsConnected()) {
                    logger.Info("Отсутствует подключение к БД ИТС.");
                    return null;
                }

                var properties = OracleCommands.Default;
                using (var command = 
                    new OracleCommand(properties.GettingCastPlan, connectionHolder.GetOracleConnection())) {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(properties.GettingCastPlan_p1, OracleDbType.RefCursor, ParameterDirection.Output);
                    command.Parameters.Add(properties.GettingCastPlan_p2, OracleDbType.Int32, ParameterDirection.Input);
                    command.Parameters[properties.GettingCastPlan_p2].Value = aFurnaceNumber;

                    using (var reader = command.ExecuteReader()) {
                        connectionHolder.UpdateLastOperationTime();
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
                connectionHolder.ProcessError(ex);
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
                connectionHolder.LockConnection();

                if (!connectionHolder.IsConnected()) {
                    logger.Info("Отсутствует подключение к БД ИТС.");
                    return false;
                }

                var properties = OracleCommands.Default;
                var connection = connectionHolder.GetOracleConnection();
                using (var transaction = connection.BeginTransaction())
                using (var command = new OracleCommand(properties.InsertFinishedProduct)) {
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
                    connectionHolder.UpdateLastOperationTime();
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
    }
}
