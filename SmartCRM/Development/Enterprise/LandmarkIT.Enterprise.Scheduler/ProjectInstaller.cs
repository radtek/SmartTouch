using System.ComponentModel;

namespace Prayogs.Communication.SmsService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();            
        }
    }
}
