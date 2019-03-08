using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using SmartTouch.CRM.JobProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.Scheduler
{
    public class MailProcessor : CronJobProcessor
    {
        private int _instance = 1;
        public MailProcessor(CronJobDb cronJob, JobService jobService, string cacheName)
            : base(cronJob, jobService, cacheName)
        {
            try
            {
                _instance = Convert.ToInt16(cacheName.Substring(cacheName.Length - 1));
            }
            catch
            {
                _instance = 1;
            }
            
        }

        protected override void Execute()
        {
            JobProcessor.SendQueueMail(_instance);
        }
    }
}
