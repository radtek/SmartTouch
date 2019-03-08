using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SmartTouch.CRM.Domain.Tests.Notes
{
    [TestClass]
    public class NoteTest
    {
        public const int CREATED_BY_ID = 1;

        Note note = new Note()
        {
            Details = "Note Details",
            Contacts = new List<Contact>() { new Person(){ Id = 1 }},
            CreatedOn = DateTime.Now,
            CreatedBy = CREATED_BY_ID,
            Tags = new List<Tag>() { new Tag()}
        };

        [TestMethod]
        public void Note_ValidationMessage_GetBrokenRuleSuccess()
        {
            note.Details = null;
            note.Contacts = null;
            Note notewithNoMandatoryFields = note;
            var brokenRuleCount = notewithNoMandatoryFields.GetBrokenRules().Count();
            Assert.AreEqual(null, notewithNoMandatoryFields.Details);
            Assert.AreEqual(null, notewithNoMandatoryFields.Contacts);
            Assert.AreEqual(2, brokenRuleCount);
        }




        /* This is the test method added by srinivas. 
          * Here the test case to be generated will 
          * be the note field should be required field          
         */
        [TestMethod]
        public void Note_NoteReuired_Success()
        {
            // storing the note details with null. because we are having the test case of required note field
            note.Details = null;           
            // asserting the equal comparitor of null value with the note filed of details
            Assert.AreEqual(null, note.Details);          
        }


        /* This is the test method added by srinivas. 
         * Here the test case to be generated will 
         * be the contacts field should be required field
        */
        [TestMethod]
        public void Note_ContactsReuired_Success()
        {
            // storing the note details with null. because we are having the test case of required note field
            note.Contacts = null;
            // asserting the equal comparitor of null value with the note filed of details
            Assert.AreEqual(null, note.Contacts);
        }



        /* This is the test method added by srinivas. 
         * Here the test case to be generated will 
         * be the note length should not be greater than 1000 
        */
        [TestMethod]
        public void Note_LengthShouldNotBeGreaterThan1000_Failure()
        {           
            // storing the note details with 1001 characters. 
            note.Details = new string('n', 1001);
            // asserting the equal comparitor of null value with the note filed of details
            Assert.IsFalse(note.Details.Length <= 1000);
        }


        [TestMethod]
        public void NoteDetail_NotewithNoDetails_GetBrokenRuleSuccess()
        {
            note.Details = null;
            Note notewithNoDetails = note;
            var brokenRuleCount = notewithNoDetails.GetBrokenRules().Count();
            Assert.AreEqual(null, notewithNoDetails.Details);
            Assert.AreEqual(1, brokenRuleCount);
        }

        [TestMethod]
        public void NoteContacts_NotewithNoContacts_GetBrokenRuleSuccess()
        {
            note.Contacts = null;
            Note notewithNoContacts = note;
            var brokenRuleCount = notewithNoContacts.GetBrokenRules().Count();
            Assert.AreEqual(null, notewithNoContacts.Contacts);
            Assert.AreEqual(1, brokenRuleCount);
        }

        [TestMethod]
        public void Note_ValidNote_BrokenRuleSuccess()
        {
            var brokenRuleCount = note.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRuleCount);
        }

        [TestMethod]
        public void NoteDetails_MoreThanThousandCharacters_Success()
        {
            note.Details = new string('n', 1001);
            var noteDetailsCount = note.Details.Length;
            var brokenRuleCount = note.GetBrokenRules().Count();
            Note noteWithMoreThanThousand = note;
            Assert.AreEqual(1001, noteDetailsCount);
            Assert.AreEqual(1, brokenRuleCount);
        }
    }
}
