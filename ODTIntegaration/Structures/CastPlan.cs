namespace Alvasoft.ODTIntegaration.Structures
{
    /// <summary>
    /// Карта плавки, пересылаемая из ИТС в ЛК.
    /// </summary>
    public class CastPlan
    {
        /// <summary>
        /// Номер миксера.
        /// </summary>
        public int FurnaceNumber { get; set; }

        /// <summary>
        /// Номер плавки.
        /// </summary>
        public int CastNumber { get; set; }

        /// <summary>
        /// Уникальный номерк карты плавки.
        /// </summary>
        public int MeltId { get; set; }

        /// <summary>
        /// Наименование продукции.
        /// </summary>
        public string ProductName { get; set; }
    }
}
