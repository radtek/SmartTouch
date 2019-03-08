using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ScheduleServices
{
    [RunInstaller(true)]
    public partial class ServicesInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller process;
        
        public ServicesInstaller()
        {
            try
            {
                process = new ServiceProcessInstaller();
                process.Account = ServiceAccount.LocalSystem;

                Installers.Add(WorkflowActionService.Installer);
                Installers.Add(BulkOperationService.Installer);
                Installers.Add(APILeadSubmissionService.Installer);
                Installers.Add(CampaignService.Installer);
                Installers.Add(FormSubmissionService.Installer);
                Installers.Add(IndexingService.Installer);
                Installers.Add(ImportLeadService.Installer);
                Installers.Add(LeadAdapterService.Installer);
                Installers.Add(LeadScoreService.Installer);
                Installers.Add(process);
            }
            catch(Exception ex)
            {

            }
            
        }
    }
}
