using Quartz;

namespace SmartTouch.CRM.JobProcessor.QuartzScheduler
{
    public interface IJobScheduler
    {
        JobKey Queue<TRequest>(TRequest request) where TRequest : IJobRequest;
        bool IsJobExists(JobKey jobKey);
    }
}