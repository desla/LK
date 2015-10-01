namespace Alvasoft.ODTIntegaration
{
    using System.IO;
    using ODTIntegration.Configuration;
    using log4net.Config;

    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("Settings/Logging.xml"));

            var c = new ConnectionsConfiguration();
            c.LoadFromFile("Settings/Network.xml");
        }
    }
}
