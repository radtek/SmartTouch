using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.ValueObjects;
using Moq;

namespace SmartTouch.CRM.Domain.Tests.Action
{
    [TestClass]
    public class ActionTest
    {

        DA.Action validAction = new DA.Action()
        {
            Details = "Sample action",
            IsCompleted = true,
            ReminderTypes = new List<ReminderType>() { ReminderType.Email },
            RemindOn = DateTime.Now.AddDays(1),
            Contacts = new List<Contacts.RawContact> {  },
            Tags = Enumerable.Empty<Tags.Tag>()
        };

        [TestMethod]
        public void Action_ActionWithNoMessage_GetBrokenRules()
        {
            validAction.Details = null;
            DA.Action actionWithNoMessage = validAction;
            var brokenRulesCount = actionWithNoMessage.GetBrokenRules().Count();
            Assert.AreEqual(null, actionWithNoMessage.Details);
            Assert.AreEqual(1, brokenRulesCount);
        }





        /*
         This testmethod is written by srinivas
         * In this test method i am comparing the 
         * reqired field for action required field
         */
        [TestMethod]
        public void GetBrokenRules_ActionWithNoMessage_Success()
        {
            validAction.Details = null;
            var brokenRulesCount = validAction.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }

        [TestMethod]
        public void Action_ActionWithNoContacts_GetBrokenRules()
        {
            validAction.Contacts = null;
            DA.Action actionWithNoContact = validAction;
            var brokenRulesCount = actionWithNoContact.GetBrokenRules().Count();
            // what is the use of this? -- srinivas (this may not be required)
            //Assert.AreEqual(null, actionWithNoContact.Contacts);
            Assert.AreEqual(1, brokenRulesCount);
        }

        [TestMethod]
        public void Action_ActionWithEarlierRemindOn_GetBrokenRules()
        {
            validAction.RemindOn = DateTime.Now.AddDays(-1);
            DA.Action actionWithEarlierRemindOn = validAction;
            var brokenRulesCount = actionWithEarlierRemindOn.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }

        [TestMethod]
        public void Action_ActionWithMessageLengthAbove1000_GetBrokenRules()
        {
            validAction.Details = new string('a', 1001);
            var brokenRulesCount = validAction.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRulesCount);
        }

        [TestMethod]
        public void Action_ActionWithNoReminderType_NoBrokenRules()
        {
            //validAction.ReminderType = ReminderType.NoReminder;
            DA.Action actionWithNoReminderType = validAction;
            var brokenRulesCount = actionWithNoReminderType.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRulesCount);
        }

        [TestMethod]
        public void Action_ActionWithValidData_NoBrokenRules()
        {
            var brokenRulesCount = validAction.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRulesCount);
        }



    }
}
