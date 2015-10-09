namespace Alvasoft.ODTIntegration.Structures
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

        public override string ToString()
        {
            return string.Format(
                "Карта плавки: " +
                "Идентификатор {0}; " +
                "Номер {1}; " +
                "Миксер {2}; " +
                "Наименование '{3}';",
                MeltId, CastNumber, FurnaceNumber, ProductName);
        }
    }
}
