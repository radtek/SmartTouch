using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.Messaging.Geo;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class FormController : SmartTouchController
    {
        IFormService formService;
        ICustomFieldService customFieldService;
        IGeoService geoService;
        IReportService reportService;

        readonly ICachingService cachingService;

        public FormController(IFormService formService, ICustomFieldService customFieldService, IGeoService geoService, ICachingService cachingService, IReportService reportService)
        {
            this.formService = formService;
            this.customFieldService = customFieldService;
            this.geoService = geoService;
            this.cachingService = cachingService;
            this.reportService = reportService;
        }

        [Route("forms")]
        [SmarttouchAuthorize(AppModules.Forms, AppOperations.Read)]
        [MenuType(MenuCategory.Forms, MenuCategory.LeftMenuCRM)]
        [OutputCache(Duration = 30)]
        public ActionResult Forms()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            var formsCountReport = reportService.GetReportByType(new ApplicationServices.Messaging.Reports.GetReportsRequest() { ReportType = Reports.FormsCountSummary, AccountId = this.Identity.ToAccountID() });
            ViewBag.FormsCountReportId = formsCountReport != null ? formsCountReport.Report.ReportID : 0;
            return View("FormsList");
        }

        [Route("formsreportlist/{userIds?}/{StartDate?}/{EndDate?}")]
        [SmarttouchAuthorize(AppModules.Forms, AppOperations.Read)]
        [MenuType(MenuCategory.Forms, MenuCategory.LeftMenuCRM)]
        [OutputCache(Duration = 30)]
        public ActionResult FormsReportList(string userIds, string StartDate, string EndDate)
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            int[] userIDs = null;
            if (!string.IsNullOrEmpty(userIds))
                userIDs = JsonConvert.DeserializeObject<int[]>(userIds);
            ViewBag.UserIds = userIDs;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            return View("FormsList");
        }

        [Route("forms/search")]
        [SmarttouchAuthorize(AppModules.Forms, AppOperations.Read)]
        [MenuType(MenuCategory.Forms, MenuCategory.LeftMenuCRM)]
        public ActionResult FormSearch()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.formId = 1;
            return View("FormList");
        }

        [Route("addform")]
        [SmarttouchAuthorize(AppModules.Forms, AppOperations.Create)]
        [MenuType(MenuCategory.AddEditForm, MenuCategory.LeftMenuCRM)]
        public ActionResult AddForm()
        {
            ViewBag.AccountId = this.Identity.ToAccountID();
            GetAllContactFieldsRequest request = new GetAllContactFieldsRequest();
            GetAllContactFieldsResponse response = formService.GetAllContactFields(request);
            var fieldsToBeRemoved = new List<int>();
            fieldsToBeRemoved.Add((int)ContactFields.PartnerTypeField);
            fieldsToBeRemoved.Add((int)ContactFields.DonotEmail);
            fieldsToBeRemoved.Add((int)ContactFields.LifecycleStageField);
            fieldsToBeRemoved.Add((int)ContactFields.LeadScore);
            fieldsToBeRemoved.Add((int)ContactFields.Owner);
            fieldsToBeRemoved.Add((int)ContactFields.WebPage);
            fieldsToBeRemoved.Add((int)ContactFields.WebPageDuration);
            fieldsToBeRemoved.Add((int)ContactFields.ContactTag);
            fieldsToBeRemoved.Add((int)ContactFields.FormName);
            fieldsToBeRemoved.Add((int)ContactFields.FormsubmittedOn);
            fieldsToBeRemoved.Add((int)ContactFields.FirstSourceType);
            fieldsToBeRemoved.Add((int)ContactFields.NoteSummary);
            fieldsToBeRemoved.Add((int)ContactFields.LastNoteDate);
            FormViewModel formViewModel = new FormViewModel();
            formViewModel.Fields = response.ContactFields.Where(c => !fieldsToBeRemoved.Contains(c.FieldId)).ToList();
            formViewModel.FormFields = new List<FormFieldViewModel>();
            FieldViewModel firstNameField = formViewModel.Fields.Where(f => f.FieldId == (int)ContactFields.FirstNameField).FirstOrDefault();
            FieldViewModel lastNameField = formViewModel.Fields.Where(f => f.FieldId == (int)ContactFields.LastNameField).FirstOrDefault();
            FieldViewModel primaryEmailField = formViewModel.Fields.Where(f => f.FieldId == (int)ContactFields.PrimaryEmail).FirstOrDefault();
            FieldViewModel mobilePhoneField = formViewModel.Fields.Where(f => f.FieldId == (int)ContactFields.MobilePhoneField).FirstOrDefault();
            FieldViewModel leadSourceField = formViewModel.Fields.Where(f => f.FieldId == (int)ContactFields.LeadSource).FirstOrDefault();
            var communityField = formViewModel.Fields.Where(c => c.FieldId == (int)ContactFields.Community).FirstOrDefault();
            communityField.FieldInputTypeId = FieldType.dropdown;
            formViewModel.FormFields.Add(new FormFieldViewModel()
            {
                DisplayName = firstNameField.Title,
                FieldId = firstNameField.FieldId,
                FieldInputTypeId = firstNameField.FieldInputTypeId,
                IsMandatory = firstNameField.IsMandatory,
                StatusId = firstNameField.StatusId,
                Title = firstNameField.Title,
                Value = firstNameField.Value,
                ValueOptions = firstNameField.ValueOptions
            });
            formViewModel.FormFields.Add(new FormFieldViewModel()
            {
                DisplayName = lastNameField.Title,
                FieldId = lastNameField.FieldId,
                FieldInputTypeId = lastNameField.FieldInputTypeId,
                IsMandatory = lastNameField.IsMandatory,
                StatusId = lastNameField.StatusId,
                Title = lastNameField.Title,
                Value = lastNameField.Value,
                ValueOptions = lastNameField.ValueOptions
            });
            formViewModel.FormFields.Add(new FormFieldViewModel()
            {
                DisplayName = primaryEmailField.Title,
                FieldId = primaryEmailField.FieldId,
                FieldInputTypeId = primaryEmailField.FieldInputTypeId,
                IsMandatory = true,
                StatusId = primaryEmailField.StatusId,
                Title = primaryEmailField.Title,
                Value = primaryEmailField.Value,
                ValueOptions = primaryEmailField.ValueOptions
            });
            formViewModel.FormFields.Add(new FormFieldViewModel()
            {
                DisplayName = mobilePhoneField.Title,
                FieldId = mobilePhoneField.FieldId,
                FieldInputTypeId = mobilePhoneField.FieldInputTypeId,
                IsMandatory = false,
                StatusId = mobilePhoneField.StatusId,
                Title = mobilePhoneField.Title,
                Value = mobilePhoneField.Value,
                ValueOptions = mobilePhoneField.ValueOptions
            });
            formViewModel.FormFields.Add(new FormFieldViewModel()
            {
                DisplayName = leadSourceField.Title,
                FieldId = leadSourceField.FieldId,
                FieldInputTypeId = leadSourceField.FieldInputTypeId,
                IsMandatory = true,
                StatusId = leadSourceField.StatusId,
                Title = leadSourceField.Title,
                Value = leadSourceField.Value
            });
            //formViewModel.FormFields.Add(new FormFieldViewModel()
            //{
            //    DisplayName = "",
            //    FieldId = (int)ContactFields.FormURLField,
            //    FieldInputTypeId = FieldType.text,
            //    IsMandatory = false,
            //    StatusId = FieldStatus.Active,
            //    Title = "redirect-override",
            //    Value = null
            //});
            GetAllCustomFieldsRequest customFieldRequest = new GetAllCustomFieldsRequest(this.Identity.ToAccountID());
            GetAllCustomFieldsResponse customFieldResponse = customFieldService.GetAllCustomFieldsForForms(customFieldRequest);
            formViewModel.CustomFields = customFieldResponse.CustomFields.Where(c => c.StatusId == FieldStatus.Active).ToList();
            formViewModel.FormId = 0;
            formViewModel.Name = "";
            formViewModel.Status = Entities.FormStatus.Active;
            formViewModel.AcknowledgementType = Entities.AcknowledgementType.Url;
            formViewModel.CreatedBy = this.Identity.ToUserID();
            formViewModel.LastModifiedBy = this.Identity.ToUserID();
            formViewModel.AccountId = this.Identity.ToAccountID();
            ViewBag.TagPopup = true;
            return View("AddForm", formViewModel);
        }

        [Route("editform")]
        [SmarttouchAuthorize(AppModules.Forms, AppOperations.Edit)]
        [MenuType(MenuCategory.AddEditForm, MenuCategory.LeftMenuCRM)]
        public ActionResult EditForm(int formId)
        {
            int accountId = this.Identity.ToAccountID();
            GetFormResponse response = formService.GetForm(new GetFormRequest(formId)
            {
                AccountId = accountId,
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            GetAllContactFieldsRequest fieldsRequest = new GetAllContactFieldsRequest();
            GetAllContactFieldsResponse fieldsResponse = formService.GetAllContactFields(fieldsRequest);
            response.FormViewModel.Fields = fieldsResponse.ContactFields.ToList();
            GetAllCustomFieldsRequest customFieldRequest = new GetAllCustomFieldsRequest(this.Identity.ToAccountID());
            GetAllCustomFieldsResponse customFieldResponse = customFieldService.GetAllCustomFieldsForForms(customFieldRequest);
            response.FormViewModel.CustomFields = customFieldResponse.CustomFields.Where(c => c.StatusId == FieldStatus.Active).ToList();
            var communityField = response.FormViewModel.Fields.Where(c => c.FieldId == (int)ContactFields.Community).FirstOrDefault();
            communityField.FieldInputTypeId = FieldType.dropdown;
            var communityFormField = response.FormViewModel.FormFields.Where(c => c.FieldId == (int)ContactFields.Community).FirstOrDefault();
            if (communityFormField != null)
                communityFormField.FieldInputTypeId = FieldType.dropdown;
            var customFieldIds = response.FormViewModel.CustomFields.Where(c => c.StatusId == FieldStatus.Active).Select(f => f.FieldId).ToList();
            var contactFieldIds = Enum.GetValues(typeof(ContactFields)).Cast<ContactFields>().ToList();
            response.FormViewModel.FormFields = response.FormViewModel.FormFields.Where(f => customFieldIds.Contains(f.FieldId) || contactFieldIds.Contains((ContactFields)f.FieldId)).OrderBy(f => f.SortId).ToList();
            foreach (FormFieldViewModel formField in response.FormViewModel.FormFields)
            {
                FieldViewModel fieldViewModel = response.FormViewModel.CustomFields.Where(c => c.FieldId == formField.FieldId).FirstOrDefault();
                if (fieldViewModel != null && fieldViewModel.ValueOptions.IsAny())
                {
                    formField.ValueOptions = fieldViewModel.ValueOptions;
                }
            }
            //if (!response.FormViewModel.FormFields.Any(a => a.FieldId == 74))
            //{
            //    response.FormViewModel.FormFields.Add(new FormFieldViewModel()
            //    {
            //        DisplayName = "",
            //        FieldId = (int)ContactFields.FormURLField,
            //        FieldInputTypeId = FieldType.text,
            //        IsMandatory = false,
            //        StatusId = FieldStatus.Active,
            //        Title = "redirect-override",
            //        Value = null
            //    });
            //}
            ViewBag.SaveAs = false;
            ViewBag.AccountID = UserExtensions.ToAccountID(this.Identity);
            var timeZone = this.Identity.ToTimeZone();
            var value = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            var newDate = TimeZoneInfo.ConvertTime(response.FormViewModel.CreatedDate, value);
            response.FormViewModel.LastModifiedOn = newDate;
            var dateFormat = this.Identity.ToDateFormat();
            response.FormViewModel.DateFormat = dateFormat;
            ViewBag.TagPopup = true;
            return View("AddForm", response.FormViewModel);
        }

        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        [SmarttouchAuthorize(AppModules.Forms, AppOperations.Delete)]
        public ActionResult DeleteForm(string formIDs)
        {
            if (!formIDs.IsAny())
            {
                return Json(new
                {
                    success = true,
                    response = "[|Could not delete the form|]."
                }, JsonRequestBehavior.AllowGet);
            }
            DeleteFormRequest request = JsonConvert.DeserializeObject<DeleteFormRequest>(formIDs);
            formService.DeleteForm(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("saveformas")]
        [SmarttouchAuthorize(AppModules.Forms, AppOperations.Edit)]
        [MenuType(MenuCategory.AddEditForm, MenuCategory.LeftMenuCRM)]
        public ActionResult SaveFormAs(int formId)
        {
            GetAllContactFieldsRequest fieldsRequest = new GetAllContactFieldsRequest();
            GetAllContactFieldsResponse fieldsResponse = formService.GetAllContactFields(fieldsRequest);
            GetFormResponse response = formService.GetForm(new GetFormRequest(formId)
            {
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID()
            });
            response.FormViewModel.Fields = fieldsResponse.ContactFields.Where(c => c.FieldId != (int)ContactFields.PartnerTypeField && c.FieldId != (int)ContactFields.LifecycleStageField && c.FieldId != (int)ContactFields.LeadScore && c.FieldId != (int)ContactFields.DonotEmail && c.FieldId != (int)ContactFields.Owner).ToList();
            GetAllCustomFieldsRequest customFieldRequest = new GetAllCustomFieldsRequest(this.Identity.ToAccountID());
            GetAllCustomFieldsResponse customFieldResponse = customFieldService.GetAllCustomFieldsForForms(customFieldRequest);
            response.FormViewModel.CustomFields = customFieldResponse.CustomFields.Where(c => c.StatusId == FieldStatus.Active).ToList();
            var customFieldIds = response.FormViewModel.CustomFields.Select(f => f.FieldId).ToList();
            var communityField = response.FormViewModel.Fields.Where(c => c.FieldId == (int)ContactFields.Community).FirstOrDefault();
            communityField.FieldInputTypeId = FieldType.dropdown;
            var communityFormField = response.FormViewModel.FormFields.Where(c => c.FieldId == (int)ContactFields.Community).FirstOrDefault();
            if (communityFormField != null)
                communityFormField.FieldInputTypeId = FieldType.dropdown;
            var contactFieldIds = Enum.GetValues(typeof(ContactFields)).Cast<ContactFields>().ToList();
            response.FormViewModel.FormFields = response.FormViewModel.FormFields.Where(f => customFieldIds.Contains(f.FieldId) || contactFieldIds.Contains((ContactFields)f.FieldId)).OrderBy(f => f.SortId).ToList();
            foreach (FormFieldViewModel formField in response.FormViewModel.FormFields)
            {
                FieldViewModel fieldViewModel = response.FormViewModel.CustomFields.Where(c => c.FieldId == formField.FieldId).FirstOrDefault();
                if (fieldViewModel != null && fieldViewModel.ValueOptions.IsAny())
                {
                    formField.ValueOptions = fieldViewModel.ValueOptions;
                }
            }
            response.FormViewModel.FormId = 0;
            response.FormViewModel.Status = Entities.FormStatus.Active;
            response.FormViewModel.Name = "";
            response.FormViewModel.CreatedDate = DateTime.Now.ToUniversalTime();
            response.FormViewModel.AllSubmissions = 0;
            response.FormViewModel.Submissions = 0;
            response.FormViewModel.UniqueSubmissions = 0;
            var dateFormat = this.Identity.ToDateFormat();
            response.FormViewModel.DateFormat = dateFormat;
            ViewBag.SaveAs = true;
            ViewBag.AccountID = UserExtensions.ToAccountID(this.Identity);
            return View("AddForm", response.FormViewModel);
        }

        [SmarttouchAuthorize(AppModules.Forms, AppOperations.Read)]
        public ActionResult FormsListView([DataSourceRequest] DataSourceRequest request, string name, string status, string userIds, string StartDate, string EndDate)
        {
            byte accountstatus = 0;
            if (!String.IsNullOrEmpty(status))
                accountstatus = Convert.ToByte(status);
            AddCookie("formpagesize", request.PageSize.ToString(), 1);
            AddCookie("formpagenumber", request.Page.ToString(), 1);
            int[] userIDs = null;
            if (!string.IsNullOrEmpty(userIds))
                userIDs = JsonConvert.DeserializeObject<int[]>(userIds);
            SearchFormsResponse response = formService.GetAllForms(new SearchFormsRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                Status = accountstatus,
                AccountId = UserExtensions.ToAccountID(this.Identity),
                RequestedBy = this.Identity.ToUserID(),
                RoleId = this.Identity.ToRoleID(),
                UserID = this.Identity.ToUserID(),
                UserIds = userIDs.IsAny() ? userIDs.ToList() : new List<int>() { },
                StartDate = !string.IsNullOrEmpty(StartDate) ? Convert.ToDateTime(StartDate) : (DateTime?)null,
                EndDate = !string.IsNullOrEmpty(EndDate) ? Convert.ToDateTime(EndDate) : (DateTime?)null,
                SortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : null,
                SortDirection = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Descending
            });

            var jsonResult = Json(new DataSourceResult
            {
                Data = response.Forms,
                Total = (int)response.TotalHits
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [Route("approveleads")]
        [MenuType(MenuCategory.ApproveLeads, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult ApproveLeads()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("ApproveLeads");
        }

        [SmarttouchAuthorize(AppModules.ApproveLeads, AppOperations.Read)]
        public ActionResult ApproveLeadsList([DataSourceRequest] DataSourceRequest request)
        {
            bool iSTAdmin = this.Identity.IsSTAdmin();
            int accountId = this.Identity.ToAccountID();
            IEnumerable<ApproveLeadsQueue> queue = formService.GetLeadsApproveQueue(new GetApproveLeadsRequest()
            {
                AccountId = accountId,
                Limit = request.PageSize,
                PageNumber = request.Page,
                DateType = FailedFormsDateType.Last7Days
            }).Queue;

            return Json(new DataSourceResult
            {
                Data = queue,
                Total = queue.IsAny() ? queue.Select(s => s.TotalCount).FirstOrDefault() : 0
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateFormData(string queue)
        {
            formService.UpdateFailedForm(new UpdateFailedFormLeadsRequest() { Queue = JsonConvert.DeserializeObject<ApproveLeadsQueue>(queue), RequestedBy = this.Identity.ToUserID() });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadFailedForms()
        {
            int accountId = this.Identity.ToAccountID();
            IEnumerable<ApproveLeadsQueue> queue = formService.GetLeadsApproveQueue(new GetApproveLeadsRequest()
            {
                AccountId = accountId,
                Limit = 10000,
                PageNumber = 1,
                DateType = FailedFormsDateType.Last14Days
            }).Queue;
            if (queue != null)
            {
                byte[] data = formService.GetFailedFormsResults(new GetFailedFormsResultsRequest() { Queue = queue }).Result;
                return File(data, @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", "Suspect_Submissions_List.xls");
            }
            else
                return Json(new
            {
                success = true,
                fileKey = "",
                fileName = "Suspect_Submissions_List"
                }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [Route("formacknowledgement")]
        public ActionResult FormSubmissionAcknowledgement(string message)
        {
            FormViewModel viewModel = new FormViewModel();
            viewModel.Acknowledgement = message;
            return View("FormSubmissionAcknowledgement", viewModel);
        }

        public JsonResult GetCountriesAndStates()
        {
            GetCountriesAndStatesRequest request = new GetCountriesAndStatesRequest();
            GetCountriesAndStatesResponse response = geoService.GetCountriesAndStates(request);
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAccountDropdownValues()
        {
            var dropdownValues = cachingService.GetDropdownValues(this.Identity.ToAccountID());
            return Json(new
            {
                success = true,
                response = dropdownValues
            }, JsonRequestBehavior.AllowGet);
        }

        [MenuType(MenuCategory.FormSubmissions, MenuCategory.LeftMenuCRM)]
        public ActionResult FormSubmissions(int formId, int periodId, string customStartDateTicks, string customEndDateTicks)
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.FormId = formId;
            ViewBag.PeriodId = periodId;
            ViewBag.CustomStarDateTicks = customStartDateTicks;
            ViewBag.CustomEndDateTicks = customEndDateTicks;
            ViewBag.FormName = formService.GetFormNameById(new GetFormNameByIdRequest() { FormId = formId }).FormName;
            ViewBag.SubmittedData = this.GetFirstFormSubmission(formId, 1, 1, customStartDateTicks, customEndDateTicks, periodId);
            return View("FormSubmissions");
        }

        public JsonResult GetFormSubmissions(int formId, int pageLimt, int pageNumber, string customStartDateTicks, string customEndDateTicks, int? periodId)
        {
            DateTime startDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue, endDate = DateTime.UtcNow;
            startDate = startDate.AddYears(1);

            if (periodId == null)
                periodId = 0;

            List<DateTime> dates = GetDates(periodId, customStartDateTicks, customEndDateTicks);
            startDate = dates.FirstOrDefault();
            endDate = dates.LastOrDefault();
            var request = new GetFormSubmissionsRequest()
                {
                    FormId = formId,
                    StartDate = startDate,
                    EndDate = endDate,
                    PageLimit = pageLimt,
                    PageNumber = pageNumber
                };
            var formSubmissions = formService.GetFormSubmissions(request);
            if (formSubmissions != null && formSubmissions.FormSubmissions.IsAny())
                formSubmissions.FormSubmissions.ToList().ForEach(f => f.SubmittedOn = f.SubmittedOn.ToUtc());
            return Json(new { success = true, response = formSubmissions }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FormSubmissionsExprortAsFile(int formId, int pageLimt, int pageNumber, string customStartDateTicks, string customEndDateTicks, int? periodId)
        {
            DateTime startDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue, endDate = DateTime.UtcNow;
            startDate = startDate.AddYears(1);

            if (periodId == null)
                periodId = 0;

            List<DateTime> dates = GetDates(periodId, customStartDateTicks, customEndDateTicks);
            startDate = dates.FirstOrDefault();
            endDate = dates.LastOrDefault();
            var request = new GetFormSubmissionsRequest()
            {
                FormId = formId,
                StartDate = startDate,
                EndDate = endDate,
                PageLimit = pageLimt,
                PageNumber = pageNumber
            };

            var formSubmissions = formService.GetFormSubmissions(request);
            var d = new Dictionary<int, Dictionary<string, string>>();
            Func<JToken, string, string> getValue = (o, p) =>
            {
                if (o is JObject)
                {
                    foreach (KeyValuePair<string, JToken> keyValuePair in (JObject)o)
                    {
                        if (p == keyValuePair.Key)
                        {
                            return keyValuePair.Value.ToString();
                        }
                    }
                }
                else if (o.Type != JTokenType.Array)
                {
                    return o.ToString();
                }

                return string.Empty;
            };
            Func<string, DateTime, Dictionary<string, string>> func = (s, submittedOn) =>
            {
                var r = new Dictionary<string, string>();
                r.Add("Submission Date", submittedOn.ToString());
                var c = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(s);
                foreach (var kv in c)
                {
                    string value = (kv.Value != null) ? getValue(kv.Value, "NewValue") : string.Empty;
                    try
                    {
                        r.Add(kv.Key, value);
                    }
                    catch { }
                }

                return r;
            };
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            foreach (var submission in formSubmissions.FormSubmissions)
            {
                d.Add(submission.FormSubmissionId, func(submission.SubmittedData, submission.SubmittedOn));
            }
            sw.Stop();
            var e = sw.ElapsedTicks;
            var dt = getDataTable(d);
            var excel = new ReadExcel();
            string fileKey = Guid.NewGuid().ToString();
            bool result = cachingService.StoreTemporaryFile(fileKey, excel.ConvertDataSetToExcel(dt, ""));

            return Json(new
            {
                success = true,
                fileKey = fileKey,
                fileName = "FormSubmissions.xlsx"
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DownloadFile(string fileKey, string fileName)
        {
            byte[] file = cachingService.GetTemporaryFile(fileKey);
            return File(file, @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", fileName);
        }

        private DataTable getDataTable(Dictionary<int, Dictionary<string, string>> input)
        {
            var dt = new DataTable();
            List<string> columns = new List<string>();
            //add columns
            foreach (var c in input.FirstOrDefault().Value)
            {
                dt.Columns.Add(c.Key);
                columns.Add(c.Key);
            }
            Func<string, Dictionary<string, string>, string> ContainsKey = (s, d) =>
             {
                 var keys = d.Keys;
                 foreach (string k in keys)
                 {
                     if (k == s || k.Replace(" ", "") == s)
                         return k;
                     else if (k == s.Replace(" ", ""))
                         return s.Replace(" ", "");
                 }
                 return string.Empty;
             };
            //add data
            foreach (var kv in input)
            {
                DataRow dr = dt.NewRow();
                var values = kv.Value;
                foreach (var c in columns)
                {
                    if (values.ContainsKey(c))
                        dr[c] = values[c];
                    else if (values.ContainsKey(c.Replace(" ", "")))
                        dr[c.Replace(" ", "")] = values[c.Replace(" ", "")];
                }
                dt.Rows.Add(dr);
            }


            return dt;
        }
        public string GetFirstFormSubmission(int formId, int pageLimt, int pageNumber, string customStartDateTicks, string customEndDateTicks, int? periodId)
        {
            DateTime startDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue, endDate = DateTime.UtcNow;
            startDate = startDate.AddYears(1);

            if (periodId == null)
                periodId = 0;

            List<DateTime> dates = GetDates(periodId, customStartDateTicks, customEndDateTicks);
            startDate = dates.FirstOrDefault();
            endDate = dates.LastOrDefault();
            var request = new GetFormSubmissionsRequest()
            {
                FormId = formId,
                StartDate = startDate,
                EndDate = endDate,
                PageLimit = 1,
                PageNumber = 1
            };
            var formSubmissions = formService.GetFormSubmissions(request);
            return formSubmissions.FormSubmissions.IsAny() ? formSubmissions.FormSubmissions.FirstOrDefault().SubmittedData : string.Empty;
        }

        List<DateTime> GetDates(int? period, string customStartDateTicks, string customEndDateTicks)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime startDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue, endDate = DateTime.UtcNow;
            switch (period)
            {
                case 1:
                    startDate = DateTime.Now.AddDays(-7);
                    break;
                case 2:
                    startDate = DateTime.Now.AddDays(-30);
                    break;
                case 3:
                    startDate = DateTime.Now.AddDays(-90);
                    break;
                case 4:
                    startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                    break;
                case 5:
                    startDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
                    break;
                case 6:
                    startDate = new DateTime(DateTime.UtcNow.Year - 1, 1, 1);
                    endDate = new DateTime(DateTime.UtcNow.Year - 1, 12, 31);
                    break;
                case 7:
                    if (string.IsNullOrEmpty(customStartDateTicks))
                        startDate = DateTime.Today;
                    else
                    {
                        startDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        TimeSpan startDateTimSpan = TimeSpan.FromMilliseconds(long.Parse(customStartDateTicks));
                        startDate = startDate.Add(startDateTimSpan);
                        //startDate = DateTime.ParseExact(customStartDateTicks, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    if (string.IsNullOrEmpty(customEndDateTicks))
                        endDate = DateTime.UtcNow;
                    else
                    {
                        endDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        TimeSpan endDateTimSpan = TimeSpan.FromMilliseconds(long.Parse(customEndDateTicks));
                        endDate = endDate.Add(endDateTimSpan);
                        //endDate = DateTime.ParseExact(customEndDateTicks, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture); 
                    }

                    break;
            }
            dates.Add(startDate);
            dates.Add(endDate);
            return dates;
        }
    }
}
