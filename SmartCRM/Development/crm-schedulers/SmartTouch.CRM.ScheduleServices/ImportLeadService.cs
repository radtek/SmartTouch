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
    public class ImportLeadService : SmartTouchServiceBase
    {
        public const string NameOfService = "Smart CRM - Import Lead Service";
        public const string ServiceDescription = "Process Imports";

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

        public ImportLeadService()
        {
            ServiceName = NameOfService;
            JobType = CronJobType.ImportLeadProcessor;
            LogFilePath = "SMART_CRM_IMPORT_LEAD_PROCESSOR_LOG_FILE_PATH";
        }
    }
}
