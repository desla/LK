namespace Alvasoft.ODTIntegration.ITS
{
    using System;
    using ODTIntegaration.ITS;
    using ODTIntegaration.Structures;

    class ItsEmulatorImpl : IIts
    {
        private Random rnd = new Random();

        public CastPlan GetCastPlat(int aFurnaceNumber)
        {
            return new CastPlan {
                CastNumber = rnd.Next() % 256,
                FurnaceNumber = rnd.Next() % 256,
                MeltId = rnd.Next() % 1000,
                ProductName = "Текст_" + rnd.Next()
            };
        }

        public bool TryAddFinishedProduct(FinishedProduct aProductPocket)
        {
            // Пусть связь будет пропадать для каждой 10-й записи.
            if (rnd.Next()%10 == 1) {
                return false;
            }

            return true;
        }
    }
}
