using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Text;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class CommunicationController : SmartTouchController
    {
        readonly ICommunicationService communicationService;
        readonly IAttachmentService attachmentService;
        readonly ICommunicationProviderService serviceProviderService;

        public CommunicationController(ICommunicationService communicationService,
            IAttachmentService attachmentService, ICommunicationProviderService serviceProviderService)
        {
            this.communicationService = communicationService;
            this.attachmentService = attachmentService;
            this.serviceProviderService = serviceProviderService;
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="sendMailViewModel">The send mail view model.</param>
        /// <returns></returns>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException">[|Send Failure|]</exception>
        [HttpPost]
        [SmarttouchAuthorize(AppModules.SendMail, AppOperations.Create)]
        public JsonResult SendMail(string sendMailViewModel)
        {
            SendMailViewModel viewModel = JsonConvert.DeserializeObject<SendMailViewModel>(sendMailViewModel);
            if (!string.IsNullOrEmpty(viewModel.Body))
            {
                viewModel.Body = ReplacingSpecialCharacterWithTheirCode(viewModel.Body);
                viewModel.Body = viewModel.Body.Replace("&amp;", "&");
            }
            var accountId = this.Identity.ToAccountID();
            var userId = this.Identity.ToUserID();
            viewModel.AccountID = accountId;
            SendMailRequest request = new SendMailRequest() { SendMailViewModel = viewModel, AccountId = accountId, RequestedBy = userId, UserName = viewModel.SenderName, AccountDomain = Request.Url.Host };
            SendMailResponse response = communicationService.SendMail(request);

            if (response.ResponseMessage == CommunicationStatus.Success)
                return Json(new { success = true, response = "[|Sent successfully|]" }, JsonRequestBehavior.AllowGet);
            else
                throw new UnsupportedOperationException("[|Send Failure|]", response.Exception);
        }

        /// <summary>
        /// Sends the text.
        /// </summary>
        /// <param name="sendTextViewModel">The send text view model.</param>
        /// <returns></returns>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException"></exception>
        [HttpPost]
        [SmarttouchAuthorize(AppModules.SendText, AppOperations.Create)]
        public JsonResult SendText(string sendTextViewModel)
        {
            SendTextViewModel viewModel = JsonConvert.DeserializeObject<SendTextViewModel>(sendTextViewModel);
            //TODO replace this with proper user id from ASPNet Identity
            viewModel.UserId = this.Identity.ToUserID();
            viewModel.AccountID = this.Identity.ToAccountID();
            SendTextRequest request = new SendTextRequest() { SendTextViewModel = viewModel };
            SendTextResponse response = communicationService.SendText(request);
            if (response.SMSStatus == "Success")
            {
                return Json(new { success = true, response = "[|Sent successfully|]" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                throw new UnsupportedOperationException(response.Message);
            }
        }

        /// <summary>
        /// Deletes the attachment.
        /// </summary>
        /// <param name="docId">The document identifier.</param>
        /// <returns></returns>
        public ActionResult DeleteAttachment(long docId)
        {
            AttachmentViewModel viewModel = new AttachmentViewModel()
            {
                DocumentID = docId
            };

            attachmentService.DeleteAttachment(
                new AttachmentRequest()
                {
                    AttachmentViewModel = viewModel
                });
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Saves the attachment.
        /// </summary>
        /// <param name="filesViewModel">The files view model.</param>
        /// <param name="page">The page.</param>
        /// <param name="StorageSource">The storage source.</param>
        /// <param name="ContactID">The contact identifier.</param>
        /// <param name="OpportunityID">The opportunity identifier.</param>
        /// <returns></returns>
        public JsonResult SaveAttachment(FilesViewModel[] filesViewModel, string page , char StorageSource, int? ContactID, int? OpportunityID)
        {
            SaveAttachmentRequest request = new SaveAttachmentRequest()
            {
                ContactId = ContactID,
                OpportunityID = OpportunityID,
                CreatedBy = this.Identity.ToUserID(),
                filesViewModel = filesViewModel,
                StorageSource = StorageSource
            };
            return Json(new { success = attachmentService.SaveAttachment(request), response = "", responseUrl = "" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Reads the attachments.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="pagename">The pagename.</param>
        /// <returns></returns>
        public ActionResult ReadAttachments([DataSourceRequest] DataSourceRequest request, string pagename)
        {
            GetAttachmentsResponse response = new GetAttachmentsResponse();

            int? ContactID = null;
            int? OpportunityID = null;
            if (pagename == "contacts")
            {
                var contactid = ReadCookie("contactid");
                ContactID = contactid == null ? new Nullable<int>() : Convert.ToInt32(contactid);
            }
            else if (pagename == "opportunities")
            {
                var opportunityid = ReadCookie("opportunityid");
                OpportunityID = opportunityid == null ? new Nullable<int>() : Convert.ToInt32(opportunityid);
            }

            response = attachmentService.GetAllAttachments(new GetAttachmentsRequest()
            {
                ContactId = ContactID,
                Limit = request.PageSize,
                PageNumber = request.Page,
                Page = pagename,
                OpportunityID = OpportunityID
            });
            return Json(
                 new DataSourceResult
                 {
                     Data = response.Attachments,
                     Total = response.TotalRecords
                 },
               JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets all attachments.
        /// </summary>
        /// <param name="attachmentviewmodel">The attachmentviewmodel.</param>
        /// <returns></returns>
        public ActionResult GetAllAttachments(AttachmentDataViewModel attachmentviewmodel)
        {

            GetAttachmentsResponse response = attachmentService.GetAllAttachments(new GetAttachmentsRequest()
            {
                ContactId = attachmentviewmodel.ContactID,
                Limit = 5,
                PageNumber = attachmentviewmodel.PageNumber,
                Page = attachmentviewmodel.PageName,
                OpportunityID = attachmentviewmodel.OpportunityID,
                DateFormat = this.Identity.ToDateFormat()
            });
            return Json(
                 new DataSourceResult
                 {
                     Data = response.Attachments,
                     Total = response.TotalRecords
                 },
               JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Reads the cookie.
        /// </summary>
        /// <param name="strValue">The string value.</param>
        /// <returns></returns>
        public string ReadCookie(string strValue)
        {
            string strValues = string.Empty;
            if (Request.Cookies[strValue] != null) { strValues = Request.Cookies[strValue].Value; }
            return strValues;
        }

        /// <summary>
        /// Gets the communication provider details.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCommunicationProviderDetails()
        {
            GetServiceProviderRequest request = new GetServiceProviderRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                CommunicationTypeId = CommunicationType.Mail,
                MailType = MailType.TransactionalEmail
            };
            GetServiceProviderResponse response = serviceProviderService.GetAccountServiceProviders(request);
            return Json(response.ServiceProviderViewModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Inserts the service provider.
        /// </summary>
        /// <param name="providerViewModel">The provider view model.</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertServiceProvider(string providerViewModel)
        {
            ProviderRegistrationViewModel viewModel = JsonConvert.DeserializeObject<ProviderRegistrationViewModel>(providerViewModel);
            viewModel.MailProviderType = MailType.BulkEmail;
            viewModel.CommunicationType = CommunicationType.Mail;
            InsertServiceProviderRequest request = new InsertServiceProviderRequest()
            {
                ProviderViewModel = viewModel,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                Url = Request.Url.Host
            };
            communicationService.AddServiceprovider(request);
            return Json(new { success = true, response = "[|Added successfully|]" }, JsonRequestBehavior.AllowGet);
        }

        public string ReplacingSpecialCharacterWithTheirCode(string content)
        {
            StringBuilder result = new StringBuilder(content.Length + (int)(content.Length * 0.1));
            foreach (char c in content)
            {
                int value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);

            }

            return result.ToString();
        }

    }
}