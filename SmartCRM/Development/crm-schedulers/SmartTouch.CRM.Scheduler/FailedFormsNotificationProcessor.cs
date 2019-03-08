using LandmarkIT.Enterprise.Utilities.Logging;
using Quartz;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Scheduler
{
    public class FailedFormsNotificationProcessor : IJob
    {
        private static bool isRunning = default(bool);
        public void Execute(IJobExecutionContext context)
        {
            Trigger(null);

        }
        public static void Trigger(Object stateInfo)
        {
            try
            {
                if (isRunning) return;
                isRunning = true;
                var formService = IoC.Container.GetInstance<IFormService>();
                Logger.Current.Verbose("Request received to send failed Form submissions report");
                Logger.Current.Informational("Failed Form submissions processor started at :" + DateTime.Now);
                formService.FailedFormsSummaryEmail();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while sending failed form submissions report in scheduler : " + ex);
            }
            finally
            {
                isRunning = false;
            }
        }
    }
}
