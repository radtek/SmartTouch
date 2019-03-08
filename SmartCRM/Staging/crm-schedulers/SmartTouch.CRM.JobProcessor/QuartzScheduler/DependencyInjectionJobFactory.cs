using System;
using Quartz;
using Quartz.Spi;

namespace SmartTouch.CRM.JobProcessor.QuartzScheduler
{
    public class DependencyInjectionJobFactory : IJobFactory
    {
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var jobDetail = bundle.JobDetail;
                var jobType = jobDetail.JobType;

                // Return job that is registrated in container
                return (IJob)IoC.Container.GetInstance(jobType);
            }
            catch (Exception e)
            {
                throw new SchedulerException("Problem instantiating class", e);
            }
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}