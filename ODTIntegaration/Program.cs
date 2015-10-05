namespace Alvasoft.ODTIntegaration
{    
    using System;
    using System.IO;
    using log4net.Config;
    using ODTIntegration.CastLineIntegration;


    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("Settings/Logging.xml"));
            var integrator = new CastLineIntegrator();
            integrator.Initialize();

            Console.ReadLine();

            integrator.Uninitialize();
        }
    }
}
