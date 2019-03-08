using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SmartTouch.CRM.WebService.Controllers;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Notes;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.Tests;

using Moq;
using Moq.Linq;

namespace SmartTouch.CRM.WebService.Tests.Notes
{
    [TestClass]
    public class NotesControllerTests : ControllerTestBase
    {
        Mock<INoteService> mockNoteService;
        MockRepository mockRepository;

        public const int SAMPLE_NOTE_ID = 1;
        public const int SAMPLE_CONTACT_ID = 1;

        [TestInitialize]
        public void InitializeTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            var mockService = mockRepository.Create<INoteService>();
            mockNoteService = mockService;
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        /// <summary>
        /// Search All Notes for contacts TestMethod
        /// </summary>
        [TestMethod]
        public void Notes_GetAllNotesforContacts_ValidNote()
        {
            NoteController controller = new NoteController(mockNoteService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/notes", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetNotesListRequest>();

            GetNotesListResponse response = mockRepository.Create<GetNotesListResponse>().Object;
            response.NotesListViewModel = MockData.CreateMockList<NoteViewModel>(mockRepository).Select(c => c.Object).ToList();
            mockNoteService.Setup(c => c.GetNotesContacts(It.IsAny<GetNotesListRequest>())).Returns(response);
            var httpResponseMessage = controller.GetContactNotes(SAMPLE_CONTACT_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetNotesListResponse>().ContinueWith(
            t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(10, contactResponse.NotesListViewModel.Count());
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactResponse.Exception, null);
        }

        /// <summary>
        /// Search All Notes for contacts if exception occured TestMethod
        /// </summary>
        [TestMethod]
        public void GetContactNotes_GetContactNotebyContactIdRuntimeError_500InternalServerError()
        {
            NoteController controller = new NoteController(mockNoteService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/notes", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetNotesListResponse>();
            mockNoteService.Setup(cs => cs.GetNotesContacts(It.IsAny<GetNotesListRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.GetContactNotes(SAMPLE_CONTACT_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetNotesListResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);
        }

        /// <summary>
        /// Get note for individual contact TestMethod
        /// </summary>
        [TestMethod]
        public void GetNote_GetNoteforContact_ReturnSuccess()
        {
            NoteController controller = new NoteController(mockNoteService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/notes/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetNoteResponse>();

            GetNoteResponse response = mockRepository.Create<GetNoteResponse>().Object;
            response.NoteViewModel = MockData.CreateMockList<NoteViewModel>(mockRepository).Select(c=>c.Object).FirstOrDefault();
            mockNoteService.Setup(c => c.GetNote(It.IsAny<GetNoteRequest>())).Returns(response);
            var httpResponseMessage = controller.GetNote(SAMPLE_NOTE_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetNoteResponse>().ContinueWith(
            t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactResponse.Exception, null);
        }

        /// <summary>
        /// Getting note while exception error occured TestMethod
        /// </summary>
        [TestMethod]
        public void GetNote_GetNoteforContact_RuntimeError_500InternalServerError()
        {
            NoteController controller = new NoteController(mockNoteService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/note/1", HttpMethod.Get);
            var mockResponse = mockRepository.Create<GetNoteResponse>();

            GetNoteResponse responce = mockRepository.Create<GetNoteResponse>().Object;
            responce.NoteViewModel = MockData.CreateMockList<NoteViewModel>(mockRepository).Select(c => c.Object).FirstOrDefault();
            mockNoteService.Setup(c => c.GetNote(It.IsAny<GetNoteRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.GetNote(SAMPLE_NOTE_ID);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<GetNoteResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        /// <summary>
        /// Insert valid note success TestMethod
        /// </summary>
        [TestMethod]
        public void PostNote_validNote_Succeed()
        {
            NoteController controller = new NoteController(mockNoteService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/note", HttpMethod.Get);
            var mockResponse = mockRepository.Create<SaveNoteResponse>();

            NoteViewModel newContactNote = new NoteViewModel() { NoteId = SAMPLE_NOTE_ID };

            mockResponse.Setup(c => c.NoteViewModel).Returns(newContactNote);
            mockNoteService.Setup(c => c.InsertNote(It.IsAny<SaveNoteRequest>())).Returns(mockResponse.Object);


            var httpResponseMessage = controller.PostNote(It.IsAny<NoteViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<SaveNoteResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.NoteViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.NoteViewModel.NoteId > 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }

        /// <summary>
        /// Insert note getting exception error occured TestMethod
        /// </summary>
        [TestMethod]
        public void PostNote_RuntimeError_500InternalServerError()
        {
            NoteController controller = new NoteController(mockNoteService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/note", HttpMethod.Get);
            var mockResponse = mockRepository.Create<SaveNoteResponse>();

            mockNoteService.Setup(c => c.InsertNote(It.IsAny<SaveNoteRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PostNote(It.IsAny<NoteViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<SaveNoteResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        /// <summary>
        /// update note success TestMethod
        /// </summary>
        [TestMethod]
        public void PutNote_UpdateAction_Succeed()
        {

            NoteController controller = new NoteController(mockNoteService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/note", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateNoteResponse>();
            NoteViewModel newContactNote = new NoteViewModel() { NoteId = SAMPLE_NOTE_ID };

            mockResponse.Setup(c => c.NoteViewModel).Returns(newContactNote);
            mockNoteService.Setup(c => c.UpdateNote(It.IsAny<UpdateNoteRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.PutNote(It.IsAny<NoteViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateNoteResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.NoteViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.NoteViewModel.NoteId > 0);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }

        /// <summary>
        /// update note getting exception error occured TestMethod
        /// </summary>
        [TestMethod]
        public void PutNote_UpdateNote_RuntimeError_500InternalServerError()
        {
            NoteController controller = new NoteController(mockNoteService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/note", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateNoteResponse>();

            mockNoteService.Setup(c => c.UpdateNote(It.IsAny<UpdateNoteRequest>())).
              Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PutNote(It.IsAny<NoteViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateNoteResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        /// <summary>
        /// delete note success TestMethod
        /// </summary>
        [TestMethod]
        public void DeleteNote_DeactivateNoteforContacts_Succeed()
        {
            NoteController controller = new NoteController(mockNoteService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/note", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeactivateNotesContactResponse>();
            mockNoteService.Setup(c => c.ContactDeleteForNote(It.IsAny<DeactivateNotesContactRequest>())).
               Returns(mockResponse.Object);

            var httpResponseMessage = controller.DeleteNote(SAMPLE_NOTE_ID, SAMPLE_CONTACT_ID);
            var noteResponse = httpResponseMessage.Content.ReadAsAsync<DeactivateNotesContactResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            //Assert.AreEqual(noteResponse., null);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        }

        /// <summary>
        /// delete note getting exception error occured TestMethod
        /// </summary>
        [TestMethod]
        public void DeleteNote_DeactivateNoteforContacts_RuntimeError_500InternalServerError()
        {
            NoteController controller = new NoteController(mockNoteService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/note", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeactivateNotesContactResponse>();

            mockNoteService.Setup(c => c.ContactDeleteForNote(It.IsAny<DeactivateNotesContactRequest>())).
             Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.DeleteNote(SAMPLE_NOTE_ID, SAMPLE_CONTACT_ID);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<DeactivateNotesContactResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }
    }
}
