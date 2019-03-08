using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;

using Moq;
using Moq.Linq;

namespace SmartTouch.CRM.ApplicationServices.Tests.Action
{
    public class ActionMockData
    {
        public static IEnumerable<DA.Action> GetMockActions(MockRepository mockRepository, int objectCount)
        {
            IList<DA.Action> mockActions = new List<DA.Action>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockAction = mockRepository.Create<DA.Action>();
                //mockAction.SetupAllProperties();
                mockAction.Object.Id = 1;
                mockActions.Add(mockAction.Object);
            }
            return mockActions;
        }

        public static IEnumerable<Mock<DA.Action>> GetMockActionsWithSetups(MockRepository mockRepository, int objectCount)
        {
            IList<Mock<DA.Action>> mockActions = new List<Mock<DA.Action>>();
            //foreach (int i in Enumerable.Range(1, objectCount))
            //{
            //    var mockAction = mockRepository.Create<DA.Action>();
            //    mockAction.Setup<int>(c => c.Id).Returns(i);
            //    mockAction.Setup<Contact>(c => c.Contacts[i]).Returns(new Person() { Id = i });
            //    mockActions.Add(mockAction);
            //}
            return mockActions;
        }

        public static ActionViewModel GetActionViewModel()
        {
            IEnumerable<ContactEntry> contact = new List<ContactEntry>() { new ContactEntry() { Id = 1, FullName = "Unit-test" } };
            IEnumerable<TagViewModel> tag = new List<TagViewModel>() { new TagViewModel() { TagID = 1001 } };
            ActionViewModel actionViewModel = new ActionViewModel();
            actionViewModel.ActionMessage = "Sample";
            actionViewModel.Contacts = contact;
            actionViewModel.TagsList = tag;
            actionViewModel.ReminderTypes = new List<dynamic>() { ReminderType.Email };
            actionViewModel.RemindOn = DateTime.Now.AddDays(1);
            return actionViewModel;
        }
    }
}
