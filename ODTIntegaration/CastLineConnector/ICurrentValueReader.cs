namespace Alvasoft.ODTIntegration.CastLineConnector
{
    using Configuration;
    using ConnectionHolders;
    using Alvasoft.Utils.Activity;

    /// <summary>
    /// Читатель текущих параметров из ОРС.
    /// </summary>
    public interface ICurrentValueReader : IInitializable
    {
        /// <summary>
        /// Устанавливает держатель соединения.
        /// </summary>
        /// <param name="aConnectionHolder">Держатель соединения.</param>
        void SetConnectionHolder(OpcConnectionHolder aConnectionHolder);

        /// <summary>
        /// Устанавливает конфигурацию.
        /// </summary>
        /// <param name="aConfiguration">Конфигурация.</param>
        void SetConfiguration(CurrentValuesConfiguration aConfiguration);

        /// <summary>
        /// Устанавливаент интерфейс для обратной связи.
        /// </summary>
        /// <param name="aCallback">Интерфейс для обратной связи.</param>
        void SetCallback(ICurrentValueReaderCallback aCallback);

        /// <summary>
        /// Запускает работу.
        /// </summary>
        void Start();

        /// <summary>
        /// Останавливает работу.
        /// </summary>
        void Stop();
    }
}
