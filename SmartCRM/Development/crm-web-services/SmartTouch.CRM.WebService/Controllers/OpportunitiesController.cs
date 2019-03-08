using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.WebService.Helpers;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Messaging.Opportunity;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating opportunities controller for opportunities module
    /// </summary>
    public class OpportunitiesController : SmartTouchApiController
    {
        readonly IOpportunitiesService opportunityService;
        readonly IContactService contactService;

        /// <summary>
        /// Creating constructor for opportunities controller for accessing
        /// </summary>
        /// <param name="contactService">contactService</param>
        /// <param name="opportunityService">opportunityService</param>
        public OpportunitiesController(IContactService contactService, IOpportunitiesService opportunityService)
        {
            this.contactService = contactService;
            this.opportunityService = opportunityService;
        }

        /// <summary>
        /// re index the opportunities.
        /// </summary>        
        /// <returns>ReIndexed Opportunities Details Response</returns>
        [Route("ReIndexOpportunities")]
        public HttpResponseMessage ReIndexOpportunities()
        {
            ReIndexDocumentResponse response = opportunityService.ReIndexOpportunities(new ReIndexDocumentRequest());
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Insert a new opportunity.
        /// </summary>
        /// <param name="viewModel">Properties of a new opportunity</param>
        /// <returns>Opportunity Insertion Details Response</returns>
        [Route("Opportunity")]
        [HttpPost]
        public HttpResponseMessage InsertOpportunity(OpportunityViewModel viewModel)
        {
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            viewModel.LastModifiedOn = DateTime.Now.ToUniversalTime();
            viewModel.CreatedBy = this.UserId;
            if (viewModel.ExpectedCloseDate.HasValue)
                viewModel.ExpectedCloseDate = viewModel.ExpectedCloseDate.Value.ToUniversalTime().Date;
            InsertOpportunityResponse response = opportunityService.InsertOpportunity(
              new InsertOpportunityRequest()
              {
                  RequestedBy = this.UserId,
                  opportunityViewModel = viewModel,
                  ModuleID = (byte)AppModules.Opportunity,
                  AccountId = this.AccountId,
                  RoleId = this.RoleId
              });


            return Request.BuildResponse(response);
        }

        
        /// <summary>
        /// update a opportunity.
        /// </summary>
        /// <param name="viewModel">update a existing opportunity</param>
        /// <returns>Opportunity Updation Details Response</returns>
        [Route("Opportunity")]
        [HttpPut]
        public HttpResponseMessage UpdateOpportunity(OpportunityViewModel viewModel)
        {
            viewModel.LastModifiedBy = this.UserId;
            viewModel.LastModifiedOn = DateTime.Now.ToUniversalTime();
            viewModel.CreatedOn = viewModel.CreatedOn.ToUniversalTime();
            if (viewModel.ExpectedCloseDate.HasValue)
                viewModel.ExpectedCloseDate = viewModel.ExpectedCloseDate.Value.ToUniversalTime().Date;
            UpdateOpportunityResponse response = opportunityService.UpdateOpportunity(
              new UpdateOpportunityRequest()
              {
                  opportunityViewModel = viewModel,
                  AccountId = this.AccountId,RequestedBy = this.UserId,
                  RoleId = this.RoleId,
                  ModuleID = (byte)AppModules.Opportunity
              });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Insert a new buyer.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("Buyer")]
        [HttpPost]
        public HttpResponseMessage InsertBuyer(OpportunityViewModel viewModel)
        {
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            viewModel.CreatedBy = this.UserId;
            if (viewModel.ExpectedCloseDate.HasValue)
                viewModel.ExpectedCloseDate = viewModel.ExpectedCloseDate.Value.ToUniversalTime().Date;
            InsertOpportunityBuyerResponse response = opportunityService.InsertOpportunityBuyer(
              new InsertOpportunityBuyerRequest()
              {
                  RequestedBy = this.UserId,
                  opportunityViewModel = viewModel,
                  AccountId = this.AccountId,
                  RoleId = this.RoleId,
                  ModuleID = (byte)AppModules.Opportunity
              });


            return Request.BuildResponse(response);
        }

        /// <summary>
        /// update a buyer.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("Buyer")]
        [HttpPut]
        public HttpResponseMessage UpdateBuyer(OpportunityViewModel viewModel)
        {
            viewModel.LastModifiedBy = this.UserId;
            viewModel.LastModifiedOn = DateTime.Now.ToUniversalTime();
            viewModel.CreatedOn = viewModel.CreatedOn.ToUniversalTime();
            if (viewModel.ExpectedCloseDate.HasValue)
                viewModel.ExpectedCloseDate = viewModel.ExpectedCloseDate.Value.ToUniversalTime().Date;
            UpdateOpportunityBuyerResponse response = opportunityService.UpdateOpportunityBuyer(
              new UpdateOpportunityBuyerRequest()
              {
                  opportunityViewModel = viewModel,
                  AccountId = this.AccountId,
                  RequestedBy = this.UserId,
                  RoleId = this.RoleId,
              });
            return Request.BuildResponse(response);
        }
        /// <summary>
        /// Getting Opportunities Search by names
        /// </summary>
        /// <param name="query"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Route("getopportunities")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage SearchOpportunityNames(string query, int accountId)
        {
            GetOpportunityListResponse response = opportunityService.GetAllOpportunitiesByName(new GetOpportunityListRequest()
            {
                AccountID = accountId,
                Query = query
            });
            return Request.CreateResponse(HttpStatusCode.OK, response.Opportunities);
        }
    }
}
