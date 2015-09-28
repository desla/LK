using System;
using System.Collections.Generic;
using System.Threading;
using OpcTagAccessProvider;
using OPCAutomation;

namespace CastLineEmulator
{
    public class CastLine : IOpcValueListener
    {
        private const string DB601_NEW_BATCH_REQUEST = "Emulator.CAST_LINE_1.DB601.NewBatchRequest";
        private const string DB601_FURNACE_NUMBER = "Emulator.CAST_LINE_1.DB601.FurnaceNumber";

        private const string DB600_NEW_BATCH_RECEIVED = "Emulator.CAST_LINE_1.DB600.NewBatchReceived";
        private const string DB600_FURNACE_NUM = "Emulator.CAST_LINE_1.DB600.FurnaceNum";
        private const string DB600_CAST_NUM = "Emulator.CAST_LINE_1.DB600.CastNum";
        private const string DB600_MELT_ID = "Emulator.CAST_LINE_1.DB600.MeltId";
        private const string DB600_PRODUCT_NAME = "Emulator.CAST_LINE_1.DB600.ProductName";

        private const string DB620_DATA_READY = "Emulator.CAST_LINE_1.DB620.DataReady";
        private const string DB620_WEIGHT = "Emulator.CAST_LINE_1.DB620.Weight";
        private const string DB620_ITEM_NO = "Emulator.CAST_LINE_1.DB620.ItemNo";
        private const string DB620_CAST_NUM = "Emulator.CAST_LINE_1.DB620.CastNum";
        private const string DB620_FURNACE_NUM = "Emulator.CAST_LINE_1.DB620.FurnaceNum";
        private const string DB620_MELT_ID = "Emulator.CAST_LINE_1.DB620.MeltId";

        private OPCServer opcServer = new OPCServer();
        private const string HOST = "192.168.1.11";
        private const string SERVER_NAME = "Kepware.KEPServerEX.V5";

        private Dictionary<string, OpcValueImpl> tag = new Dictionary<string, OpcValueImpl>();

        private Random rnd = new Random();

        public void Activate()
        {
            opcServer.Connect(SERVER_NAME, HOST);

            tag[DB601_NEW_BATCH_REQUEST] = new OpcValueImpl(opcServer, DB601_NEW_BATCH_REQUEST);
            tag[DB601_FURNACE_NUMBER] = new OpcValueImpl(opcServer, DB601_FURNACE_NUMBER);

            tag[DB600_NEW_BATCH_RECEIVED] = new OpcValueImpl(opcServer, DB600_NEW_BATCH_RECEIVED);
            tag[DB600_CAST_NUM] = new OpcValueImpl(opcServer, DB600_CAST_NUM);
            tag[DB600_FURNACE_NUM] = new OpcValueImpl(opcServer, DB600_FURNACE_NUM);
            tag[DB600_MELT_ID] = new OpcValueImpl(opcServer, DB600_MELT_ID);
            tag[DB600_PRODUCT_NAME] = new OpcValueImpl(opcServer, DB600_PRODUCT_NAME);

            tag[DB620_DATA_READY] = new OpcValueImpl(opcServer, DB620_DATA_READY);
            tag[DB620_WEIGHT] = new OpcValueImpl(opcServer, DB620_WEIGHT);
            tag[DB620_ITEM_NO] = new OpcValueImpl(opcServer, DB620_ITEM_NO);
            tag[DB620_CAST_NUM] = new OpcValueImpl(opcServer, DB620_CAST_NUM);
            tag[DB620_FURNACE_NUM] = new OpcValueImpl(opcServer, DB620_FURNACE_NUM);
            tag[DB620_MELT_ID] = new OpcValueImpl(opcServer, DB620_MELT_ID);

            tag[DB601_NEW_BATCH_REQUEST].Activate();
            tag[DB601_FURNACE_NUMBER].Activate();

            tag[DB600_NEW_BATCH_RECEIVED].IsListenValueChanging = true;
            tag[DB600_NEW_BATCH_RECEIVED].SubscribeToValueChange(this);
            tag[DB600_NEW_BATCH_RECEIVED].Activate();
            tag[DB600_CAST_NUM].Activate();
            tag[DB600_FURNACE_NUM].Activate();
            tag[DB600_MELT_ID].Activate();
            tag[DB600_PRODUCT_NAME].Activate();

            tag[DB620_DATA_READY].Activate();
            tag[DB620_WEIGHT].Activate();
            tag[DB620_ITEM_NO].Activate();
            tag[DB620_CAST_NUM].Activate();
            tag[DB620_FURNACE_NUM].Activate();
            tag[DB620_MELT_ID].Activate();
        }

