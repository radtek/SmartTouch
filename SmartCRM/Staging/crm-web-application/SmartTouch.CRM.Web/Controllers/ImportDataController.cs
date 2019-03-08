using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class ImportDataController : SmartTouchController
    {
        readonly IAccountService accountService;
        readonly ICustomFieldService customFieldsService;
        readonly ICachingService cachingService;

        public ImportDataController(IAccountService accountService, ICustomFieldService customFieldsService, ICachingService cachingService)
        {
            this.accountService = accountService;
            this.customFieldsService = customFieldsService;
            this.cachingService = cachingService;
        }

        #region Import Data

        [Route("importdata")]
        [SmarttouchAuthorize(AppModules.ImportData, AppOperations.Read)]
        [MenuType(MenuCategory.ImportData, MenuCategory.LeftMenuAccountConfiguration)]
        [OutputCache(Duration = 30)]
        public ActionResult ImportDataList()
        {
            ImportListViewModel viewModel = new ImportListViewModel();
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            return View("ImportDataList", viewModel);
        }

        [SmarttouchAuthorize(AppModules.ImportData, AppOperations.Create)]
        [MenuType(MenuCategory.ImportData, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult CancelImport(string fileName)
        {
            var filepath = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["LEADADAPTER_PHYSICAL_PATH"].ToString(),
                                        this.Identity.ToAccountID().ToString(), fileName);
            if (System.IO.File.Exists(filepath))
                System.IO.File.Delete(filepath);
            return RedirectToAction("ImportDataList");
        }

        [SmarttouchAuthorize(AppModules.ImportData, AppOperations.Read)]
        public ActionResult ImportsViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            AddCookie("importpagesize", request.PageSize.ToString(), 1);
            AddCookie("importpagenumber", request.Page.ToString(), 1);
            GetImportsResponse response = accountService.GetAllImports(new GetImportsRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountId = UserExtensions.ToAccountID(this.Identity)
            });
           response.Imports.Each(i => i.CreatedDateTime = i.CreatedDateTime.ToUtcBrowserDatetime());
            var imports = response.Imports;
            return Json(new DataSourceResult
            {
                Data = imports,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        [Route("downloadexcelfile")]
        public FileResult downloadExcelfile()
        {
            return DownloadFile("sampleexcel.xls");
        }

        [Route("downloadcsvfile")]
        public FileResult downloadCSVfile()
        {
            return DownloadFile("samplecsv.csv");
        }

        private FileResult DownloadFile(string fileName)
        {
            var filepath = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["LEADADAPTER_PHYSICAL_PATH"].ToString(), string.Format("excelsamplefiles\\{0}", fileName));
            if (System.IO.File.Exists(filepath))
            {
                FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                return File(fileStream, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else
                return null;
        }

        private static byte[] GetFile(string s)
        {
            using (var fs = System.IO.File.OpenRead(s))
            {
                byte[] data = new byte[fs.Length];
                int br = fs.Read(data, 0, data.Length);
                if (br != fs.Length)
                    throw new System.IO.IOException(s);
                return data;
            }
        }


        [Route("importdatastep1")]
        [SmarttouchAuthorize(AppModules.ImportData, AppOperations.Create)]
        [MenuType(MenuCategory.EmptyTopMenuItem, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult ImportDataStep1()
        {
            ImportDataListViewModel viewModel = new ImportDataListViewModel();
            return View("ImportDataStep1", viewModel);
        }

        [Route("importdatastep2")]
        [SmarttouchAuthorize(AppModules.ImportData, AppOperations.Create)]
        [MenuType(MenuCategory.ImportDataStep2, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult ImportDataStep2(ImportDataListViewModel viewModel)
        {

            ViewBag.IsAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            //var imports = TempData["imports"] as ImportDataListViewModel;
            if (viewModel == null)
            {
                Logger.Current.Informational("If the Temp data is null");
                return RedirectToAction("ImportDataStep1");
            }
            viewModel.OwnerId = this.Identity.ToUserID();

            return View("ImportDataStep2", viewModel);
        }

        [HttpPost]
        //[MenuType(MenuCategory.EmptyTopMenuItem, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchAuthorize(AppModules.ImportData, AppOperations.Create)]
        [MenuType(MenuCategory.ImportDataStep2, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult Save(IEnumerable<HttpPostedFileBase> files,bool NeverBounceValidation = false)
        {
            Logger.Current.Informational("Save method in import data controller is called and the file count is: " + files.Count());
            GetImportdataResponse response = new GetImportdataResponse();
            var AccountID = this.Identity.ToAccountID();
            if (files != null)
            {
                foreach (var file in files)
                {
                    var destinationPath = Path.Combine(ConfigurationManager.AppSettings["LEADADAPTER_PHYSICAL_PATH"].ToString(), AccountID.ToString(), file.FileName);
                    file.SaveAs(destinationPath);
                    response = accountService.GetImportData(new GetImportdataRequest() { filename = Path.GetFileName(file.FileName), AccountID = AccountID });
                    GetAllCustomFieldsRequest customfieldsrequest = new GetAllCustomFieldsRequest(AccountID);
                    customfieldsrequest.RequestedBy = this.Identity.ToUserID();
                    Logger.Current.Informational("File name after fetching data from file");
                }
                if (response.ImportDataListViewModel != null)
                {
                    response.ImportDataListViewModel.UpdateOnDuplicate = true;
                    response.ImportDataListViewModel.DuplicateLogic = 2;
                    response.ImportDataListViewModel.NeverBounceValidation = NeverBounceValidation;
                    response.ImportDataListViewModel.OwnerId = this.Identity.ToUserID();
                    ViewBag.IsAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
                    //TempData["imports"] = response.ImportDataListViewModel;
                    Logger.Current.Informational("Storing view model in tempdata before redirection");
                    return View("ImportDataStep2", response.ImportDataListViewModel);
                }
                else
                {
                    ViewBag.ErrorMsg = response.Exception.Message;
                    return View("ImportDataStep1");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "[|Please select a file|]";
                return View("ImportDataStep1");
            }
        }

        [SmarttouchAuthorize(AppModules.ImportData, AppOperations.Create)]
        public ActionResult InsertImportData(string importViewModel)
        {
            ImportDataListViewModel viewModel = JsonConvert.DeserializeObject<ImportDataListViewModel>(importViewModel);
            viewModel.AccountID = UserExtensions.ToAccountID(this.Identity);
            viewModel.UserId = UserExtensions.ToUserID(this.Identity);
            ImportDataRequest request = new ImportDataRequest()
            {
                ImportDataListViewModel = viewModel,
                RequestedBy = viewModel.OwnerId
            };
            accountService.ImportData(request);
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ImportErrorLog

        [Route("importfilehistory")]
        [SmarttouchAuthorize(AppModules.ImportData, AppOperations.Read)]
        [MenuType(MenuCategory.EmptyTopMenuItem, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult ImportFileHistory()
        {
            GetImportForAccountRequest request = new GetImportForAccountRequest()
            {
                AccountId = this.Identity.ToAccountID()
            };
            GetImportForAccountResponse response = accountService.GetImportDataByAccountID(request);
            ViewBag.Name = response.Import.LeadAdapterName;
            ViewBag.leadAdapterID = response.Import.LeadAdapterAndAccountMapId;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewLeadAdapterViewModel viewModel = new ViewLeadAdapterViewModel();
            return View("~/Views/LeadAdapter/ViewLeadAdapter.cshtml", viewModel);
        }

        [Route("importerrorlist")]
        [SmarttouchAuthorize(AppModules.ImportData, AppOperations.Read)]
        [MenuType(MenuCategory.EmptyTopMenuItem, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult ErrorImportList(int JobID, int ImportID)
        {
            ViewBag.LeadAdapterJobLogID = JobID;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.LeadAdapterID = ImportID;
            ViewBag.LeadAdapterName = "Import";
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            LeadAdapterJobLogDeailsViewModel viewModel = new LeadAdapterJobLogDeailsViewModel();
            return View("~/Views/LeadAdapter/LeadAdapterErrorDetails.cshtml", viewModel);
        }
        #endregion

  
    }
}