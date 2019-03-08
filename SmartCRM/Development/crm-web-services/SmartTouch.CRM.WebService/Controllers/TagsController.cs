using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.WebService.Helpers;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating tags controller for tags module
    /// </summary>
    public class TagsController : SmartTouchApiController
    {
        readonly ITagService tagService;

        /// <summary>
        /// Creating constructor for tags controller for accessing
        /// </summary>
        /// <param name="tagService">tagService</param>
        public TagsController(ITagService tagService)
        {
            this.tagService = tagService;
        }

        /// <summary>
        /// Search a tag by name.
        /// </summary>
        /// <param name="query">pass the name of the tag name</param>
        /// <returns>All tags names</returns>
        [Route("Tag/Names")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage SearchTagNames(string query)
        {
            SearchTagsResponse response = tagService.SearchTagByName(new SearchTagsRequest() { Query = query, AccountId = this.AccountId });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Insert a new tag.
        /// </summary>
        /// <param name="viewModel">Properties of a new tag</param>
        /// <returns>Tag Insertion Details Response</returns>
        [Route("Tag")]
        [HttpPost]
        [Authorize]
        public HttpResponseMessage PostTag(TagViewModel viewModel)
        {
            SaveTagResponse response = tagService.SaveTag(new SaveTagRequest() { TagViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Update a tag.
        /// </summary>
        /// <param name="viewModel">Properties of a new tag</param>
        /// <returns>Tag Updation Details Response</returns>
        [Route("Tag")]
        [HttpPut]
        [Authorize]
        public HttpResponseMessage PutTag(TagViewModel viewModel)
        {
            UpdateTagResponse response = tagService.UpdateTag(new UpdateTagRequest() { TagViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Update a tag.
        /// </summary>
        /// <param name="viewModel">Properties of a new tag</param>
        /// <returns>Tag Updation Details Response</returns>
        [Route("ContinueTag")]
        [HttpPut]
        [Authorize]
        public HttpResponseMessage ContinuePutTag(TagViewModel viewModel)
        {
            UpdateTagResponse response = tagService.ContinueUpdateTag(new UpdateTagRequest() { TagViewModel = viewModel });
            return Request.BuildResponse(response);
        }
        

        /// <summary>
        /// Deletes a tag for the contact.
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="contactId"></param>
        /// <returns>Tag Deletion Details Response</returns>
        [Route("Tag/DeleteTag")]
        [HttpDelete]
        public HttpResponseMessage Delete(string tagName, int contactId)
        {
            DeleteTagResponse response = tagService.DeleteTag(new DeleteTagRequest() { TagName = tagName, ContactID = contactId });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Deletes a tag.
        /// </summary>
        /// <param name="tagIds"></param>
        /// <returns>Deleted Tags Details Response</returns>
        [Route("DeleteTags")]
        [HttpPost]
        public HttpResponseMessage Delete(int[] tagIds)
        {
            DeleteTagResponse response = tagService.DeleteTags(new DeleteTagIdsRequest() { TagID = tagIds });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Reindex all the tags.
        /// </summary>        
        /// <returns>ReIndexed Tags Response Details</returns>
        [Route("ReIndexTags")]
        [HttpPost]
        public HttpResponseMessage ReIndexTags()
        {
            ReIndexTagsResponse response = tagService.ReIndexTags(new ReIndexTagsRequest());
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Reindex all the tags.
        /// </summary>     
        /// <param name="summaryViewModel">Properties of a tag summaryviewmodel</param>
        /// <returns>Tags Summary</returns>
        [Route("contactsummarybytag")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]     
        [Authorize]
        public HttpResponseMessage TagSummary(TagSummaryRequestViewModel summaryViewModel)
        {
            //todo: remove the hardcoded account id and take it from authorization token.
            ContactTagSummaryResponse response =
                tagService.ContactSummaryByTag(new ContactTagSummaryRequest() { Tag = summaryViewModel.Tag,
                    AllTags = summaryViewModel.AllTags, AccountId = summaryViewModel.AccountId });
            return Request.BuildResponse(response);
        }

        [Route("RecentPopularTags")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRecentPopularTags(string tagList)
        {
            var tagIds = new int[] { };
            if (!String.IsNullOrEmpty(tagList))
            {
                tagIds= Array.ConvertAll(tagList.Split(','), s=>int.Parse(s));
            }
            GetRecentAndPopularTagsResponse response = tagService.GetRecentAndPopularTags(new GetRecentAndPopularTagsRequest() { AccountId = this.AccountId, TagsList = tagIds});
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Saving Contact Tags.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("SaveContactTags")]
        [HttpPost]
        public HttpResponseMessage SaveContactTags(AddTagViewModel viewModel)
        {

            SaveContactTagsResponse response = tagService.SaveContactTags(new SaveContactTagsRequest()
            {
                Contacts = viewModel.Contacts,
                Tags = viewModel.TagsList,
                Opportunities = viewModel.Opportunities,
                UserId = this.UserId,
                AccountId = this.AccountId
            });

            return Request.BuildResponse(response);
        }
        
    }
}