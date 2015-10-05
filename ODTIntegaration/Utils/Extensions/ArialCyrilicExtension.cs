namespace Alvasoft.ODTIntegration.Utils.Extensions
{
    using System;
    using System.Text;

    /// <summary>
    /// Расширение для перевода из обычной строки в символы, 
    /// которые понимает австралийский принтер на производстве.
    /// Константы менять нельзя. У австралийцев в контроллере зашита именно такая таблица.
    /// </summary>
    public static class ArialCyrilicExtension
    {
        private const string ALPHABEL = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯфбвгдежзийклмнопрстуфхцчшщъыьэюя";
        private const int START_INDEX_CODE = 128;
        private const int MAX_STRING_LENGTH = 40;

        /// <summary>
        /// Конвертирует обычную строку в такую, которую понимает принтер на производстве.
        /// Первые два байта - служебные:
        /// Первый - максимальная длина строки. Должна всегда быть 40.
        /// Второй - фактическая длина строки.
        /// </summary>
        /// <param name="aSource">Исходная строка.</param>
        /// <returns>Результирующая строка.</returns>
        public static string ToArialCyrilic(this string aSource)
        {
            var buffer = new byte[Math.Min(MAX_STRING_LENGTH, aSource.Length + 2)];
            buffer[0] = MAX_STRING_LENGTH;
            buffer[1] = (byte)aSource.Length;
            for (var i = 0; i < aSource.Length && i + 2 < MAX_STRING_LENGTH; ++i) {
                buffer[i + 2] = GetCharacterCode(aSource[i]);
            }

            return Encoding.Default.GetString(buffer);
        }

        private static byte GetCharacterCode(char aChar)
        {
            for (var i = 0; i < ALPHABEL.Length; ++i) {
                if (aChar == ALPHABEL[i]) {
                    return (byte)(START_INDEX_CODE + i);
                }
            }

            return (byte)aChar;
        }
    }
}
