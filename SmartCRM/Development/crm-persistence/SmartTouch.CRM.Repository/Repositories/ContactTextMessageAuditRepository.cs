using AutoMapper;
using SmartTouch.CRM.Domain.ContactAudit;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class ContactTextMessageAuditRepository : Repository<ContactTextMessageAudit, int, ContactTextMessageAuditDb>,IContactTextMessageAuditRepository
    {
        public ContactTextMessageAuditRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        public override ContactTextMessageAudit FindBy(int id)
        {
            throw new NotImplementedException();
        }

        public override ContactTextMessageAuditDb ConvertToDatabaseType(ContactTextMessageAudit contactTextMessageAudit, CRMDb context)
        {
            return Mapper.Map<ContactTextMessageAudit, ContactTextMessageAuditDb>(contactTextMessageAudit);
        }

        public override ContactTextMessageAudit ConvertToDomain(ContactTextMessageAuditDb contactTextMessageAuditdb)
        {
            return Mapper.Map<ContactTextMessageAuditDb, ContactTextMessageAudit>(contactTextMessageAuditdb);
        }

        public override void PersistValueObjects(ContactTextMessageAudit domainType, ContactTextMessageAuditDb dbType, CRMDb context)
        {
           //for future use
        }

        public IEnumerable<ContactTextMessageAudit> FindAll()
        {
            throw new NotImplementedException();
        }
    }
}
