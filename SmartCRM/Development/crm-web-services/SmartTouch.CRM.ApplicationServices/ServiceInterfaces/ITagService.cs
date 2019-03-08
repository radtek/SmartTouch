using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.Domain.Tags;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ITagService
    {
        //GetTagListResponse GetTags();
        //GetTagListResponse GetAllTags(GetTagListRequest request);
        //GetTagResponse GetTags(GetTagRequest request);

        SaveTagResponse SaveTag(SaveTagRequest request);
        UpdateTagResponse UpdateTag(UpdateTagRequest request);
        DeleteTagResponse DeleteTag(DeleteTagRequest request);

        UpdateTagResponse ContinueUpdateTag(UpdateTagRequest request);

        ReIndexTagsResponse ReIndexTags(ReIndexTagsRequest request);
        // SearchTagsResponse SearchTag(SearchTagsRequest request);
        SearchTagsResponse SearchTagByName(SearchTagsRequest request);
        SearchTagsResponse SearchTagsByTagName(SearchTagsRequest request);

        GetTagListResponse GetAllTags(GetTagListRequest request);

        DeleteTagResponse DeleteTags(DeleteTagIdsRequest request);

        SaveTagResponse MergeTag(SaveTagRequest request);
        SaveTagResponse ContinueMergeTag(SaveTagRequest request);

        GetTagListResponse GetTagsBasedonaccount(int accountId);

        ContactTagSummaryResponse ContactSummaryByTag(ContactTagSummaryRequest request);
        GetTagListResponse GetAllTagsByContacts(GetTagListRequest request);
        GetTagResponse GetTag(GetTagRequest request);

        PopularTagsResponse GetPopularTags(PopularTagsRequest request);
        RecentTagsResponse GetRecentTags(RecentTagsRequest request);
        GetRecentAndPopularTagsResponse GetRecentAndPopularTags(GetRecentAndPopularTagsRequest request);
        SaveContactTagsResponse SaveContactTags(SaveContactTagsRequest request);

        SaveTagResponse SaveOpportunityTag(SaveTagRequest request);
        DeleteTagResponse DeleteOpportunityTag(DeleteTagRequest request);
        SaveTagResponse LeadScore_SaveTag(SaveTagRequest request);
        WorkflowAddTagResponse AddTag(WorkflowAddTagRequest request);
        WorkflowRemoveTagResponse RemoveTag(WorkflowRemoveTagRequest request);
        void addToTopic(IEnumerable<int> tagIds, IEnumerable<int> contactIds, int userId, int accountId);
        void addLeadAdapterToTopic(int leadadapterid, IEnumerable<int> contactIds, int accountId);
        TagIndexingResponce TagIndexing(TagIndexingRequest request);

        Tag UpdateTagBulkData(int tagId, int accounId,int userId);
    }
}
