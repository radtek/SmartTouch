using System.Collections.Generic;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Tag
{
    public interface ITagRepository : IRepository<Tag, int>
    {
        IEnumerable<Tag> Search(string name);
        Tag SaveContactTag(int contactId, string tagName);
        IEnumerable<string> FindByContact(int contactId);
        Tag FindBy(string tagName);
        void DeleteForContact(int tagId, int contactId);
    }
}
