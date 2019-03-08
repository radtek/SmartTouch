using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor
{
    public abstract class CronJobProcessor
    {
        private static object _syncLock = new Object();
        //public CronJobProcessor(CronJobDb cronJob, JobService jobService)
        //{
        //    _CronJob = cronJob;
        //    this._JobService = jobService;
        //}
        public CronJobProcessor(CronJobDb cronJob, JobService jobService, string cacheName)
        {
            _CronJob = cronJob;
            _cacheName = cacheName;
            this._JobService = jobService;
        }
        private JobService _JobService = default(JobService);
        //[ThreadStatic]
        private CronJobDb _CronJob = default(CronJobDb);        

        protected abstract void Execute();
        
        //Set default cache name as Current Cron Job
        private string _cacheName = "CurrentCronJob";
        public virtual void Start()
        {
            try
            {
                Trace.CorrelationManager.ActivityId = this._CronJob.JobUniqueId;
                //_CronJob.LastRunOn = _CronJob.LastNotifyDateTime = DateTime.UtcNow;
                //_CronJob.IsRunning = true;
                lock (_syncLock)
                {
                    _JobService.StartJob(_CronJob.CronJobID, _CronJob.LastRunOn.Value);
                }

                //start the job logic
                this.Execute();
            }
            catch (Exception ex)
            {
                Logger.Current.Critical("error while executing, " + _CronJob.ToString(), ex);
            }
            finally
            {
                try
                {
                    //stop the job
                    this.Stop();
                }
                catch(Exception ef)
                {
                    Logger.Current.Critical("error while stopping the service, " + _CronJob.ToString(), ef);
                }
                
            }
        }


        private static MemoryCache cache = MemoryCache.Default;
        
        public virtual void Stop()
        {
            lock (_syncLock)
            {
                try
                {
                    Logger.Current.Informational("Request for service stopping, cronjobid : " +(byte) _CronJob.CronJobID);
                    _CronJob.LastNotifyDateTime = DateTime.UtcNow;
                    _CronJob.IsRunning = false;
                    _JobService.StopCronJob(_CronJob.CronJobID, _CronJob.LastNotifyDateTime.Value);
                    //var currentJob = cache.Get(_cacheName) as CronJobDb;
                    //if (currentJob == null)
                    //    Logger.Current.Informational("currrentjob from cache is null");
                    //currentJob.LastNotifyDateTime = DateTime.UtcNow;
                    //currentJob.IsRunning = false;
                    cache.Add(_cacheName, _CronJob, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.MaxValue });
                    Logger.Current.Informational("Added to cache " + _CronJob.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("An error occured while stopping cronjob, cronjobid : " + (byte)_CronJob.CronJobID, ex);
                }
            }
        }

        public virtual void UpdateLastNotifyDateTime()
        {
            lock (_syncLock)
            {
                _CronJob.LastNotifyDateTime = DateTime.UtcNow;
                _JobService.UpdateLastNotifyDateTime(_CronJob.CronJobID, _CronJob.LastNotifyDateTime.Value);

                var currentJob = cache.Get(_cacheName) as CronJobDb;
                currentJob.LastNotifyDateTime = DateTime.UtcNow;

                cache.Add(_cacheName, currentJob, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.MaxValue });
            }
        }

    }
}
