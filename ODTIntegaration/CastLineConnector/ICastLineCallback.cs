namespace Alvasoft.ODTIntegration.CastLineConnector
{
    using ODTIntegaration.Structures;

    /// <summary>
    /// Обратная связь от ЛК.
    /// </summary>
    public interface ICastLineCallback
    {
        /// <summary>
        /// Выполняет запрос на новую карту плавки.
        /// </summary>
        /// <param name="aConnector">Текущий коннектор к ЛК.</param>
        /// <param name="aMixerNumber">Номер миксера.</param>
        void OnCastRequest(ICastLineConnector aConnector, int aMixerNumber);

        /// <summary>
        /// Оповещает о появлении нового пакета готовой продукции.
        /// </summary>
        /// <param name="aConnector">Текущий коннектор к ЛК.</param>
        /// <param name="aPocket">Информация о пакете готовой продукции.</param>
        void OnFinishedProductAppeared(ICastLineConnector aConnector, FinishedProduct aPocket);
    }
}
