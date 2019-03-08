using Quartz;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using System;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;

namespace SmartTouch.CRM.JobProcessor.Jobs.Schedulers
{
    public class DailySummaryEmailJob : BaseJob
    {
        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                var accountService = IoC.Container.GetInstance<IAccountService>();
                Logger.Current.Verbose("Request received to send Daily-Summary emails");
                Logger.Current.Informational("Daily-Summary emails processor started at :" + DateTime.Now);
                accountService.SendDailySummaryEmails(new GetDailySummaryEmailsRequest());
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while sending Daily-Summary email in scheduler : " + ex);
            }
        }
    }
}
