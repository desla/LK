namespace Alvasoft.ODTIntegration.Structures
{
    using System;

    /// <summary>
    /// Значение, получаемое из ОРС-сервера.
    /// </summary>
    public class CurrentValue
    {
        /// <summary>
        /// Ссылка на описание.
        /// </summary>
        public CurrentValueInfo Info { get; set; }

        /// <summary>
        /// Идентификаторы.
        /// </summary>
        public CurrentValueIdentifiers Ids { get; set; }

        /// <summary>
        /// Время считывания.
        /// </summary>
        public DateTime ValueTime { get; set; }

        /// <summary>
        /// Значение.
        /// </summary>
        public double Value { get; set; }
    }
}
