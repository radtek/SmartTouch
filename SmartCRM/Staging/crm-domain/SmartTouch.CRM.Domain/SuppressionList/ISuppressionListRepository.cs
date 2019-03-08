using SmartTouch.CRM.Domain.SuppressedEmails;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.SuppressionList
{
    public interface ISuppressionListRepository: IRepository<SuppressedEmail, int>
    {
        IEnumerable<SuppressedEmail> InsertSuppressedEmailsList(IEnumerable<SuppressedEmail> suppressedEmails);
        IEnumerable<SuppressedDomain> InsertSuppressedDomainsList(IEnumerable<SuppressedDomain> suppressedDomains);
        IEnumerable<SuppressedEmail> FindAll(int accountId);
        IEnumerable<SuppressedEmail> FindAllEmails(int accountId, int lastIndexedId, int limit);
        IEnumerable<SuppressedDomain> FindAllDomains(int accountId, int lastIndexedId, int limit);
        bool RemoveSuppressedEmail(int suppressedEmailId);
    }
}
