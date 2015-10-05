namespace Alvasoft.ODTIntegration.Buffer
{
    using System.Collections.Generic;
    using ODTIntegaration.Structures;
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

        public void Clear()
        {
            lock (products) {
                products.Clear();
            }
        }

        public bool IsEmpty()
        {
            lock (products) {
                return products.Count == 0;
            }
        }
    }
}
