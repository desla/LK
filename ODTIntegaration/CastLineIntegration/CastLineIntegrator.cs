namespace Alvasoft.ODTIntegration.CastLineIntegration
{
    using ODTIntegaration.ITS;
    using Buffer;

    /// <summary>
    /// Мост для взаимодействия между заводской ИТС и ЛК.
    /// </summary>
    public class CastLineIntegrator
    {
        /// <summary>
        /// Интерфейс для связи с ИТС.
        /// </summary>
        private IIts its;

        /// <summary>
        /// Буфер для временного хранения данных при обрыве связи с ИТС.
        /// </summary>
        private IDataBuffer dataBuffer;

        
    }
}
