using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging.Notes;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.Domain.Notes;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface INoteService
    {
        SaveNoteResponse InsertNote(SaveNoteRequest request);
        UpdateNoteResponse UpdateNote(UpdateNoteRequest request);
        DeleteNoteResponse DeleteNote(DeleteNoteRequest request);
        DeactivateNotesContactResponse ContactDeleteForNote(DeactivateNotesContactRequest request);
        GetNoteResponse GetNote(GetNoteRequest request);
        GetNotesListResponse GetNotesContacts(GetNotesListRequest request);
        GetContactCountsResponse NoteContactsCount(GetContactCountsRequest request);
        ReIndexDocumentResponse ReIndexNotes(ReIndexDocumentRequest request);
        Note UpdateNoteBulkData(int noteId,int accountId, IEnumerable<int> contactIds);
        short GetActionDetailsNoteCategoryID(int accountId, short dropdownValueTypeId, byte dropdownFieldTypeId);
    }
}
