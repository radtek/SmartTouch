using LandmarkIT.Enterprise.Common;
using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using System.Data.Entity;

namespace LandmarkIT.Enterprise.CommunicationManager.Repositories
{
    public class EfUnitOfWork : DbContext, IUnitOfWork
    {
        #region Other Private Variables
        private static object _syncLock = new object();
        #endregion

        #region Constructor
        public EfUnitOfWork() : base(EnterpriseCommunicationConfigurationSection.Instance.CommunicationManager.ConnectionStringName) { }
        #endregion

        #region Private Variables
        private EfGenericRepository<MailRegistrationDb> _MailRegistrationsRepository;
        private EfGenericRepository<FtpRegistrationDb> _FtpRegistrationsRepository;
        private EfGenericRepository<TextRegistrationDb> _TextRegistrationsRepository;
        private EfGenericRepository<SentTextDb> _SentTextRepository;
        private EfGenericRepository<SentTextDetailsDb> _SentTextDetailsRepository;
        private EfGenericRepository<SendTextQueueDb> _SendTextQueueRepository;

        private EfGenericRepository<SentMailDb> _SentMailsRepository;
        private EfGenericRepository<SentMailDetailDb> _SentMailDetailsRepository;
        private EfGenericRepository<SentMailQueueDb> _SentMailQueuesRepository;
        private EfGenericRepository<EmailLinksDb> _EmailLinksRepository;
        private EfGenericRepository<EmailStatisticsDb> _EmailStatisticsRepository;

        private EfGenericRepository<JobDb> _JobsRepository;
        private EfGenericRepository<ScheduledJobDb> _ScheduledJobsRepository;

        private EfGenericRepository<CronJobDb> _CronJobsRepository;
        private EfGenericRepository<CronJobHistoryDb> _CronJobHistoryRepository;
        private EfGenericRepository<ReceivedMailInfoDb> _ReceivedMailInfoRepository;
        #endregion

        #region DbSet's
        public DbSet<MailRegistrationDb> MailRegistrations { get; set; }
        public DbSet<FtpRegistrationDb> FtpRegistrationDbs { get; set; }
        public DbSet<TextRegistrationDb> TextRegistrationDbs { get; set; }
        public DbSet<SentTextDb> SentTextDbs { get; set; }
        public DbSet<SentTextDetailsDb> SentTextDetailsDbs { get; set; }
        public DbSet<SendTextQueueDb> SendTextQueueDbs { get; set; }

        public DbSet<SentMailDb> SentMailDbs { get; set; }
        public DbSet<SentMailDetailDb> SentMailDetailDbs { get; set; }
        public DbSet<SentMailQueueDb> SentMailQueueDbs { get; set; }
        public DbSet<EmailLinksDb> EmailLinksDbs { get; set; }
        public DbSet<EmailStatisticsDb> EmailStatisticsDbs { get; set; }

        public DbSet<JobDb> JobDbs { get; set; }
        public DbSet<ScheduledJobDb> ScheduledJobDbs { get; set; }

        public DbSet<CronJobDb> CronJobDbs { get; set; }
        public DbSet<CronJobHistoryDb> CronJobHistoryDbs { get; set; }
        public DbSet<ReceivedMailInfoDb> ReceivedMailInfoDb { get; set; }
        #endregion

        #region Repository Properties & IUnitOfWork Implementation
        public IGenericRepository<MailRegistrationDb> MailRegistrationsRepository
        {
            get
            {
                lock (_syncLock) { _MailRegistrationsRepository = new EfGenericRepository<MailRegistrationDb>(MailRegistrations, this); }
                return _MailRegistrationsRepository;
            }
        }
        public IGenericRepository<FtpRegistrationDb> FtpRegistrationsRepository
        {
            get
            {
                lock (_syncLock) { _FtpRegistrationsRepository = new EfGenericRepository<FtpRegistrationDb>(FtpRegistrationDbs, this); }
                return _FtpRegistrationsRepository;
            }
        }
        public IGenericRepository<TextRegistrationDb> TextRegistrationsRepository
        {
            get
            {
                lock (_syncLock) { _TextRegistrationsRepository = new EfGenericRepository<TextRegistrationDb>(TextRegistrationDbs, this); }
                return _TextRegistrationsRepository;
            }
        }
        public IGenericRepository<SendTextQueueDb> SendTextQueueRepository
        {
            get
            {
                lock (_syncLock) { _SendTextQueueRepository = new EfGenericRepository<SendTextQueueDb>(SendTextQueueDbs, this); }
                return _SendTextQueueRepository;
            }
        }

