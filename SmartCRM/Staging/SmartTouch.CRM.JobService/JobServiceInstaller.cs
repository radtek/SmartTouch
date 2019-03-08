using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Threading;

namespace SmartTouch.CRM.JobServices
{
    [RunInstaller(true)]
    public class JobServiceInstaller : Installer
    {
        public JobServiceInstaller()
        {
            var processInstaller = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };

            var serviceName = ConfigurationManager.AppSettings["ServiceName"];
            var displayName = ConfigurationManager.AppSettings["DisplayName"];

            var serviceInstaller = new ServiceInstaller
            {
                ServiceName = serviceName,
                DisplayName = displayName,
                StartType = ServiceStartMode.Automatic
            };

            var currentThread = Thread.CurrentThread;
            currentThread.Name = serviceName;

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
