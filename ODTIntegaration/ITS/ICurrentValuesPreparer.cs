namespace Alvasoft.ODTIntegration.ITS
{
    using ConnectionHolder;
    using Alvasoft.Utils.Activity;
    using Structures;

    /// <summary>
    /// Подготавливает текущие данные для сохранения в ИТС.    
    /// </summary>
    public interface ICurrentValuesPreparer : IInitializable
    {
        /// <summary>
        /// Устанавливает держатель соединения.
        /// </summary>
        /// <param name="aConnectionHolder">Держатель соединения.</param>
        void SetConnectionHoder(OracleConnectionHolder aConnectionHolder);

        /// <summary>
        /// Устанавливает список текущих значений, для которых необходимо 
        /// производить подготовку.
        /// </summary>
        /// <param name="aCurrentValueInfos">Текущие значения.</param>
        void SetCurrentValues(CurrentValueInfo[] aCurrentValueInfos);

        /// <summary>
        /// Подготавливает данные для отправления в ИТС.
        /// Устанавливает идентификаторы.
        /// </summary>
        /// <param name="aCurrentValues">Текущеи данные.</param>
        bool TryPrepareValues(CurrentValue[] aCurrentValues);
    }
}
