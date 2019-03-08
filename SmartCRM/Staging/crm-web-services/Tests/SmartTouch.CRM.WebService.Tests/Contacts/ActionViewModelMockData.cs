using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.WebService.Controllers;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.ViewModels;

using Moq;
using Moq.Linq;

namespace SmartTouch.CRM.WebService.Tests.Contacts
{
   internal class ActionViewModelMockData
    {
        public static IEnumerable<Mock<ActionViewModel>> GetMock(MockRepository mockRepository)
        {
            IList<Mock<ActionViewModel>> mockActions = new List<Mock<ActionViewModel>>();
            foreach (int i in Enumerable.Range(1, 10))
            {
                var mockAction = mockRepository.Create<ActionViewModel>();
                mockActions.Add(mockAction);
            }
            return mockActions;
        }

        public static IEnumerable<Mock<ActionViewModel>> GetMockActionWithSetups(MockRepository mockRepository)
        {
            IList<Mock<ActionViewModel>> mockActions = new List<Mock<ActionViewModel>>();
            foreach (int i in Enumerable.Range(1, 10))
            {
                var mockAction = mockRepository.Create<ActionViewModel>();
                mockAction.Setup<int>(c => c.ActionId).Returns(i);
                mockActions.Add(mockAction);
            }
            return mockActions;
        }

        public static IEnumerable<ActionViewModel> GetIEnumerableActionViewModels(int objectCount)
        {
            IList<ActionViewModel> actions = new List<ActionViewModel>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                ActionViewModel action = new ActionViewModel();
                action.ActionId = i;
               // action.Contacts.SingleOrDefault(c => c.Id == objectCount);
                actions.Add(action);

            }
            return actions;
        }

    }
}
