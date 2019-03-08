using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ContactAudit;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.ContactAudit
{
   public interface IContactEmailAuditRepository : IRepository<ContactEmailAudit, int>
    {
       int? UpdateContactEmailStatus(int accountId, int contactId, int campaignId,string email,DateTime snoozeUntill);
       void InsertContactEmailAuditList(List<ContactEmailAudit> EmailAudits);
    }
}
