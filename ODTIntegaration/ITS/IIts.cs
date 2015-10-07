namespace Alvasoft.ODTIntegaration.ITS
{
    using Structures;
    using Utils.Activity;
    using ConnectionHolder;


    /// <summary>
    /// Интерфейс обмена информацией с ИТС.
    /// </summary>
    public interface IIts : IInitializable
    {        
        /// <summary>
        /// Устанавливает держатель соединения.
        /// </summary>
        /// <param name="aConnectionHolder">Держатель соединения.</param>
        void SetConnectionHoder(OracleConnectionHolder aConnectionHolder);

        /// <summary>
        /// Возвращает карту плавки, готовую к выливки для указанного миксера или null.
        /// </summary>
        /// <param name="aFurnaceNumber">Номер миксера.</param>
        /// <returns>Карта плавки или null.</returns>
        CastPlan GetCastPlat(int aFurnaceNumber);

        /// <summary>
        /// Добавляет в ИТС информацию о единице готовой продукции.
        /// </summary>
        /// <param name="aPocket">Единица готовой продукции.</param>
        /// <returns>True, если добавление успешно, false - иначе.</returns>
        bool TryAddFinishedProduct(FinishedProduct aPocket);

        /// <summary>
        /// Добавляет в ИТС массив данных о ЕГП.
        /// </summary>
        /// <param name="aPockets">Данные ЕГП.</param>
        /// <returns>True - если добавление успешно, false - иначе.</returns>
        bool TryAddFinishedProducts(FinishedProduct[] aPockets);
    }
}
