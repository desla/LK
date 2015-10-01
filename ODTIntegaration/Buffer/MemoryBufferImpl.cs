namespace Alvasoft.ODTIntegration.Buffer
{
    using System.Collections.Generic;
    using ODTIntegaration.Structures;

    /// <summary>
    /// Буфер в оперативной памяти.
    /// </summary>
    public class MemoryBufferImpl : IDataBuffer
    {
        /// <summary>
        /// Набор для хранения данных.
        /// </summary>
        private List<FinishedProduct> products = new List<FinishedProduct>();
        
        public void AddFinishedProduct(FinishedProduct aPocket)
        {
            lock (products) {
                products.Add(aPocket);
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
