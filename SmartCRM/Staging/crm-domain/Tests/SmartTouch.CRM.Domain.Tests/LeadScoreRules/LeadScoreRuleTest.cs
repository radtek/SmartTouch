using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Linq;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.LeadScoreRules;

namespace SmartTouch.CRM.Domain.Tests.LeadScoreRules
{
    [TestClass]
    public class LeadScoreRuleTest
    {
        public const int CREATED_BY_ID = 1;
        public const int ACCOUNT_ID = 1;

        LeadScoreRule leadScoreRule = new LeadScoreRule()
        {
            AccountID = ACCOUNT_ID,
            Score = 100,
            Condition = new Condition() { Id = 1, Name = "condition1" },
            ConditionDescription = "Sample",
            ConditionValue = "1"
        };

        [TestMethod]
        public void LeadScoreRule_ValidationMessage_GetBrokenRuleSuccess()
        {
            leadScoreRule.Score = null;
            leadScoreRule.ConditionValue = null;
            leadScoreRule.ConditionDescription = null;
            leadScoreRule.Condition.Id = 0;
            LeadScoreRule leadScorewithNoMandatoryFields = leadScoreRule;
            var brokenRuleCount = leadScorewithNoMandatoryFields.GetBrokenRules().Count();
            Assert.AreEqual(0, leadScorewithNoMandatoryFields.Condition.Id);
            Assert.AreEqual(null, leadScorewithNoMandatoryFields.Score);
            Assert.AreEqual(null, leadScorewithNoMandatoryFields.ConditionValue);
            Assert.AreEqual(null, leadScorewithNoMandatoryFields.ConditionDescription);
            Assert.AreEqual(4, brokenRuleCount);
        }

        [TestMethod]
        public void IsLeadScoreRuleDescriptionValid_DescriptionLengthIs500_ValidLeadScoreRuleDescription()
        {
            leadScoreRule.ConditionDescription = new string('a', 501);
            Assert.IsFalse(leadScoreRule.ConditionDescription.Length <= 500);
        }

        [TestMethod]
        public void LeadScoreRuleDescription_RulewithDescription_GetBrokenRuleSuccess()
        {
            leadScoreRule.ConditionDescription = null;
            LeadScoreRule leadScoreRulewithNoDescription = leadScoreRule;
            var brokenRuleCount = leadScoreRulewithNoDescription.GetBrokenRules().Count();
            Assert.AreEqual(null, leadScoreRulewithNoDescription.ConditionDescription);
            Assert.AreEqual(1, brokenRuleCount);
        }

        [TestMethod]
        public void LeadScoreRuleCondition_ValueZero_GetBrokenRuleSuccess()
        {
            leadScoreRule.Condition.Id = 0;
            LeadScoreRule leadScoreCondition = leadScoreRule;
            var brokenRuleCount = leadScoreRule.GetBrokenRules().Count();
            Assert.AreEqual(0, leadScoreCondition.Condition.Id);
            Assert.AreEqual(1, brokenRuleCount);
        }

        [TestMethod]
        public void LeadScoreRuleCondition_ConditionDefined_GetBrokenRuleSuccess()
        {
            leadScoreRule.Condition.Id = 1;
            LeadScoreRule leadScoreCondition = leadScoreRule;
            var brokenRuleCount = leadScoreRule.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRuleCount);
        }

        [TestMethod]
        public void LeadScoreRule_PointscannotbeNull_Success()
        {
            leadScoreRule.Score = 100;
            var brokenRuleCount = leadScoreRule.GetBrokenRules().Count();
            Assert.AreEqual(0, brokenRuleCount);
        }

        [TestMethod]
        public void LeadScoreRule_IsvalideUrl_Success()
        {
            Assert.AreEqual(true, leadScoreRule.IsValidUrl("www.smartttouch.com"));
            Assert.AreEqual(true, leadScoreRule.IsValidUrl("www.smartttouch.com/contacts"));
            Assert.AreEqual(true, leadScoreRule.IsValidUrl("www.smartttouch.com/persons/1"));
            Assert.AreEqual(true, leadScoreRule.IsValidUrl("smarttouch.com"));
        }

        [TestMethod]
        public void LeadScoreRule_PointsscanbeNull_ReturnSuccess()
        {
            leadScoreRule.Score = null;
            var brokenRuleCount = leadScoreRule.GetBrokenRules().Count();
            Assert.AreEqual(1, brokenRuleCount);
        }
    }
}
