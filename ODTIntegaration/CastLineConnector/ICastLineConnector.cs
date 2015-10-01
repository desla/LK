namespace Alvasoft.ODTIntegration.CastLineConnector
{
    using Utils.Activity;
    using ODTIntegaration.Structures;

    /// <summary>
    /// Интерфейс обмена данными с литейным конвейером.
    /// </summary>
    public interface ICastLineConnector : IInitializable
    {
        /// <summary>
        /// Устанавливает интерфейс обратной связи.
        /// </summary>
        /// <param name="aCallback">Callback.</param>
        void SetCastLineCallback(ICastLineCallback aCallback);

        /// <summary>
        /// Записывает карту плавки в контроллер.
        /// </summary>
        /// <param name="aCastPlan">Карта плавки.</param>
        void WriteCastPlan(CastPlan aCastPlan);
    }
}
