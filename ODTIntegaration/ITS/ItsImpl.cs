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

        public bool TryAddFinishedProduct(FinishedProduct aProductPocket)
        {
            return true;
        }
    }
}
