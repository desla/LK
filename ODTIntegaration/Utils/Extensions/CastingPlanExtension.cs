namespace Alvasoft.ODTIntegration.Utils.Extensions
{
    using System;
    using Oracle.ManagedDataAccess.Client;
    using Structures;

    public static class CastingPlanExtensions
    {
        public static CastPlan ReadCastPlan(this OracleDataReader aReader)
        {
            var result = new CastPlan();
            try {
                result.FurnaceNumber = Convert.ToInt32(aReader.GetValue(0));
                result.CastNumber = Convert.ToInt32(aReader.GetValue(1));
                result.MeltId = Convert.ToInt32(aReader.GetValue(2));
                result.ProductName = Convert.ToString(aReader.GetValue(3));
                return result;
            }
            catch (Exception ex) {
                throw new Exception("Не удалось привести результат к типу CastPlan: " + ex.Message);
            }
        }
    }
}
