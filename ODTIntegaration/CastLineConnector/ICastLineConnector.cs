namespace Alvasoft.ODTIntegration.CastLineConnector
{
    using Alvasoft.Utils.Activity;
    using ODTIntegaration.Structures;
    using ODTIntegaration.ConnectionHolder;    
    using System.Collections.Generic;

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
        /// Устанавливает конфигурацию opc-тегов.
        /// </summary>        
        /// <param name="aOpcTagsList">Список opc-тегов.</param>
        void SetOpcTagsList(Dictionary<string, string> aOpcTagsList);

        /// <summary>
        /// Устанавливает держатель соединенния.
        /// </summary>
        /// <param name="aConnectionHolder">Держатель соединения.</param>
        void SetOpcConnectionHolder(OpcConnectionHolder aConnectionHolder);

        /// <summary>
        /// Возвращает номер ЛК.
        /// </summary>
        /// <returns>Номер ЛК.</returns>
        int GetCastLineNumber();

        /// <summary>
        /// Устанавливает номер ЛК.
        /// </summary>
        /// <param name="aCastLineNumber">Номер ЛК.</param>
        void SetCastLineNumber(int aCastLineNumber);

        /// <summary>
        /// Записывает карту плавки в контроллер.
        /// </summary>
        /// <param name="aCastPlan">Карта плавки.</param>
        /// <returns>True - в случае успешной записи, false - иначе.</returns>
        bool TryWriteCastPlan(CastPlan aCastPlan);
    }
}
