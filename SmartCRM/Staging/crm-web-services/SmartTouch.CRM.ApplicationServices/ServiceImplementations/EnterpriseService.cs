using AutoMapper;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Processors;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.Enterprices;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Enterprises;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.CommunicationManager.Database;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class EnterpriseService : IEnterpriseService
    {
        readonly IEnterpriseServicesRepository enterpriseServiceRepository;
        readonly IContactRepository contactRepository;
        readonly IAccountRepository accountRepository;
        readonly IServiceProviderRepository serviceProviderRepository;
        readonly ICommunicationProviderService communicationService;
        readonly IAccountService accountService;

        public EnterpriseService(IEnterpriseServicesRepository enterpriseServiceRepository, IContactRepository contactRepository, IAccountRepository accountRepository,
            IServiceProviderRepository serviceProviderRepository, ICommunicationProviderService communicationService, IAccountService accountService)
        {
            this.enterpriseServiceRepository = enterpriseServiceRepository;
            this.contactRepository = contactRepository;
            this.accountRepository = accountRepository;
            this.serviceProviderRepository = serviceProviderRepository;
            this.communicationService = communicationService;
            this.accountService = accountService;
        }

        /// <summary>
        /// Get All Invalid Coupons 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetAllReportedCouponsResponse GetAllCoupons(GetAllReportedCouponsRequest request)
        {
            GetAllReportedCouponsResponse response = new GetAllReportedCouponsResponse();
            IEnumerable<ReportedCoupons> invalidCouponDta = enterpriseServiceRepository.GetAllReportedCoupons(request.PageNumber, request.Pagesize);
            IEnumerable<ReportedCouponsViewModel> invalidCouponDataViewModel = Mapper.Map<IEnumerable<ReportedCoupons>, IEnumerable<ReportedCouponsViewModel>>(invalidCouponDta);
            response.ReportedCouponsViewModel = invalidCouponDataViewModel;
            return response;
        }


        /// <summary>
        /// Sending Email
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SendEmailResponse SendEmail(SendEmailRequest request)
        {
            Logger.Current.Informational("In Send Email method");
            SendEmailResponse response = new SendEmailResponse();
            response.Success = false;
            string toEmail = string.Empty;
            string emailBody = string.Empty;
            string subject = "Coupon Report - Response - GrabOn Team";
            bool success = false;
            IDictionary<int, bool> emailStatus = new Dictionary<int, bool>();
            string[] formSubmissionIds = request.FormSubmissionIds.ToArray();
            IList<InvalidCouponEnagedFrom> invalidCouponEngatedcontacts = new List<InvalidCouponEnagedFrom>();
            
            IEnumerable<Person> contacts = contactRepository.GetEmailById(request.Contacts);
            foreach(var contact in contacts)
            {
                toEmail = contact.Email;
                emailBody = string.Format("Hi {0} {1}, <br> {2}", contact.FirstName, contact.LastName, request.EmailBody);
                success = EmailSend(subject, emailBody, toEmail, request.AccountId, request.RequestedBy.Value);
                if(!emailStatus.ContainsKey(contact.Id))
                    emailStatus.Add(contact.Id, success);

            }

            if (emailStatus != null)
            {
                var sucessContacts = emailStatus.Where(k => k.Value).Select(k => k.Key);
                if (sucessContacts.IsAny())
                {
                    contactRepository.UpdateContactLastTouchedThrough(sucessContacts,request.AccountId);
                    foreach(int conId in sucessContacts)
                    {
                        int index = Array.IndexOf(request.Contacts.ToArray(), conId);
                        string formSubmissionId = formSubmissionIds[index];
                        int[] submisionIds = formSubmissionId.Split(',').Select(x => int.Parse(x)).ToArray();
                        foreach(int fmId in submisionIds)
                        {
                            InvalidCouponEnagedFrom invalidCouponEngatedData = new InvalidCouponEnagedFrom();
                            invalidCouponEngatedData.ContactId = conId;
                            invalidCouponEngatedData.FormSubmissionId = fmId;
                            invalidCouponEngatedcontacts.Add(invalidCouponEngatedData);
                        }
                        
                    }

                    enterpriseServiceRepository.BulkInvalidCouponEngagedContacts(invalidCouponEngatedcontacts);

                }
                    
            }
            response.Success = emailStatus.Where(k => !k.Value).Any() ? false : true;
            response.Message = response.Success ? "Email sent to all selected contacts" : "Email sent to partial list of selected contacts";
            return response;
        }

        private bool EmailSend(string subject, string emailBody, string toEmail, int accountId, int userId)
        {
            Logger.Current.Informational("In Email Send Method:  " + toEmail);
            var success = false;
            var mailService = new MailService();
            ServiceProviderViewModel serviceProviderViewModel = communicationService.GetAccountServiceProviders(new GetServiceProviderRequest()
            {
                CommunicationTypeId = CommunicationType.Mail,
                AccountId = accountId,
                MailType = MailType.TransactionalEmail
            }).ServiceProviderViewModel.FirstOrDefault();
            MailRegistrationDb mailRegistration = mailService.GetMailRegistrationDetails(serviceProviderViewModel.LoginToken);
            LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest sendMailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
            var senderEmail = serviceProviderRepository.GetServiceProviderEmail(serviceProviderViewModel.CommunicationLogID);
            sendMailRequest.TokenGuid = mailRegistration.Guid;
            sendMailRequest.RequestGuid = Guid.NewGuid();
            sendMailRequest.IsBodyHtml = true;
            sendMailRequest.From = senderEmail.EmailId;
            sendMailRequest.Subject = subject;
            sendMailRequest.ScheduledTime = DateTime.Now.ToUniversalTime().AddSeconds(5);
            sendMailRequest.Body = emailBody;
            sendMailRequest.DisplayName = mailRegistration.Name;
            sendMailRequest.To = new List<string>() { toEmail };
            sendMailRequest.CategoryID = (short)EmailNotificationsCategory.InvalidCouponReport;
            sendMailRequest.AccountID = accountId;
            try
            {
                mailService.Send(sendMailRequest);
                success = true;
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("AccountId", accountId);
                ex.Data.Add("UserID", userId);
                ex.Data.Add("TO Email", toEmail);
                Logger.Current.Error("Error while sending email through transaction provider ", ex);
                success = false;
            }
            return success;
        }
    }
}
