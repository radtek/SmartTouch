using LandmarkIT.Enterprise.CommunicationManager.Database;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace LandmarkIT.Enterprise.CommunicationManager.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<MailRegistrationDb> MailRegistrationsRepository { get; }
        IGenericRepository<FtpRegistrationDb> FtpRegistrationsRepository { get; }
        IGenericRepository<TextRegistrationDb> TextRegistrationsRepository { get; }
        IGenericRepository<SentTextDb> SentTextRepository { get; }
        IGenericRepository<SentTextDetailsDb> SentTextDetailsRepository { get; }
        IGenericRepository<SentMailDb> SentMailsRepository { get; }
        IGenericRepository<SentMailDetailDb> SentMailDetailsRepository { get; }
        IGenericRepository<SentMailQueueDb> SentMailQueuesRepository { get; }
        IGenericRepository<JobDb> JobsRepository { get; }
        IGenericRepository<ScheduledJobDb> ScheduledJobsRepository { get; }
        IGenericRepository<SendTextQueueDb> SendTextQueueRepository { get; }
        IGenericRepository<CronJobDb> CronJobsRepository { get; }
        IGenericRepository<CronJobHistoryDb> CronJobHistoryRepository { get; }
        IGenericRepository<ReceivedMailInfoDb> ReceivedMailInfoRepository { get; }
        IGenericRepository<EmailLinksDb> EmailLinksRepository { get; }
        IGenericRepository<EmailStatisticsDb> EmailStatisticsRepository { get; }
        void Commit();
    }
}
