using System.ComponentModel;
using System.Configuration.Install;

namespace Alvasoft.ODTIntegration
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
