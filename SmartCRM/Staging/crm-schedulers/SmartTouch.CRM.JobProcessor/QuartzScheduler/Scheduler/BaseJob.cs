using System;
using System.Diagnostics;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using Quartz;

namespace SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler
{
    public abstract class BaseJob : IJob
    {
        public const string DbJobTypeKey = "dbJobType";

        private static readonly object SyncLock = new Object();
        private readonly JobService _jobService;
        private CronJobDb _cronJob;

        protected readonly Logger Log;

        protected BaseJob()
        {
            Log = Logger.Current;
            _jobService = IoC.Container.GetInstance<JobService>();
        }

        void IJob.Execute(IJobExecutionContext context)
        {
            Execute(context);
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                lock (SyncLock)
                {
                    OnBeforeStart(context);
                    if (_cronJob.IsActive)
                    {
                        ExecuteInternal(context);
                    }
                    OnBeforeStop(context);
                }
            }
            catch (JobExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Critical("Error executing Job, not retrying", ex);
                throw new JobExecutionException(ex, false);
            }
        }

        protected virtual void OnBeforeStart(IJobExecutionContext context)
        {
            var jobType = (CronJobType)context.
                JobDetail.
                JobDataMap.
                GetIntValue(DbJobTypeKey);

            _cronJob = _jobService.GetCronJobByType(jobType);
            if (_cronJob != null && _cronJob.IsActive)
            {
                Trace.CorrelationManager.ActivityId = _cronJob.JobUniqueId;
                _jobService.StartJob(_cronJob.CronJobID, DateTime.UtcNow);
            }
        }

        protected virtual void OnBeforeStop(IJobExecutionContext context)
        {
            if (_cronJob != null && _cronJob.IsActive)
            {
                _jobService.StopCronJob(_cronJob.CronJobID, DateTime.UtcNow);
            }
        }

        protected void RetryJob(IJobExecutionContext context, Exception ex)
        {
            if (context.RefireCount < 5)
            {
                Log.Critical("Error executing Job, will retry", ex);
                throw new JobExecutionException(ex, true);
            }
            Log.Critical("Error executing Job, reached max refires, not retrying anymore", ex);
            throw new JobExecutionException(ex, false);
        }

        protected abstract void ExecuteInternal(IJobExecutionContext context);
    }

    public abstract class BaseJob<T> : BaseJob where T : IJobRequest
    {
        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            var req = GetRequest(context.JobDetail.JobDataMap);
            Execute(context, req);
        }

        protected abstract void Execute(IJobExecutionContext context, T request);

        private static T GetRequest(JobDataMap jobDataMap)
        {
            var value = jobDataMap.GetString("data");
            return value != null ? JsonConvert.DeserializeObject<T>(value) : default(T);
        }
    }
}