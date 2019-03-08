using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SimpleInjector;
using SmartTouch.CRM.ApplicationServices;
using SmartTouch.CRM.JobProcessor;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ScheduleServices
{
    partial class BulkOperationService : SmartTouchServiceBase
    {
        public const string NameOfService = "Smart CRM - Bulk Operation Processor";
        public const string ServiceDescription = "Process bulk operations";
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

        public BulkOperationService()
            : base()
        {
            ServiceName = NameOfService;
            JobType = CronJobType.BulkOperationProcessor;
            CacheName = "BulkOperationCacheName";
            LogFilePath = "SMART_CRM_BULK_PROCESSOR_LOG_FILE_PATH";
        }

        
    }
}
