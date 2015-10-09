namespace Alvasoft.ODTIntegration.Buffer
{
    using Structures;

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
        /// Очищает буфер от данных ЕГП.
        /// </summary>
        void ClearFinishedProducts();

        /// <summary>
        /// Возвращает результат проверки на отсутствие данных.
        /// </summary>
        /// <returns>True - если данных нет, false - иначе.</returns>
        bool IsEmpty();

        /// <summary>
        /// Добавляет текущие значения в буфер.
        /// </summary>
        /// <param name="aCurrentValues">Текущие значения.</param>
        void AddCurrentValues(CurrentValue[] aCurrentValues);

        /// <summary>
        /// Возвращает сохраненные текущие параметры.
        /// </summary>
        /// <returns>Текущие параметры в буфере.</returns>
        CurrentValue[] GetStoredCurrentValuesOrDefault();

        /// <summary>
        /// Очищает буфер от текущих значений.
        /// </summary>
        void ClearCurrentValues();
    }
}
