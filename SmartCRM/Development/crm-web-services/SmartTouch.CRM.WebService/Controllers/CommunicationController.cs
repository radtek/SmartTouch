using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.WebService.Helpers;
using System;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Communication Controller
    /// </summary>
    public class CommunicationController : SmartTouchApiController
    {
        readonly ICommunicationService communicationService;
        readonly ICommunicationProviderService serviceProviderService;
        readonly IAttachmentService docrepositoryService;
        /// <summary>
        /// Creating constructor for communication controller for accessing
        /// </summary>
        /// <param name="communicationService">communicationService </param>
        /// <param name="docrepositoryService">docrepositoryService</param>
        /// <param name="serviceProviderService">serviceProviderService </param>
        public CommunicationController(ICommunicationService communicationService, IAttachmentService docrepositoryService, ICommunicationProviderService serviceProviderService)
        {
            this.communicationService = communicationService;
            this.docrepositoryService = docrepositoryService;
            this.serviceProviderService = serviceProviderService;
        }
        
        /// <summary>
        /// Get all attachments by contact.
        /// </summary>
        /// <param name="ContactID">Id of a contact.</param>
        /// <returns>Docmentaion data</returns>
        [Route("GetDocRepository")]
        [HttpGet]
        public HttpResponseMessage GetDocRepository(int ContactID)
        {
            GetAttachmentsResponse response = docrepositoryService.GetAllAttachments(new GetAttachmentsRequest() { ContactId = ContactID });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get guid.
        /// </summary>
        /// <param name="request">Properties of Communication Tracker.</param>
        /// <returns>Guid</returns>
        [Route("GetGuid")]
        [HttpPost]
        public HttpResponseMessage GetGuid(CommunicationTrackerViewModel request)
        {
            CommunicationTrackerResponse response = communicationService.GetFindByContactId(new CommunicationTrackerRequest() { CommunicationTrackerViewModel = request });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get communication login details.
        /// </summary>
        /// <param name="request">Properties of service provider.</param>
        /// <returns>Communication Log Details</returns>
        [Route("GetCommunicationLogInDetails")]
        [HttpPost]
        public HttpResponseMessage GetCommunicationLogInDetails(ServiceProviderViewModel request)
        {
            ServiceProviderResponse response = serviceProviderService.GetServiceProvider(new ServiceProviderRequest() { ServiceProviderViewModel = request });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Save communication login details.
        /// </summary>
        /// <param name="request">Properties of service provider.</param>
        /// <returns>Communication LogIn Details</returns> 
        [Route("SaveCommunicationLogInDetails")]
        [HttpPost]
        public HttpResponseMessage SaveCommunicationLogInDetails(ServiceProviderViewModel request)
        {
            ServiceProviderResponse response = serviceProviderService.SaveServiceProvider(new ServiceProviderRequest() { ServiceProviderViewModel = request });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Send Text.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("SendText")]
        [HttpPost]
        public HttpResponseMessage SendText(SendTextViewModel viewModel)
        {
            //TODO replace this with proper user id from ASPNet Identity
            viewModel.UserId = this.UserId;
            viewModel.AccountID = this.AccountId;
            SendTextRequest request = new SendTextRequest() { SendTextViewModel = viewModel };
            SendTextRequest ServiceProviderRequest = new SendTextRequest
            {
                UserId = this.UserId,
                AccountId = this.AccountId
            };

            SendTextViewModel ServiceProviderViewmodel = communicationService.GetSendTextviewModel(ServiceProviderRequest).SendTextViewModel;
            request.SendTextViewModel.ServiceProvider = ServiceProviderViewmodel.ServiceProvider;
            SendTextResponse response = communicationService.SendText(request);

            if (response.SMSStatus == "Success")
                response.Message = "Sent successfully";
            else
            {
                if (response.Exception != null)
                {
                    response.Message = response.Exception.Message;
                }
            }

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("SendEmail")]
        [HttpPost]
        public HttpResponseMessage SendMail(SendMailViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.Body))
            {
                viewModel.Body = ReplacingSpecialCharacterWithTheirCode(viewModel.Body);
                viewModel.Body = viewModel.Body.Replace("&amp;", "&");
            }
            var accountId = this.AccountId;
            var userId = this.UserId;
            viewModel.AccountID = accountId;
            SendMailRequest request = new SendMailRequest() { SendMailViewModel = viewModel, AccountId = accountId, RequestedBy = userId, UserName = viewModel.SenderName };
            ApplicationServices.Messaging.Communication.SendMailResponse response = communicationService.SendMail(request);

            if (response.ResponseStatus == CommunicationStatus.Success)
                response.ResponseMessage = "Sent successfully";
            else
                response.ResponseMessage = response.Exception.Message;

            return Request.BuildResponse(response);
        }

        private string ReplacingSpecialCharacterWithTheirCode(string content)
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

        /// <summary>
        /// Get Contact Attachments
        /// </summary>
        /// <param name="attachmentviewmodel"></param>
        /// <returns></returns>
        [Route("GetAttachments")]
        [HttpPost]
        public HttpResponseMessage GetAllAttachments(AttachmentDataViewModel attachmentviewmodel)
        {
            GetAttachmentsResponse response = docrepositoryService.GetAllAttachments(new GetAttachmentsRequest()
            {
                ContactId = attachmentviewmodel.ContactID,
                Limit = 5,
                PageNumber = attachmentviewmodel.PageNumber,
                Page = attachmentviewmodel.PageName,
                OpportunityID = attachmentviewmodel.OpportunityID,
                DateFormat = this.DateFormat
            });

            return Request.BuildResponse(response);

        }

        /// <summary>
        /// For Delete Attachemnt.
        /// </summary>
        /// <param name="docId"></param>
        /// <returns></returns>
        [Route("DeleteAttachment")]
        [HttpPost]
        public HttpResponseMessage DeleteAttachment(long docId)
        {
            AttachmentViewModel viewModel = new AttachmentViewModel()
            {
                DocumentID = docId
            };

            var successMessage = string.Empty;

            try
            {
              AttachmentResponse resonse= docrepositoryService.DeleteAttachment(
                new AttachmentRequest()
                {
                    AttachmentViewModel = viewModel
                });

                if (resonse.Exception != null)
                    successMessage = resonse.Exception.Message;
                else
                    successMessage = "Successfully Deleted Attachement";

            }
            catch (Exception ex)
            {
                successMessage = ex.Message;
            }
   

            return Request.CreateResponse(successMessage);
        }


    }
}
