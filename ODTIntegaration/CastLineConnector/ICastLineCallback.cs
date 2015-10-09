﻿namespace Alvasoft.ODTIntegration.CastLineConnector
{
    using Structures;

    /// <summary>
    /// Обратная связь от ЛК.
    /// </summary>
    public interface ICastLineCallback
    {
        /// <summary>
        /// Выполняет запрос на новую карту плавки.
        /// </summary>
        /// <param name="aConnector">Текущий коннектор к ЛК.</param>
        /// <param name="aFurnaceNumber">Номер миксера.</param>
        void OnCastRequest(ICastLineConnector aConnector, int aFurnaceNumber);

        /// <summary>
        /// Оповещает о появлении нового пакета готовой продукции.
        /// </summary>
        /// <param name="aConnector">Текущий коннектор к ЛК.</param>
        /// <param name="aPocket">Информация о пакете готовой продукции.</param>
        void OnFinishedProductAppeared(ICastLineConnector aConnector, FinishedProduct aPocket);
    }
}
