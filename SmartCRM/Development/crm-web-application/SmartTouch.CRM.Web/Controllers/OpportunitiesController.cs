using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Extensions;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;
using SmartTouch.CRM.ApplicationServices.Messaging.Image;
using SmartTouch.CRM.ApplicationServices.Messaging.Notes;
using SmartTouch.CRM.ApplicationServices.Messaging.Opportunity;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace SmartTouch.CRM.Web.Controllers
{
    public class OpportunitiesController : SmartTouchController
    {
        readonly IOpportunitiesService opportunityService;
        readonly ITagService tagService;
        readonly IUserService userService;
        readonly IDropdownValuesService dropdownValuesService;
        readonly IContactService contactService;
        readonly IActionService actionService;
        readonly ICachingService cachingService;
        readonly INoteService noteService;
        readonly IImageService imageService;

        public OpportunitiesController(IOpportunitiesService opportunityService, INoteService noteService, IActionService actionService,
                                       ITagService tagService, IUserService userService, IDropdownValuesService dropdownValuesService, ICachingService cachingService, IContactService contactService, IImageService imageService)
        {
            this.opportunityService = opportunityService;
            this.tagService = tagService;
            this.actionService = actionService;
            this.userService = userService;
            this.dropdownValuesService = dropdownValuesService;
            this.cachingService = cachingService;
            this.noteService = noteService;
            this.contactService = contactService;
            this.imageService = imageService;
        }


        #region Notes

        [SmarttouchAuthorize(AppModules.OpportunityNotes, AppOperations.Create)]
        public ActionResult _OpportunityAddNote()
        {
            NoteViewModel nviewmodel = new NoteViewModel();
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            nviewmodel.NoteCategories= GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.NoteCategory);
            nviewmodel.NoteCategory = nviewmodel.NoteCategories.Where(s => s.IsDefault).Select(p => p.DropdownValueID).FirstOrDefault();
            nviewmodel.CreatedBy = this.Identity.ToUserID();
            ViewBag.TagPopup = false;            
            return PartialView("~/Views/Contact/_AddEditNote.cshtml", nviewmodel);
        }

        [SmarttouchAuthorize(AppModules.OpportunityNotes, AppOperations.Edit)]
        public ActionResult OpportunityEditNote(int noteId)
        {
            GetNoteResponse response = noteService.GetNote(new GetNoteRequest() { NoteId = noteId ,AccountId = this.Identity.ToAccountID()});
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            response.NoteViewModel.NoteCategories = GetFromDropdownValues(dropdownValues, (byte)DropdownFieldTypes.NoteCategory);
            ViewBag.IsModal = true;
            ViewBag.TagPopup = false;
            
            var view = PartialView("~/Views/Contact/_AddEditNote.cshtml", response.NoteViewModel);
            return view;
        }

        [SmarttouchAuthorize(AppModules.OpportunityNotes, AppOperations.Delete)]
        public JsonResult OpportunityDeleteNote(int noteId)
        {
            noteService.DeleteNote(new DeleteNoteRequest() { NoteId = noteId });
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Actions
        public ActionResult GetActionContactsCount(int actionId)
        {
            GetContactsCountResponse response = new GetContactsCountResponse();
            IList<int> OwnerIds = actionService.GetAllAssignedUserIds(actionId);

            bool hasPermission = true;
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            if (!isSTAdmin && !isACTAdmin && OwnerIds.IsAny())
                hasPermission = OwnerIds.Contains(this.Identity.ToUserID());

            if (hasPermission)
                response = actionService.ActionContactsCount(new GetContactsCountRequest() { Id = actionId });

            return Json(new { success = hasPermission, response = response.Count }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.OpportunityActions, AppOperations.Create)]
        public ActionResult _OpportunityAddAction()
        {
            ActionViewModel viewModel = new ActionViewModel();
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.OwnerIds = new List<int>();
            viewModel.RemindOn = DateTime.Now.AddDays(1).ToUserDateTime(); ;
            var dateFormat = this.Identity.ToDateFormat();
            List<ReminderType> types = new List<ReminderType>();
            viewModel.SelectedReminderTypes = types;

            viewModel.DateFormat = dateFormat + " hh:mm tt";

            ViewBag.TagPopup = false;
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, this.Identity.ToAccountID());
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsACTAdmin = (isACTAdmin || isSTAdmin) ? true : false;
            ViewBag.IsPrivate = isPrivate;
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            viewModel.ActionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.ActionType = viewModel.ActionTypes.Where(s => s.IsDefault).Select(p => p.DropdownValueID).FirstOrDefault();
            ViewBag.IsIncludeSignature = userService.IsIncludeSignatureByDefaultOrNot(this.Identity.ToUserID());
            return PartialView("~/Views/Contact/_AddEditAction.cshtml", viewModel);
        }


        //[Route("addactionmodal")]
        [SmarttouchAuthorize(AppModules.OpportunityActions, AppOperations.Create)]
        public ActionResult _OpportunityAddActionModal()
        {
            ActionViewModel viewModel = new ActionViewModel();
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.OwnerIds = new List<int>();
            ViewBag.IsModal = true;
            viewModel.RemindOn = DateTime.Now.AddDays(1).ToUserDateTime(); ;
            var dateFormat = this.Identity.ToDateFormat();
            List<ReminderType> types = new List<ReminderType>();
            viewModel.SelectedReminderTypes = types;
            viewModel.DateFormat = dateFormat + " hh:mm tt";
            ViewBag.TagPopup = false;
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            viewModel.ActionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.ActionType = viewModel.ActionTypes.Where(s => s.IsDefault).Select(p => p.DropdownValueID).FirstOrDefault();
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, this.Identity.ToAccountID());
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsACTAdmin = (isACTAdmin || isSTAdmin) ? true : false;
            ViewBag.IsPrivate = isPrivate;
            ViewBag.IsIncludeSignature = userService.IsIncludeSignatureByDefaultOrNot(this.Identity.ToUserID());
            return PartialView("~/Views/Contact/_AddEditAction.cshtml", viewModel);
        }

        //[Route("editaction")]
        [SmarttouchAuthorize(AppModules.OpportunityActions, AppOperations.Edit)]
        public ActionResult OpportunityEditAction(int actionId)
        {
            bool hasPermission = false;
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, this.Identity.ToAccountID());
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());

            GetActionResponse response = actionService.GetAction(new GetActionRequest() { Id = actionId });

            if (!isSTAdmin && !isACTAdmin && response.ActionViewModel.OwnerIds.IsAny())
                hasPermission = !response.ActionViewModel.OwnerIds.Contains(this.Identity.ToUserID());

            if (hasPermission)
            {
                ViewBag.Message = " Access denied. You do not have permission to edit this Action.";
                ViewBag.CustomMessage = "Action Update Alert!";
                var partialView = PartialView("~/Views/Contact/_PermissionView.cshtml");
                return partialView;
            }
            if (response.ActionViewModel.RemindOn.HasValue)
                response.ActionViewModel.RemindOn = response.ActionViewModel.RemindOn.Value.ToJSDate();
            var dateFormat = this.Identity.ToDateFormat();
            response.ActionViewModel.DateFormat = dateFormat + " hh:mm tt";
            ViewBag.IsModal = true;
            ViewBag.TagPopup = false;
            if (!response.ActionViewModel.OwnerIds.IsAny())
                response.ActionViewModel.OwnerIds = new List<int>();
           
            ViewBag.IsACTAdmin = (isACTAdmin || isSTAdmin) ? true : false;
            ViewBag.EditView = true;
            ViewBag.IsPrivate = isPrivate;
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            response.ActionViewModel.ActionTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            ViewBag.IsIncludeSignature = userService.IsIncludeSignatureByDefaultOrNot(this.Identity.ToUserID());
            return PartialView("~/Views/Contact/_AddEditAction.cshtml", response.ActionViewModel);
        }

        [SmarttouchAuthorize(AppModules.OpportunityActions, AppOperations.Delete)]
        public ActionResult OpportunityDeleteAction(int actionId)
        {
            actionService.DeleteAction(new DeleteActionRequest() { ActionId = actionId, DeleteForAll = true,RequestedBy=this.Identity.ToUserID() });
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ActionCompleted(int actionID, bool isActionCompleted, int opportunityId,bool isSchedule,int? mailBulkId,bool AddToNoteSummary)
        {
            IList<int> OwnerIds = actionService.GetAllAssignedUserIds(actionID);

            bool hasPermission = true;
            bool isSTAdmin = this.Identity.IsSTAdmin();
            bool isACTAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            if (!isSTAdmin && !isACTAdmin && OwnerIds.IsAny())
                hasPermission = OwnerIds.Contains(this.Identity.ToUserID());

            if (hasPermission)
                actionService.ActionStatus(new CompletedActionRequest() {
                    actionId = actionID,
                    isCompleted = isActionCompleted,
                    IsSchedule = isSchedule,
                    MailBulkId = mailBulkId,
                    CompletedForAll = true,
                    opportunityId = opportunityId ,
                    RequestedBy = this.Identity.ToUserID(),
                    UpdatedOn = DateTime.Now.ToUniversalTime(),
                    AccountId = this.Identity.ToAccountID(),
                    AddToNoteSummary = AddToNoteSummary
                });

            return Json(new { success = hasPermission, response = "" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ViewOpportunity

        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [Route("viewopportunity/{opportunityID}/{index?}")]
        [Route("viewopportunity")]
        [SmarttouchAuthorize(AppModules.Opportunity, AppOperations.Read)]
        [MenuType(MenuCategory.ViewOpportunity, MenuCategory.LeftMenuCRM)]
        public ActionResult ViewOpportunity(int opportunityID, int index = 0)
        {
            RemoveCookie("opportunityid");
            AddCookie("opportunityid", opportunityID.ToString(), 1);
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            AddCookie("pagesize", ItemsPerPage.ToString(), 1);
            ViewBag.IsOpportunity = true;
            GetOpportunityResponse response = opportunityService.getOpportunity(new GetOpportunityRequest()
            {
                OpportunityID = opportunityID,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                IncludeTags = true
            });
            if (response.OpportunityViewModel == null)
                return RedirectToAction("NotFound", "Error");
            else
                UpdateUserActivityLog(opportunityID, response.OpportunityViewModel.OpportunityName);

            response.OpportunityViewModel.DateFormat = this.Identity.ToDateFormat();
            if (response.OpportunityViewModel.PeopleInvolved == null)
            {
                response.OpportunityViewModel.PeopleInvolved = new List<PeopleInvolvedViewModel>();
            }
            response.OpportunityViewModel.Actions = actionService.GetOpportunityActions(new GetActionListRequest() { Id = opportunityID }).ActionListViewModel.Reverse();
            response.OpportunityViewModel.Currency = this.Identity.ToCurrency();
            response.OpportunityViewModel.ExpectedCloseDate = null;
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            GetUserListResponse users = userService.GetUsers(new GetUserListRequest() { AccountID = this.Identity.ToAccountID(), IsSTAdmin = this.Identity.IsSTAdmin() });
            response.OpportunityViewModel.Users = users.Users;

            var stages = dropdownValues.Where(k => k.DropdownID == (byte)DropdownFieldTypes.OpportunityStage).
                Select(k => k.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            response.OpportunityViewModel.Stages = stages;
            ViewBag.TagPopup = true;
            if (response.OpportunityViewModel.Image == null)
                response.OpportunityViewModel.Image = new ImageViewModel();

            AddCookie("dateformat", this.Identity.ToDateFormat(), 1);
            Session["opportunityindex"] = index;
            ViewBag.OpportunityPage = "ViewOpportunity";
            AddCookie("currency", this.Identity.ToCurrency(), 1);
            return View("ViewOpportunity", response.OpportunityViewModel);
        }

        #endregion

        #region Next and Previous Opportunity
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [Route("previousopportunity")]
        public ActionResult PreviousOpportunity()
        {
            var index = Session["opportunityindex"].ToString();
            var searchText = ReadCookie("opportunitysearchtext");
            int currentIndex = 0;
            int.TryParse(index, out currentIndex);
            currentIndex = currentIndex - 1;
            var sortField = GetPropertyName<OpportunityViewModel, DateTime?>(r => r.LastModifiedOn);
            var result = getOpportunities(currentIndex, 1, searchText, null, "", true, ReadCookie("summaryopportunities"),null,"","", sortField, ListSortDirection.Descending);
            IEnumerable<OpportunityViewModel> opportunities = result.Opportunities;
            long totalHits = result.TotalHits;
            OpportunityViewModel entry = new OpportunityViewModel();
            if (opportunities.Any())
            {
                entry = opportunities.First();
                Session["opportunityindex"] = currentIndex;
                Session["OpportunityTotalHits"] = totalHits;
            }
            return RedirectToAction("ViewOpportunity", new { opportunityID = entry.OpportunityID, index = currentIndex });
        }

        private dynamic getOpportunities(int page, int pageSize, string name, int? JobID, string recordStatus, bool navigationEnabled, string Ids, int[] userIds, string StartDate, string EndDate, string sortField = null, ListSortDirection sortDirection = ListSortDirection.Descending)
        {
            SearchOpportunityResponse response = opportunityService.GetAllOpportunities(new SearchOpportunityRequest()
            {
                Query = name,
                Limit = pageSize,
                PageNumber = page,
                AccountId = UserExtensions.ToAccountID(this.Identity),
                TimeZone = this.Identity.ToTimeZone(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                OpportunityIDs = !string.IsNullOrEmpty(Ids) ? Ids.Split(',').ToArray() : null,
                SortField = sortField,
                SortDirection = sortDirection,
                UserIDs = userIds.IsAny()?userIds:new int[] { },
                StartDate = !string.IsNullOrEmpty(StartDate) ? Convert.ToDateTime(StartDate) : (DateTime?)null,
                EndDate = !string.IsNullOrEmpty(EndDate) ? Convert.ToDateTime(EndDate) : (DateTime?)null
            });

            if (!navigationEnabled)
            {
                AddCookie("pagesize", pageSize.ToString(), 1);
                AddCookie("pagenumber", page.ToString(), 1);
                AddCookie("opportunitysearchtext", name, 1);
            }
            Session["OpportunityTotalHits"] = response.TotalHits.ToString();
            return new
            {
                Opportunities = response.Opportunities,
                TotalHits = response.TotalHits
            };
        }
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [Route("nextopportunity")]
        public ActionResult NextOpportunity()
        {
            var index = Session["opportunityindex"].ToString();
            var searchText = ReadCookie("opportunitysearchtext");
            int currentIndex = 0;
            int.TryParse(index, out currentIndex);
            currentIndex = currentIndex + 1;
            var sortField = GetPropertyName<OpportunityViewModel, DateTime?>(r => r.LastModifiedOn);
            var result = getOpportunities(currentIndex, 1, searchText, null, "", true, ReadCookie("summaryopportunities"), null,"","",sortField, ListSortDirection.Descending);
            IEnumerable<OpportunityViewModel> opportunities = result.Opportunities;
            long totalHits = result.TotalHits;
            OpportunityViewModel entry = new OpportunityViewModel();
            if (opportunities.Any())
            {
                entry = opportunities.First();
                Session["opportunityindex"] = currentIndex;
                Session["OpportunityTotalHits"] = totalHits;
            }
            return RedirectToAction("ViewOpportunity", new { opportunityID = entry.OpportunityID, index = currentIndex });
        }


        #endregion

        #region Cookies
        public void RemoveCookie(string cookieName)
        {
            if (Request.Cookies[cookieName] != null)
            {
                HttpCookie myCookie = new HttpCookie(cookieName);
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
        }

        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        public string ReadCookie(string strValue)
        {
            string strValues = string.Empty;

            if (Request.Cookies[strValue] != null)
            {
                strValues = Request.Cookies[strValue].Value;
            }
            return strValues;
        }

        #endregion

        #region Opportunities CRUD Operaions


        public ActionResult StoreContacts(string ContactIDs)
        {
            string stringKey = Guid.NewGuid().ToString();
            cachingService.StoreTemporaryString(stringKey, ContactIDs);            
            return Json(new { stringKey = stringKey, success = true }, JsonRequestBehavior.AllowGet);
        }


        [MenuType(MenuCategory.AddEditOpportunity, MenuCategory.LeftMenuCRM)]
        public ActionResult AddOpportunityWithBuyers(string referencekey)
        {
            string ContactIDs = cachingService.GetTemporaryStringContent(referencekey);
            int[] ContactIDArray = ContactIDs.Split(',').Select(n => int.Parse(n)).ToArray();
            OpportunityViewModel viewModel = new OpportunityViewModel();
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.DateFormat = this.Identity.ToDateFormat();
            viewModel.PeopleInvolved = new List<PeopleInvolvedViewModel>();

            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString(), 1);

            SearchContactsResponse<ContactEntry> searchresponse = new SearchContactsResponse<ContactEntry>();
            searchresponse = contactService.GetAllContacts<ContactEntry>(new SearchContactsRequest()
            {
                Query = "",
                Limit = ContactIDArray.Length,
                PageNumber = 1,
                SortFieldType = ContactSortFieldType.NoSort,
                AccountId = this.Identity.ToAccountID(),
                ContactIDs = ContactIDArray,
                ShowingFieldType = ContactShowingFieldType.PeopleAndComapnies,
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });

            viewModel.Contacts = searchresponse.Contacts;

            viewModel.RelationshipTypes = dropdownValuesService.GetDropdownValue(new GetDropdownValueRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                DropdownID = 9
            }).DropdownValues.DropdownValuesList;

            GetUserListResponse users = userService.GetUsers(new GetUserListRequest() { AccountID = this.Identity.ToAccountID(), IsSTAdmin = this.Identity.IsSTAdmin() });
            viewModel.Users = users.Users;
            viewModel.OwnerId = this.Identity.ToUserID();
            var stages = dropdownValuesService.GetDropdownValue(new GetDropdownValueRequest()
            {
                AccountId = this.Identity.ToAccountID(),
                DropdownID = 6
            }).DropdownValues.DropdownValuesList;
            var defaultForOpportunityStage = stages.Where(x => x.IsDefault.Equals(true)).FirstOrDefault();
            viewModel.StageID = defaultForOpportunityStage == null ? stages.FirstOrDefault().DropdownValueID : defaultForOpportunityStage.DropdownValueID;
            viewModel.Stages = stages;
            ViewBag.IsPeople = ReadCookie("IsPeople");
            ViewBag.OpportunityPage = "AddEditOpportunity";
            ViewBag.IsModal = true;
            return View("_AddEditBuyer", viewModel);
        }

        public ActionResult _AddBuyerModal(int contactId)
        {
            OpportunityViewModel viewModel = new OpportunityViewModel();
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.DateFormat = this.Identity.ToDateFormat();
            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString(), 1);

            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            GetUserListResponse users = userService.GetUsers(new GetUserListRequest() { AccountID = this.Identity.ToAccountID(), IsSTAdmin = this.Identity.IsSTAdmin() });
            viewModel.Users = users.Users;

            viewModel.OwnerId = this.Identity.ToUserID();
            var stages = dropdownValues.Where(k => k.DropdownID == (byte)DropdownFieldTypes.OpportunityStage).
                Select(k => k.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.Stages = stages;
            var defaultvalue = stages.Where(i => i.IsDefault == true).FirstOrDefault();
            viewModel.StageID = defaultvalue != null ? defaultvalue.DropdownValueID : stages.FirstOrDefault().DropdownValueID;
            ViewBag.IsPeople = ReadCookie("IsPeople");
            ViewBag.OpportunityPage = "AddEditOpportunity";
            ViewBag.IsModal = true;
            SearchContactsResponse<ContactEntry> searchresponse = new SearchContactsResponse<ContactEntry>();
            searchresponse = contactService.GetAllContacts<ContactEntry>(new SearchContactsRequest()
            {
                Query = "",
                Limit = 1,
                PageNumber = 1,
                SortFieldType = ContactSortFieldType.NoSort,
                AccountId = this.Identity.ToAccountID(),
                ContactIDs = new List<int>() { contactId }.ToArray(),
                ShowingFieldType = ContactShowingFieldType.PeopleAndComapnies,
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            viewModel.Contacts = searchresponse.Contacts;
            return View("_AddEditBuyer", viewModel);
        }

        [Route("addopportunity")]
        [SmarttouchAuthorize(AppModules.Opportunity, AppOperations.Create)]
        [MenuType(MenuCategory.AddEditOpportunity, MenuCategory.LeftMenuCRM)]
        public ActionResult AddOpportunity()
        {
            OpportunityViewModel viewModel = new OpportunityViewModel();
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.DateFormat = this.Identity.ToDateFormat();

            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString(), 1);

            GetUserListResponse users = userService.GetUsers(new GetUserListRequest() { AccountID = this.Identity.ToAccountID(), IsSTAdmin = this.Identity.IsSTAdmin() });
            viewModel.Users = users.Users;

            viewModel.OwnerId = this.Identity.ToUserID();
            ViewBag.IsPeople = ReadCookie("IsPeople");
            ViewBag.OpportunityPage = "AddEditOpportunity";
            viewModel.Image = new ImageViewModel();
            return View("AddEditOpportunity", viewModel);
        }

        [Route("editopportunity")]
        [SmarttouchAuthorize(AppModules.Opportunity, AppOperations.Edit)]
        [MenuType(MenuCategory.AddEditOpportunity, MenuCategory.LeftMenuCRM)]
        public ActionResult EditOpportunity(int opportunityID)
        {
            GetOpportunityResponse response = opportunityService.getOpportunity(new GetOpportunityRequest()
            {
                OpportunityID = opportunityID,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                IncludeTags = false
            });
            if (response.OpportunityViewModel == null)
                return RedirectToAction("NotFound", "Error");

            response.OpportunityViewModel.CreatedOn = response.OpportunityViewModel.CreatedOn.ToUserUtcDateTimeV2();
            response.OpportunityViewModel.DateFormat = this.Identity.ToDateFormat();
           
            response.OpportunityViewModel.Currency = this.Identity.ToCurrency();

            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString(), 1);

            GetUserListResponse users = userService.GetUsers(new GetUserListRequest() { AccountID = this.Identity.ToAccountID(), IsSTAdmin = this.Identity.IsSTAdmin() });
            response.OpportunityViewModel.Users = users.Users;            
            ViewBag.IsPeople = ReadCookie("IsPeople");
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.OpportunityPage = "AddEditOpportunity";
            if (response.OpportunityViewModel.Image == null)
                response.OpportunityViewModel.Image = new ImageViewModel();

            return View("AddEditOpportunity", response.OpportunityViewModel);
        }


        [Route("opportunities")]
        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        [SmarttouchAuthorize(AppModules.Opportunity, AppOperations.Read)]
        [OutputCache(Duration = 30)]
        public ActionResult OpportunitiesList()
        {
            OpportunityViewModel viewModel = new OpportunityViewModel();
            ViewBag.OpportunityID = 0;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            viewModel.DateFormat = this.Identity.ToDateFormat();
            ViewBag.Currency = this.Identity.ToCurrency();
            AddCookie("opportunitypagenumber", "1", 1);
            AddCookie("opportunitypagesize", this.Identity.ToItemsPerPage(), 1);

            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString(), 1);

            ViewBag.IsPeople = ReadCookie("IsPeople");
            RemoveCookie("opportunitysearchtext");
            return View("OpportunitiesList", viewModel);
        }

        [Route("opportunitiesreportlist/{userIds?}/{StartDate?}/{EndDate?}")]
        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        [SmarttouchAuthorize(AppModules.Opportunity, AppOperations.Read)]
        [OutputCache(Duration = 30)]
        public ActionResult OpportunitiesReportList(string userIds, string StartDate, string EndDate)
        {
            OpportunityViewModel viewModel = new OpportunityViewModel();
            ViewBag.OpportunityID = 0;
            int[] userIDs = null;
            if (!string.IsNullOrEmpty(userIds))
                userIDs = JsonConvert.DeserializeObject<int[]>(userIds);
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            viewModel.DateFormat = this.Identity.ToDateFormat();
            ViewBag.Currency = this.Identity.ToCurrency();
            AddCookie("opportunitypagenumber", "1", 1);
            AddCookie("opportunitypagesize", this.Identity.ToItemsPerPage(), 1);

            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString(), 1);

            ViewBag.IsPeople = ReadCookie("IsPeople");
            RemoveCookie("opportunitysearchtext");
            ViewBag.UserIds = userIDs;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            return View("OpportunitiesList", viewModel);
        }


        [Route("opportunities/search")]
        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        [SmarttouchAuthorize(AppModules.Opportunity, AppOperations.Read)]
        public ActionResult OpportunitiesSearchList()
        {
            OpportunityViewModel viewModel = new OpportunityViewModel();
            ViewBag.OpportunityID = 0;
            ViewBag.OpportunityDetail = 1;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            viewModel.DateFormat = this.Identity.ToDateFormat();
            ViewBag.Currency = this.Identity.ToCurrency();
            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString(), 1);

            ViewBag.IsPeople = ReadCookie("IsPeople");

            AddCookie("opportunitypagenumber", "1", 1);
            AddCookie("opportunitypagesize", this.Identity.ToItemsPerPage(), 1);
            return View("OpportunitiesList", viewModel);
        }

        [Route("summaryopportunities")]
        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        [SmarttouchAuthorize(AppModules.Opportunity, AppOperations.Read)]
        [OutputCache(Duration=30)]
        public ActionResult OpportunitiesList(string opportunities)
        {
            OpportunityViewModel viewModel = new OpportunityViewModel();
            ViewBag.OpportunityID = 0;
            ViewBag.OpportunityDetail = 1;
            ViewBag.SummaryOpportunities = opportunities;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            viewModel.DateFormat = this.Identity.ToDateFormat();
            ViewBag.Currency = this.Identity.ToCurrency();
            AddCookie("opportunitypagenumber", "1", 1);
            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString() , 1);
            ViewBag.IsPeople = ReadCookie("IsPeople");

            AddCookie("opportunitypagesize", this.Identity.ToItemsPerPage(), 1);
            RemoveCookie("opportunitysearchtext");
            return View("OpportunitiesList", viewModel);
        }

        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        [SmarttouchAuthorize(AppModules.Opportunity, AppOperations.Read)]
        public ActionResult OpportunitiesViewRead([DataSourceRequest] DataSourceRequest request, string name, string Ids, string userIds, string StartDate, string EndDate)
       {
            AddCookie("userpagesize", request.PageSize.ToString(), 1);
            AddCookie("userpagenumber", request.Page.ToString(), 1);
            AddCookie("opportunitysearchtext", name, 1);
            AddCookie("summaryopportunities", Ids, 1);
            int[] userIDs = null;
            if (!string.IsNullOrEmpty(userIds))
                userIDs = JsonConvert.DeserializeObject<int[]>(userIds);
            var sortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : GetPropertyName<OpportunityViewModel,DateTime?>(r => r.LastModifiedOn);
            var direction = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Descending;
            var result = getOpportunities(request.Page, request.PageSize, name, null, "", true, Ids, userIDs, StartDate, EndDate, sortField, direction);

            IEnumerable<DropdownViewModel> DropDownFields = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            IEnumerable<DropdownValueViewModel> OpportunityStage = DropDownFields.Where(i => i.DropdownID == (byte)DropdownFieldTypes.OpportunityStage)
                                                                                 .Select(x => x.DropdownValuesList).FirstOrDefault();
            foreach (var opp in result.Opportunities)
            {
                var stage = OpportunityStage.Where(i => i.DropdownValueID == opp.StageID).Select(x => x.DropdownValue).FirstOrDefault();
                if(!string.IsNullOrEmpty(stage))
                    opp.Stage = stage;
            }               


            return Json(new DataSourceResult
            {
                Data = result.Opportunities,
                Total = (int)result.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Tags

        [SmarttouchAuthorize(AppModules.Tags, AppOperations.Create)]
        public ActionResult _AddTag()
        {
            ViewBag.IsContactTag = false;
            AddTagViewModel addTagViewModel = new AddTagViewModel();
            var tags = tagService.GetRecentAndPopularTags(new GetRecentAndPopularTagsRequest(){ AccountId = this.Identity.ToAccountID() });
            if (tags.PopularTags != null)
            {
                addTagViewModel.PopularTags = tags.PopularTags;
            }
            else { addTagViewModel.PopularTags = null; }
            if (tags.RecentTags != null)
            {
                addTagViewModel.RecentTags = tags.RecentTags;
            }
            else { addTagViewModel.RecentTags = null; }

            return PartialView("~/Views/Contact/_AddTag.cshtml", addTagViewModel);
        }


        public JsonResult AddOpportunityTag(string tagName, int opportunityID,int tagId)
        {
            TagViewModel tagViewModel = new TagViewModel()
                                        {
                                            TagName = tagName,
                                            OpportunityID = opportunityID,
                                            AccountID = this.Identity.ToAccountID(),
                                            CreatedBy = this.Identity.ToUserID(),
                                            TagID = tagId
                                        };

            SaveTagResponse response = tagService.SaveOpportunityTag(new SaveTagRequest() { TagViewModel = tagViewModel });
            return Json(new { success = true, response = response.TagViewModel }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemoveOpportunityTag(string tagName,int tagId, int opportunityID)
        {
            tagService.DeleteOpportunityTag(
                new DeleteTagRequest()
                {
                    TagName = tagName,
                    TagId = tagId,
                    OpportunityID = opportunityID,
                    AccountId = this.Identity.ToAccountID()
                });
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }



        #endregion

        #region DeleteOpportunities

        public ActionResult DeleteOpportunities(int[] id)
        {
            DeleteOpportunityRequest request = new DeleteOpportunityRequest() { OpportunityIDs = id, RequestedBy = this.Identity.ToUserID() };
            opportunityService.DeleteOpportunities(request);
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        void UpdateUserActivityLog(int entityId, string entityName)
        {
            int userId = this.Identity.ToUserID();
            UserReadActivityRequest request = new UserReadActivityRequest();
            request.ActivityName = UserActivityType.Read; request.EntityId = entityId; request.ModuleName = AppModules.Opportunity; request.UserId = userId;
            request.AccountId = this.Identity.ToAccountID(); request.EntityName = entityName;
            userService.InsertReadActivity(request);
        }

        /// <summary>
        /// Gets from dropdown values.
        /// </summary>
        /// <param name="dropdownValues">The dropdown values.</param>
        /// <param name="dropdownId">The dropdown identifier.</param>
        /// <returns></returns>
        private static IEnumerable<dynamic> GetFromDropdownValues(IEnumerable<DropdownViewModel> dropdownValues, byte dropdownId)
        {
            return dropdownValues.Where(s => s.DropdownID == dropdownId).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
        }

        public ActionResult _AddEditBuyer()
        {
            OpportunityViewModel viewModel = new OpportunityViewModel();
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.DateFormat = this.Identity.ToDateFormat();
            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString(), 1);

            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            GetUserListResponse users = userService.GetUsers(new GetUserListRequest() { AccountID = this.Identity.ToAccountID(), IsSTAdmin = this.Identity.IsSTAdmin() });
            viewModel.Users = users.Users;

            viewModel.OwnerId = this.Identity.ToUserID();
            var stages = dropdownValues.Where(k => k.DropdownID == (byte)DropdownFieldTypes.OpportunityStage).
                Select(k => k.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.Stages = stages;
            var defaultvalue = stages.Where(i => i.IsDefault == true).FirstOrDefault();
            viewModel.StageID = defaultvalue != null ? defaultvalue.DropdownValueID : stages.FirstOrDefault().DropdownValueID;
            ViewBag.IsPeople = ReadCookie("IsPeople");
            ViewBag.OpportunityPage = "AddEditOpportunity";
            ViewBag.IsModal = false;
            return PartialView("_AddEditBuyer", viewModel);
        }

       // [Route("editbuyer")]
        public ActionResult EditBuyer(int buyerId)
        {
            OpportunityViewModel viewModel = new OpportunityViewModel();
            OpportunityBuyer buyer = opportunityService.GetOpportunityBuyerById(buyerId);
            viewModel.Potential = buyer.Potential;
            viewModel.ExpectedCloseDate = buyer.ExpectedToClose;
            viewModel.Comments = buyer.Comments;
            viewModel.OwnerId = buyer.Owner;
            viewModel.UserID = buyer.Owner;
            viewModel.StageID = (short)buyer.StageID;
            viewModel.OpportunityID = buyer.OpportunityID;
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.DateFormat = this.Identity.ToDateFormat();
            SearchContactsResponse<ContactEntry> searchresponse = new SearchContactsResponse<ContactEntry>();
            searchresponse = contactService.GetAllContacts<ContactEntry>(new SearchContactsRequest()
            {
                Query = "",
                Limit = 1,
                PageNumber = 1,
                SortFieldType = ContactSortFieldType.NoSort,
                AccountId = this.Identity.ToAccountID(),
                ContactIDs = new List<int>() { buyer.ContactID }.ToArray(),
                ShowingFieldType = ContactShowingFieldType.PeopleAndComapnies,
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });

            viewModel.Contacts = searchresponse.Contacts;
            var IsPeople = cachingService.GetOpportunityCustomers(this.Identity.ToUserID(), this.Identity.ToAccountID(), this.Identity.ToRoleID());
            AddCookie("IsPeople", IsPeople.ToString(), 1);

            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            GetUserListResponse users = userService.GetUsers(new GetUserListRequest() { AccountID = this.Identity.ToAccountID(), IsSTAdmin = this.Identity.IsSTAdmin() });
            viewModel.Users = users.Users;

            var stages = dropdownValues.Where(k => k.DropdownID == (byte)DropdownFieldTypes.OpportunityStage).
                Select(k => k.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            viewModel.Stages = stages;
            ViewBag.IsPeople = ReadCookie("IsPeople");
            ViewBag.OpportunityPage = "AddEditOpportunity";
            ViewBag.IsModal = true;
            ViewBag.OpportunityName = opportunityService.GetOpportunityNameByOPPContactMapId(buyerId);
            return PartialView("_AddEditBuyer", viewModel);
        }
        
        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateOpportunityName(int opportunityId,string oppName)
        {
            UpdateOpportunityViewResponse updateresponse = opportunityService.UpdateOpportunityName(new UpdateOpportunityViewRequest()
            {
               OpportunityID = opportunityId,
               OpportunityName= oppName,
               RequestedBy = this.Identity.ToUserID()
            });
           
            return Json(new
            {
                success = true,
                response = updateresponse.opportunityViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateOpportunityStage(int opportunityId, int stageId)
        {
            UpdateOpportunityViewResponse updateresponse = opportunityService.UpdateOpportunityStage(new UpdateOpportunityViewRequest()
            {
                OpportunityID = opportunityId,
                StageID = stageId
            });

            return Json(new
            {
                success = true,
                response = updateresponse.opportunityViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateOpportunityOwner(int opportunityId, int ownerId)
        {
            UpdateOpportunityViewResponse updateresponse = opportunityService.UpdateOpportunityOwner(new UpdateOpportunityViewRequest()
            {
                OpportunityID = opportunityId,
                OwnerId = ownerId
            });

            return Json(new
            {
                success = true,
                response = updateresponse.opportunityViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateOpportunityDescription(int opportunityId, string description)
        {
            UpdateOpportunityViewResponse updateresponse = opportunityService.UpdateOpportunityDescription(new UpdateOpportunityViewRequest()
            {
                OpportunityID = opportunityId,
                Description = description,
                RequestedBy = this.Identity.ToUserID()
            });

            return Json(new
            {
                success = true,
                response = updateresponse.opportunityViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateOpportunityPotential(int opportunityId, decimal potential)
        {
            UpdateOpportunityViewResponse updateresponse = opportunityService.UpdateOpportunityPotential(new UpdateOpportunityViewRequest()
            {
                OpportunityID = opportunityId,
                Potential = potential,
                RequestedBy = this.Identity.ToUserID()
            });

            return Json(new
            {
                success = true,
                response = updateresponse.opportunityViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateOpportunityExpectedCloseDate(int opportunityId, DateTime expectedCloseDate)
        {
            UpdateOpportunityViewResponse updateresponse = opportunityService.UpdateOpportunityExpectedCloseDate(new UpdateOpportunityViewRequest()
            {
                OpportunityID = opportunityId,
                ExpectedCloseDate = expectedCloseDate
            });

            return Json(new
            {
                success = true,
                response = updateresponse.opportunityViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateOpportunityType(int opportunityId, string type)
        {
            UpdateOpportunityViewResponse updateresponse = opportunityService.UpdateOpportunityType(new UpdateOpportunityViewRequest()
            {
                OpportunityID = opportunityId,
                OpportunityType = type,
                RequestedBy = this.Identity.ToUserID()
            });

            return Json(new
            {
                success = true,
                response = updateresponse.opportunityViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateOpportunityProductType(int opportunityId, string productType)
        {
            UpdateOpportunityViewResponse updateresponse = opportunityService.UpdateOpportunityProductType(new UpdateOpportunityViewRequest()
            {
                OpportunityID = opportunityId,
                ProductType = productType,
                RequestedBy = this.Identity.ToUserID()
            });

            return Json(new
            {
                success = true,
                response = updateresponse.opportunityViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateOpportunityAddress(int opportunityId, string address)
        {
            UpdateOpportunityViewResponse updateresponse = opportunityService.UpdateOpportunityAddress(new UpdateOpportunityViewRequest()
            {
                OpportunityID = opportunityId,
                Address = address,
                RequestedBy = this.Identity.ToUserID()
            });

            return Json(new
            {
                success = true,
                response = updateresponse.opportunityViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        [MenuType(MenuCategory.Opportunities, MenuCategory.LeftMenuCRM)]
        public JsonResult UpdateOpportunityImage(int opportunityId,ImageViewModel image)
        {
            ImageViewModel imageviewmodel = new ImageViewModel();
            SaveImageResponse imageresponse = imageService.SaveImage(new SaveImageRequest()
            {
                ImageCategory = ImageCategory.OpportunityProfile,
                ViewModel = image,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID()
            });
            imageviewmodel = imageresponse.ImageViewModel;

            UpdateOpportunityViewResponse updateresponse = opportunityService.UpdateOpportunityImage(new UpdateOpportunityViewRequest()
            {
                OpportunityID = opportunityId,
                image = imageviewmodel,
                RequestedBy = this.Identity.ToUserID()
            });

            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOpportunityBuyers(int opportunityId,int accountId,int pageNumber,int pageSize)
        {

            GetOpportunityBuyerResponse response = opportunityService.GetOpportunityBuyers(new GetOpportunityBuyerRequest()
            {
                OpportunityId = opportunityId,
                AccountId = accountId,
                PageNumber = pageNumber,
                PageSize = pageSize
            });

            return Json(new
            {
                success = true,
                response = new DataSourceResult
                {
                    Data = response.OpportunityBuyers.OrderBy(b=>b.ExpectedToClose),
                    Total = response.OpportunityBuyers.IsAny() ? (int)response.OpportunityBuyers.Select(s => s.TotalCount).FirstOrDefault() : 0
                }
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOpportunityBuyerNames(int opportunityId)
        {
            int accountId = this.Identity.ToAccountID();
            IEnumerable<OpportunityBuyer> buyers = opportunityService.GetAllOpportunityBuyersName(opportunityId, accountId);

            return Json(new
            {
                success = true,
                response = buyers
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteOpportunityBuyer(int buyerId)
        {

            opportunityService.DeleteOpportunityBuyer(buyerId);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

     
    }
}