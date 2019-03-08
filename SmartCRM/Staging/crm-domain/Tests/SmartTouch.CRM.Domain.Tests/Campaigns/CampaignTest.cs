using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;


namespace SmartTouch.CRM.Domain.Tests.Campaigns
{
    [TestClass]
    public class CampaignTest
    {
        #region Properties
        public CampaignData campaignData = new CampaignData();
        MockRepository mockRepository = new MockRepository(MockBehavior.Default);
        Campaign campaign1 = new Campaign();

        CampaignTemplate sampleLayout = new CampaignTemplate() { Id = 1, Type = Entities.CampaignTemplateType.Layout };
        CampaignTemplate samplePredesigned = new CampaignTemplate() { Id = 1, Type = Entities.CampaignTemplateType.PreDesigned };
        #endregion


        #region Initialize
        [TestInitialize]
        public void Initialize()
        {
            Campaign sampleCampaign = campaignData.GetCustomCampaign(campaign1, CampaignStatus.Draft, 1,"stcrm@st.com" , new DateTime(), new DateTime(), null);
        }
        #endregion

        #region TestCases
        [TestMethod]
        public void CampaignDomain_CampaignNameIsNullOrEmpty_ThrowException()
        {
            Campaign campaign = campaignData.GetCustomCampaign(null, "TestSubject", "<div><h1>Hello</h1></div>", sampleLayout, 1);
            Assert.IsFalse(campaign.IsPropValid(campaign.Subject));
        }

        [TestMethod]
        public void CampaignDomain_CampaignNameIsNotNullOrEmpty_NoException()
        {
            Campaign campaign = campaignData.GetCustomCampaign("Test", "TestSubject", "<div><h1>Hello</h1></div>", sampleLayout, 1);
            Assert.IsTrue(campaign.IsPropValid(campaign.Name));
        }

        [TestMethod]
        public void CampaignDomain_CampaignSubjectIsNullOrEmpty_ThrowException()
        {
            Campaign campaign = campaignData.GetCustomCampaign("Test", null, "<div><h1>Hello</h1></div>", sampleLayout, 1);
            Assert.IsFalse(campaign.IsPropValid(campaign.Subject));
        }

        [TestMethod]
        public void CampaignDomain_CampaignSubjectIsNotNullOrEmpty_NoException()
        {
            Campaign campaign = campaignData.GetCustomCampaign("Test", "TestSubject", "<div><h1>Hello</h1></div>", sampleLayout, 1);
            Assert.IsTrue(campaign.IsPropValid(campaign.Subject));
        }

        [TestMethod]
        public void CampaignDomain_CampaignContentIsNullOrEmpty_ThrowException()
        {
            Campaign campaign = campaignData.GetCustomCampaign("Test", "TestSubject", null, sampleLayout, 1);
            Assert.IsFalse(campaign.IsPropValid(campaign.HTMLContent));
        }

        [TestMethod]
        public void CampaignDomain_CampaignContentIsNotNullOrEmpty_NoException()
        {
            Campaign campaign = campaignData.GetCustomCampaign("Test", "TestSubject", "<div><h1>Hello</h1></div>", sampleLayout, 1);
            Assert.IsTrue(campaign.IsPropValid(campaign.HTMLContent));
        }
        #endregion

        //[TestMethod]
        //public void CampaignDomain_ToMailingListIsNull_ThrowException()
        //{
        //    Campaign campaign = campaignData.GetCustomCampaign(CampaignStatus.Scheduled, 1, "stcrm@st.com", 0, new DateTime(), new DateTime(), null);
        //    Assert.IsFalse(campaign.IsToMailingListValid());
        //}

        //[TestMethod]
        //public void CampaignDomain_ToMailingListIsNotNull_NoException()
        //{
        //    Campaign campaign = campaignData.GetCustomCampaign(CampaignStatus.Scheduled, 1,"stcrm@st.com", 5, new DateTime(), new DateTime(), null);
        //    Assert.IsTrue(campaign.IsToMailingListValid());
        //}

        //[TestMethod]
        //public void CampaignDomain_FromEmailIsNullOrEmpty_ThrowException()
        //{
        //    Campaign campaign = campaignData.GetCustomCampaign(CampaignStatus.Scheduled, 1, "stcrm@st.com", 5, new DateTime(), new DateTime(), null);
        //    Assert.IsTrue(campaign.IsToMailingListValid());
        //}

        //[TestMethod]
        //public void CampaignDomain_FromEmailIsNotNullOrEmpty_NoException()
        //{
        //    Campaign campaign = campaignData.GetCustomCampaign(CampaignStatus.Scheduled, 1, null, 5, new DateTime(), new DateTime(), null);
        //    Assert.IsFalse(campaign.IsFromEmailValid());
        //}

        //[TestMethod]
        //public void CampaignDomain_FromEmailIsNotRelatedToAccount_ThrowException()
        //{
        //    Campaign campaign = campaignData.GetCustomCampaign(CampaignStatus.Scheduled, 1, "stcrm@st.com", 5, new DateTime(), new DateTime(), null);
        //    Assert.IsTrue(campaign.IsFromEmailValid());
        //}

        [TestMethod]
        public void CampaignDomain_CampaignStatusIsScheduledButScheduleTimeIsPastTime_ThrowException()
        {
            Campaign campaign = campaignData.GetCustomCampaign(CampaignStatus.Scheduled, 1, "stcrm@st.com", 5, new DateTime(2013, 1, 1), new DateTime(), null);
            Assert.IsFalse(campaign.IsScheduleTimeValid());
        }

        [TestMethod]
        public void CampaignDomain_CampaignStatusIsScheduledAndScheduleTimeIsFutureTime_NoException()
        {
            Campaign campaign = campaignData.GetCustomCampaign(CampaignStatus.Scheduled, 1, "stcrm@st.com", 5, new DateTime(2020, 1, 1), new DateTime(), null);
            Assert.IsTrue(campaign.IsScheduleTimeValid());
        }

        //[TestMethod]
        //public void CampaignDomain_TestEmailIsNullOnClickTestEmailButton_ThrowException()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void CampaignDomain_TestEmailIsNotRelatedToAccountOnClickTestEmailButton_ThrowException()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void CampaignDomain_TestEmailIsRelatedToAccountOnClickTestEmailButton_NoException()
        //{
        //    Assert.Fail();
        //}
    }
}