using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.WebService.Controllers;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Notes;
using SmartTouch.CRM.ApplicationServices.ViewModels;

using Moq;
using Moq.Linq;

namespace SmartTouch.CRM.WebService.Tests.Notes
{
    public class NoteViewModelMockData
    {
        public static IEnumerable<Mock<NoteViewModel>> GetMock(MockRepository mockRepository)
        {
            IList<Mock<NoteViewModel>> mockNotes = new List<Mock<NoteViewModel>>();
            foreach (int i in Enumerable.Range(1, 10))
            {
                var mockNote = mockRepository.Create<NoteViewModel>();
                mockNotes.Add(mockNote);
            }
            return mockNotes;
        }

        public static IEnumerable<Mock<NoteViewModel>> GetMockNoteWithSetups(MockRepository mockRepository)
        {
            IList<Mock<NoteViewModel>> mockNotes = new List<Mock<NoteViewModel>>();
            foreach (int i in Enumerable.Range(1, 10))
            {
                var mockNote = mockRepository.Create<NoteViewModel>();
                mockNote.Setup<int>(c => c.NoteId).Returns(i);
                mockNotes.Add(mockNote);
            }
            return mockNotes;
        }

        public static IEnumerable<NoteViewModel> GetIEnumerableNoteViewModels(int objectCount)
        {
            IList<NoteViewModel> notes = new List<NoteViewModel>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                NoteViewModel note = new NoteViewModel();
                note.NoteId = i;
                notes.Add(note);
            }
            return notes;
        }
    }
}
