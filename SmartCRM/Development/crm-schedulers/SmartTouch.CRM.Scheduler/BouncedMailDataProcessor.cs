using LandmarkIT.Enterprise.Utilities.Caching;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using Quartz;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Scheduler
{
    [Obsolete("Functionality of this class is moved to SQL Job.")]
    public class BouncedMailDataProcessor : IJob
    {
        static DateTime lastRunDate = DateTime.MinValue;
        private static bool isRunning = default(bool);
        public void Execute(IJobExecutionContext context)
        {
            Trigger(null);
        }
        /// <summary>
        /// Trigger bounced mail process
        /// </summary>
        /// <param name="stateInfo"></param>
        public static void Trigger(Object stateInfo)
        {
            try
            {
                if (isRunning) return;
                isRunning = true;
                lastRunDate = (lastRunDate == DateTime.MinValue) ? DateTime.Now.ToUniversalTime() : lastRunDate;
                Process();
            }
            catch(Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex,
                                DefaultExceptionPolicies.LOG_ONLY_POLICY);
            }
            finally
            {
                isRunning = false;
            }
            
        }
        private static void Process()
        {
            //get the email id's processed after last run date time from campaign recipients table
            //get the last 5 occurances of email status for each soft bounced email
            //check last 5 email statuses if continuous 5 occurances update contact email table else leave it as is
            var campaignRepository = IoC.Container.GetInstance<ICampaignRepository>();
            var contactRepository = IoC.Container.GetInstance<IContactRepository>();
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
