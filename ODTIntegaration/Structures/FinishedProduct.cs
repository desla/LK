using System;

namespace Alvasoft.ODTIntegaration.Structures
{
    /// <summary>
    /// Единица готовой продукции, пересылаемая из ЛК в ИТС.
    /// </summary>
    public class FinishedProduct
    {
        /// <summary>
        /// Уникальный номер карты плавки.
        /// </summary>
        public int MeltId { get; set; }

        /// <summary>
        /// Номер миксера.
        /// </summary>
        public int FurnaceNumber { get; set; }

        /// <summary>
        /// Номер плавки.
        /// </summary>
        public int CastNumber { get; set; }

        /// <summary>
        /// Порядковый номер пакета.
        /// </summary>
        public int StackNumber { get; set; }

        /// <summary>
        /// Вес пакета (кг).
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Дата получения информации пакета.
        /// </summary>
        public DateTime ReceiveTime { get; set; }

        public override string ToString()
        {
            return string.Format("ЕГП: Идентификатор плавки {0}; " +
                                 "Номер плавки: {1}; " +
                                 "Миксер: {2}; " +
                                 "порядковый номер пакета: {3}; " +
                                 "Вес {4};",
                                 MeltId,
                                 CastNumber,
                                 FurnaceNumber,
                                 StackNumber,
                                 Weight);
        }
    }
}
