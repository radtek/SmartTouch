using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class TagController : SmartTouchController
    {
        private readonly ITagService tagService;

        public TagController(ITagService tagService)
        {
            this.tagService = tagService;
        }

        [Route("tags")]
        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Read)]
        [MenuType(MenuCategory.ManageTags, MenuCategory.LeftMenuAccountConfiguration)]
        [OutputCache(Duration=30)]
        public ActionResult TagList(int? tagId)
        {
            if (tagId == 1)
                ViewBag.tagId = 1;
            else
                ViewBag.tagId = 0;
            TagViewModel viewModel = new TagViewModel();
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("TagList", viewModel);
        }

        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Read)]
        public ActionResult TagsViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            string sortField = request.Sorts.Count > 0 ? request.Sorts.First().Member :null;
            var direction = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Descending;
            GetTagListResponse response = tagService.GetAllTags(new GetTagListRequest()
            {
                Name = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountId = UserExtensions.ToAccountID(this.Identity),
                SortField = sortField,
                SortDirection = direction
            });

            return Json(new DataSourceResult
            {
                Data = response.Tags,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Edit)]
        public PartialViewResult _EditTag()
        {
            TagViewModel viewModel = new TagViewModel();
            return PartialView("_EditTag", viewModel);
        }

        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Edit)]
        public ActionResult _MergeTag()
        {
            TagViewModel viewModel = new TagViewModel();
            GetTagListResponse response = tagService.GetTagsBasedonaccount(UserExtensions.ToAccountID(this.Identity));
            viewModel.Tags = response.Tags;
            return PartialView("_MergeTag", viewModel);
        }
 
        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Create)]
        public ActionResult _AddTag()
        {
            TagViewModel viewModel = new TagViewModel();
            return PartialView("_AddTag", viewModel);
        }
        [Route("edittag")]
        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Edit)]
        public ActionResult _EditTagModal(int tagId)
        {
            GetTagResponse response = tagService.GetTag(new GetTagRequest(tagId));
            TagViewModel viewModel = new TagViewModel();
            viewModel.TagID = tagId;
            viewModel.TagName = response.TagViewModel.TagName;
            ViewBag.IsModal = true;
            return PartialView("_EditTag", viewModel);
        }

        [Route("mergetag")]
        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Edit)]
        public ActionResult _MergeTagModal(int tagId)
        {
            GetTagResponse response = tagService.GetTag(new GetTagRequest(tagId));
            TagViewModel viewModel = new TagViewModel();
            GetTagListResponse tagresponse = tagService.GetTagsBasedonaccount(UserExtensions.ToAccountID(this.Identity));
            viewModel.Tags = tagresponse.Tags;
            viewModel.sourceTagID = tagId;
            viewModel.sourceTagName = response.TagViewModel.TagName;
            viewModel.Count = response.TagViewModel.Count;
            ViewBag.IsModal = true;           
            return PartialView("_MergeTag", viewModel);
        }

        public ActionResult GetRecentPopularTags(string tagList)
        {
            int[] myInts = new int[] { };
            if (!String.IsNullOrEmpty(tagList))
            {
                string[] array = tagList.Split(',');
                myInts = array.Select(int.Parse).ToArray();
            }
            
            GetTagListResponse response = new GetTagListResponse();
            PopularTagsResponse popularTagsResponse = tagService.GetPopularTags(new PopularTagsRequest() { AccountId = this.Identity.ToAccountID(), TagsList = myInts, Limit = 10 });
            if (popularTagsResponse.TagsViewModel != null)
            {
                response.PopularTags = popularTagsResponse.TagsViewModel;
            }
            else { response.PopularTags = null; }

            RecentTagsResponse recentTagsResponse = tagService.GetRecentTags(new RecentTagsRequest() { AccountId = this.Identity.ToAccountID(), TagsList = myInts, Limit = 10 });
            if (recentTagsResponse.TagsViewModel != null)
            {
                response.RecentTags = recentTagsResponse.TagsViewModel;
            }

            return Json(new { success = true, response }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Delete)]
        public ActionResult DeleteTag(string tagIds)
        {
            DeleteTagIdsRequest request = JsonConvert.DeserializeObject<DeleteTagIdsRequest>(tagIds);
            request.AccountId = this.Identity.ToAccountID();
            request.RequestedBy = this.Identity.ToUserID();
            tagService.DeleteTags(request);
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Read)]
        public JsonResult GetTags()
        {
            GetTagListResponse response = tagService.GetTagsBasedonaccount(UserExtensions.ToAccountID(this.Identity));
            return Json(new { success = true, response.Tags }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Edit)]
        public JsonResult MergeTag(string tagViewModel)
        {
            TagViewModel viewModel = JsonConvert.DeserializeObject<TagViewModel>(tagViewModel);
            viewModel.AccountID = UserExtensions.ToAccountID(this.Identity);
            Logger.Current.Informational("Logging Tag Information while merging tags: TagID " + viewModel.TagID + ", SourceTagId: " + viewModel.sourceTagID);
            SaveTagRequest request = new SaveTagRequest() { TagViewModel = viewModel };

            SaveTagResponse response = tagService.MergeTag(request);
            return Json(new
            {
                success = true,
                response = new
                {
                    IsInvolvedInLeadScore = response.IsAssociatedWithLeadScoreRules,
                    IsInvolvedInWorkflows = response.IsAssociatedWithWorkflows
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Edit)]
        public JsonResult ContinueMerging(string tagViewModel)
        {
            TagViewModel viewModel = JsonConvert.DeserializeObject<TagViewModel>(tagViewModel);
            viewModel.AccountID = UserExtensions.ToAccountID(this.Identity);
            SaveTagRequest request = new SaveTagRequest() { TagViewModel = viewModel };

            SaveTagResponse response = tagService.ContinueMergeTag(request);
            return Json(new
            {
                success = true,
                response = new
                {
                    IsInvolvedInLeadScore = response.IsAssociatedWithLeadScoreRules,
                    IsInvolvedInWorkflows = response.IsAssociatedWithWorkflows
                }
            }, JsonRequestBehavior.AllowGet);
        }
    }
}