using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;
using port25.pmta.api.submitter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.CommunicationManager.Processors
{
    public class VmtaMailService : IMailService
    {
        #region Variables
        private IUnitOfWork unitOfWork = default(IUnitOfWork);
        private MailRegistrationDb registration = default(MailRegistrationDb);
        #endregion

        public VmtaMailService(IUnitOfWork unitOfWork, Guid token)
        {
            Logger.Current.Verbose("Request received for sending an email through vmta-mailservice");
            this.unitOfWork = unitOfWork;
            registration = this.unitOfWork.MailRegistrationsRepository.Single(mr => mr.Guid == token);

        }
        public SendMailResponse Send(SendMailRequest request)
        {
            Logger.Current.Verbose("VmtaMailService > Send - Subject: " + request.Subject);
            SendMailResponse response = new SendMailResponse();
            try
            {
                var result = SendEmail(request.Subject, request.Body, request.To, request.CC, request.BCC
                                , request.From, request.ServiceProviderEmail, request.DisplayName, request.AccountDomain,request.AttachmentGUID, request.NotificationAttachementGuid,request.NightlyAttachmentGUIDS,request.MergeValues);
                Logger.Current.Verbose("VmtaMailService > Send - Subject: " + request.Subject);
                response.StatusID = result ? CommunicationStatus.Success : CommunicationStatus.Failed;
                if (response.StatusID == CommunicationStatus.Success)
                {
                    Logger.Current.Verbose("Email sent successfully with subject: " + request.Subject);
                    response.ServiceResponse = "Email sent successfully";
                }
                else
                {
                    Logger.Current.Verbose("Email failed with subject: " + request.Subject);
                    response.ServiceResponse = "Email failed";
                }
                response.RequestGuid = request.RequestGuid == Guid.Empty ? Guid.NewGuid() : request.RequestGuid;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured while sending email: " + ex);
                response.ServiceResponse = ex.Message;
                response.StatusID = CommunicationStatus.Failed;
            }
            return response;

        }

        public bool SendEmail_Legacy(string subject, string content, IEnumerable<string> to,
           IEnumerable<string> cc, IEnumerable<string> bcc, string fromEmail, string vmtaEmail, string senderName, string accountDomain, Guid? attachmentGuid)
        {
            Logger.Current.Verbose("Request received to send transactional email with subject: " + subject);
            String sender = fromEmail;
            Message msg = new Message(fromEmail);

            var emails = new List<string>();
            if (to != null)
                emails.AddRange(to);
            if (cc != null)
                emails.AddRange(cc);
            if (bcc != null)
                emails.AddRange(bcc);

            foreach (var item in emails)
            {
                var recipient = new Recipient(item);
                msg.AddRecipient(recipient);
            }


            String boundary = "boundary";

            msg.AddDateHeader();
            msg.AddData(
                "From: " + senderName + "<" + sender + ">\n" +
                "To: You There <" + emails[0] + ">\n" +
                "Subject: " + subject + "\n" +
                "MIME-Version: 1.0\n" +
                "Content-Type: multipart/related; boundary=\"" + boundary + "\"\n" +
                "\n");
            msg.AddData(
                "\n" +
                "This is a multi-part message in MIME format.\n" +
                "\n");
            msg.AddData(
                "--" + boundary + "\n" +
                "Content-Type: text/html\n" +
                "\n" + "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n" + content + "\r\n</body>\r\n</html>" + "\n\n");
            
            if (attachmentGuid != null && attachmentGuid != Guid.Empty)
            {
                Logger.Current.Verbose("Has Attachment: " + attachmentGuid.Value.ToString());
                var fileName = attachmentGuid.Value.ToString() + ".ics";
                string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ATTACHMENT_PHYSICAL_PATH"].ToString(), fileName);
                var icsContent = string.Empty;

                using (var reader = new StreamReader(savedFileName))
                {
                    icsContent = reader.ReadToEnd();
                }
                msg.AddData(
                    "--" + boundary + "\n" +
                    "Content-Type: text/plain\n" +
                    "Content-Disposition: attachment; filename=invite.ics\n" +
                    "\n" +
                    icsContent);

            }
            else
            {
                Logger.Current.Verbose("No Attachment");
            }

            msg.AddData("--" + boundary + "--\n");

            Connection con = new Connection(registration.Host, (int)registration.Port, registration.UserName, registration.Password);
            con.Submit(msg);
            con.Close();

            return true;
        }

        public bool SendEmail(string subject, string content, IEnumerable<string> to,
            IEnumerable<string> cc, IEnumerable<string> bcc, string fromEmail, string vmtaEmail, string senderName, string accountDomain,Guid? attachmentGuid, string notificationAttachementGuid,IEnumerable<Guid> nightlyAttachmentGuids,Dictionary<string,Dictionary<string,string>> mergeFields)
        {
            Logger.Current.Verbose("Request received to send transactional email with subject: " + subject);

            Connection con = new Connection(registration.Host, (int)registration.Port, registration.UserName, registration.Password);
            if (con == null)
               return false;
            else
            {
                Logger.Current.Verbose("PMTA connection established successfully");
            }
            var emails = new List<string>();
            if (to != null)
                emails.AddRange(to);
            if (cc != null)
                emails.AddRange(cc);
            if (bcc != null)
                emails.AddRange(bcc);
            Logger.Current.Informational("Recipients: " + emails.ToString());
            foreach (var item in emails)
            {
                Logger.Current.Informational("Preparing message to recipient: " + item);

                var returnPath = item.Split('@').First().ToString() + "@bounce." + registration.SenderDomain;
                Message msg = new Message(returnPath);
                if (msg == null)
                    continue;
                msg.AddDateHeader();
                msg.ReturnType = ReturnType.Headers;
                String boundary = "boundary";
                string mailBody = content;
                if (mergeFields != null)
                {
                   
                    var mergeData = mergeFields.Where(m=>m.Key==item).Select(m => m.Value).FirstOrDefault();
                    if(mergeData != null)
                    {
                        mergeData.Each(m =>
                        {
                            mailBody = mailBody.Replace("*|" + m.Key + "|*", m.Value);
                        });
                    }

                }

                var unsubscribeLink = !string.IsNullOrEmpty(accountDomain) ? "https://" + accountDomain + "/campaignUnsubscribe?crid=" + 0 : accountDomain; 
                String headers =
                    "Subject: " + subject + "\n" +
                    "Content-Type: multipart/related; boundary=\"" + boundary + "\"\n" +

                    "MIME-Version: 1.0" + "\n" +
                    "X-Mailer: STMailer" + "\n" +
                    "X-STCustomer: " + item.ToString() + "\n" +
                    "List-Unsubscribe: " + "<" + unsubscribeLink + ">" + "\n" +
                    "Return-Path: " + returnPath + "\n";
                headers = headers + "envelope-from: " + item.Split('@').First().ToString() + "@bounce." + registration.SenderDomain + "\n";
                msg.EnvID = item.Split('@').First().ToString();

                Logger.Current.Informational("From email : " + fromEmail);
                Logger.Current.Informational("vmta email : " + vmtaEmail);

                if (fromEmail == vmtaEmail)
                {
                    headers = headers +
                        "From: " + senderName + " <" + fromEmail + ">\n" +
                        "To:  " + item + "\n" +
                        "\n";
                }
                else
                {
                    headers = headers +
                        "From: " + senderName + " <" + fromEmail + ">\n" +
                        "To:  " + item + "\n" +
                            "Sender: " + vmtaEmail + "\n" +
                            "Reply-to: " + senderName + " <" + fromEmail + ">\n" +
                        "\n";
                }
                Logger.Current.Verbose("Headers: " + headers);

                msg.Verp = false;
                msg.VirtualMTA = registration.VMTA;
                msg.JobID = "TransactionalEmail" + "/" + item.Split('@').First();
                msg.AddData(headers);
                msg.AddData("<!DOCTYPE html>\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n" + mailBody + "\r\n</body>\r\n</html>");

                Logger.Current.Informational("Merged Content: " + mailBody);
                //String textHeaders = headers.Replace("Content-Type: text/html;", "Content-Type: text/plain;");

                msg.AddData(
                "\n" +
                "This is a multi-part message in MIME format.\n" +
                "\n");
                msg.AddData(
                    "--" + boundary + "\n" +
                    "Content-Type: text/html\n" +
                    "\n" + "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n" + mailBody + "\r\n</body>\r\n</html>" + "\n\n");

                if (attachmentGuid != null && attachmentGuid != Guid.Empty)
                {
                    var fileName = attachmentGuid.Value.ToString() + ".ics";
                    
                    string path = System.Configuration.ConfigurationManager.AppSettings["ATTACHMENT_PHYSICAL_PATH"].ToString();
                    var file = Directory.GetFiles(path, fileName, SearchOption.AllDirectories).FirstOrDefault();
                    if (file == null)
                    {
                        string pdfFileName = attachmentGuid.Value.ToString() + ".pdf";
                        string pdfPath = System.Configuration.ConfigurationManager.AppSettings["NOTIFICATION_ATTACHMENT_PHYSICAL_PATH"].ToString();
                        var pdfFile = Directory.GetFiles(pdfPath, pdfFileName, SearchOption.AllDirectories).FirstOrDefault();
                        Func<string, byte[]> ReadFile = (fname) =>
                        {
                            byte[] buffer = null;
                            using (FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read))
                            {
                                buffer = new byte[fs.Length];
                                fs.Read(buffer, 0, (int)fs.Length);
                            }
                            return buffer;
                        };
                       
                            string savedPdfFileName = Path.Combine(pdfPath, pdfFileName);
                            //var attaContent = new byte[int.MaxValue];
                            //attaContent = ReadFile(savedPdfFileName);
                            //Logger.Current.Informational("Stream data" + attaContent);
                            msg.AddData(
                                "--" + boundary + "\n" +
                                "Content-Type: application/pdf\n; " +
                                "Content-Disposition: attachment; filename=FormSubmissionNotification.pdf\n" +
                                "/n" +
                                System.Text.Encoding.UTF8.GetString(ReadFile(savedPdfFileName)));
                            //msg.AddData(ReadFile(savedPdfFileName));
                            //System.Text.Encoding.UTF8.GetString(ReadFile(savedPdfFileName)));
                            //msg.AddData(ReadFile(savedPdfFileName));
                            //msg.AddData(
                            //    "--" + boundary + "\n" +
                            //    "Content-Type: application/octet-stream\n" +
                            //    "Content-Disposition: attachment; filename=FormSubmissionNotification3.pdf\n" +
                            //    "Content-Transfer-Encoding: base64"+
                            //    "\n" +
                            //    attaContent);

                    }
                    else
                    {
                        var icsContent = string.Empty;
                        string savedFileName = Path.Combine(path, fileName);
                        using (var reader = new StreamReader(savedFileName))
                        {
                            icsContent = reader.ReadToEnd();
                        }
                        msg.AddData(
                            "--" + boundary + "\n" +
                            "Content-Type: text/plain\n" +
                            "Content-Disposition: attachment; filename=invite.ics\n" +
                            "\n" +
                            icsContent);
                    }
                }

                if(nightlyAttachmentGuids.IsAny())
                {
                    int i = 0;
                    foreach (Guid guid in nightlyAttachmentGuids)
                    {
                        string excelFileName = guid.ToString() + ".csv";
                        string excelPath = System.Configuration.ConfigurationManager.AppSettings["NIGHTLYREPORT_PHYSICAL_PATH"].ToString();
                        string savedExcelFileName = Path.Combine(excelPath, excelFileName);
                       
                        Func<string, byte[]> ReadFile = (fname) =>
                        {
                            byte[] buffer = null;
                            using (FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read))
                            {
                                buffer = new byte[fs.Length];
                                fs.Read(buffer, 0, (int)fs.Length);
                            }
                            return buffer;
                        };

                        string actualFileName = i == 0 ? "1 Day Report.csv" : " 7 Days Report.csv";
                            
                        msg.AddData(
                            "--" + boundary + "\n" +
                            "Content-Type: application/csv\n; " +
                            "Content-Disposition: attachment; filename=" + actualFileName + "\n" +
                            "/n" +
                           System.Text.Encoding.UTF8.GetString(ReadFile(savedExcelFileName)));
                            i++;
                    }
                }

                msg.AddData("--" + boundary + "--\n");
                //msg.BeginPart(2);
                //msg.AddData(textHeaders);
                //msg.AddData("<!DOCTYPE html>\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n" + content + "\r\n</body>\r\n</html>");                

                Recipient recipient = new Recipient(item);
                msg.AddRecipient(recipient);
                
                Logger.Current.Informational("Sending email to" + item);
                SubmitWithTimeOut(con, msg, 0, subject);
                Logger.Current.Informational("Email sent to " + item + " successfully");

            }
            return true;
        }

        public void SubmitWithTimeOut(Connection connection, Message message, int Id, string subject)
        {
            if (connection != null)
            {
                var tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                int timeOut = 10000;

                var task = Task.Factory.StartNew(() => connection.Submit(message), token);

                if (!task.Wait(timeOut, token))
                {
                    string errorMessage = "Sending a transactional email through VMTA has timed out, Email subject : " + subject;
                    Exception ex = new Exception(errorMessage);
                    Logger.Current.Critical(errorMessage, ex);
                    throw ex;
                }
                else
                    Logger.Current.Informational("Campaign send successfully through VMTA");
            }
        }

        public Task<SendMailResponse> SendAsync(SendMailRequest request)
        {
            return Task<List<SendMailResponse>>.Run(() => Send(request));
        }
        public List<SendMailResponse> Send(List<SendMailRequest> request)
        {
            return new List<SendMailResponse>();
        }
        public Task<List<SendMailResponse>> SendAsync(List<SendMailRequest> request)
        {
            return Task<List<SendMailResponse>>.Run(() => Send(request));
        }

        public static byte[] GetBytesFromIcsFile(string savedFileName)
        {
            StreamReader reader = new StreamReader(savedFileName);
            var bytes = reader.CurrentEncoding.GetBytes(reader.ReadToEnd());
            return bytes.ToArray();

        }
    }
}
