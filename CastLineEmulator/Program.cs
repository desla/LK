using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CastLineEmulator
{
    class Program
    {
        static void Main(string[] args)
        {
            var castLine = new CastLine();

            Console.WriteLine("Инициализация...");            
            castLine.Activate();

            Console.WriteLine("Инициализация завершена.");
            Console.WriteLine("Команды:");
            Console.WriteLine("   Backspace - посылаем номер миксера.");
            Console.WriteLine("   Enter - посылаем пакетные данные.");
            Console.WriteLine("   ESC - выходим из программы.");

            while (true) {
                var consoleKey = Console.ReadKey();
                if (consoleKey.Key == ConsoleKey.Backspace) {
                    castLine.SendCastMapRequest();
                } 
                else if (consoleKey.Key == ConsoleKey.Enter) {
                    castLine.SentEgpInformation();
                } 
                else if (consoleKey.Key == ConsoleKey.Escape) {
                    break;
                }
            }

            Console.WriteLine("Отключение...");
            castLine.Deactivate();
            Console.WriteLine("Отключено успешно.");
        }
    }
}
