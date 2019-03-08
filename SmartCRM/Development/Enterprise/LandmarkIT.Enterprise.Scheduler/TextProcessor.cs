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
    public class TextProcessor : CronJobProcessor
    {
        public TextProcessor(CronJobDb cronJob, JobService jobService, string cacheName)
            : base(cronJob, jobService, cacheName)
        {
        }

        protected override void Execute()
        {
            JobProcessor.SendQueueText();
        }
    }
}
