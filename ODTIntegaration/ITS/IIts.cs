﻿namespace Alvasoft.ODTIntegaration.ITS
{
    using Structures;

    /// <summary>
    /// Интерфейс обмена информацией с ИТС.
    /// </summary>
    public interface IIts
    {
        /// <summary>
        /// Возвращает карту плавки, готовую к выливки для указанного миксера или null.
        /// </summary>
        /// <param name="aFurnaceNumber">Номер миксера.</param>
        /// <returns>Карта плавки или null.</returns>
        CastPlan GetCastPlat(int aFurnaceNumber);

        /// <summary>
        /// Добавляет в ИТС информацию о единице готовой продукции.
        /// </summary>
        /// <param name="aProductPocket">Единица готовой продукции.</param>
        /// <returns>True, если добавление успешно, false - иначе.</returns>
        bool TryAddFinishedProduct(FinishedProduct aProductPocket);
    }
}