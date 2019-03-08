using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System.Linq.Expressions;
using System.Data.Entity;
using SmartTouch.CRM.Entities;
using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class CommunicationRepository : Repository<CommunicationTracker, int, CommunicationTrackerDb>, ICommunicationRepository
    {
        public CommunicationRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CommunicationTracker> FindAll()
        {
            var db = ObjectContextFactory.Create();
            var varCommunicationtracker = db.CommunicationTrackers.ToList();

            foreach (CommunicationTrackerDb dc in varCommunicationtracker)
                yield return Mapper.Map<CommunicationTrackerDb, CommunicationTracker>(dc);
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override CommunicationTracker FindBy(int id)
        {
            var target = default(CommunicationTracker);
            try
            {
                var db = ObjectContextFactory.Create();
                CommunicationTrackerDb communicationTrackerDb = db.CommunicationTrackers.SingleOrDefault(c => c.CommunicationTrackerID == id);
                if (communicationTrackerDb != null)
                    target = ConvertToDomain(communicationTrackerDb);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting communication tracker information by using id", ex);
            }
            return target;
        }

        /// <summary>
        /// Finds the by contact identifier.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="CommunicationType">Type of the communication.</param>
        /// <returns></returns>
        public CommunicationTracker FindByContactId(int contactId, CommunicationType CommunicationType)
        {
            var target = default(CommunicationTracker);
            try
            {
                var db = ObjectContextFactory.Create();
                CommunicationTrackerDb communicationTrackerDb = db.CommunicationTrackers.Where(c => c.ContactID == contactId && c.CommunicationTypeID == CommunicationType).FirstOrDefault();
                if (communicationTrackerDb != null) target = ConvertToDomain(communicationTrackerDb);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting communication tracker information by using contactId and communicationtype", ex);
            }
            return target;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="CommunicationType">Type of the communication.</param>
        /// <returns></returns>
        public CommunicationTracker FindAll(long contactId, CommunicationType CommunicationType)
        {
            var target = default(CommunicationTracker);
            try
            {
                var db = ObjectContextFactory.Create();
                CommunicationTrackerDb communicationTrackerDb = db.CommunicationTrackers.Where(c => c.ContactID == contactId && c.CommunicationTypeID == CommunicationType).FirstOrDefault();
                if (communicationTrackerDb != null) target = ConvertToDomain(communicationTrackerDb);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while performing getting all communication details by using contactId and communicationtype", ex);
                throw;
            }
            return target;

        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="communicationTrackerDb">The communication tracker database.</param>
        /// <returns></returns>
        public override CommunicationTracker ConvertToDomain(CommunicationTrackerDb communicationTrackerDb)
        {
            CommunicationTracker modelCommunicationTracker = new CommunicationTracker();
            try
            {
                Mapper.Map<CommunicationTrackerDb, CommunicationTracker>(communicationTrackerDb, modelCommunicationTracker);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while converting DbObject(CommunicationTrackerDb) to Domain(CommunicationTracker)", ex);
            }
            return modelCommunicationTracker;
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        public override CommunicationTrackerDb ConvertToDatabaseType(CommunicationTracker domainType, CRMDb db)
        {
            CommunicationTrackerDb communicationTrackerDb = null;
            try
            {
                communicationTrackerDb = Mapper.Map<CommunicationTracker, CommunicationTrackerDb>(domainType as CommunicationTracker);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while converting Domain(CommunicationTracker) to DbObject(CommunicationTrackerDb)", ex);
            }
            return communicationTrackerDb;
        }

        public override void PersistValueObjects(CommunicationTracker domainType, CommunicationTrackerDb communicationTrackerDb, CRMDb db)
        {
            //for future use
        }

        /// <summary>
        /// Gets the contact email.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public Email GetContactEmail(int contactId, int accountId)
        {
            Email email = new Email();
            var db = ObjectContextFactory.Create();
            var contactEmail = db.ContactEmails.Where(ce => ce.ContactID == contactId && ce.AccountID == accountId && (ce.EmailStatus == (byte)EmailStatus.NotVerified || ce.EmailStatus == (byte)EmailStatus.SoftBounce
                || ce.EmailStatus == (byte)EmailStatus.Subscribed || ce.EmailStatus == (byte)EmailStatus.Verified) && ce.IsPrimary == true).FirstOrDefault();
            if (contactEmail != null)
                email = Mapper.Map<ContactEmailsDb, Email>(contactEmail);
            return email;
        }
    }
}
