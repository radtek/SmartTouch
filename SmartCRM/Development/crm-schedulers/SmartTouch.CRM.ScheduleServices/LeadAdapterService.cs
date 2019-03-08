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
    public class LeadAdapterService : SmartTouchServiceBase
    {
        public const string NameOfService = "Smart CRM - Lead Adapter Service";
        public const string ServiceDescription = "Process Lead adapters";

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

        public LeadAdapterService()
        {
            ServiceName = NameOfService;
            JobType = CronJobType.LeadProcessor;
            LogFilePath = "SMART_CRM_LEADADAPTER_PROCESSOR_LOG_FILE_PATH";
        }
    }
}
