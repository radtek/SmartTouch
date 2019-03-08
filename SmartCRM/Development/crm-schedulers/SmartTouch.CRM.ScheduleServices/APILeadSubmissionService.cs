using LandmarkIT.Enterprise.CommunicationManager.Database;
using SmartTouch.CRM.JobProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ScheduleServices
{
    public class APILeadSubmissionService: SmartTouchServiceBase
    {
        public const string NameOfService = "Smart CRM - API Lead Submission Service";
        public const string ServiceDescription = "Process Web API Lead Submissions";

        public static ServiceInstaller Installer
        {
            get
            {
                var installer = new ServiceInstaller();
                installer.ServiceName = NameOfService;
                installer.Description = ServiceDescription;
                installer.StartType = ServiceStartMode.Automatic;
                return installer;
            }
        }

        public APILeadSubmissionService()
        {
            ServiceName = NameOfService;
            JobType = CronJobType.APILeadSubmissionProcessor;
            LogFilePath = "SMART_CRM_API_LEAD_PROCESSOR_LOG_FILE_PATH";
        }
    }
}
