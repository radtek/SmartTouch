using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using System.Collections.Generic;
using MailChimp;
using MailChimp.Reports;
namespace LandmarkIT.Enterprise.CommunicationManager.UnitTests
{
    [TestClass]
    public class MailChimpUnitTest
    {
        [TestMethod]
        public void TestCampaign()
        {
            MailChimpCampaign mc = new MailChimpCampaign("71a4b48110cf9e7de70d56772d31257d-us8");
            //mc.SendCampaign(
            //    "",
            //    new List<string> { "haripratap.elduri@landmarkit.co.in", "nmreddy.arimanda@landmarkit.in", "bharath.chandra@landmarkit.in", "sudheer.boddapati@landmarkit.in" },
            //    "mailchimp test campaign (Sent by UNIT Test method)",
            //    "campaign subject test - no more footers",
            //    "<div>hello (footer removed??)</div>",
            //    "nm@outlook.in",
            //    "NM",
            //    null);
        }

        [TestMethod]
        public void TestCampaignSummary()
        {
            MailChimpCampaign mc = new MailChimpCampaign("");
            MailChimpManager mm = new MailChimpManager("71a4b48110cf9e7de70d56772d31257d-us8");
            var temp = mm.GetCampaigns();
            ReportSummary reportSummary = mm.GetReportSummary("c77ad12743");
            Assert.IsNotNull(reportSummary);
        }
    }

}
