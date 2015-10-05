namespace Alvasoft.ODTIntegaration
{
    using System;
    using System.IO;
    using ODTIntegration.CastLineIntegration;
    using log4net.Config;

    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("Settings/Logging.xml"));           
        }
    }
}
