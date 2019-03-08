using LandmarkIT.Enterprise.Utilities.Logging;
using Quartz;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Scheduler
{
    public class DailySummaryEmailProcessor : IJob
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
                var accountService = IoC.Container.GetInstance<IAccountService>();
                Logger.Current.Verbose("Request received to send Daily-Summary emails");
                Logger.Current.Informational("Daily-Summary emails processor started at :" + DateTime.Now);
                accountService.SendDailySummaryEmails(new GetDailySummaryEmailsRequest());
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while sending Daily-Summary email in scheduler : " + ex);
            }
            finally
            {
                isRunning = false;
            }
        }
    }
}
