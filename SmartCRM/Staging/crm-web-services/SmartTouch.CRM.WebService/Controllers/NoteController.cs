using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Notes;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.WebService.Helpers;
using System.Net.Http;
using System.Web.Http;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    ///Creating note controller for notes module 
    /// </summary>
    public class NoteController : SmartTouchApiController
    {
        readonly INoteService noteService;

        /// <summary>
        /// Creating constructor for note controller for accessing
        /// </summary>
        /// <param name="noteService"></param>
        public NoteController(INoteService noteService)
        {
            this.noteService = noteService;
        }

        /// <summary>
        /// Insert a new note.
        /// </summary>
        /// <param name="viewModel">Properties of a new note</param>
        /// <returns>Note Insertion Details</returns>
        [Route("Note/InsertNote")]
        [HttpPost]
        public HttpResponseMessage PostNote(NoteViewModel viewModel)
        {
            SaveNoteResponse response = noteService.InsertNote(new SaveNoteRequest() { NoteViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// update a note.
        /// </summary>
        /// <param name="viewModel">update a existing note</param>
        /// <returns>Note Updation Details</returns>
        [Route("Note/UpdateNote")]
        [HttpPut]
        public HttpResponseMessage PutNote(NoteViewModel viewModel)
        {
            UpdateNoteResponse response = noteService.UpdateNote(new UpdateNoteRequest() { NoteViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Note by id.
        /// </summary>
        /// <param name="id">The Id value that uniquely identifies a note.</param>
        /// <returns>Note Details</returns>
        [Route("Note/GetNote")]
        public HttpResponseMessage GetNote(int id)
        {
            GetNoteResponse response = noteService.GetNote(new GetNoteRequest() { NoteId = id,AccountId = this.AccountId });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Delete Note by id.
        /// </summary>
        /// <param name="id">The Id value that uniquely identifies a note.</param>
        /// <param name="contactId">The Id value that uniquely identifies a note.</param>
        /// <returns>Deleted Note Details</returns>
        [Route("Note/DeleteNote")]
        public HttpResponseMessage DeleteNote(int id, int contactId)
        {
            DeactivateNotesContactResponse response = noteService.ContactDeleteForNote(new DeactivateNotesContactRequest() { noteId = id, ContactId = contactId ,AccountId = this.AccountId});
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Note for contacts.
        /// </summary>
        /// <param name="contactId">The Id value that uniquely identifies a note.</param>
        /// <returns>Contact Created Note Details</returns>
        [Route("Note/GetContactNote")]
        public HttpResponseMessage GetContactNotes(int contactId)
        {
            GetNotesListResponse response = noteService.GetNotesContacts(new GetNotesListRequest() { Id = contactId,AccountId = this.AccountId });
            return Request.BuildResponse(response);
        }
    }
}