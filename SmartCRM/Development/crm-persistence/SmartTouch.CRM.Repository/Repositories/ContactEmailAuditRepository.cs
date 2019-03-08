using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.ContactAudit;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class ContactEmailAuditRepository : Repository<ContactEmailAudit, int, ContactEmailAuditDb>, IContactEmailAuditRepository
    {
        public ContactEmailAuditRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        public override ContactEmailAudit FindBy(int id)
        {
            throw new NotImplementedException();
        }

        public override ContactEmailAuditDb ConvertToDatabaseType(ContactEmailAudit contactEmailAudit, CRMDb context)
        {
            return Mapper.Map<ContactEmailAudit, ContactEmailAuditDb>(contactEmailAudit);
        }

        public override ContactEmailAudit ConvertToDomain(ContactEmailAuditDb contactEmailAuditDb)
        {
            return Mapper.Map<ContactEmailAuditDb, ContactEmailAudit>(contactEmailAuditDb);
        }

        public override void PersistValueObjects(ContactEmailAudit domainType, ContactEmailAuditDb dbType, CRMDb context)
        {
            //for future use
        }
        public int? UpdateContactEmailStatus(int accountId, int contactId, int campaignId, string email, DateTime snoozeUntill)
        {
            var db = ObjectContextFactory.Create();
            try
            {
                ContactEmailsDb ContactEmail = null;
                ContactEmail = db.ContactEmails.Where(ce => ce.Email == email && ce.ContactID == contactId && ce.IsDeleted==false && ce.IsPrimary == true).FirstOrDefault();
                if (string.IsNullOrEmpty(email))
                    ContactEmail = db.ContactEmails.Where(ce => ce.ContactID == contactId && ce.IsPrimary == true && ce.IsDeleted == false).FirstOrDefault();
                if (ContactEmail != null)
              {
                ContactEmail.EmailStatus = (short)EmailStatus.UnSubscribed;
                if (snoozeUntill != default(DateTime))
                    ContactEmail.SnoozeUntil = snoozeUntill;
                db.SaveChanges();
                return ContactEmail.ContactID;
              }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while updating ContactEmailStatus.", ex);
                return null;
            }
            return null;
        }

        public IEnumerable<ContactEmailAudit> FindAll()
        {
            throw new NotImplementedException();
        }

        public void InsertContactEmailAuditList(List<ContactEmailAudit> EmailAudits)
        {
                            
            using (var db = ObjectContextFactory.Create())
            {
                List<ContactEmailAuditDb> EmailAuditDb = Mapper.Map<IEnumerable<ContactEmailAudit>, IEnumerable<ContactEmailAuditDb>>(EmailAudits).ToList();
                db.BulkInsert<ContactEmailAuditDb>(EmailAuditDb);
                db.SaveChanges();
            }            
        }
    }
}
