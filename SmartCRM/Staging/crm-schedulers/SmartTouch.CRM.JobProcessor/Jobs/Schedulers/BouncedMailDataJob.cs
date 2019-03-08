using Quartz;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using System;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using System.Linq;
using System.Text;

namespace SmartTouch.CRM.JobProcessor.Jobs.Schedulers
{
    [Obsolete("Functionality of this class is moved to SQL Job.")]
    public class BouncedMailDataJob : BaseJob
    {
        static DateTime _lastRunDate = DateTime.MinValue;

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                _lastRunDate = (_lastRunDate == DateTime.MinValue) ? DateTime.Now.ToUniversalTime() : _lastRunDate;
                Process();
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex,
                                DefaultExceptionPolicies.LOG_ONLY_POLICY);
            }
        }

        private static void Process()
        {
            //get the email id's processed after last run date time from campaign recipients table
            //get the last 5 occurances of email status for each soft bounced email
            //check last 5 email statuses if continuous 5 occurances update contact email table else leave it as is
            //var campaignRepository = IoC.Container.GetInstance<ICampaignRepository>();
            //var contactRepository = IoC.Container.GetInstance<IContactRepository>();
            //get last 5 statuses
            //var recipients = campaignRepository.GetCampaignRecipientsByLastModifiedDate(lastRunDate);

            //recipients.ToList().ForEach(r =>
            //    {
            //        var statuses = campaignRepository.GetDeliveryStatusesByMailId(r.To, 5);
            //        if(statuses.All(s=> s== Entities.CampaignDeliveryStatus.SoftBounce) && statuses.Count() == 5)
            //        {
            //            try
            //            {
            //                contactRepository.UpdateContactEmail(r.ContactID, r.To, EmailStatus.HardBounce,null);
            //            }
            //            catch(Exception ex)
            //            {
            //                ExceptionHandler.Current.HandleException(ex, 
            //                    DefaultExceptionPolicies.LOG_ONLY_POLICY, 
            //                    values: new object[]
            //                    { 
            //                        "Exception while updating contact email to hardbounce.", 
            //                        "ContactId: " + r.ContactID, 
            //                        "Email: " + r.To
            //                    });
            //            }

            //        }
            //    });
        }
        /// <summary>
        /// Log details to logger.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        private static void Log(string message, params object[] args)
        {
            StringBuilder format = new StringBuilder();
            var idx = 0;
            args.ToList().ForEach(s =>
            {
                format.Append(" {" + (idx++) + "} :");
            });
#if DEBUG
            Console.WriteLine(string.Format(message + format.ToString().TrimEnd(':'), args));
#endif
            Logger.Current.Informational(string.Format(message + format.ToString().TrimEnd(':'), args));
        }
    }
}