        public void Deactivate()
        {
            tag[DB601_NEW_BATCH_REQUEST].Deactivate();
            tag[DB601_FURNACE_NUMBER].Deactivate();
            
            tag[DB600_NEW_BATCH_RECEIVED].UnSubscribeToValueChange(this);
            tag[DB600_NEW_BATCH_RECEIVED].Deactivate();
            tag[DB600_CAST_NUM].Deactivate();
            tag[DB600_FURNACE_NUM].Deactivate();
            tag[DB600_MELT_ID].Deactivate();
            tag[DB600_PRODUCT_NAME].Deactivate();

            tag[DB620_DATA_READY].Deactivate();
            tag[DB620_WEIGHT].Deactivate();
            tag[DB620_ITEM_NO].Deactivate();
            tag[DB620_CAST_NUM].Deactivate();
            tag[DB620_FURNACE_NUM].Deactivate();
            tag[DB620_MELT_ID].Deactivate();

            opcServer.Disconnect();
        }

        public void SendCastMapRequest()
        {
            tag[DB601_FURNACE_NUMBER].WriteValue(rnd.Next() % 10);
            tag[DB601_NEW_BATCH_REQUEST].WriteValue(true);

            Console.WriteLine("Номер миксера записан... Ждем 5 секунд.");
            Thread.Sleep(5000);
            
            var currentValue = (bool)tag[DB601_NEW_BATCH_REQUEST].ReadCurrentValue();
            if (currentValue == true) {
                Console.WriteLine("В течение 5 секунд не был получен ответ от ИТС.");
                tag[DB601_NEW_BATCH_REQUEST].WriteValue(false);
            }
            else {
                Console.WriteLine("Номер миксера прочитан.");
            }            
        }

        public void SentEgpInformation()
        {
            Console.WriteLine("Записываем данные ЕГП...");
            tag[DB620_WEIGHT].WriteValue(rnd.Next() % 1000);
            tag[DB620_ITEM_NO].WriteValue(rnd.Next() % 100);
            tag[DB620_CAST_NUM].WriteValue(rnd.Next() % 100);
            tag[DB620_FURNACE_NUM].WriteValue(rnd.Next() % 100);            
            tag[DB620_MELT_ID].WriteValue(rnd.Next() % 100);
            tag[DB620_DATA_READY].WriteValue(true);
            Console.WriteLine("Данные ЕГП записаны.");
        }

        public void OnValueChanged(IOpcValue aOpcValue, OpcValueChangedEventArgs aValueChangedEventArgs)
        {
            if (aOpcValue.Name == DB600_NEW_BATCH_RECEIVED) {
                if ((bool)aValueChangedEventArgs.Value == true) {
                    Console.WriteLine("Получены данные из ИТС:");
                    Console.WriteLine("Номер плавки: " + tag[DB600_CAST_NUM].ReadCurrentValue());
                    Console.WriteLine("Номер миксера: " + tag[DB600_FURNACE_NUM].ReadCurrentValue());
                    Console.WriteLine("Идентификатор плавки: " + tag[DB600_MELT_ID].ReadCurrentValue());
                    Console.WriteLine("Наименование продукции: " + tag[DB600_PRODUCT_NAME].ReadCurrentValue());
                    aOpcValue.WriteValue("false");
                }
            }            
        }
    }
}
