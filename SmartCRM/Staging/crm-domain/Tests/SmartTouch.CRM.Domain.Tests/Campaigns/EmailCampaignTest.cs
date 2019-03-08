using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using DC = SmartTouch.CRM.Domain.Campaigns;

namespace SmartTouch.CRM.Domain.Tests.Campaigns
{
    [TestClass]
    public class EmailCampaignTest
    {
        #region Properties
        public CampaignData campaignData = new CampaignData();
        MockRepository mockRepository = new MockRepository(MockBehavior.Default);
        Campaign campaign1 = new Campaign();
        EmailCampaign emailcampaign1 = new EmailCampaign();
        #endregion

        [TestInitialize]
        public void Initialize()
        {
            EmailCampaign sampleEmailCampaign = campaignData.GetCustomEmailCampaign(campaign1, CampaignStatus.Draft, 1, new Email() { EmailId = "stcrm@st.com" }, campaignData.GetSampleMailingList(3), new DateTime(), new DateTime(), null);
        }

        [TestMethod]
        public void EmailCampaignDomain_CampaignIsNotValidNull_ThrowException()
        {
            DC.Campaign campaign = campaignData.GetCustomCampaign(null, null, null, CampaignTemplate.Blank, 1);
            EmailCampaign emailCampaign = campaignData.GetCustomEmailCampaign(CampaignStatus.Draft, 1, new Email() { EmailId="stcrm@st.com"}, campaignData.GetSampleMailingList(3), new DateTime(), new DateTime(),null);
            emailCampaign.Campaign = campaign;
            Assert.IsFalse(emailCampaign.IsCampaignValid());
        }

        [TestMethod]
        public void EmailCampaignDomain_CampaignIsValid_NoException()
        {
            DC.Campaign campaign = campaignData.GetCustomCampaign("Test", "Test subject","test", CampaignTemplate.Blank, 1);
            EmailCampaign emailCampaign = campaignData.GetCustomEmailCampaign(CampaignStatus.Draft, 1, new Email() { EmailId = "stcrm@st.com" }, campaignData.GetSampleMailingList(3), new DateTime(), new DateTime(), null);
            emailCampaign.Campaign = campaign;
            Assert.IsTrue(emailCampaign.IsCampaignValid());
        }

        [TestMethod]
        public void EmailCampaignDomain_ToMailingListIsNull_ThrowException()
        {
            DC.Campaign campaign = campaignData.GetCustomCampaign("Test", "Test subject", "test", CampaignTemplate.Blank, 1);
            EmailCampaign emailCampaign = campaignData.GetCustomEmailCampaign(CampaignStatus.Draft, 1, new Email() { EmailId = "stcrm@st.com" }, campaignData.GetSampleMailingList(0), new DateTime(), new DateTime(), null);
            emailCampaign.Campaign = campaign;
            Assert.IsFalse(emailCampaign.IsToMailingListValid());
        }

        [TestMethod]
        public void EmailCampaignDomain_ToMailingListIsNotNull_NoException()
        {
            DC.Campaign campaign = campaignData.GetCustomCampaign("Test", "Test subject", "test", CampaignTemplate.Blank, 1);
            EmailCampaign emailCampaign = campaignData.GetCustomEmailCampaign(CampaignStatus.Draft, 1, new Email() { EmailId = "stcrm@st.com" }, campaignData.GetSampleMailingList(3), new DateTime(), new DateTime(), null);
            emailCampaign.Campaign = campaign;
            Assert.IsTrue(emailCampaign.IsToMailingListValid());
        }

        [TestMethod]
        public void EmailCampaignDomain_FromEmailIsNullOrEmpty_ThrowException()
        {
            DC.Campaign campaign = campaignData.GetCustomCampaign("Test", "Test subject", "test", CampaignTemplate.Blank, 1);
            EmailCampaign emailCampaign = campaignData.GetCustomEmailCampaign(CampaignStatus.Draft, 1, new Email() { EmailId = "stcrm@st.com" }, campaignData.GetSampleMailingList(3), new DateTime(), new DateTime(), null);
            emailCampaign.Campaign = campaign;
            Assert.IsTrue(emailCampaign.IsToMailingListValid());
        }

        [TestMethod]
        public void EmailCampaignDomain_FromEmailIsNotNullOrEmpty_NoException()
        {
            DC.Campaign campaign = campaignData.GetCustomCampaign("Test", "Test subject", "test", CampaignTemplate.Blank, 1);
            EmailCampaign emailCampaign = campaignData.GetCustomEmailCampaign(CampaignStatus.Draft, 1, null, campaignData.GetSampleMailingList(3), new DateTime(), new DateTime(), null);
            emailCampaign.Campaign = campaign;
            Assert.IsFalse(emailCampaign.IsFromEmailValid());
        }

        [TestMethod]
        public void EmailCampaignDomain_FromEmailIsNotRelatedToAccount_ThrowException()
        {
            DC.Campaign campaign = campaignData.GetCustomCampaign("Test", "Test subject", "test", CampaignTemplate.Blank, 1);
            EmailCampaign emailCampaign = campaignData.GetCustomEmailCampaign(CampaignStatus.Draft, 1, new Email() { EmailId = "stcrm@st.com" }, campaignData.GetSampleMailingList(3), new DateTime(), new DateTime(), null);
            emailCampaign.Campaign = campaign;
            Assert.IsTrue(emailCampaign.IsFromEmailValid());
        }

        [TestMethod]
        public void EmailCampaignDomain_CampaignStatusIsScheduledButScheduleTimeIsPastTime_ThrowException()
        {
            DC.Campaign campaign = campaignData.GetCustomCampaign("Test", "Test subject", "test", CampaignTemplate.Blank, 1);
            EmailCampaign emailCampaign = campaignData.GetCustomEmailCampaign(CampaignStatus.Scheduled, 1, new Email() { EmailId = "stcrm@st.com" }, campaignData.GetSampleMailingList(3), new DateTime(2013,1,1), new DateTime(), null);
            emailCampaign.Campaign = campaign;
            Assert.IsFalse(emailCampaign.IsScheduleTimeValid());
        }

        [TestMethod]
        public void EmailCampaignDomain_CampaignStatusIsScheduledAndScheduleTimeIsFutureTime_NoException()
        {
            DC.Campaign campaign = campaignData.GetCustomCampaign("Test", "Test subject", "test", CampaignTemplate.Blank, 1);
            EmailCampaign emailCampaign = campaignData.GetCustomEmailCampaign(CampaignStatus.Scheduled, 1, new Email() { EmailId = "stcrm@st.com" }, campaignData.GetSampleMailingList(3), new DateTime(2020, 1, 1), new DateTime(), null);
            emailCampaign.Campaign = campaign;
            Assert.IsTrue(emailCampaign.IsScheduleTimeValid());
        }

        [TestMethod]
        public void EmailCampaignDomain_TestEmailIsNullOnClickTestEmailButton_ThrowException()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void EmailCampaignDomain_TestEmailIsNotRelatedToAccountOnClickTestEmailButton_ThrowException()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void EmailCampaignDomain_TestEmailIsRelatedToAccountOnClickTestEmailButton_NoException()
        {
            Assert.Fail();
        }
    }
}
