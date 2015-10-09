namespace Alvasoft.ODTIntegration.Structures
{
    /// <summary>
    /// Идентификаторы для текущего значения.
    /// </summary>
    public class CurrentValueIdentifiers
    {
        /// <summary>
        /// Идентификатор типа в ИТС. 
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Идентификатор объекта в ИТС.
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        /// Идентификатор значения в ИТС.
        /// </summary>
        public int DataId { get; set; }

        /// <summary>
        /// Возвращает копию.
        /// </summary>
        /// <returns>Копия.</returns>
        public CurrentValueIdentifiers GetCopy()
        {
            return new CurrentValueIdentifiers {
                DataId = DataId,
                ObjectId = ObjectId,
                TypeId = TypeId
            };
        }
    }
}