        public IGenericRepository<SentMailDb> SentMailsRepository
        {
            get
            {
                lock (_syncLock) { _SentMailsRepository = new EfGenericRepository<SentMailDb>(SentMailDbs, this); }
                return _SentMailsRepository;
            }
        }
        public IGenericRepository<SentMailDetailDb> SentMailDetailsRepository
        {
            get
            {
                lock (_syncLock) { _SentMailDetailsRepository = new EfGenericRepository<SentMailDetailDb>(SentMailDetailDbs, this); }
                return _SentMailDetailsRepository;
            }
        }

        public IGenericRepository<EmailLinksDb> EmailLinksRepository
        {
            get
            {
                lock (_syncLock) { _EmailLinksRepository = new EfGenericRepository<EmailLinksDb>(EmailLinksDbs, this); }
                return _EmailLinksRepository;
            }
        }

        public IGenericRepository<EmailStatisticsDb> EmailStatisticsRepository
        {
            get
            {
                lock (_syncLock) { _EmailStatisticsRepository = new EfGenericRepository<EmailStatisticsDb>(EmailStatisticsDbs, this); }
                return _EmailStatisticsRepository;
            }
        }

        public IGenericRepository<SentTextDetailsDb> SentTextDetailsRepository
        {
            get
            {
                lock (_syncLock) { _SentTextDetailsRepository = new EfGenericRepository<SentTextDetailsDb>(SentTextDetailsDbs, this); }
                return _SentTextDetailsRepository;
            }
        }
        public IGenericRepository<SentTextDb> SentTextRepository
        {
            get
            {
                lock (_syncLock) { _SentTextRepository = new EfGenericRepository<SentTextDb>(SentTextDbs, this); }
                return _SentTextRepository;
            }
        }
        public IGenericRepository<SentMailQueueDb> SentMailQueuesRepository
        {
            get
            {
                lock (_syncLock) { _SentMailQueuesRepository = new EfGenericRepository<SentMailQueueDb>(SentMailQueueDbs, this); }
                return _SentMailQueuesRepository;
            }
        }

        public IGenericRepository<JobDb> JobsRepository
        {
            get
            {
                lock (_syncLock) { _JobsRepository = new EfGenericRepository<JobDb>(JobDbs, this); }
                return _JobsRepository;
            }
        }
        public IGenericRepository<ScheduledJobDb> ScheduledJobsRepository
        {
            get
            {
                lock (_syncLock) { _ScheduledJobsRepository = new EfGenericRepository<ScheduledJobDb>(ScheduledJobDbs, this); }
                return _ScheduledJobsRepository;
            }
        }
        public IGenericRepository<CronJobDb> CronJobsRepository
        {
            get
            {
                lock (_syncLock) { _CronJobsRepository = new EfGenericRepository<CronJobDb>(CronJobDbs, this); }
                return _CronJobsRepository;
            }
        }
        public IGenericRepository<CronJobHistoryDb> CronJobHistoryRepository
        {
            get
            {
                lock (_syncLock) { _CronJobHistoryRepository = new EfGenericRepository<CronJobHistoryDb>(CronJobHistoryDbs, this); }
                return _CronJobHistoryRepository;
            }
        }
        public IGenericRepository<ReceivedMailInfoDb> ReceivedMailInfoRepository
        {
            get
            {
                lock (_syncLock) { _ReceivedMailInfoRepository = new EfGenericRepository<ReceivedMailInfoDb>(ReceivedMailInfoDb, this); }
                return _ReceivedMailInfoRepository;
            }
        }
        #endregion

        public void Commit()
        {
            this.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MailRegistrationDb>().ToTable("MailRegistration");
            modelBuilder.Entity<SentMailDb>().ToTable("SentMails");
            modelBuilder.Entity<SentMailDetailDb>().ToTable("SentMailDetails");
            modelBuilder.Entity<SentTextDb>().ToTable("TextResponse");
            modelBuilder.Entity<SentTextDetailsDb>().ToTable("TextResponseDetails");
            modelBuilder.Entity<SendTextQueueDb>().ToTable("SendTextQueue");
            modelBuilder.Entity<SentMailQueueDb>().ToTable("SentMailQueue");
            modelBuilder.Entity<EmailLinksDb>().ToTable("EmailLinks");
            modelBuilder.Entity<EmailStatisticsDb>().ToTable("EmailStatistics");

            modelBuilder.Entity<TextRegistrationDb>().ToTable("TextRegistration");

            modelBuilder.Entity<JobDb>().ToTable("Jobs");
            modelBuilder.Entity<ScheduledJobDb>().ToTable("ScheduledJobs");

            modelBuilder.Entity<FtpRegistrationDb>().ToTable("FTPRegistration");
            modelBuilder.Entity<CronJobDb>().ToTable("CronJobs");
            modelBuilder.Entity<CronJobHistoryDb>().ToTable("CronJobHistory");
            modelBuilder.Entity<ReceivedMailInfoDb>().ToTable("ReceivedMailInfo");
        }
    }
}

