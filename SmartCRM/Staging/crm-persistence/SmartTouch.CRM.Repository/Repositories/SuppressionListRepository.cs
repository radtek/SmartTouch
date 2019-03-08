using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Domain.SuppressedEmails;
using SmartTouch.CRM.Domain.SuppressionList;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class SuppressionListRepository : Repository<SuppressedEmail, int, SuppressedEmailsDb>, ISuppressionListRepository
    {
        public SuppressionListRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {

        }

        /// <summary>
        /// Inserts the suppressed emails list.
        /// </summary>
        /// <param name="suppressedEmails">The suppressed emails.</param>
        /// <returns></returns>
        public IEnumerable<SuppressedEmail> InsertSuppressedEmailsList(IEnumerable<SuppressedEmail> suppressedEmails)
        {
            var db = ObjectContextFactory.Create();
            if (suppressedEmails.IsAny())
            {
                List<SuppressedEmailsDb> dbObjects = new List<SuppressedEmailsDb>();
                foreach (SuppressedEmail email in suppressedEmails)
                {
                    SuppressedEmailsDb emaildb = Mapper.Map<SuppressedEmail, SuppressedEmailsDb>(email);
                    dbObjects.Add(emaildb);
                }
                db.SuppressedEmails.AddRange(dbObjects);
                db.SaveChanges();
                return Mapper.Map<List<SuppressedEmailsDb>, List<SuppressedEmail>>(dbObjects);
            }
            else
                return null;
        }

        /// <summary>
        /// Inserts the suppressed domains list.
        /// </summary>
        /// <param name="suppressedDomains">The suppressed domains.</param>
        /// <returns></returns>
        public IEnumerable<SuppressedDomain> InsertSuppressedDomainsList(IEnumerable<SuppressedDomain> suppressedDomains)
        {
            var db = ObjectContextFactory.Create();
            if (suppressedDomains.IsAny())
            {
                List<SuppressedDomainsDb> domains = new List<SuppressedDomainsDb>();
                foreach(SuppressedDomain domain in suppressedDomains)
                {
                    SuppressedDomainsDb domainDb = Mapper.Map<SuppressedDomain, SuppressedDomainsDb>(domain);
                    domains.Add(domainDb);
                }
                db.SuppressedDomains.AddRange(domains);
                db.SaveChanges();
                return Mapper.Map<List<SuppressedDomainsDb>, List<SuppressedDomain>>(domains);
            }
            else
                return null;
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override SuppressedEmailsDb ConvertToDatabaseType(SuppressedEmail domainType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override SuppressedEmail ConvertToDomain(SuppressedEmailsDb databaseType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override SuppressedEmail FindBy(int accountId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void PersistValueObjects(SuppressedEmail domainType, SuppressedEmailsDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<SuppressedEmail> FindAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<SuppressedEmail> FindAll(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var emailsDb = db.SuppressedEmails.Where(w => w.AccountID == accountId);
            if (emailsDb != null && emailsDb.Any())
                return AutoMapper.Mapper.Map<IEnumerable<SuppressedEmailsDb>, IEnumerable<SuppressedEmail>>(emailsDb);
            else
                return null;
        }

        /// <summary>
        /// Finds all emails.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="lastIndexedId">The last indexed identifier.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public IEnumerable<SuppressedEmail> FindAllEmails(int accountId, int lastIndexedId, int limit)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<SuppressedEmail> emails = new List<SuppressedEmail>();
            var emailsDb = db.SuppressedEmails.Where(w => w.AccountID == accountId && w.SuppressedEmailID > lastIndexedId).Take(limit);
            if (emailsDb != null)
                return emails = AutoMapper.Mapper.Map<IEnumerable<SuppressedEmailsDb>, IEnumerable<SuppressedEmail>>(emailsDb);
            else
                return null;
        }

        /// <summary>
        /// Finds all domains.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="lastIndexedId">The last indexed identifier.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        public IEnumerable<SuppressedDomain> FindAllDomains(int accountId, int lastIndexedId, int limit)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<SuppressedDomain> domains = new List<SuppressedDomain>();
            var domainsDb = db.SuppressedDomains.Where(w => w.AccountID == accountId && w.SuppressedDomainID > lastIndexedId).Take(limit);
            if (domainsDb != null)
                return domains = AutoMapper.Mapper.Map<IEnumerable<SuppressedDomainsDb>, IEnumerable<SuppressedDomain>>(domainsDb);
            else
                return null;
        }

        /// <summary>
        /// Removes the suppressed email.
        /// </summary>
        /// <param name="suppressedEmailId">The suppressed email identifier.</param>
        /// <returns></returns>
        public bool RemoveSuppressedEmail(int suppressedEmailId)
        {
            var db = ObjectContextFactory.Create();
            var emailDb = db.SuppressedEmails.Where(w => w.SuppressedEmailID == suppressedEmailId).FirstOrDefault();
            db.SuppressedEmails.Remove(emailDb);
            int count = db.SaveChanges();
            return count > 0 ? true : false;
        }
    }
}
