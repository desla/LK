namespace Alvasoft.ODTIntegration
{    
    using System;
    using System.IO;
    using System.ServiceProcess;
    using System.Windows.Forms;
    using log4net.Config;
    using CastLineIntegration;


    class CastLineIntegrationApp : ServiceBase
    {
        private static CastLineIntegrator _integrator = new CastLineIntegrator();

        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower().Equals("console")) {
                var appPath = Application.StartupPath + "/";
                XmlConfigurator.Configure(new FileInfo(appPath + "Settings/Logging.xml"));
                _integrator.Initialize();
                Console.WriteLine("Для выхода нажмите Enter...");
                Console.ReadLine();
                _integrator.Uninitialize();
            }
            else {
                ServiceBase.Run(new CastLineIntegrationApp());
            }
        }

        protected override void OnStart(string[] args)
        {
            var appPath = Application.StartupPath + "/";
            XmlConfigurator.Configure(new FileInfo(appPath + "Settings/Logging.xml"));
            _integrator.Initialize();
        }

        protected override void OnStop()
        {            
            _integrator.Uninitialize();
        }
    }
}
