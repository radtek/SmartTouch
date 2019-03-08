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
    public class CampaignService : SmartTouchServiceBase
    {
        public const string NameOfService = "Smart CRM - Campaign Service";
        public const string ServiceDescription = "Process Campaigns";

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

        public CampaignService()
        {
            ServiceName = NameOfService;
            JobType = CronJobType.CampaignProcessor;
            LogFilePath = "SMART_CRM_CAMPAIGN_PROCESSOR_LOG_FILE_PATH";
        }
    }
}
