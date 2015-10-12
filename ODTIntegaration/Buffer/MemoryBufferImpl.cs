namespace Alvasoft.ODTIntegration.Buffer
{
    using System.Collections.Generic;
    using Structures;
    using log4net;

    /// <summary>
    /// Буфер в оперативной памяти.
    /// </summary>
    public class MemoryBufferImpl : IDataBuffer
    {
        private static readonly ILog logger = LogManager.GetLogger("MemoryBufferImpl");

        /// <summary>
        /// Ограничение на количество записей.
        /// </summary>
        private const int MAX_SIZE = 1000000;

        /// <summary>
        /// Набор для хранения данных.
        /// </summary>
        private List<FinishedProduct> products = new List<FinishedProduct>();

        private List<CurrentValue> currentValues = new List<CurrentValue>(); 
        
        public void AddFinishedProduct(FinishedProduct aPocket)
        {
            lock (products) {
                if (products.Count < MAX_SIZE) {
                    products.Add(aPocket);
                }
                else {
                    logger.Warn("Превышено количество максимальных записей в буфере.");
                }
            }            
        }

        public FinishedProduct[] GetStoredProductsOrDefault()
        {            
            lock (products) {
                if (products.Count == 0) {
                    return null;
                }

                return products.ToArray();
            }
        }

        public void ClearFinishedProducts()
        {
            lock (products) {
                products.Clear();
            }
        }

        public bool IsEmpty()
        {
            lock (products) {
                return products.Count == 0 && currentValues.Count == 0;
            }
        }

        public void AddCurrentValues(CurrentValue[] aCurrentValues)
        {
            lock (currentValues) {
                currentValues.AddRange(aCurrentValues);
            }
        }

        public CurrentValue[] GetStoredCurrentValuesOrDefault()
        {
            lock (currentValues) {
                if (currentValues.Count == 0) {
                    return null;
                }

                return currentValues.ToArray();
            }
        }

        public void ClearCurrentValues()
        {
            lock (currentValues) {
                currentValues.Clear();
            }
        }
    }
}
