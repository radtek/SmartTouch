using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.NotificationProcessor
{
    [RunInstaller(true)]
    public partial class ServicesInstaller : Installer
    {
        private ServiceProcessInstaller process;

        public ServicesInstaller()
        {
            try
            {
                process = new ServiceProcessInstaller();
                process.Account = ServiceAccount.LocalSystem;
                var installer = new ServiceInstaller();
                installer.ServiceName = "Smart CRM - Notification Processor";
                installer.Description = "Process Notification Messages";
                installer.StartType = ServiceStartMode.Automatic;

                Installers.Add(process);
                Installers.Add(installer);

            }
            catch
            {

            }

        }
    }
}
