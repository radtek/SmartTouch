using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Note
{
    public interface INoteRepository : IRepository<Note, int>
    {
        Note FindBy(int noteId, int contactId);
        IEnumerable<Note> FindByContact(int contactId);
        Tag.Tag FindTag(string noteName);
        void DeleteNote(int noteId);
        int ContactsCount(int noteId);
    }
}
