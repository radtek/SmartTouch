using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using port25.pmta.api.submitter;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using LandmarkIT.Enterprise.Extensions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace LandmarkIT.Enterprise.CommunicationManager.MailTriggers
{
    public class SmartTouchProvider : IMailProvider
    {
        public int BatchCount => 5000;

        public string SendCampaign(Campaign campaign, List<EmailRecipient> emails,
            IEnumerable<FieldValueOption> customFieldsValueOptions
            , string accountCode, string accountAddress, string accountDomain
            , string providerEmail, MailRegistrationDb mailRegistration)
        {
            Connection connection = null;
            try
            {
                connection = new Connection(
                      mailRegistration.Host, mailRegistration.Port ?? 25,
                      mailRegistration.UserName, mailRegistration.Password);

                /*
                 * HtmlEncode method encodes any special characters into their relavent html codes.
                 * */
                campaign.HTMLContent = campaign.HTMLContent; //.FormatHTML();

                var message = BuildMessage(campaign, accountDomain, accountCode, accountAddress, providerEmail,
                    mailRegistration);

                for (var i = 0; i < emails.Count; i++)
                {
                    var item = emails[i];
                    var recipient = new Recipient(item.EmailId);
                    recipient["*from"] = string.Format("{0}-{1}_{2}@bounce.{3}", item.CampaignRecipientID, accountCode,
                        campaign.Id, mailRegistration.SenderDomain);
                    recipient["*envid"] = item.CampaignRecipientID.ToString();
                    recipient["*parts"] = "1";
                    foreach (var mergeField in item.ContactFields)
                    {
                        recipient[mergeField.Key] = mergeField.Value;
                        // var key = mergeField.Key;
                        //// int fid = 0;
                        // var value = mergeField.Value;
                        // if (key.Contains("CF"))
                        // {
                        //     //int.TryParse(key.Replace("CF", ""), out fid);
                        //     //var SelectedValues = value.ToIntList();
                        //     //var customfieldoptions =
                        //     //    customFieldsValueOptions.Where(cf => cf.FieldId == fid && SelectedValues.Contains(cf.Id))
                        //     //        .Select(x => x.Value);
                        //     //string replacablecode = string.Join(",", customfieldoptions);
                        //     value = (string.IsNullOrEmpty(value) ? key : value);
                        //     recipient[mergeField.Key] = value;
                        // }
                        // else
                        // {

                        // }
                    }

                    message.AddRecipient(recipient);
                }

                SubmitWithTimeOut(connection, message, campaign.Id, campaign.Subject);
            }
            finally
            {
                connection?.Close();
            }
            return string.Empty;
        }

        #region Private Methods
        public void SubmitWithTimeOut(Connection connection, Message message, int campaignId, string subject)
        {
            if (connection != null)
            {
                var tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                int timeOut = 10000;

                var task = Task.Factory.StartNew(() => connection.Submit(message), token);

                if (!task.Wait(timeOut, token))
                {
                    string errorMessage = "Sending a campaign with subject : " + subject + " and campaignId : " + campaignId + " through VMTA has timed out";
                    TimeoutException ex = new TimeoutException(errorMessage);
                    Logger.Current.Critical(errorMessage, ex);
                    throw ex;
                }
                Logger.Current.Informational("Campaign send successfully through VMTA");
            }
        }

        private Message BuildMessage(Campaign campaign, string accountDomain
            , string accountCode, string accountAddress
            , string providerEmail, MailRegistrationDb mailRegistration)
        {
            var addressLine = "<p style='margin-bottom:2px;margin-top:2px;'> " + accountAddress + "</p>";
            var contentType = campaign.CampaignTypeId == 132 ? "text/plain" : "text/html";
            String innerBoundary = campaign.IncludePlainText == true ? "--=_innerBoundary" : "";
            var returnPath = string.Format(providerEmail);
            var message = new Message(returnPath)
            {
                Verp = true,
                VirtualMTA = mailRegistration.VMTA,
                JobID = string.Format("{0}/{1}", accountCode, campaign.Id)
            };

            message.AddDateHeader();
            message.ReturnType = ReturnType.Headers;

            var discliamarAccounts = ConfigurationManager.AppSettings["EXCLUDING_DISCLIAMAR_ACCOUNTS"].ToString();
            Logger.Current.Informational("Campaign send successfully through VMTA AccountID" + campaign.AccountID);
            bool accountFound = discliamarAccounts.Contains(campaign.AccountID.ToString());
            var index = mailRegistration.ImageDomain.IndexOf("//");
            var dotCount = mailRegistration.ImageDomain.Count(d => d == '.');
            var linkDomain = accountDomain;
            if (index >= 0 && dotCount == 1)
                linkDomain = mailRegistration.ImageDomain.Insert(index + 2, accountCode + ".");

            var unsubscribeLink = string.Format(linkDomain.IndexOf("http") >= 0 ? "{0}/campaignUnsubscribe?crid=[CRID]&acct={1}" : "https://{0}/campaignUnsubscribe?crid=[CRID]&acct={1}", linkDomain, campaign.AccountID);
            var headers = BuildMergeHeader(campaign, accountCode, mailRegistration.SenderDomain, returnPath, unsubscribeLink, providerEmail, innerBoundary);
            if (string.Equals(ConfigurationManager.AppSettings["INCLUDE_VMTA_ENVID"], "YES", StringComparison.OrdinalIgnoreCase)) message.EnvID = campaign.Id.ToString();
            message.AddMergeData(headers);

            var regex = new Regex(@"\*\|\w+\|\*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = regex.Matches(campaign.HTMLContent);
            var mergeFields = new List<string>();

            foreach (Match match in matches)
                mergeFields.Add(match.Value.Replace("*|", string.Empty).Replace("|*", string.Empty));

            campaign.HTMLContent = campaign.HTMLContent.Replace("*|", "[").Replace("|*", "]");

            if (campaign.CampaignTypeId == 132)
            {
                var recipientInfoLine = (campaign.HasDisclaimer.Value == false && accountFound) ? "" : "\r\n\r\n\r\n\r\nThis e-mail was sent to [EMAILID] by " + campaign.SenderName + " " + campaign.From + ". \r\n";
                var unsubscribeLine = (campaign.HasDisclaimer.Value == false && accountFound) ? "Click the following link to opt out from mailing list: \r\n" + unsubscribeLink + "\r\n" : "Click the following link to unsubscribe: \r\n" + unsubscribeLink + "\r\n";
                if (campaign.HasDisclaimer.Value == false && accountFound)
                    accountAddress = string.Empty;

                var footer = recipientInfoLine + accountAddress + "\r\n" + unsubscribeLine;

                message.AddMergeData(string.Concat(campaign.HTMLContent, footer));
            }
            else
            {
                if (campaign.IncludePlainText == true && !string.IsNullOrEmpty(campaign.PlainTextContent))
                {
                    Logger.Current.Verbose("Adding plain text boundary");
                    var plainTextMergeFields = new List<string>();
                    var pMatches = regex.Matches(campaign.PlainTextContent);
                    foreach (Match match in pMatches)
                        plainTextMergeFields.Add(match.Value.Replace("*|", string.Empty).Replace("|*", string.Empty));

                    campaign.PlainTextContent = campaign.PlainTextContent.Replace("*|", "[").Replace("|*", "]");

                    var plainTextContent = "\n\n" + innerBoundary + "\n"
                        + "Content-Disposition: inline;" + "\n"
                        + "Content-Type: text/plain; charset=utf-8" + "\n\n"
                        + campaign.PlainTextContent + "\n";
                    message.AddMergeData(plainTextContent);
                }
                message.AddData("\n\n" + innerBoundary + "\n");

                if (campaign.IncludePlainText == true)
                {
                    message.AddData("Content-Disposition: inline;" + "\n");
                    message.AddData("Content-Type: text/html; charset=utf-8" + "\n\n");
                }
                var unsubscribeLine = string.Empty;
                var recipientInfoLine = "<p style='margin-bottom:2px;margin-top:2px;'> This e-mail was sent to "
                    + "[EMAILID] by " + campaign.SenderName + " &lt;" + campaign.From + "&gt;. </p>";

                if (campaign.HasDisclaimer.Value == false && accountFound)
                    unsubscribeLine = " <p style='margin-bottom:2px;margin-top:2px;'> <a href='" + unsubscribeLink + "' style='color: #808080;'>Click here to opt out from mailing list</a></p>";
                else
                    unsubscribeLine = " <p style='margin-bottom:2px;margin-top:2px;'> <a href='" + unsubscribeLink + "' style='color: #808080;'>Unsubscribe</a></p>";

                var htmlWrapper = "<!DOCTYPE html PUBLIC \" -//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html>\r\n"
                    + "<head>\r\n<meta name=\"robots\" content=\"noindex\">\r\n<meta http-equiv=\"Content-Type\" content=\"text/html\"; charset=utf-8\">\r\n<title></title>\r\n</head>\r\n<body>\r\n " + campaign.HTMLContent + "\r\n</body>\r\n</html>";
                var footer = "<div style='color: #808080; font-family: Arial, Verdana, Helvetica; font-size: 8pt; font-weight: bold; width: 80%; text-align: center;margin: auto; '>"
                    + ((campaign.HasDisclaimer.Value == false && accountFound) ? "" : recipientInfoLine)
                    + ((campaign.HasDisclaimer.Value == false && accountFound) ? "" : addressLine)
                    + unsubscribeLine + "</div>";

                message.AddMergeData(string.Concat(htmlWrapper, footer));
                if (campaign.IncludePlainText == true)
                    message.AddData("\n\n" + innerBoundary + "--\n\n");

            }

            return message;
        }

        private string BuildMergeHeader(Campaign campaign, string accountCode, string senderDomain, string returnPath, string unsubscribeLink, string providerEmail, string innerBoundary)
        {
            var contentType = "";
            if (campaign.CampaignTypeId == 132)
                contentType = "text/plain";
            else if (campaign.IncludePlainText == false)
                contentType = "text/html";
            else
                contentType = "multipart/alternative";
            string mailType = GetMailtype(campaign.Subject);

            String headers =
                "Subject: " + campaign.Subject + "\n" +
                "X-Mailer: STMailer" + "\n" +
                "X-STCustomer: [CRID]\n" +
                "X-Campaign: " + accountCode + "_" + campaign.Id + "\n" +
                "X-SearchCriteria: " + campaign.Id + "\n" +
                "List-Unsubscribe: " + "<" + unsubscribeLink + ">" + "\n" +
                "From: " + campaign.SenderName + " <" + campaign.From + ">\n" +
                "Return-Path: " + returnPath + "\n" +
                "MIME-Version: 1.0" + "\n" +
                "Feedback-ID: " + campaign.Id.ToString() + ":" + mailType.ToString() + ":" + accountCode.ToString() + "\n" +
                "Content-Type: " + contentType + "; "
                    + (campaign.IncludePlainText == true ? "boundary=\"" + innerBoundary.Replace("--", "") + "\"\n" : "");

            var includeVMTAEnvelope = ConfigurationManager.AppSettings["INCLUDE_VMTA_ENVELOPE"].ToString();
            if (includeVMTAEnvelope == "YES")
                headers = headers + "envelope-from: [CRID]-" + accountCode + "_" + campaign.Id + "@bounce." + senderDomain + "\n";


            if (campaign.From == providerEmail)
                headers = headers + "To:  [EMAILID]\n";
            else
                headers = headers + "To:  [EMAILID]\n" + "Sender: " + providerEmail + "\n" + "Reply-to: " + campaign.SenderName + " <" + campaign.From + ">\n";


            return headers;
        }

        private string GetMailtype(string subject)
        {
            var mailType = "campaign";
            Dictionary<string, string> utm_medium_sources = new Dictionary<string, string>();
            utm_medium_sources.Add("1", "campaign");
            utm_medium_sources.Add("2", "webinar");
            utm_medium_sources.Add("3", "series");
            utm_medium_sources.Add("4", "prospecting");
            utm_medium_sources.Add("5", "reengagement");
            utm_medium_sources.Add("6", "inventory");

            utm_medium_sources.Each(s =>
            {
                if (subject.Contains(s.Value))
                    mailType = s.Value;
            });
            return mailType;
        }

        public bool SubmitWithTimeOut_testing(MailRegistrationDb mailRegistration, Message message, int campaignId, string subject) // created this method for testing PMTA emails by ram on 11th may 2018 for NEXG-3007 NEXG-3004
        {
            bool connectionStatus = false;
            var connection = new Connection(mailRegistration.Host, mailRegistration.Port ?? 25, mailRegistration.UserName, mailRegistration.Password);
            try
            {
                var tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                int timeOut = 10000;

                var task = Task.Factory.StartNew(() => connection.Submit(message), token);
                if (!task.Wait(timeOut, token))
                {
                    string errorMessage = "Sending a campaign with subject : " + subject + " and campaignId : " + campaignId + " through VMTA has timed out";
                    TimeoutException ex = new TimeoutException(errorMessage);
                    Logger.Current.Critical(errorMessage, ex);
                    throw ex;
                }
                connectionStatus = true;
                Logger.Current.Informational("Campaign send successfully through VMTA");
            }
            catch (Exception ex)
            {
                Logger.Current.Critical("PMTA Connection failed for Job Id : " + message.JobID + " Error Message : ", ex);
                connectionStatus = false;
                connection.Close();
            }
            finally
            {
                connection.Close();
            }
            return connectionStatus;
        }
        #endregion
    }
}
