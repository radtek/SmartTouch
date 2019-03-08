using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Extensions;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using System.IO;
using System.Net.Mime;

namespace LandmarkIT.Enterprise.CommunicationManager.Processors
{
    public class SmtpMailService : IMailService
    {
        #region Variables
        private IUnitOfWork unitOfWork = default(IUnitOfWork);
        private SmtpClient smtpClient = default(SmtpClient);
        private MailRegistrationDb registration = default(MailRegistrationDb);
        private short parallelLoad = default(short);
        private MailProvider provider = MailProvider.SendGrid;
        #endregion

        public SmtpMailService(IUnitOfWork unitOfWork, Guid token, short parallelLoad = 10)
        {
            Logger.Current.Verbose("Request received for sending an email through smtp-mailservice");
            this.unitOfWork = unitOfWork;
            this.parallelLoad = parallelLoad;
            registration = this.unitOfWork.MailRegistrationsRepository.Single(mr => mr.Guid == token);
            if (registration != null && registration.Port.HasValue)
                smtpClient = new SmtpClient(registration.Host, registration.Port.Value);
            else
                smtpClient = new SmtpClient(registration.Host);
            smtpClient.Credentials = new NetworkCredential(registration.UserName, registration.Password);
            if (registration.Port.HasValue) smtpClient.Port = registration.Port.Value;
            smtpClient.EnableSsl = registration.IsSSLEnabled;
        }
        public SmtpMailService(MailRegistrationDb registration)
        {
            smtpClient = new SmtpClient(registration.Host);
            smtpClient.Credentials = new NetworkCredential(registration.UserName, registration.Password);
            if (registration.Port.HasValue) smtpClient.Port = registration.Port.Value;
            smtpClient.EnableSsl = registration.IsSSLEnabled;
            provider = MailProvider.Smtp;
            this.registration = registration;
        }
        public SendMailResponse Send(SendMailRequest request)
        {
            if (provider == MailProvider.Smtp)
            {
                var failedRecipients = new List<string>();
                var requests = new List<MailMessage>();
                var campaignReceipients = request.CampaignReceipients;
                
                while (campaignReceipients.Any())
                {
                    var message = new MailMessage();
                    message.From = new MailAddress(request.From, request.DisplayName);
                    message.Subject = request.Subject;
                    message.IsBodyHtml = true;

                    var campaignReceipient = campaignReceipients.First();
                    try
                    {
                        var campaignBody = request.Body.Replace("*|CRID|*", campaignReceipient.CampaignRecipientID.ToString());
                        message.To.Add(campaignReceipient.EmailId);
                        string MergeStringContent = campaignBody;
                        if (campaignReceipient.ContactInfo != null)
                            MergeStringContent = ReplaceMergeFields(campaignBody, campaignReceipient.ContactInfo);

                        message.Body = MergeStringContent;
                        requests.Add(message);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Error while adding recipient " + campaignReceipient.EmailId + " to list. Hence skipped. Error: ", ex);
                        failedRecipients.Add(campaignReceipient.EmailId);
                    }
                    campaignReceipients = campaignReceipients.Skip(1).ToList();
                }
                SendMailResponse sendMailResponse = SmtpSend(registration.Guid, requests);
                failedRecipients.AddRange(sendMailResponse.FailedRecipients);
                sendMailResponse.FailedRecipients = failedRecipients;
                return sendMailResponse;
            }
            else
            {
                return SmtpSend(registration.Guid, request);
            }

        }

        public Task<SendMailResponse> SendAsync(SendMailRequest request)
        {
            return Task<SendMailResponse>.Run(() => Send(request));
        }

