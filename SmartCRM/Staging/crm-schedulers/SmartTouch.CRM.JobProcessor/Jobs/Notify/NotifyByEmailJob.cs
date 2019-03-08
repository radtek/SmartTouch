using LandmarkIT.Enterprise.CommunicationManager.Operations;
using Quartz;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.Jobs.Notify
{
    public class NotifyByEmailJob : BaseJob
    {
        private readonly JobService _jobService;
        private readonly JobServiceConfiguration _jobConfig;

        public NotifyByEmailJob(
            JobService jobService,
            JobServiceConfiguration jobConfig)
        {
            _jobService = jobService;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            Log.Informational("Request received for sending queue mails");
            var instances = 4;

            for (var instance = 1; instance < (instances + 1); instance++)
            {
                _jobService.ProcessMailQueue(instance);
            }
        }
    }
}
