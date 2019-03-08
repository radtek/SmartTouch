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
    public class IndexingService : SmartTouchServiceBase
    {
        public const string NameOfService = "Smart CRM - Indexing Service";
        public const string ServiceDescription = "Index data to elastic";

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

        public IndexingService()
        {
            ServiceName = NameOfService;
            JobType = CronJobType.IndexProcessor;
            LogFilePath = "SMART_CRM_INDEX_PROCESSOR_LOG_FILE_PATH";
        }
    }
}
