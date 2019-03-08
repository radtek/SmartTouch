using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;
using port25.pmta.api.submitter;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;
using LandmarkIT.Enterprise.CommunicationManager.Database;


namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
    public class VMTACampaign
    {
        string userName;
        string password;
        string host;
        int port;
        string vmta;

        public VMTACampaign(string vmta, string userName, string password, string host, int? port)
        {
            this.vmta = vmta;
            this.userName = userName;
            this.password = password;
            this.host = host;

            this.port = port.HasValue ? port.Value : 25;
        }

        private Message BuildMessage(int campaignId, string subject, string content, string fromEmail
            , string fromName, string accountCode, string senderDomain, string accountDomain, string vmtaEmail, string accountAddress, byte? campaignTypeId, int accountId)
        {
            var addressLine = "<p style='margin-bottom:2px;margin-top:2px;'> " + accountAddress + "</p>";
            var contentType = campaignTypeId == 132 ? "text/plain" : "text/html";
            var returnPath = string.Format("nmreddy83@gmail.com");
            var message = new Message(returnPath)
            {
                Verp = true,
                VirtualMTA = vmta,
                JobID = accountCode + "/" + campaignId
            };

            message.AddDateHeader();
            message.ReturnType = ReturnType.Headers;

            var unsubscribeLink = string.Format("https://{0}/campaignUnsubscribe?crid=[CRID]&acct={1}", accountDomain, accountId);
            var headers = BuildMergeHeader(campaignId, subject, accountCode, returnPath, unsubscribeLink, fromName, fromEmail, vmtaEmail, senderDomain, campaignTypeId);
            if (string.Equals(ConfigurationManager.AppSettings["INCLUDE_VMTA_ENVID"], "YES", StringComparison.OrdinalIgnoreCase)) message.EnvID = campaignId.ToString();
            message.AddMergeData(headers);

            var regex = new Regex(@"\*\|\w+\|\*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = regex.Matches(content);
            var mergeFields = new List<string>();

            foreach (Match match in matches)
            {
                mergeFields.Add(match.Value.Replace("*|", string.Empty).Replace("|*", string.Empty));
            }

            content = content.Replace("*|", "[").Replace("|*", "]");

            var recipientInfoLine = "<p style='margin-bottom:2px;margin-top:2px;'> This e-mail was sent to "
                + "[EMAILID] by " + fromName + " &lt;" + fromEmail + "&gt;. </p>";
            var unsubscribeLine = " <p style='margin-bottom:2px;margin-top:2px;'> <a href='" + unsubscribeLink + "' style='color: #808080;'>Unsubscribe</a></p>";
            var htmlWrapper = "<!DOCTYPE html PUBLIC \" -//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html>\r\n" +
                "<head>\r\n" +
                "<meta name='robots' content='noindex'>\r\n" +
                "<meta http-equiv=\"Content-Type\" content=\"" + contentType + "; charset=utf-8\">\r\n<title></title>\r\n</head>\r\n<body>\r\n " + content + "\r\n</body>\r\n</html>";
            var footer = "<div style='color: #808080; font-family: Arial, Verdana, Helvetica; font-size: 8pt; font-weight: bold;margin: auto;width: 80%;text-align: center;'>"
                + recipientInfoLine
                + addressLine
                + unsubscribeLine + "</div>";

            message.AddMergeData(string.Concat(htmlWrapper, footer));

            var textHeaders = headers.Replace("Content-Type: text/html;", "Content-Type: text/plain;");
            message.BeginPart(2);
            message.AddMergeData(textHeaders);
            message.AddMergeData(content);

            return message;
        }

        public IList<int> SendCampaignMailMerge(int campaignId, string campaignName, List<int> contactTagIds, List<int> searchDefinitionIds, IEnumerable<EmailRecipient> emails
            , IEnumerable<Company> Companies, IEnumerable<FieldValueOption> customFieldsValueOptions, string title, string subject, string content, string fromEmail
            , string fromName, string accountCode, string senderDomain, string accountDomain, string vmtaEmail, string accountAddress, int accountId, string listName = null)
        {
            var pageNumber = 5000;
            var totalCount = emails.Count();
            var numberOfIternations = Math.Ceiling(totalCount / 5000M);
            var emailList = emails.ToList();

            var successfulRecipients = new List<int>();

            for (int i = 0; i < numberOfIternations; i++)
            {
                var connection = new Connection(host, port, userName, password);
                var message = BuildMessage(campaignId, subject, content, fromEmail, fromName, accountCode, senderDomain, accountDomain, vmtaEmail, accountAddress, null, accountId);

                try
                {
                    for (int j = 0; j < pageNumber; j++)
                    {
                        var currentIndex = (i * 5000) + j;
                        if ((totalCount - 1) < currentIndex) break;

                        var item = emailList[currentIndex];
                        var recipient = new Recipient(item.EmailId);
                        recipient["*from"] = string.Format("{0}-{1}_{2}@bounce.{3}", item.CampaignRecipientID, accountCode, campaignId, senderDomain);
                        recipient["*envid"] = item.CampaignRecipientID.ToString();

                        foreach (var mergeField in item.ContactFields)
                        {
                            var key = mergeField.Key;
                            int fid = 0;

                            var value = mergeField.Value;
                            if (key.Contains("CF"))
                            {
                                Convert.ToInt32(key.Replace("CF", ""));
                                var SelectedValues = StringToIntList(value);
                                var customfieldoptions = customFieldsValueOptions.Where(cf => cf.FieldId == fid && SelectedValues.Contains(cf.Id)).Select(x => x.Value);
                                string replacablecode = string.Join(",", customfieldoptions);
                                value = (string.IsNullOrEmpty(replacablecode) ? key : replacablecode);
                                recipient[mergeField.Key] = value;
                            }
                            else
                            {
                                recipient[mergeField.Key] = mergeField.Value;
                            }
                        }
                        message.AddRecipient(recipient);
                        successfulRecipients.Add(item.CampaignRecipientID);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("Unable to send campaign to: " + i.ToString() + ". Error:", ex);
                }
                SubmitWithTimeOut(connection, message, campaignId, subject);
                connection.Close();
            }
            return successfulRecipients;

        }

        private static string BuildMergeHeader(int campaignId, string subject, string accountCode, string returnPath, string unsubscribeLink, string fromName, string fromEmail, string vmtaEmail, string senderDomain, byte? campaignTypeId)
        {
            var contentType = campaignTypeId == 132 ? "text/plain" : "text/html";

            String headers =
                "Subject: " + subject + "\n" +
                "Content-Type: " + contentType + "; charset=\"utf-8\"\n" +
                "MIME-Version: 1.0" + "\n" +
                "X-Mailer: STMailer" + "\n" +
                "X-STCustomer: [CRID]\n" +
                "X-Campaign: " + accountCode + "_" + campaignId + "\n" +
                "X-SearchCriteria: " + campaignId + "\n" +
                "List-Unsubscribe: " + "<" + unsubscribeLink + ">" + "\n" +
                "Return-Path: " + returnPath + "\n";
            var includeVMTAEnvelope = ConfigurationManager.AppSettings["INCLUDE_VMTA_ENVELOPE"].ToString();
            if (includeVMTAEnvelope == "YES")
            {
                headers = headers + "envelope-from: [CRID]-" + accountCode + "_" + campaignId + "@bounce." + senderDomain + "\n";
            }

            if (fromEmail == vmtaEmail)
            {
                headers = headers +
                    "From: " + fromName + " <" + fromEmail + ">\n" +
                    "To:  [EMAILID]\n" +
                    "\n";
            }
            else
            {
                headers = headers +
                    "From: " + fromName + " <" + fromEmail + ">\n" +
                    "To:  [EMAILID]\n" +
                        "Sender: " + vmtaEmail + "\n" +
                        "Reply-to: " + fromName + " <" + fromEmail + ">\n" +
                    "\n";
            }

            return headers;
        }

        /// <summary>
        /// Send Campaign through VMTA for test email
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="campaignName"></param>
        /// <param name="contactTagIds"></param>
        /// <param name="searchDefinitionIds"></param>
        /// <param name="emails"></param>
        /// <param name="Companies">Obsolete</param>
        /// <param name="customFieldsValueOptions"></param>
        /// <param name="title"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <param name="fromEmail"></param>
        /// <param name="fromName"></param>
        /// <param name="accountCode"></param>
        /// <param name="senderDomain"></param>
        /// <param name="accountDomain"></param>
        /// <param name="vmtaEmail"></param>
        /// <param name="accountAddress"></param>
        /// <param name="listName"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public IList<int> SendCampaignFromScheduler(int campaignId, string campaignName, List<int> contactTagIds, List<int> searchDefinitionIds, IEnumerable<EmailRecipient> emails
            , IEnumerable<Company> Companies, IEnumerable<FieldValueOption> customFieldsValueOptions, string title, string subject, string content, string fromEmail
            , string fromName, string accountCode, string senderDomain, string accountDomain, string vmtaEmail, string accountAddress, int accountId, byte? campaignTypeId, MailRegistrationDb mailRegistration, bool hasDisCliamer, string listName = null)
        {
            #region Variables Declaration
            var count = emails.Count();
            string returnPath = string.Empty;
            EmailRecipient item = null;
            Message msg = null;
            IList<int> successfulRecipients = new List<int>();
            #endregion

            var includeVMTAENVId = ConfigurationManager.AppSettings["INCLUDE_VMTA_ENVID"].ToString();
            content = content.Replace("*|CAMPID|*", campaignId.ToString()).Replace("<o:p>", "").Replace("</o:p>", "");//.FormatHTML();
            var contentType = campaignTypeId == 132 ? "text/plain" : "text/html";
            var discliamarAccounts = ConfigurationManager.AppSettings["EXCLUDING_DISCLIAMAR_ACCOUNTS"].ToString();
            bool accountFound = discliamarAccounts.Contains(accountId.ToString());
            Connection con = new Connection(host, port, userName, password);
            if (con == null)
                return successfulRecipients;

            var addressLine = "<p style='margin-bottom:2px;margin-top:2px;'> " + accountAddress + "</p>";

            for (var i = 0; i < count; i++)
            {
                item = emails.ToArray()[i];
                try
                {
                    returnPath = item.CampaignRecipientID.ToString() + "-" + accountCode + "_" + campaignId + "@bounce." + senderDomain;
                    msg = new Message(returnPath);
                    if (msg == null)
                        continue;
                    msg.AddDateHeader();
                    msg.ReturnType = ReturnType.Headers;
                    var index = mailRegistration.ImageDomain.IndexOf("//");
                    var dotCount = mailRegistration.ImageDomain.Count(d => d == '.');
                    var linkDomain = accountDomain;
                    if (index >= 0 && dotCount == 1)
                    {
                        linkDomain = mailRegistration.ImageDomain.Insert(index + 2, accountCode + ".");

                    }
                    //var unsubscribeLink = "https://" + linkDomain + "/campaignUnsubscribe?crid=" + item.CampaignRecipientID.ToString() + "&acct=" + accountId;
                    var unsubscribeLink = string.Format(linkDomain.IndexOf("http") >= 0 ? "{0}/campaignUnsubscribe?crid=[CRID]&acct={1}" : "https://{0}/campaignUnsubscribe?crid=[CRID]&acct={1}", linkDomain, accountId);

                    String headers = BuildHeader(campaignId, subject, accountCode, item, returnPath, unsubscribeLink, fromName, fromEmail, vmtaEmail, senderDomain, campaignTypeId);

                    if (includeVMTAENVId == "YES")
                    {
                        msg.EnvID = item.CampaignRecipientID.ToString();
                    }

                    msg.Verp = true;
                    msg.VirtualMTA = vmta;
                    msg.JobID = accountCode + "/" + campaignId;
                    msg.AddData(headers);
                    var contentSb = new StringBuilder(content, content.Length * 2);
                    contentSb.Replace("*|CID|*", item.ContactId.ToString()).Replace("*|CAMPID|*", campaignId.ToString()).Replace("*|CRID|*", item.CampaignRecipientID.ToString()).Replace("<o:p>", "").Replace("</o:p>", "");
                    var body = contentSb.ToString();
                    string mergedhtmlcontent = item.ContactFields != null ? ReplaceMergeFields(body, item.ContactInfo, customFieldsValueOptions, null, item.ContactFields) : body;
                    var unsubscribeLine = string.Empty;

                    var recipientInfoLine = "<p style='margin-bottom:2px;margin-top:2px;'> This e-mail was sent to "
                        + item.EmailId + " by " + fromName + " &lt;" + fromEmail + "&gt;. </p>";
                    if(accountFound && !hasDisCliamer)
                       unsubscribeLine = " <p style='margin-bottom:2px;margin-top:2px;'> <a href='" + unsubscribeLink + "' style='color: #808080;'>Click here to opt out from mailing list</a></p>";
                    else
                       unsubscribeLine = " <p style='margin-bottom:2px;margin-top:2px;'> <a href='" + unsubscribeLink + "' style='color: #808080;'>Unsubscribe</a></p>";

                    var htmlWrapper = "<!DOCTYPE html PUBLIC \" -//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html>\r\n"
                        + "<head>\r\n"
                        + "<meta name='robots' content='noindex'>\r\n"
                        + "<meta http-equiv=\"Content-Type\" content=\"" + contentType + "; charset=utf-8\">\r\n<title></title>\r\n</head>\r\n<body>\r\n*|CONTENT|*\r\n</body>\r\n</html>";
                    var footer = "<div style='color: #808080; font-family: Arial, Verdana, Helvetica; font-size: 8pt; font-weight: bold;margin: auto;width: 80%;text-align: center;'>"
                        +((accountFound && !hasDisCliamer) ? "" : recipientInfoLine) 
                        + ((accountFound && !hasDisCliamer) ? "" : addressLine)
                        + unsubscribeLine + "</div>";


                    mergedhtmlcontent = campaignTypeId == 132 ? body : htmlWrapper.Replace("*|CONTENT|*", mergedhtmlcontent + footer);
                    msg.AddData(mergedhtmlcontent);

                    String textHeaders = headers.Replace("Content-Type: text/html;", "Content-Type: text/plain;");
                    msg.BeginPart(2);
                    msg.AddData(textHeaders);
                    msg.AddData(mergedhtmlcontent);
                    Recipient recipient = new Recipient(item.EmailId);
                    msg.AddRecipient(recipient);
                    SubmitWithTimeOut(con, msg, campaignId, subject);
                    successfulRecipients.Add(item.CampaignRecipientID);
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("Unable to send campaign to: " + item.EmailId + ". Error:", ex);
                }
            }
            return successfulRecipients;
        }

        public IList<int> SendCampaign(int campaignId, string campaignName, List<int> contactTagIds, List<int> searchDefinitionIds, IEnumerable<EmailRecipient> emails
            , IEnumerable<Company> Companies, IEnumerable<FieldValueOption> customFieldsValueOptions, string title, string subject, string content, string fromEmail
            , string fromName, string accountCode, string senderDomain, string accountDomain, string vmtaEmail, string accountAddress, int accountId, string listName = null)
        {
            var includeVMTAEnvelope = ConfigurationManager.AppSettings["INCLUDE_VMTA_ENVELOPE"].ToString();
            var includeVMTAENVId = ConfigurationManager.AppSettings["INCLUDE_VMTA_ENVID"].ToString();

            content = content.Replace("*|CAMPID|*", campaignId.ToString()).Replace("<o:p>", "").Replace("</o:p>", "");//.FormatHTML();
            IList<int> successfulRecipients = new List<int>();

            Connection con = new Connection(host, port, userName, password);
            if (con == null)
                return successfulRecipients;
            var addressLine = string.Empty;

            if (addressLine != null)
            {
                addressLine = "<p style='margin-bottom:2px;margin-top:2px;'> " + accountAddress + "</p>";
            }
            foreach (var item in emails)
            {
                try
                {
                    var returnPath = item.CampaignRecipientID.ToString() + "-" + accountCode + "_" + campaignId + "@bounce." + senderDomain;
                    Message msg = new Message(returnPath);
                    if (msg == null)
                        continue;
                    msg.AddDateHeader();
                    msg.ReturnType = ReturnType.Headers;

                    var unsubscribeLink = "https://" + accountDomain + "/campaignUnsubscribe?crid=" + item.CampaignRecipientID.ToString() + "&acct=" + accountId;
                    String headers =
                        "Subject: " + subject + "\n" +
                        "Content-Type: text/html; charset=\"utf-8\"\n" +
                        "MIME-Version: 1.0" + "\n" +
                        "X-Mailer: STMailer" + "\n" +
                        "X-STCustomer: " + item.CampaignRecipientID.ToString() + "\n" +
                        "X-Campaign: " + accountCode + "_" + campaignId + "\n" +
                        "X-SearchCriteria: " + campaignId + "\n" +
                        "List-Unsubscribe: " + "<" + unsubscribeLink + ">" + "\n" +
                        "Return-Path: " + returnPath + "\n";
                    if (includeVMTAEnvelope == "YES")
                    {
                        headers = headers + "envelope-from: " + item.CampaignRecipientID.ToString() + "-" + accountCode + "_" + campaignId + "@bounce." + senderDomain + "\n";
                    }

                    if (includeVMTAENVId == "YES")
                    {
                        msg.EnvID = item.CampaignRecipientID.ToString();
                    }

                    if (fromEmail == vmtaEmail)
                    {
                        headers = headers +
                            "From: " + fromName + " <" + fromEmail + ">\n" +
                            "To:  " + item.EmailId + "\n" +
                            "\n";
                    }
                    else
                    {
                        headers = headers +
                            "From: " + fromName + " <" + fromEmail + ">\n" +
                            "To:  " + item.EmailId + "\n" +
                                "Sender: " + vmtaEmail + "\n" +
                                "Reply-to: " + fromName + " <" + fromEmail + ">\n" +
                            "\n";
                    }

                    msg.Verp = true;
                    msg.VirtualMTA = vmta;
                    msg.JobID = accountCode + "/" + campaignId;
                    msg.AddData(headers);
                    var body = content.Replace("*|CID|*", item.ContactId.ToString()).Replace("*|CAMPID|*", campaignId.ToString())
                        .Replace("*|CRID|*", item.CampaignRecipientID.ToString());
                    var Company = Companies.Where(i => i.Id == item.ContactId).FirstOrDefault();
                    string mergedhtmlcontent = item.ContactInfo != null ? ReplaceAutomationMergeFields(body, item.ContactInfo, customFieldsValueOptions, Company) : body;

                    var recipientInfoLine = "<p style='margin-bottom:2px;margin-top:2px;'> This e-mail was sent to "
                        + item.EmailId + " by " + fromName + " &lt;" + fromEmail + "&gt;. </p>";
                    var unsubscribeLine = " <p style='margin-bottom:2px;margin-top:2px;'> <a href='" + unsubscribeLink + "' style='color: #808080;'>Unsubscribe</a></p>";
                    var htmlWrapper = "<!DOCTYPE html PUBLIC \" -//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html>\r\n"
                        + "<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\r\n<title></title>\r\n</head>\r\n<body>\r\n*|CONTENT|*\r\n</body>\r\n</html>";
                    var footer = "<div style='color: #808080; font-family: Arial, Verdana, Helvetica; font-size: 8pt; font-weight: bold;margin: auto;width: 80%;text-align: center;'>"
                        + recipientInfoLine
                        + addressLine
                        + unsubscribeLine + "</div>";

                    mergedhtmlcontent = htmlWrapper.Replace("*|CONTENT|*", mergedhtmlcontent + footer);
                    msg.AddData(mergedhtmlcontent);

                    String textHeaders = headers.Replace("Content-Type: text/html;", "Content-Type: text/plain;");
                    msg.BeginPart(2);
                    msg.AddData(textHeaders);
                    msg.AddData(mergedhtmlcontent);
                    Recipient recipient = new Recipient(item.EmailId);
                    msg.AddRecipient(recipient);
                    SubmitWithTimeOut(con, msg, campaignId, subject);
                    successfulRecipients.Add(item.CampaignRecipientID);
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("Unable to send campaign to: " + item.EmailId + ". Error:", ex);
                }
            }
            return successfulRecipients;
        }

        /// <summary>
        /// Build VMTA Mail header
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="subject"></param>
        /// <param name="accountCode"></param>
        /// <param name="item"></param>
        /// <param name="returnPath"></param>
        /// <param name="unsubscribeLink"></param>
        /// <param name="fromName"></param>
        /// <param name="fromEmail"></param>
        /// <param name="vmtaEmail"></param>
        /// <param name="senderDomain"></param>
        /// <returns></returns>
        private static string BuildHeader(int campaignId, string subject, string accountCode,
            EmailRecipient item, string returnPath, string unsubscribeLink, string fromName, string fromEmail, string vmtaEmail, string senderDomain, byte? campaignTypeId)
        {
            var contentType = campaignTypeId == 132 ? "text/plain" : "text/html";
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

            String headers =
                "Subject: " + subject + "\n" +
                "Content-Type: " + contentType + "; charset=\"utf-8\"\n" +
                "MIME-Version: 1.0" + "\n" +
                "X-Mailer: STMailer" + "\n" +
                "X-STCustomer: " + item.CampaignRecipientID.ToString() + "\n" +
                "X-Campaign: " + accountCode + "_" + campaignId + "\n" +
                "X-SearchCriteria: " + campaignId + "\n" +
                "List-Unsubscribe: " + "<" + unsubscribeLink + ">" + "\n" +
                "Return-Path: " + returnPath + "\n" +
                "Feedback-ID: " + campaignId.ToString() + ":" + mailType .ToString()+ ":" + accountCode.ToString();
            var includeVMTAEnvelope = ConfigurationManager.AppSettings["INCLUDE_VMTA_ENVELOPE"].ToString();
            if (includeVMTAEnvelope == "YES")
            {
                headers = headers + "envelope-from: " + item.CampaignRecipientID.ToString() + "-" + accountCode + "_" + campaignId + "@bounce." + senderDomain + "\n";
            }

            if (fromEmail == vmtaEmail)
            {
                headers = headers +
                    "From: " + fromName + " <" + fromEmail + ">\n" +
                    "To:  " + item.EmailId + "\n" +
                    "\n";
            }
            else
            {
                headers = headers +
                    "From: " + fromName + " <" + fromEmail + ">\n" +
                    "To:  " + item.EmailId + "\n" +
                        "Sender: " + vmtaEmail + "\n" +
                        "Reply-to: " + fromName + " <" + fromEmail + ">\n" +
                    "\n";
            }

            return headers;
        }
        /// <summary>
        /// Send Webvisit Mail through VMTA
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ownerEmail"></param>
        /// <param name="ownerName"></param>
        /// <param name="fromEmail"></param>
        /// <param name="ownerId"></param>
        /// <param name="isAdmin"></param>
        /// <returns></returns>
        public IList<string> SendWebVisitEmail(string content, string ownerEmail, string ownerName, string fromEmail, int ownerId, bool isAdmin, string subjectLine, string jobId)
        {
            Logger.Current.Verbose("In SendWebVisitEmail. OwnerEmail: " + ownerEmail + " IsAdmin: " + isAdmin);
            var subject = subjectLine;
            IList<string> successfulRecipients = new List<string>();

            //Connection con = new Connection(host, port, userName, password);
            //if (con == null)
            //    return successfulRecipients;
            try
            {
                Message msg = new Message("");

                msg.AddDateHeader();
                msg.ReturnType = ReturnType.Headers;

                String headers =
                    "Subject: " + subject + "\n" +
                    "Content-Type: text/html; charset=\"utf-8\"\n" +
                    "MIME-Version: 1.0" + "\n" +
                    "X-Mailer: STMailer" + "\n" ;
                    

                headers = headers +
                    "From: " + fromEmail + "\n" +
                    "To:  " + ownerEmail + "\n" +
                    "\n";
                msg.VirtualMTA = vmta;
                msg.JobID = jobId;
                msg.AddData(headers);
                msg.AddData("<!DOCTYPE html>\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n" + content + "\r\n</body>\r\n</html>");

                String textHeaders = headers.Replace("Content-Type: text/html;", "Content-Type: text/plain;");
                msg.BeginPart(2);
                msg.AddData(textHeaders);
                msg.AddData("<!DOCTYPE html>\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n" + content + "\r\n</body>\r\n</html>");
                Recipient recipient = new Recipient(ownerEmail);
                msg.AddRecipient(recipient);
                //con.Submit(msg);
                successfulRecipients.Add(ownerEmail);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Unable to send campaign to: " + ". Error:", ex);
            }
            return successfulRecipients;

        }

        public bool SendEmail(string content, IEnumerable<string> toEmail, string fromEmail, string fromName, string subject, string vmtaEmail, string senderDomain, string accountDomain)
        {
            Logger.Current.Verbose("Request received to send transactional email with subject: " + subject);

            Connection con = new Connection(host, port, userName, password);
            if (con == null)
                return false;
            else
            {
                Logger.Current.Verbose("PMTA connection established successfully");
            }
            var emails = new List<string>();
            if (toEmail != null && toEmail.Any())
                emails.AddRange(toEmail);

            Logger.Current.Informational("Recipients: " + emails.ToString());
            foreach (var item in emails)
            {
                Logger.Current.Informational("Preparing message to recipient: " + item);

                var returnPath = item.Split('@').First().ToString() + "@bounce." + senderDomain;
                Message msg = new Message(returnPath);
                if (msg == null)
                    continue;
                msg.AddDateHeader();
                msg.ReturnType = ReturnType.Headers;

                var unsubscribeLink = "https://" + accountDomain + "/campaignUnsubscribe?crid=" + 0;

                String headers =
                    "Subject: " + subject + "\n" +
                    "Content-Type: text/html; charset=\"utf-8\"\n" +
                    "MIME-Version: 1.0" + "\n" +
                    "X-Mailer: STMailer" + "\n" +
                    "X-STCustomer: " + item.ToString() + "\n" +
                    "List-Unsubscribe: " + "<" + unsubscribeLink + ">" + "\n" +
                    "Return-Path: " + returnPath + "\n";
                headers = headers + "envelope-from: " + item.Split('@').First().ToString() + "@bounce." + senderDomain + "\n";
                msg.EnvID = item.Split('@').First().ToString();

                if (fromEmail == vmtaEmail)
                {
                    headers = headers +
                        "From: " + fromName + " <" + fromEmail + ">\n" +
                        "To:  " + item + "\n" +
                        "\n";
                }
                else
                {
                    headers = headers +
                        "From: " + fromName + " <" + fromEmail + ">\n" +
                        "To:  " + item + "\n" +
                            "Sender: " + vmtaEmail + "\n" +
                            "Reply-to: " + fromName + " <" + fromEmail + ">\n" +
                        "\n";
                }
                Logger.Current.Verbose("Headers: " + headers);

                msg.Verp = false;
                msg.VirtualMTA = vmta;
                msg.JobID = "TransactionalEmail" + "/" + item.Split('@').First();
                msg.AddData(headers);
                msg.AddData("<!DOCTYPE html>\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n" + content + "\r\n</body>\r\n</html>");

                Logger.Current.Informational("Merged Content: " + content);
                String textHeaders = headers.Replace("Content-Type: text/html;", "Content-Type: text/plain;");
                msg.BeginPart(2);
                msg.AddData(textHeaders);
                msg.AddData("<!DOCTYPE html>\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n" + content + "\r\n</body>\r\n</html>");
                Recipient recipient = new Recipient(item);
                msg.AddRecipient(recipient);

                Logger.Current.Informational("Sending email to" + item);
                SubmitWithTimeOut(con, msg, 0, subject);
                Logger.Current.Informational("Email sent to " + item + " successfully");

            }
            return true;
        }

        /// <summary>
        /// Process merge fields
        /// </summary>
        /// <param name="HtmlContent"></param>
        /// <param name="contact"></param>
        /// <param name="customFieldValueOptions"></param>
        /// <param name="Company"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private string ReplaceMergeFields(string HtmlContent, Contact contact, IEnumerable<FieldValueOption> customFieldValueOptions, Company Company, IDictionary<string, string> info = null)
        {
            try
            {
                Logger.Current.Informational("In Replace Merge Fields");
                var doc = new HtmlDocument();
                doc.LoadHtml(HtmlContent);
                var html = new StringBuilder(HtmlContent);
                foreach (string key in info.Keys)
                {
                    var mf = new StringBuilder();
                    var rv = info[key].ToString();
                    int fid = 0;
                    if (key.Contains("CF"))
                    {
                        fid = Convert.ToInt32(key.Replace("CF", ""));
                        IEnumerable<int> SelectedValues = StringToIntList(rv);
                        var customfieldoptions = customFieldValueOptions.Where(i => i.FieldId == fid && SelectedValues.Contains(i.Id)).Select(x => x.Value);
                        string replacablecode = string.Join(",", customfieldoptions);
                        rv = (string.IsNullOrEmpty(replacablecode) ? rv : replacablecode);
                        Logger.Current.Informational("Custom field Id: " + fid + ", value: " + rv + ", contact id: " + info["CRID"]);
                    }
                    mf.Append("*|").Append(key).Append("|*");
                    html.Replace(mf.ToString(), rv);
                }

                doc.DocumentNode.InnerHtml = html.ToString();
                string outerhtml = doc.DocumentNode.OuterHtml;
                return outerhtml;
            }
            catch (Exception ex)
            {
                Logger.Current.Informational(ex.ToString());
                return HtmlContent;
            }
        }

        private string ReplaceAutomationMergeFields(string HtmlContent, Contact contact, IEnumerable<FieldValueOption> customFieldValueOptions, Company Company)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(HtmlContent);
                var allspantags = doc.DocumentNode.Descendants("span");
                var contacttype = contact.GetType().Name;
                Person person = default(Person);
                Company company = default(Company);
                if (contacttype == "Person")
                    person = (Person)contact;
                else
                    company = (Company)contact;

                var defaultaddress = contact.Addresses.Where(i => i.IsDefault).FirstOrDefault();
                foreach (var node in allspantags)
                {
                    var nodetype = node.Attributes.Where(x => x.Name == "fieldtype").Select(x => x.Value).FirstOrDefault();
                    if (nodetype == "basicfield")
                    {
                        ContactFields contactfield = default(ContactFields);
                        Enum.TryParse(node.Id, out contactfield);

                        #region ContactFields
                        if (contactfield == ContactFields.FirstNameField)
                            node.InnerHtml = node.InnerHtml.Replace("*|FIRSTNAME|*", person == null ? "" : person.FirstName);
                        else if (contactfield == ContactFields.LastNameField)
                            node.InnerHtml = node.InnerHtml.Replace("*|LASTNAME|*", person == null ? "" : person.LastName);
                        else if (contactfield == ContactFields.CompanyNameField)
                        {

                            node.InnerHtml = node.InnerHtml.Replace("*|COMPANY|*", Company == null ? "" : Company.CompanyName == null ? "" : Company.CompanyName);
                        }
                        else if (contactfield == ContactFields.TitleField)
                            node.InnerHtml = node.InnerHtml.Replace("*|TITLE|*", person == null ? "" : person.Title);
                        else if (contactfield == ContactFields.PrimaryEmail)
                        {
                            var primaryemail = contact.Emails.Where(i => i.IsPrimary).Select(x => x.EmailId).FirstOrDefault();
                            node.InnerHtml = node.InnerHtml.Replace("*|EMAILID|*", primaryemail == null ? "" : primaryemail);
                        }
                        #endregion

                        #region Socail

                        else if (contactfield == ContactFields.FacebookUrl)
                            node.InnerHtml = node.InnerHtml.Replace("*|FBURL|*", contact.FacebookUrl != null ? contact.FacebookUrl.URL : "");
                        else if (contactfield == ContactFields.LinkedInUrl)
                            node.InnerHtml = node.InnerHtml.Replace("*|LINKEDURL|*", contact.LinkedInUrl != null ? contact.LinkedInUrl.URL : "");
                        else if (contactfield == ContactFields.GooglePlusUrl)
                            node.InnerHtml = node.InnerHtml.Replace("*|GPLUSURL|*", contact.GooglePlusUrl != null ? contact.GooglePlusUrl.URL : "");
                        else if (contactfield == ContactFields.TwitterUrl)
                            node.InnerHtml = node.InnerHtml.Replace("*|TWITERURL|*", contact.TwitterUrl != null ? contact.TwitterUrl.URL : "");
                        else if (contactfield == ContactFields.WebsiteUrl)
                            node.InnerHtml = node.InnerHtml.Replace("*|WEBSITEURL|*", contact.WebsiteUrl != null ? contact.WebsiteUrl.URL : "");
                        else if (contactfield == ContactFields.BlogUrl)
                            node.InnerHtml = node.InnerHtml.Replace("*|BLOGURL|*", contact.BlogUrl != null ? contact.BlogUrl.URL : "");

                        #endregion

                        #region Address Mapping


                        else if (contactfield == ContactFields.AddressLine1Field)
                            node.InnerHtml = node.InnerHtml.Replace("*|ADDLINE1|*", defaultaddress != null ? defaultaddress.AddressLine1 : "");
                        else if (contactfield == ContactFields.AddressLine2Field)
                            node.InnerHtml = node.InnerHtml.Replace("*|ADDLINE2|*", defaultaddress != null ? defaultaddress.AddressLine2 : "");
                        else if (contactfield == ContactFields.CityField)
                            node.InnerHtml = node.InnerHtml.Replace("*|CITY|*", defaultaddress != null ? defaultaddress.City : "");
                        else if (contactfield == ContactFields.StateField)
                            node.InnerHtml = node.InnerHtml.Replace("*|STATE|*", defaultaddress != null ? defaultaddress.State.Name : "");
                        else if (contactfield == ContactFields.ZipCodeField)
                            node.InnerHtml = node.InnerHtml.Replace("*|ZIPCODE|*", defaultaddress != null ? defaultaddress.ZipCode : "");
                        else if (contactfield == ContactFields.CountryField)
                            node.InnerHtml = node.InnerHtml.Replace("*|COUNTRY|*", defaultaddress != null ? defaultaddress.Country.Name : "");

                        #endregion
                    }
                    else if (nodetype == "customfield")
                    {
                        var contactcustomfiled = contact.CustomFields.Where(i => i.CustomFieldId.ToString() == node.Id && i.ContactId == contact.Id)
                                                                              .FirstOrDefault();
                        string replacablecode = string.Empty;
                        if (contactcustomfiled != null && !string.IsNullOrEmpty(contactcustomfiled.Value) &&
                             customFieldValueOptions.Where(i => i.FieldId.ToString() == node.Id).Any())
                        {
                            IEnumerable<int> SelectedValues = StringToIntList(contactcustomfiled.Value);
                            var customfieldoptions = customFieldValueOptions.Where(i => i.FieldId.ToString() == node.Id && SelectedValues.Contains(i.Id)).Select(x => x.Value);
                            replacablecode = string.Join(",", customfieldoptions);
                        }
                        else if (contactcustomfiled != null && !string.IsNullOrEmpty(contactcustomfiled.Value))
                            replacablecode = contactcustomfiled.Value;
                        node.InnerHtml = node.InnerHtml.Replace("*|" + node.Id + "CF|*", replacablecode);
                    }
                    else if (nodetype == "dropdownfield")
                    {
                        var phonefield = contact.Phones.Where(i => i.PhoneType.ToString() == node.Id && i.ContactID == contact.Id).FirstOrDefault();
                        string replacablecode = string.Empty;
                        if (phonefield != null && !string.IsNullOrEmpty(phonefield.Number))
                            replacablecode = phonefield.Number;
                        node.InnerHtml = node.InnerHtml.Replace("*|" + node.Id + "DF|*", replacablecode);
                    }
                }
                string outerhtml = doc.DocumentNode.OuterHtml;
                return outerhtml;
            }
            catch (Exception ex)
            {
                Logger.Current.Informational(ex.ToString());
                return HtmlContent;
            }
        }

        public static IEnumerable<int> StringToIntList(string str)
        {
            if (string.IsNullOrEmpty(str))
                yield break;

            foreach (var s in str.Split('|'))
            {
                int num;
                if (int.TryParse(s, out num))
                    yield return num;
            }
        }

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
                    Exception ex = new Exception(errorMessage);
                    Logger.Current.Critical(errorMessage, ex);
                    throw ex;
                }
                else
                    Logger.Current.Informational("Campaign send successfully through VMTA");
            }
        }
    }
}
