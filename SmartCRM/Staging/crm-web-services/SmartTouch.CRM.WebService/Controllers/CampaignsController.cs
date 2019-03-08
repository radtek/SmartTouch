using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.WebService.Helpers;
using SmartTouch.CRM.WebService.DependencyResolution;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using LandmarkIT.Enterprise.Utilities.Logging;
using System.IO;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Campaigns;
using Newtonsoft.Json;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Collections;
using System.Text;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating campaigns controller for campaigns module
    /// </summary>
    public class CampaignsController : SmartTouchApiController
    {
        readonly ICampaignService campaignService;
        readonly IAdvancedSearchService advancedSearchService;
        readonly ITagService tagService;
        readonly IAccountService accountService;
        /// <summary>
        /// Creating constructor for campaigns controller for accessing
        /// </summary>
        /// <param name="campaignService">campaignService </param>
        /// <param name="advancedSearchService">advancedSearchService</param>
        /// <param name="tagService">tagService</param>
        public CampaignsController(ICampaignService campaignService, IAdvancedSearchService advancedSearchService, ITagService tagService, IAccountService accountService)
        {
            this.campaignService = campaignService;
            this.advancedSearchService = advancedSearchService;
            this.tagService = tagService;
            this.accountService = accountService;
        }


        /// <summary>
        /// Replicate a campaign
        /// </summary>
        /// <param name="viewModel">Properties of a campaign</param>
        /// <returns>Replicate campaign details</returns>

        [Route("savecampaignas")]
        [HttpPost]
        public HttpResponseMessage ReplicateCampaign(CampaignViewModel viewModel)
        {
            viewModel.CampaignID = 0;
            InsertCampaignRequest request = new InsertCampaignRequest() { CampaignViewModel = viewModel, AccountId = this.AccountId };
            if (request.CampaignViewModel.ScheduleTime != null)
            {
                request.CampaignViewModel.ScheduleTime = request.CampaignViewModel.ScheduleTime.Value.ToUniversalTime();
            }
            DateTime universaldate = DateTime.Now.ToUniversalTime();
            viewModel.CreatedDate = universaldate;
            viewModel.CreatedBy = this.UserId;
            viewModel.LastUpdatedBy = this.UserId;
            viewModel.LastUpdatedOn = universaldate;
            viewModel.Posts = viewModel.GetPosts();
            foreach (var post in viewModel.Posts)
            {
                post.UserID = viewModel.CreatedBy;
                post.CampaignID = 0;
            }
            InsertCampaignResponse response = campaignService.InsertCampaign(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Inserts a new campaign.
        /// </summary>
        /// <param name="viewModel">Properties of a new campaign</param>
        /// <returns>Insertion details</returns>

        [Route("Campaign")]
        [HttpPost]
        public HttpResponseMessage PostCampaign(CampaignViewModel viewModel)
        {
            InsertCampaignRequest request = new InsertCampaignRequest()
            {
                CampaignViewModel = viewModel,
                AccountId = this.AccountId
            };
            request.CampaignViewModel.ScheduleTime = DateTime.Now.ToUniversalTime();
            DateTime universaldate = DateTime.Now.ToUniversalTime();
            
            viewModel.CampaignTemplate = new CampaignTemplateViewModel() { TemplateId = 1 };
            viewModel.CreatedDate = universaldate;
            viewModel.CreatedBy = this.UserId;
            viewModel.LastUpdatedBy = this.UserId;
            viewModel.LastUpdatedOn = universaldate;
            viewModel.SenderName = "";
            viewModel.Posts = viewModel.GetPosts();
            viewModel.ToTagStatus = 2;
            viewModel.SSContactsStatus = 2;
            viewModel.HasDisclaimer = accountService.AccountHasDisclaimer(this.AccountId).HasValue && accountService.AccountHasDisclaimer(this.AccountId).Value == true? true:false;
            viewModel.IncludePlainText = (CampaignType)viewModel.CampaignTypeId == CampaignType.PlainText ? false : true;
            if(viewModel.CampaignStatus != CampaignStatus.Active)
               viewModel.CampaignStatus = CampaignStatus.Draft;

            if (!string.IsNullOrEmpty(viewModel.HTMLContent))
            {
                StringBuilder result = new StringBuilder(viewModel.HTMLContent.Length + (int)(viewModel.HTMLContent.Length * 0.1));

                foreach (char c in viewModel.HTMLContent)
                {
                    int value = Convert.ToInt32(c);
                    if (value > 127)
                        result.AppendFormat("&#{0};", value);
                    else
                        result.Append(c);

                }

                viewModel.HTMLContent = result.ToString();
            }

            viewModel.AccountID = this.AccountId;
            foreach (var post in viewModel.Posts)
            {
                post.UserID = viewModel.CreatedBy;
                post.CampaignID = 0;
            }
            InsertCampaignResponse response = campaignService.InsertCampaign(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Update a campaign.
        /// </summary>
        /// <param name="viewModel">Properties of a camapign.</param>
        /// <returns>updation details</returns>

        [Route("Campaign")]
        [HttpPut]
        public HttpResponseMessage PutCampaign(CampaignViewModel viewModel)
        {
            viewModel.ScheduleTime = DateTime.Now.ToUniversalTime();
            viewModel.CreatedDate = viewModel.CreatedDate.ToUniversalTime();
            DateTime universaldate = DateTime.Now.ToUniversalTime();
            viewModel.LastUpdatedBy = this.UserId;
            viewModel.LastUpdatedOn = universaldate;
            viewModel.Posts = viewModel.GetPosts();

            if (!string.IsNullOrEmpty(viewModel.HTMLContent))
            {
                StringBuilder result = new StringBuilder(viewModel.HTMLContent.Length + (int)(viewModel.HTMLContent.Length * 0.1));

                foreach (char c in viewModel.HTMLContent)
                {
                    int value = Convert.ToInt32(c);
                    if (value > 127)
                        result.AppendFormat("&#{0};", value);
                    else
                        result.Append(c);

                }

                viewModel.HTMLContent = result.ToString();
            }
            

            viewModel.HasDisclaimer = accountService.AccountHasDisclaimer(this.AccountId).HasValue && accountService.AccountHasDisclaimer(this.AccountId).Value == true ? true : false;
            foreach (var post in viewModel.Posts)
            {
                post.UserID = this.UserId;
                post.CampaignID = viewModel.CampaignID;
            }
            UpdateCampaignRequest request = new UpdateCampaignRequest() { CampaignViewModel = viewModel, RequestedBy = this.UserId, AccountId = this.AccountId };
            UpdateCampaignResponse response = campaignService.UpdateCampaign(request);
            return Request.BuildResponse(response);
        }

       

        /// <summary>
        /// Cancel a campaign.
        /// </summary>
        /// <param name="campaignId">Id of a campaign</param>
        /// <returns>cancel campaign</returns>

        [Route("Campaign/Cancel")]
        [HttpGet]
        public HttpResponseMessage CancelCampaign(int campaignId)
        {
            CancelCampaignRequest request = new CancelCampaignRequest(campaignId)
            {
                RequestedBy = this.UserId,
                AccountId = this.AccountId,
                RoleId = this.RoleId
            };
            CancelCampaignResponse response = campaignService.CancelCampaign(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Delete a campaign.
        /// </summary>
        /// <param name="campaignId">Id of a campaign</param>
        /// <returns>Delete campaign details </returns>

        [Route("Campaign/Delete")]
        [HttpDelete]
        public HttpResponseMessage DeleteCampaign(int[] campaignId)
        {
            DeleteCampaignRequest request = new DeleteCampaignRequest();
            request.CampaignID = campaignId;
            request.AccountId = this.AccountId;
            request.RequestedBy = this.UserId;
            DeleteCampaignResponse response = campaignService.Deactivate(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get a campaign by Id.
        /// </summary>
        /// <param name="campaignId">Id of the campaign.</param>
        /// <returns>campaign details by Id</returns>

        [Route("Campaign")]
        [HttpGet]
        public HttpResponseMessage GetCampaign(int campaignId)
        {
            GetCampaignRequest request = new GetCampaignRequest(campaignId)
            {
                RequestedBy = this.UserId,
                AccountId = this.AccountId,
                RoleId = this.RoleId
            };
            GetCampaignResponse response = campaignService.GetCampaign(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get campaign templates.
        /// </summary>       
        /// <returns>All Campaign templates</returns>

        [Route("CampaignTemplates")]
        [HttpGet]
        public HttpResponseMessage GetCampaignTemplates()
        {
            GetCampaignTemplatesRequest request = new GetCampaignTemplatesRequest() { AccountId = this.AccountId };
            GetCampaignTemplatesResponse response = campaignService.GetCampaignTemplates(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get a campaign template by Id.
        /// </summary>
        /// <param name="campaignTemplateID">Id of the Campaign template.</param>
        /// <returns>Single Campaign Template </returns>

        [Route("CampaignTemplate")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage GetCampaignTemplate(int campaignTemplateID)
        {
            GetCampaignTemplateRequest request = new GetCampaignTemplateRequest() { CampaignTemplateID = campaignTemplateID, AccountId = this.AccountId };
            GetCampaignTemplateResponse response = campaignService.GetCampaignTemplate(request);
            return Request.BuildResponse(response);
        }


        /// <summary>
        /// Get campaign public images.
        /// </summary>       
        /// <returns>Campaign Public Images</returns>
        /// 
        [Route("CampaignPublicImages")]
        [HttpGet]
        public HttpResponseMessage GetCampaignPublicImages()
        {
            GetCampaignImagesResponse response = campaignService.GetPublicCampaignImages(new GetCampaignImagesRequest() { AccountID = null });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get campaign images.
        /// </summary>       
        /// <returns>Campaign Image by accountID</returns>

        [Route("CampaignImages")]
        [HttpGet]
        public HttpResponseMessage GetCampaignImages(int accountID)
        {
            GetAccountCampaignImagesResponse response = campaignService.GetCampaignImages(new GetCampaignImagesRequest() { AccountID = this.AccountId });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Delete campaign image by Id.
        /// </summary>
        /// <param name="imageID">Id of the Campaign image.</param>
        /// <returns>Deleted Campaign Details By imageID</returns>

        [Route("DeleteCampaignImage")]
        [HttpGet]
        public HttpResponseMessage DeleteCampaignImage(int imageID)
        {
            DeleteCampaignImageResponse response = campaignService.DeleteCampaignImage(new DeleteCampaignImageRequest(imageID));
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// All camapign images.
        /// </summary>
        /// <param name="accountID">Account Id. (Not using this, will be retired in future release)</param>
        /// <param name="Limit">Limit of the grid page</param>
        /// <param name="PageNumber">Page number.</param>
        /// <param name="name">name</param>
        /// <returns>All Campaign Images</returns>

        [Route("AllCampaignImages")]
        [HttpGet]
        public HttpResponseMessage GetAllCampaignImages(int accountID, byte Limit, byte PageNumber, string name)
        {
            GetAccountCampaignImagesResponse response = campaignService.FindAllImages(new GetCampaignImagesRequest() { AccountID = this.AccountId, Limit = Limit, PageNumber = PageNumber, name = name });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// ReIndex all campaigns
        /// </summary>
        /// <returns>Indexed Campaigns</returns>

        [Route("ReIndexCampaigns")]
        public HttpResponseMessage ReIndexCampaigns()
        {
            ReIndexDocumentResponse response = campaignService.ReIndexCampaigns(new ReIndexDocumentRequest());
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get campaign unique recipients count.
        /// </summary>
        /// <param name="summaryViewModel">Property of the Campaign Recipient Summary.</param>
        /// <returns>Campaign Unique recipients Count</returns>

        [Route("CampaignRecipientsCount")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        [Authorize]
        public HttpResponseMessage GetCampaignUniqueRecipientsCount(CampaignRecipientsSummaryRequest summaryViewModel)
        {
            Logger.Current.Informational("For Getting Unique recepients count by search by tags or Saved Searches in Campaign Review and Send Screen of Campains Module");
            GetCampaignUniqueRecipientsCountResponse recipientCounts = new GetCampaignUniqueRecipientsCountResponse();
            recipientCounts.Recipients = new CampaignRecipientTypes();
            if(summaryViewModel.AllSearchDefinitions != null && summaryViewModel.AllContactTags != null)
            {
                GetUniqueRecipientsCountResponse response = //new GetUniqueRecipientsCountResponse();
                campaignService.GetUniqueRecipients(new GetUniqueRecipientsCountRequest()
                {
                    Tags = summaryViewModel.AllContactTags.Select(t => t.TagID),
                    SDefinitions = summaryViewModel.AllSearchDefinitions.Select(s => (int)s.SearchDefinitionID),
                    AccountId = summaryViewModel.AccountId,
                    RoleId = this.RoleId,
                    RequestedBy = this.UserId
                });


                recipientCounts.Recipients = new CampaignRecipientTypes()
                {
                    ActiveContactsBySS = response.SDefinitionActiveCount,
                    AllContactsBySS = response.SDefinitionAllCount,
                    AllContactsByTag = response.TagAllCount,
                    ActiveContactsByTag = response.TagActiveCount,

                    CampaignACTIVEandALLRecipientsCount = response.TagsActiveSdsAllCount,
                    CampaignALLandACTIVERecipientsCount = response.TagsAllSDsActiveCount,
                    CampaignActiveRecipientsCount = response.TotalActiveUniqueCount,
                    CampaignRecipientsCount = response.TotalAllUniqueCount,
                };

                #region Get Current Tag/SD Count
                int currentTagCount = 0;
                int currentSDefinitionCount = 0;

                if (summaryViewModel.ContactTag != null && response.TagCounts != null && response.TagCounts.Where(t => summaryViewModel.ContactTag.TagID == t.Key).Any())
                    currentTagCount = response.TagCounts.Where(t => summaryViewModel.ContactTag.TagID == t.Key).FirstOrDefault().Value;
                if (summaryViewModel.SearchDefinition != null && response.SDefinitionCounts != null && response.SDefinitionCounts.Where(s => summaryViewModel.SearchDefinition.SearchDefinitionID == s.Key).Any())
                    currentSDefinitionCount = response.SDefinitionCounts.Where(s => summaryViewModel.SearchDefinition.SearchDefinitionID == s.Key).FirstOrDefault().Value;
                recipientCounts.CountByTag = currentTagCount;
                recipientCounts.CountBySearchDefinition = currentSDefinitionCount;
                recipientCounts.TagCounts = response.TagCounts;
                recipientCounts.SDefinitionCounts = response.SDefinitionCounts;
                #endregion

                recipientCounts.CampaignRecipientsCount = recipientCounts.Recipients.CampaignRecipientsCount;
            }
            return Request.BuildResponse(recipientCounts);
        }

        [Route("EmailValidatorContactsCount")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        [Authorize]
        public HttpResponseMessage GetEmailValidatorContactsCount(CampaignRecipientsSummaryRequest summaryViewModel)
        {
            Logger.Current.Informational("For Getting Unique recepients count by search by tags or Saved Searches in Campaign Review and Send Screen of Campains Module");
            GetCampaignUniqueRecipientsCountResponse recipientCounts = new GetCampaignUniqueRecipientsCountResponse();
            recipientCounts.Recipients = new CampaignRecipientTypes();
            if (summaryViewModel.AllSearchDefinitions != null && summaryViewModel.AllContactTags != null)
            {
                GetUniqueRecipientsCountResponse response = 
                campaignService.GetEmailValidatorContactsCount(new GetUniqueRecipientsCountRequest()
                {
                    Tags = summaryViewModel.AllContactTags.Select(s => s.TagID),
                    SDefinitions = summaryViewModel.AllSearchDefinitions.Select(s => (int)s.SearchDefinitionID),
                    AccountId = summaryViewModel.AccountId,
                    RoleId = this.RoleId,
                    RequestedBy = this.UserId,
                    SelectedSearhDefinitionID = summaryViewModel.SearchDefinition != null ? summaryViewModel.SearchDefinition.SearchDefinitionID : default(int),
                    SelectedTagID = summaryViewModel.ContactTag != null ? summaryViewModel.ContactTag.TagID : default(int)
                });

                recipientCounts.Recipients = new CampaignRecipientTypes()
                {
                    AllContactsByTag = response.TagActiveCount,
                    AllContactsBySS = response.SDefinitionActiveCount,
                    CampaignRecipientsCount = response.TagAllCount == 0 ? response.SDefinitionAllCount : response.TagAllCount
                };

            }
            return Request.BuildResponse(recipientCounts);
        }

        /// <summary>
        /// Campaign open tracker.
        /// </summary>
        /// <param name="accountId">Account Id. (not using this, will be removed in furture release)</param>
        /// <param name="cid">Contact Id.</param>
        /// <param name="cmpid">Campaign Id.</param>
        /// <returns>Campaign Open Track Details</returns>
        /// 
        [System.Web.Mvc.AllowAnonymous]
        [Route("CampaignOpenTracker")]
        [HttpPost]
        public HttpResponseMessage CampaignOpenTracker(int accountId, string cid, int cmpid)
        {
            int contactId = default(int);
            bool isValidContactId = int.TryParse(cid, out contactId);
            InsertCampaignOpenOrClickEntryResponse response = new InsertCampaignOpenOrClickEntryResponse();
            InsertCampaignOpenOrClickEntryRequest request = new InsertCampaignOpenOrClickEntryRequest()
            {
                AccountId = this.AccountId,
                CampaignId = cmpid,
                ContactId = contactId

            };
            if (isValidContactId)
            {
                response = campaignService.InsertCampaignOpenEntry(request);
            }
            else
                response.Exception = new Exception("Invalid Contact");

            return Request.BuildResponse(response);
        }


        /// <summary>
        /// GetCampaignRecipientsByID.
        /// </summary>
        /// <param name="campaignId">campaign Id.</param>   
        /// <param name="accountId">accountId.</param>  
        /// <param name="userId">userId.</param>  
        /// <param name="roleId">roleId.</param>  
        /// <returns></returns>           
        [System.Web.Mvc.AllowAnonymous]
        [Route("GetCampaignRecipientsByID")]
        public async Task<HttpResponseMessage> GetCampaignRecipientsByID(int campaignId,int accountId,int userId,short roleId)
        {
            GetCampaignRecipientIdsResponse response = await campaignService.GetCampaignRecipientsByID(new GetCampaignRecipientIdsRequest()
            {
                CampaignId = campaignId,
                AccountId =accountId,
                RequestedBy = userId,
                RoleId = roleId
            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get campaign stats
        /// </summary>
        /// <param name="accountId">Account Id. (Not using this, will be removed in future release)</param>       
        /// <param name="campaignId">Campaign Id.</param>
        /// <returns>Campaign Statistics Details</returns>        

        [Route("CampaignStatistics")]
        [HttpPost]
        public HttpResponseMessage CampaignStatistics(int accountId, int campaignId)
        {
            GetCampaignStatisticsRequest request = new GetCampaignStatisticsRequest()
            {
                AccountId = this.AccountId,
                CampaignId = campaignId
            };
            return Request.BuildResponse(campaignService.GetCampaignStatistics(request));
        }

        /// <summary>
        /// Mailchimp Webhook configuration.
        /// </summary>
        /// <param name="data">Form data collection.</param>             
        /// <returns>bool value</returns>
        /// 

        [Route("mailchimpwebhook")]
        [System.Web.Mvc.AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage MailChimpWebhook(FormDataCollection data)
        {
            string userAgent = "";
            if (Request.Headers != null && Request.Headers.UserAgent != null && Request.Headers.UserAgent.Any())
            {
                foreach (var agent in Request.Headers.UserAgent)
                {
                    if (agent != null && agent.Product != null && !string.IsNullOrEmpty(agent.Product.Name) && agent.Product.Name == "VonChimpenfurlr")
                        userAgent = agent.Product.Name;
                }
            }
            var dataReceived = data != null ? data.ToList() : new List<KeyValuePair<string, string>>();

            //Ignoring MailChimp auto link checks. Ref: http://kb.mailchimp.com/quick-answers #Why-are-my-links-being-clicked
            if (userAgent != "VonChimpenfurlr" && dataReceived != null && dataReceived.Any())
            {
                CampaignResponseViewModel campaignResponseViewModel = new CampaignResponseViewModel() { ResponseItems = dataReceived };
                MailChimpWebhookRequest request = new MailChimpWebhookRequest() { CampaignResponse = campaignResponseViewModel, AccountId = this.AccountId };
                campaignService.MailChimpWebhookUpdate(request);
            }
            else if (userAgent == "VonChimpenfurlr" && dataReceived != null)
            {
                Logger.Current.Informational("Request received from Omnivore to verify the link in campaign. " + dataReceived.ToString());
            }
            else if (data == null)
            {
                Logger.Current.Informational("Mailchimp Webhook configured successfully");
            }
            return Request.BuildResponse(new UnsubscribeContactResponse() { Success = true });//Revisit
        }

        /// <summary>
        /// Get IP Address by hostname.
        /// </summary>
        /// <param name="hostName">Host Name.</param>             
        /// <returns>Ip Address</returns>
        /// 

        [Route("getipaddress")]
        [System.Web.Mvc.AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage GetIPAddress(string hostName)
        {
            GetClientIPAddressRequest request = new GetClientIPAddressRequest() { Domain = hostName };
            var response = campaignService.GetClientIPAddress(request);
            return Request.BuildResponse(new UnsubscribeContactResponse() { Acknowledgement = response.IPAddress }); //Revisit ipaddress
        }
                
        /// <summary>
        /// Insert campaign recipients
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        [Route("insertCampaignRecipients/{campaignId}")]
        [System.Web.Mvc.AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public async Task<HttpResponseMessage> InsertCampaignRecipients(int campaignId)//, int accountId, int userId)
        {
            Logger.Current.Verbose("In CampaignsController/InsertCampaignRecipients campaignid = " + campaignId);
            InsertBulkRecipientsResponse response = new InsertBulkRecipientsResponse();
            try
            {
                response = await campaignService.InsertCampaignRecipients(new InsertBulkRecipientsRequest()
                {
                    CampaignId = campaignId
                });
                Logger.Current.Informational("Successfully inserted temporary recipients for campaign " + campaignId);
            }
            catch (Exception ex)
            {
                response.Exception = ex;
                Logger.Current.Error("Error while processing recipients.", ex);
            }
            return Request.BuildResponse(response); ;
        }

        [Route("requestlitmuscheck/{campaignid}")]
        [HttpPost]
        public void RequestLitmusCheck(int campaignId)
        {
            campaignService.RequestLitmusCheck(new RequestLitmusCheck()
                {
                    CampaignId = campaignId
                });
        }

        [Route("mailtester/{campaignid}")]
        [HttpPost]
        public void MailTester(int campaignId)
        {
            campaignService.InsertMailTesterRequest(new InsertCampaignMailTesterRequest()
            {
                CampaignID = campaignId,
                AccountId = this.AccountId,
                RequestedBy = this.UserId
            });
        }
    }
}