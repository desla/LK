namespace Alvasoft.ODTIntegration.CastLineConnector
{
    using Structures;

    /// <summary>
    /// Интерфейс обратной связи для читателя текущих параметров из ОРС.
    /// </summary>
    public interface ICurrentValueReaderCallback
    {
        /// <summary>
        /// Возникает, когда появились новые данные о текущих параметрах системы.
        /// </summary>
        /// <param name="aReader">Теукщий читатель.</param>
        /// <param name="aCurrentValues">Даанные о параметрах.</param>
        void OnCurrentValueReceived(ICurrentValueReader aReader, CurrentValue[] aCurrentValues);
    }
}
