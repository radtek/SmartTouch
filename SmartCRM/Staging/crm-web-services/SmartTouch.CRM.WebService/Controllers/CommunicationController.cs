using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.WebService.Helpers;
using System.Net.Http;
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
    }
}