        public List<SendMailResponse> Send(List<SendMailRequest> request)
        {
            var response = new List<SendMailResponse>();
            //parallelLoad
            var iterations = (request.Count / parallelLoad);
            if ((request.Count % parallelLoad) > 0) iterations++;
            for (int i = 0; i < iterations; i++)
            {
                var currentIterations = request.Skip(i * parallelLoad).Take(parallelLoad).ToList();

                Parallel.ForEach(currentIterations, item =>
                    { response.Add(SmtpSend(registration.Guid, item)); });
            }
            return response;
        }
        public Task<List<SendMailResponse>> SendAsync(List<SendMailRequest> request)
        {
            return Task<List<SendMailResponse>>.Run(() => Send(request));
        }
        private SendMailResponse SmtpSend(Guid token, SendMailRequest request)
        {
            Logger.Current.Verbose("sending an email from smtp-mail service");
            var result = new SendMailResponse();
            try
            {
                 result.Token = token;
                var message = request.ToMailMessage();
                System.Net.Mail.Attachment attachment = default(System.Net.Mail.Attachment);

                if (request.To.Count() > 1)
                {
                    result.Token = token;
                    request.To.Each(s =>
                        {
                            string mailBody = request.Body;
                            if (request.MergeValues != null)
                            {
                                var mergeData = request.MergeValues.Where(m => m.Key == s).Select(m => m.Value).FirstOrDefault();
                                if (mergeData != null)
                                {
                                    mergeData.Each(m =>
                                    {
                                        mailBody = mailBody.Replace("*|" + m.Key + "|*", m.Value);
                                    });
                                }
                                request.To = new List<string>() { s };
                                var contactMessage = request.ToMailMessage();
                                contactMessage.Body = mailBody;
                                smtpClient.Send(contactMessage);
                            }
                            else
                            {
                                request.To = new List<string>() { s };
                                var contactMessage = request.ToMailMessage();
                                contactMessage.Body = mailBody;
                                smtpClient.Send(contactMessage);
                            }
                            result.StatusID = LandmarkIT.Enterprise.CommunicationManager.Responses.CommunicationStatus.Success;
                            Logger.Current.Verbose("Sending an email from smtp-mail service is successful");

                        });
                }
                else
                {
                    if (request.MergeValues != null)
                    {
                        var mergeData = request.MergeValues.Where(m => m.Key == request.To.FirstOrDefault()).Select(m => m.Value).FirstOrDefault();
                        if (mergeData != null)
                        {
                            mergeData.Each(m =>
                            {
                                message.Body = message.Body.Replace("*|" + m.Key + "|*", m.Value);
                            });
                        }
                    }

                    if (request.AttachmentGUID != null && request.AttachmentGUID != Guid.Empty)
                    {
                        string fileName = request.AttachmentGUID.ToString() + ".ics";
                        string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["ATTACHMENT_PHYSICAL_PATH"].ToString(), fileName);
                        StreamReader reader = new StreamReader(savedFileName);
                        attachment = new System.Net.Mail.Attachment(reader.BaseStream, fileName, MediaTypeNames.Application.Octet);
                        attachment.ContentDisposition.FileName = fileName;

                    }
                    if (request.NightlyAttachmentGUIDS.IsAny())
                    {
                        int i = 0;
                        foreach (Guid giud in request.NightlyAttachmentGUIDS)
                        {
                            System.Net.Mail.Attachment nightlyAttachment = default(System.Net.Mail.Attachment);
                            string fileName = giud.ToString() + ".csv";
                            string excelPath = System.Configuration.ConfigurationManager.AppSettings["NIGHTLYREPORT_PHYSICAL_PATH"].ToString();

                            var icsContent = string.Empty;
                            string savedExcelFileName = Path.Combine(excelPath, fileName);
                            StreamReader reader = new StreamReader(savedExcelFileName);
                            nightlyAttachment = new System.Net.Mail.Attachment(reader.BaseStream, fileName, MediaTypeNames.Application.Octet);
                            nightlyAttachment.ContentDisposition.FileName = i == 0 ? "1 Day Report.csv" : "7 Days Report.csv ";
                            message.Attachments.Add(nightlyAttachment);
                            i++;
                        }
                    }

                    result.RequestGuid = request.RequestGuid == Guid.Empty ? Guid.NewGuid() : request.RequestGuid;

                    if (attachment != null)
                        message.Attachments.Add(attachment);

                    smtpClient.Send(message);
                    result.StatusID = LandmarkIT.Enterprise.CommunicationManager.Responses.CommunicationStatus.Success;
                    Logger.Current.Verbose("Sending an email from smtp-mail service is successful");
                   

                }
           
            }
            catch (Exception ex)
            {
                result.StatusID = LandmarkIT.Enterprise.CommunicationManager.Responses.CommunicationStatus.Failed;
                result.ServiceResponse = ex.Message;
                Logger.Current.Error("An error occured while sending an email from smtp-mail service/send-grid" , ex);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY, values: new object[] { request });
            }
            return result;
        }

        private SendMailResponse SmtpSend(Guid token, IEnumerable<MailMessage> requests)
        {
            Logger.Current.Verbose("sending an email from smtp-mail service");
            var result = new SendMailResponse();
            var _parallelLoad = 100;
            var iterations = (requests.Count() / _parallelLoad);
            if ((requests.Count() % _parallelLoad) > 0) iterations++;
            result.FailedRecipients = new List<string>();
            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    Logger.Current.Verbose("Iteration: " + i);
                    var currentIterations = requests.Skip(i * _parallelLoad).Take(_parallelLoad).ToList();

                    currentIterations.ForEach(s =>
                    {
                        var failedRecipients = new List<string>();
                        try
                        {
                            result.Token = token;
                            result.RequestGuid = Guid.NewGuid();
                            smtpClient.Send(s);
                            result.StatusID = LandmarkIT.Enterprise.CommunicationManager.Responses.CommunicationStatus.Success;
                            Logger.Current.Verbose("Sent email to: " + s.To);
                        }
                        catch (Exception ex)
                        {
                            ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY, values: new object[] { requests });
                            try
                            {
                                smtpClient = new SmtpClient(this.registration.Host, this.registration.Port.Value);
                                smtpClient.Credentials = new NetworkCredential(this.registration.UserName, this.registration.Password);
                                smtpClient.EnableSsl = this.registration.IsSSLEnabled;
                                if (this.registration.Port.HasValue) smtpClient.Port = this.registration.Port.Value;
                                smtpClient.Send(s);
                            }
                            catch (Exception exception)
                            {
                                result.StatusID = LandmarkIT.Enterprise.CommunicationManager.Responses.CommunicationStatus.Failed;
                                result.ServiceResponse = exception.Message;
                                Logger.Current.Verbose("An error occured while sending an email from smtp-mail service/send-grid" + exception);
                                ExceptionHandler.Current.HandleException(exception, DefaultExceptionPolicies.LOG_ONLY_POLICY, values: new object[] { requests });
                                failedRecipients.AddRange(s.To.Select(c => c.Address));
                                result.FailedRecipients = failedRecipients;
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("Exception occured in iteration: " + i, ex);
                }
            }

            return result;
        }

        //public static string MergeFields(string htmlcontent, Contact contact)
        //{
        //    var doc = new HtmlDocument();
        //    doc.LoadHtml(htmlcontent);
        //    var allspantags = doc.DocumentNode.Descendants("span");
        //    foreach (var node in allspantags)
        //    {
        //        if ((node.Id == ((int)CommonEntites.ContactFields.FirstNameField).ToString()))
        //        {
        //            node.InnerHtml = "Srinivas";
        //        }
        //        else if ((node.Id == ((int)CommonEntites.ContactFields.LastNameField).ToString()))
        //            node.InnerHtml = "Naidu";
        //        else
        //            node.InnerHtml = contact.CreatedOn.ToString();
        //    }
        //    var htm = doc.DocumentNode.OuterHtml;
        //    return htm;
        //}

        private string ReplaceMergeFields(string HtmlContent, Contact contact)
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

                            node.InnerHtml = node.InnerHtml.Replace("*|COMPANY|*", contacttype == "Person" ?
                                                                        person == null ? "" : person.CompanyName :
                                                                        company == null ? "" : company.CompanyName);
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
                        if (contactcustomfiled != null && !string.IsNullOrEmpty(contactcustomfiled.Value))
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
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return HtmlContent;
            }
        }
    }
}
