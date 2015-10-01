namespace Alvasoft.ODTIntegration.Buffer
{
    using ODTIntegaration.Structures;

    /// <summary>
    /// Интерфейс буфера для временного хранения данных при возникновении обрывов связи.
    /// </summary>
    public interface IDataBuffer
    {
        /// <summary>
        /// Сохраняет информацию о конечном продукте, которая пришла с контроллера.
        /// </summary>
        /// <param name="aPocket">Информация о конечном продукте.</param>
        void AddFinishedProduct(FinishedProduct aPocket);

        /// <summary>
        /// Возвращает все сохраненные данные или null.
        /// </summary>
        /// <returns>Сохраненные данные или null.</returns>
        FinishedProduct[] GetStoredProductsOrDefault();

        /// <summary>
        /// Очищает буфер от данных.
        /// </summary>
        void Clear();

        /// <summary>
        /// Возвращает результат проверки на отсутствие данных.
        /// </summary>
        /// <returns>True - если данных нет, false - иначе.</returns>
        bool IsEmpty();
    }
}
