using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Notes
{
    public interface INoteRepository : IRepository<Note, int>
    {
        Note FindBy(int noteId, int contactId,int accountId);
        IEnumerable<Note> FindByContact(int contactId,int accountId);
        Tags.Tag FindTag(string noteName);
        IEnumerable<int> DeleteNote(int noteId,int contactId);
        int ContactsCount(int noteId);
        Note GetNoteById(int noteId,int accountId);
        bool IsNoteFromSelectAll(int noteId,int accountId);
        Note FindNoteById(int noteId, int accountId);
        short GetActionDetailsNoteCategoryID(int accountId,short dropdownValueTypeId,byte dropdownFieldTypeId);
    }
}
