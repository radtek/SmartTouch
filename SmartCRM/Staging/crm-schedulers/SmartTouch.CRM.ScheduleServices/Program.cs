using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ScheduleServices
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            try
            {
                var ac = new WorkflowActionService();
                ac.Start(null);
                //var bc = new BulkOperationService();
                //bc.Init();
            }
            catch(Exception e)
            {
                throw e;
            }
           
            
#else
             ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new APILeadSubmissionService(), 
                new BulkOperationService(),
                new CampaignService(), 
                new FormSubmissionService(), 
                new ImportLeadService(), 
                new IndexingService(),
                new LeadAdapterService(), 
                new LeadScoreService(), 
                new WorkflowActionService()
            };
            ServiceBase.Run(ServicesToRun);
#endif

        }
    }
}
