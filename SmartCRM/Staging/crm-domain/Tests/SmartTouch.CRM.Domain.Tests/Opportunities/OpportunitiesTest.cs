using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Linq;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.Contacts;

namespace SmartTouch.CRM.Domain.Tests.Opportunities
{
    [TestClass]
    public class OpportunitiesTest
    {
        public const int CREATED_BY_ID = 1;

        Opportunity opportunity = new Opportunity()
        {
            Description = "Opportunity Description",
            Contacts = new List<int>() { 1, 3, 5 },
            OpportunityName = "Sample Opportunity",
            OwnerId = 1,
            StageID = 1,
            Potential = 2,
            CreatedOn = DateTime.Now,
            CreatedBy = CREATED_BY_ID,          
        };

        [TestMethod]
        public void OpportunityDetail_OpportunityNoName_GetBrokenRuleSuccess()
        {
            opportunity.OpportunityName= null;
            Opportunity opportunitywithNoDescription = opportunity;
            var brokenRuleCount = opportunitywithNoDescription.GetBrokenRules().Count();
            Assert.AreEqual(null, opportunitywithNoDescription.OpportunityName);
            Assert.AreEqual(1, brokenRuleCount);
        }

        //[TestMethod]
        //public void OpportunityContacts_OpportunitywithNoContacts_GetBrokenRuleSuccess()
        //{
        //    opportunity.Contacts.Count = 0;
        //    Opportunity opportunitywithNoContacts = opportunity;
        //    var brokenRuleCount = opportunitywithNoContacts.GetBrokenRules().Count();
        //    Assert.AreEqual(null, opportunitywithNoContacts.Contacts);
        //    Assert.AreEqual(1, brokenRuleCount);
        //}

        [TestMethod]
        public void OpportunityName_MoreThanSeventyfiveCharacters_Success()
        {
            opportunity.OpportunityName = new string('n', 76);
            var opportunityNameCount = opportunity.OpportunityName.Length;
            var brokenRuleCount = opportunity.GetBrokenRules().Count();
            Opportunity opportunityWithMoreThanThousand = opportunity;
            Assert.AreEqual(76, opportunityNameCount);
            Assert.AreEqual(1, brokenRuleCount);
        }

        [TestMethod]
        public void OpportunityStage_StagewithRequired_GetBrokenRulesSuccess()
        {
            opportunity.StageID = 0;
            Opportunity opportunitywithStageID = opportunity;
            var brokenRuleCount = opportunitywithStageID.GetBrokenRules().Count();
            Assert.AreEqual(0, opportunitywithStageID.StageID);
            Assert.AreEqual(1, brokenRuleCount);
        }

        [TestMethod]
        public void OpportunityDescription_MoreThanThousandCharacters_Success()
        {
            opportunity.Description = new string('n', 1001);
            var opportunityDescriptionCount = opportunity.Description.Length;
            var brokenRuleCount = opportunity.GetBrokenRules().Count();
            Opportunity opportunityWithMoreThanThousand = opportunity;
            Assert.AreEqual(1001, opportunityDescriptionCount);
            Assert.AreEqual(1, brokenRuleCount);
        }

        [TestMethod]
        public void OpprtunityOwner_OwnerRequired_GetBrokenRulesSuccess()
        {
            opportunity.OwnerId = 0;
            Opportunity opportunitywithOwner = opportunity;
            var brokenRuleCount = opportunitywithOwner.GetBrokenRules().Count();
            Assert.AreEqual(0, opportunitywithOwner.OwnerId);
            Assert.AreEqual(1, brokenRuleCount);
        }

        [TestMethod]
        public void OpportunityPotential_PotentialwithRequired_GetBrokenRulesSuccess()
        {
            opportunity.Potential = 0;
            Opportunity opportunitywithPotential = opportunity;
            var brokenRuleCount = opportunitywithPotential.GetBrokenRules().Count();
            Assert.AreEqual(0, opportunitywithPotential.Potential);
            Assert.AreEqual(1, brokenRuleCount);
        }
    }
}
