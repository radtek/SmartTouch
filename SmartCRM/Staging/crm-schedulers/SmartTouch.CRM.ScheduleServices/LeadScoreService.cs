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
    public class LeadScoreService : SmartTouchServiceBase
    {
        public const string NameOfService = "Smart CRM - LeadScore Service";
        public const string ServiceDescription = "Process LeadScores";

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

        public LeadScoreService()
        {
            ServiceName = NameOfService;
            JobType = CronJobType.LeadScoreProcessor;
            LogFilePath = "SMART_CRM_LEADSCORE_PROCESSOR_LOG_FILE_PATH";
        }
    }
}
