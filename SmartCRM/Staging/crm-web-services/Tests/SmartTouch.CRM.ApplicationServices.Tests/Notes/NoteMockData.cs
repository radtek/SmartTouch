using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Notes;
using SmartTouch.CRM.ApplicationServices.Tests.Contacts;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Contacts;

using Moq;
using Moq.Linq;
using SmartTouch.CRM.Domain.Tags;

namespace SmartTouch.CRM.ApplicationServices.Tests.NoteTests
{
    public class NoteMockData
    {
        public static IEnumerable<Note> GetMockNotes(MockRepository mockRepository, int objectCount)
        {
            IList<Note> mockNotes = new List<Note>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockNote = mockRepository.Create<Note>();
                mockNote.Object.Id = 1;
                mockNotes.Add(mockNote.Object);
            }
            return mockNotes;
        }

        public static IList<Contact> GetMockContacts(MockRepository mockRepository, int objectCount)
        {
            IList<Contact> mockContacts = new List<Contact>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockContact = mockRepository.Create<Person>();
                mockContacts.Add(mockContact.Object);
            }
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockContact = mockRepository.Create<Company>();
                mockContacts.Add(mockContact.Object);
            }
            return mockContacts;
        }

        public static IEnumerable<Note> GetMockNoteswithSetups(MockRepository mockRepository, int objectCount)
        {
            IList<Note> mockNotes = new List<Note>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockNote = mockRepository.Create<Note>();
                mockNote.Setup<int>(c => c.Id).Returns(i);
                mockNote.Setup<Contact>(c => c.Contacts[i]).Returns(new Person() { Id = i });
                mockNotes.Add(mockNote.Object);
            }
            return mockNotes;
        }

        public static IEnumerable<Mock<Note>> GetMockNotesWithSetups(MockRepository mockRepository, int objectCount)
        {
            IList<Mock<Note>> mockNotes = new List<Mock<Note>>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockNote = mockRepository.Create<Note>();
                mockNote.Setup<int>(c => c.Id).Returns(i);
                mockNote.Setup<IList<Contact>>(c => c.Contacts).Returns(ContactMockData.GetMockContacts(mockRepository, 2).ToList());
                mockNotes.Add(mockNote);
            }
            return mockNotes;
        }

        public static IEnumerable<Mock<Note>> GetMockNoteTagsWithSetups(MockRepository mockRepository, int objectCount)
        {
            IList<Mock<Note>> mockNotes = new List<Mock<Note>>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockNote = mockRepository.Create<Note>();
                mockNote.Setup<int>(c => c.Id).Returns(i);
                mockNote.Setup<ICollection<Tag>>(c => c.Tags).Returns(GetMockTags(mockRepository, 2).ToList());
                mockNotes.Add(mockNote);
            }
            return mockNotes;
        }

        public static IEnumerable<Tag> GetMockTags(MockRepository mockRepository, int objectCount)
        {
            IList<Tag> mockTags = new List<Tag>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockTag = mockRepository.Create<Tag>();
                mockTags.Add(mockTag.Object);
            }
            return mockTags;
        }


        public static NoteViewModel GetNoteViewModel()
        {
            IList<ContactEntry> contact = new List<ContactEntry>() { new ContactEntry() { Id = 1, FullName = "This is Sample" } };
            IList<TagViewModel> tag = new List<TagViewModel>() { new TagViewModel() { TagID = 1001 } };
            NoteViewModel noteViewModel = new NoteViewModel();
            noteViewModel.NoteDetails = "Sample";
            noteViewModel.Contacts = contact;
            noteViewModel.TagsList = tag;
            return noteViewModel;
        }

        public static Mock<Note> GetMockNoteWithId(MockRepository mockRepository, int Id)
        {
            IList<Mock<Note>> mockNotes = new List<Mock<Note>>();
            var mockNote = mockRepository.Create<Note>();
            mockNote.SetupGet(mt => mt.Id).Returns(Id);
            mockNote.Setup<ICollection<Contact>>(c => c.Contacts).Returns(ContactMockData.GetMockContacts(mockRepository, 1).ToList());
            return mockNote;
        }

        public static IList<Note> GetMockNotesWithId(MockRepository mockRepository, int objectCount)
        {
            IList<Note> mockNotes = new List<Note>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockNote = mockRepository.Create<Note>();
                mockNote.SetupGet(mt => mt.Id).Returns(i);
                mockNote.Setup<ICollection<Contact>>(c => c.Contacts).Returns(ContactMockData.GetMockContacts(mockRepository, 3).ToList());
                mockNotes.Add(mockNote.Object);
            }
            return mockNotes;
        }

        //confirm
        public static Mock<Note> CreateMockNote(MockRepository mockRepository, int Id)
        {
            var mockNote = mockRepository.Create<Note>();
            mockNote.SetupGet(mnt => mnt.Id).Returns(Id);
            mockNote.Setup<ICollection<Contact>>(c => c.Contacts).Returns(ContactMockData.GetMockContacts(mockRepository, 1).ToList());
            mockNote.Setup(mnt => mnt.CreatedBy).Returns(1);
            mockNote.Setup(mnt => mnt.CreatedOn).Returns(new DateTime());
            mockNote.Setup<ICollection<Tag>>(mnt => mnt.Tags).Returns(GetMockTags(mockRepository, 2).ToList());
            return mockNote;
        }

        ////confirm
        //public static Mock<Note> CreateMockNote(MockRepository mockRepository, int Id)
        //{
        //    var mockNote = mockRepository.Create<Note>();
        //    mockNote.SetupGet(mnt => mnt.Id).Returns(Id);
        //    mockNote.Setup<ICollection<Contact>>(c => c.Contacts).Returns(ContactMockData.GetMockContacts(mockRepository, 1).ToList());
        //    mockNote.Setup(mnt => mnt.CreatedBy).Returns(1);
        //    mockNote.Setup(mnt => mnt.CreatedOn).Returns(new DateTime());
        //    mockNote.Setup<ICollection<Tag>>(mnt => mnt.Tags).Returns(GetMockTags(mockRepository, 2).ToList());
        //    return mockNote;
        //}
    }
}
