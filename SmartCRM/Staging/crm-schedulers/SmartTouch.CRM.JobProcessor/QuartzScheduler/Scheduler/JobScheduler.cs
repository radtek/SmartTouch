using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;

namespace SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler
{
    public class JobScheduler : IJobScheduler
    {
        private readonly IScheduler _scheduler;

        public string JobGroup { get; set; }

        public JobScheduler()
        {
            JobGroup = "Default";

           var config = (NameValueCollection)ConfigurationManager.GetSection("quartz");
            _scheduler = new StdSchedulerFactory(config).GetScheduler();
        }

        /// <summary>
        /// Creates an async job based on the given type
        /// </summary>
        public JobKey Queue<TRequest>(TRequest request) where TRequest : IJobRequest
        {
            var jobType = JobTypeFor<TRequest>();        
            if (jobType == null)
                throw new ArgumentException(string.Format("Couldn't find job to handle request {0}", typeof(TRequest)));

            var jobName = jobType.Name + Guid.NewGuid().ToString("N");      
            var triggerName = jobName + "Trigger";
         
            var jobDetail = new JobDetailImpl(jobName, JobGroup, jobType);
            FillDataMap(jobDetail.JobDataMap, request);

            var trigger = new SimpleTriggerImpl(triggerName, 0, new TimeSpan(0));

            _scheduler.ScheduleJob(jobDetail, trigger);

            return jobDetail.Key;
        }

        public bool IsJobExists(JobKey jobKey)
        {
            return _scheduler.CheckExists(jobKey);
        }

        private static void FillDataMap(JobDataMap jobDataMap, IJobRequest request)
        {
            jobDataMap.Put("data", JsonConvert.SerializeObject(request));
        }

        private static Type JobTypeFor<T>() where T : IJobRequest
        {
            return typeof(T).Assembly.GetTypes()
               .SingleOrDefault(x => typeof(IJob).IsAssignableFrom(x)
                   && !x.IsAbstract
                   && x.BaseType.IsGenericType
                   && x.BaseType.GetGenericArguments().First() == typeof(T));
        }
    }
}
