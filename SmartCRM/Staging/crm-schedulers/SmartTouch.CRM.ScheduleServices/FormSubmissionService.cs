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
    public class FormSubmissionService:SmartTouchServiceBase
    {
        public const string NameOfService = "Smart CRM - Form Submission Service";
        public const string ServiceDescription = "Process Form Submissions";

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

        public FormSubmissionService()
        {
            ServiceName = NameOfService;
            JobType = CronJobType.FormSubmissionProcessor;
            LogFilePath = "SMART_CRM_FORM_SUBMISSION_PROCESSOR_LOG_FILE_PATH";
        }
    }
}
