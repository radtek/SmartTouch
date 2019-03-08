using LandmarkIT.Enterprise.CommunicationManager.Operations;
using Quartz;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.Jobs.Notify
{
    public class NotifyByTextJob : BaseJob
    {
        private readonly JobService _jobService;

        public NotifyByTextJob(
            JobService jobService)
        {
            _jobService = jobService;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            Log.Informational("Request received for sending queue text messages");
            _jobService.ProcessTextQueue();
        }
    }
}
