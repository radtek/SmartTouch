using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Communication
{
    public interface IAttachmentRepository : IRepository<Attachment, int>
    {
        IEnumerable<Attachment> FindAll(int contactId, long attachementId);
        IEnumerable<Attachment> FindAttachement(int contactId, string originalFilename, string storageFilename);
        IEnumerable<Attachment> FindAllAttachments(int? contactId,int? opportunityID, string page, int limit, int pageNumber);
        void DeleteAttachment(long attachementId);
        int TotalNumberOfAttachments(int? contactId, int? opportunityID, string page);
    }
}
