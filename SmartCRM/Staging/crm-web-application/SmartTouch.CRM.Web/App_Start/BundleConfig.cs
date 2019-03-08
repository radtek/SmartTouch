
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Optimization;
using System.Data;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            var staticDomain = System.Configuration.ConfigurationManager.AppSettings["STATIC_DOMAIN"].ToString();
            //var version = System.Configuration.ConfigurationManager.AppSettings["STATIC_VERSION"].ToString();

            //if static domain or version are not available,then use default.
            if (!(string.IsNullOrEmpty(staticDomain)))
            {
                Styles.DefaultTagFormat = "<link href='" + staticDomain + "{0}" + "' rel='stylesheet'/>";
                Scripts.DefaultTagFormat = "<script src='" + staticDomain + "{0}" + "'></script>";
            }

            bundles.IgnoreList.Clear();
            bundles.UseCdn = true;
            bundles.Add(new ScriptBundle("~/bundles/modernizr")
                .Include("~/Scripts/modernizr-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/Scripts/jquery-{version}.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/jqueryui")
                .Include("~/Scripts/jquery-ui*")
                .Include("~/Scripts/jquery.ui.touch-punch.min.js")
                .Include("~/Scripts/jquery.jcarousel.min.js")
                .Include("~/Scripts/jquery.nearest.min.js")
                .Include("~/Scripts/jquery.signalR-{version}.js")
                .Include("~/signalr/js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/knockout")
                .Include("~/Scripts/knockout-{version}.js")
                .Include("~/Scripts/knockout.mapping-latest.js")
                .Include("~/Scripts/knockout.clear.js"));

            bundles.Add(new ScriptBundle("~/bundles/knockout2")
                .Include("~/Scripts/knockout.validation.js")
                .Include("~/Scripts/knockout-sortable.min.js")
                .Include("~/Scripts/knockout-kendo.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/kendoknockout")
                 .Include("~/Scripts/knockout-kendo.js")
                 );

            /*
             * List of controls included in custom build
             * 
             * */

            bundles.Add(new ScriptBundle("~/bundles/kendoui")
              .Include("~/Scripts/kendoui/kendo.all.min.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/kendouiaspnetmvc")
              .Include("~/Scripts/kendoui/kendo.aspnetmvc.min.js")
             );


            //bundles.Add(new ScriptBundle("~/bundles/kendoui")
            //    .Include("~/Scripts/kendoui/kendo.all.min.js")
            //    .Include("~/Scripts/kendoui/kendo.aspnetmvc.min.js")
            //    );

            bundles.Add(new ScriptBundle("~/bundles/utilities")
               .Include("~/Scripts/alertify/alertify.min.js")
               .Include("~/Scripts/ViewModels/LayoutViewModel.js")
               .Include("~/Scripts/ViewModels/Tagify.js")
               .Include("~/Scripts/moment.js")
               .Include("~/Scripts/moment-timezone-with-data.js"));

            bundles.Add(new ScriptBundle("~/bundles/smarttouchframework")
                .Include("~/Scripts/bootstrap.min.js")
                .Include("~/Scripts/bootstrap-select.js")
                .Include("~/Scripts/bootstrap-switch.js")
                .Include("~/Scripts/flatui-checkbox.js")
                .Include("~/Scripts/flatui-radio.js")
                .Include("~/Scripts/jquery.tagsinput.js")
                .Include("~/Scripts/jquery.placeholder.js"));

            bundles.Add(new ScriptBundle("~/bundles/application")
                .Include("~/Scripts/application.js")
                .Include("~/Scripts/Jquery.idletimer.js")
                .Include("~/Scripts/jquery.idletimeout.js"));

            bundles.Add(new ScriptBundle("~/bundles/viewmodels")
                .Include("~/Scripts/XSockets.latest.js")
                .Include("~/Scripts/ViewModels/QuickSearchViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/Opportunityvm")
                .Include("~/Scripts/ViewModels/AddTagViewModel.js")
                .Include("~/Scripts/ViewModels/TimeLineViewModel.js")
                .Include("~/Scripts/ViewModels/AttachmentViewModel.js")
                .Include("~/Scripts/ViewModels/OpportunityByerViewModel.js")
                .Include("~/Scripts/ViewModels/AddAction.js")
                .Include("~/Scripts/ViewModels/AddNoteViewModel.js")
                .Include("~/Scripts/ViewModels/TourViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/campaignvm")
                .Include("~/Scripts/draganddrop.js")
                .Include("~/Scripts/ViewModels/TagViewModel.js")
                .Include("~/Scripts/ViewModels/AdvancedSearchViewModel.js")
                .Include("~/Scripts/ViewModels/CampaignDesigner.js")
                .Include("~/Scripts/jquery.htmlbeautifier.js")
                .Include("~/Scripts/ViewModels/NewCampaignViewModel.js")
                .Include("~/Scripts/Prettify/prettify.js")
                .Include("~/Scripts/ST-Utilities/ImageHelper.js")
                .Include("~/Scripts/MailGun/mailGun.js")
                .Include("~/Scripts/MailGun/mailgun_validator.js")
                .Include("~/Scripts/jquery.lazyload.js")
                .Include("~/Scripts/plugins/bootstrap-datetimepicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/campaignUnsubscribevm")
               .Include("~/Scripts/CampaignUnsubscribe.js"));

            bundles.Add(new ScriptBundle("~/bundles/campaignsvm")
                .Include("~/Scripts/ViewModels/NewCampaignViewModel.js")
                .Include("~/Scripts/ViewModels/CampaignListViewModel.js")
                .Include("~/Scripts/ViewModels/AddAction.js")
                .Include("~/Scripts/ViewModels/AddNoteViewModel.js")
                .Include("~/Scripts/ViewModels/TourViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/campaignstatsvm")
            .Include("~/Scripts/ViewModels/CampaignStatistics.js")
            .Include("~/Scripts/ViewModels/NewCampaignViewModel.js")
            .Include("~/Scripts/ViewModels/ResendCampaignViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/formvm")
                .Include("~/Scripts/ViewModels/FormDesigner.js")
                .Include("~/Scripts/draganddrop.js")
                .Include("~/Scripts/knockout-sortable.min.js")
                .Include("~/Scripts/ZeroClipboard.js"));

            bundles.Add(new ScriptBundle("~/bundles/formsvm")
                .Include("~/Scripts/ViewModels/FormListViewModel.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/customfieldvm")
                .Include("~/Scripts/draganddrop.js")
                .Include("~/Scripts/knockout-sortable.min.js")
                .Include("~/Scripts/ViewModels/CustomFieldsViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/dropdownFieldsvm")
               .Include("~/Scripts/ViewModels/DropdownValuesViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/sendMailvm")
            .Include("~/Scripts/ViewModels/SendMailViewModel.js")
            .Include("~/Scripts/redactor/redactor.js"));

            bundles.Add(new ScriptBundle("~/bundles/sendTextvm")
          .Include("~/Scripts/ViewModels/SendTextViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/relationshipvm")
           .Include("~/Scripts/ViewModels/RelationshipViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/newLeadsvm")
                .Include("~/Scripts/ViewModels/NewLeadsReportViewModel.js")
                //.Include("~/Scripts/Kendoui/jszip.min.js")
                //.Include("~/Scripts/Kendoui/kendo.excel.min.js")
              .Include("~/Scripts/kendo-excel-all.js")
              .Include("~/Scripts/kendo-excel-jszip.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/reportslistvm")
              .Include("~/Scripts/ViewModels/ReportListViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/customreportvm")
              .Include("~/Scripts/ViewModels/CustomReportViewModel.js")
                //.Include("~/Scripts/Kendoui/jszip.min.js")
                //.Include("~/Scripts/Kendoui/kendo.excel.min.js")
              .Include("~/Scripts/kendo-excel-all.js")
              .Include("~/Scripts/kendo-excel-jszip.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/hotListvm")
           .Include("~/Scripts/ViewModels/HotListViewModel.js")
            .Include("~/Scripts/jquery.cookie.js")
                //.Include("~/Scripts/Kendoui/jszip.min.js")
                //.Include("~/Scripts/Kendoui/kendo.excel.min.js")
              .Include("~/Scripts/kendo-excel-all.js")
              .Include("~/Scripts/kendo-excel-jszip.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/campaignListvm")
           .Include("~/Scripts/ViewModels/CampaignListReport.js")
                //.Include("~/Scripts/Kendoui/jszip.min.js")
                //.Include("~/Scripts/Kendoui/kendo.excel.min.js")
              .Include("~/Scripts/kendo-excel-all.js")
              .Include("~/Scripts/kendo-excel-jszip.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/editreportvm")
           .Include("~/Scripts/ViewModels/ReportViewModel.js")
                //.Include("~/Scripts/Kendoui/jszip.min.js")
                //.Include("~/Scripts/Kendoui/kendo.excel.min.js")
              .Include("~/Scripts/kendo-excel-all.js")
              .Include("~/Scripts/kendo-excel-jszip.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/firstleadsourcevm")
                .Include("~/Scripts/ViewModels/FirstLeadSourceReportViewModel.js")
                .Include("~/Scripts/kendo-excel-all.js")
                .Include("~/Scripts/kendo-excel-jszip.js"));

            bundles.Add(new ScriptBundle("~/bundles/allleadsourcevm")
                .Include("~/Scripts/ViewModels/AllLeadSourceReportViewModel.js")
                .Include("~/Scripts/kendo-excel-all.js")
                .Include("~/Scripts/kendo-excel-jszip.js"));

            bundles.Add(new ScriptBundle("~/bundles/nightlyreportvm")
                .Include("~/Scripts/ViewModels/NightlyStatusReport.js")
                .Include("~/Scripts/kendo-excel-all.js")
                .Include("~/Scripts/kendo-excel-jszip.js"));

            bundles.Add(new ScriptBundle("~/bundles/databaselifecyclevm")
               .Include("~/Scripts/ViewModels/DatabaseLifeCyleReportViewModel.js")
               .Include("~/Scripts/kendo-excel-all.js")
               .Include("~/Scripts/kendo-excel-jszip.js"));

            bundles.Add(new ScriptBundle("~/bundles/formsCountSummaryvm")
                .Include("~/Scripts/ViewModels/FormsCountSummaryViewModel.js")
                .Include("~/Scripts/kendo-excel-all.js")
                .Include("~/Scripts/kendo-excel-jszip.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/reengagementreport")
                .Include("~/Scripts/ViewModels/ReEngagementReportViewModel.js")
                .Include("~/Scripts/kendo-excel-all.js")
                .Include("~/Scripts/kendo-excel-jszip.js"));

            bundles.Add(new ScriptBundle("~/bundles/bdxFreemiumCustomLeadReportvm")
                .Include("~/Scripts/ViewModels/BDXContactsViewModel.js")
                .Include("~/Scripts/jquery.cookie.js"));

            bundles.Add(new ScriptBundle("~/bundles/bdxCustomLeadReportvm")
                .Include("~/Scripts/ViewModels/BDXCustomLeadReportViewModel.js")
                .Include("~/Scripts/jquery.cookie.js")
                //.Include("~/Scripts/Kendoui/jszip.min.js")
                //.Include("~/Scripts/Kendoui/kendo.excel.min.js")
              .Include("~/Scripts/kendo-excel-all.js")
              .Include("~/Scripts/kendo-excel-jszip.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/dashboardvm")
                    .Include("~/Scripts/ViewModels/DashboardViewModel.js"));

            #region Advanced search script bundles
            bundles.Add(new ScriptBundle("~/bundles/advancedsearchvm")
                   .Include("~/Scripts/ViewModels/SendMailViewModel.js")
                   .Include("~/Scripts/ViewModels/AdvancedSearchViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/savedsearchesgrid")
                   .Include("~/Scripts/ViewModels/SearchListViewModel.js"));
            #endregion

            #region Leadadapter script bundles
            bundles.Add(new ScriptBundle("~/bundles/leadadapterviewmodel")
                   .Include("~/Scripts/ViewModels/LeadAdapterViewModel.js"));

            #endregion

            #region Importdata script bundles
            bundles.Add(new ScriptBundle("~/bundles/importdatavm")
                   .Include("~/Scripts/ViewModels/ImportDataViewModel.js"));

            #endregion

            #region Automation Script Bundles

            bundles.Add(new ScriptBundle("~/bundles/automationvm")
                  .Include("~/Scripts/ViewModels/WorkflowViewModel.js")
                  .Include("~/Scripts/ViewModels/WorkflowActionsViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/automationlistvm")
              .Include("~/Scripts/ViewModels/WorkflowListViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/automationreportvm")
            .Include("~/Scripts/ViewModels/WorkflowReportViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/quickeditnotifyuservm")
                .Include("~/Scripts/ViewModels/WorkflowNotifyUserViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/quickedituserassignmntvm")
            .Include("~/Scripts/ViewModels/WorkflowUserAssignmentActionViewModel.js"));

            #endregion

            #region Contacts script bundles
            bundles.Add(new ScriptBundle("~/bundles/contactsgridvm").
                Include("~/Scripts/ViewModels/SendTextViewModel.js").
                Include("~/Scripts/ViewModels/AddAction.js").
                Include("~/Scripts/ViewModels/AddNoteViewModel.js").
                Include("~/Scripts/ViewModels/TourViewModel.js").
                Include("~/Scripts/ViewModels/AddTagViewModel.js").
                Include("~/Scripts/ViewModels/ChangeOwnerViewModel.js").
                Include("~/Scripts/ViewModels/SendMailViewModel.js").
                Include("~/Scripts/ViewModels/CompanyViewModel.js").
                Include("~/Scripts/ViewModels/PersonViewModel.js").
                Include("~/Scripts/ViewModels/RelationshipViewModel.js").
                Include("~/Scripts/ViewModels/ExportPersonViewModel.js")
                );
            bundles.Add(new ScriptBundle("~/bundles/contactsresultsview").Include("~/Scripts/ViewModels/ContactResultsViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/quicklinksvm").
              Include("~/Scripts/ViewModels/AddAction.js").
              Include("~/Scripts/ViewModels/AddNoteViewModel.js").
              Include("~/Scripts/ViewModels/TourViewModel.js").
              Include("~/Scripts/ViewModels/AddTagViewModel.js").
              Include("~/Scripts/ViewModels/SendMailViewModel.js").
              Include("~/Scripts/ViewModels/SendTextViewModel.js").
              Include("~/Scripts/ViewModels/ChangeOwnerViewModel.js").
              Include("~/Scripts/ViewModels/RelationshipViewModel.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/contactsaddeditvm").
                Include("~/Scripts/ViewModels/CompanyViewModel.js").
                Include("~/Scripts/ViewModels/PersonViewModel.js").
                Include("~/Scripts/MailGun/mailGun.js").
                Include("~/Scripts/MailGun/mailgun_validator.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/contactsviewvm").
               Include("~/Scripts/ViewModels/TimeLineViewModel.js").
               Include("~/Scripts/ViewModels/AttachmentViewModel.js").
               Include("~/Scripts/ViewModels/PersonDetailsViewModel.js").
               Include("~/Scripts/ViewModels/ContactDetailsViewModel.js").
               Include("~/Scripts/ViewModels/CustomFieldsManagerViewModel.js").
               Include("~/Scripts/MailGun/mailGun.js").
               Include("~/Scripts/MailGun/mailgun_validator.js")

               );

            bundles.Add(new ScriptBundle("~/bundles/persondetailsvm")
            .Include("~/Scripts/ViewModels/PersonDetailsViewModel.js"));
            #endregion

            /*
             * Use the development version of Modernizr to develop with and learn from. Then, when you're
             * ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
             * **/
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/actionvm")
                .Include("~/Scripts/ViewModels/AddAction.js"));

            bundles.Add(new ScriptBundle("~/bundles/addtagvm")
                .Include("~/Scripts/ViewModels/AddTagViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/accountsgrid")
                .Include("~/Scripts/ViewModels/AccountListViewModel.js")
                .Include("~/Scripts/ViewModels/AccountViewModel.js")
                .Include("~/Scripts/kendo-excel-all.js")
                .Include("~/Scripts/kendo-excel-jszip.js"));

            bundles.Add(new ScriptBundle("~/bundles/scrubqueuelist")
                .Include("~/Scripts/ViewModels/NeverBounceViewModel.js")
                .Include("~/Scripts/kendo-excel-all.js")
                .Include("~/Scripts/kendo-excel-jszip.js"));

            bundles.Add(new ScriptBundle("~/bundles/senderreputationreport")
                .Include("~/Scripts/ViewModels/CampaignReputationReport.js"));

            bundles.Add(new ScriptBundle("~/bundles/accountsettingsvm")

                .Include("~/Scripts/ViewModels/ServiceProviderViewModel.js")
                .Include("~/Scripts/ViewModels/AccountViewModel.js")
                .Include("~/Scripts/MailGun/mailGun.js")
                .Include("~/Scripts/MailGun/mailgun_validator.js")
               );

            bundles.Add(new ScriptBundle("~/bundles/rolevm")
                .Include("~/Scripts/ViewModels/RolePermissionsViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/recentactivities")
                .Include("~/Scripts/ViewModels/UserActivitiesViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/uservm").
                 Include("~/Scripts/ViewModels/UserViewModel.js").
                 Include("~/Scripts/MailGun/mailGun.js").
                 Include("~/Scripts/MailGun/mailgun_validator.js"));

            bundles.Add(new ScriptBundle("~/bundles/addedituservm").
                 Include("~/Scripts/ViewModels/AddEditUserViewModel.js").
                 Include("~/Scripts/MailGun/mailGun.js").
                 Include("~/Scripts/MailGun/mailgun_validator.js"));

            bundles.Add(new ScriptBundle("~/bundles/resendcampaignviewModelvm")
             .Include("~/Scripts/ViewModels/ResendCampaignViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/myprofilevm")
                .Include("~/Scripts/ViewModels/UserSettingsViewModel.js")
                .Include("~/Scripts/ViewModels/UserNotificationsViewModel.js")
                .Include("~/Scripts/ViewModels/AddEditUserViewModel.js")
                .Include("~/Scripts/MailGun/mailGun.js")
                .Include("~/Scripts/MailGun/mailgun_validator.js"));

            bundles.Add(new ScriptBundle("~/bundles/usergrid")
               .Include("~/Scripts/ViewModels/UserListViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/redactor")
                .Include("~/Scripts/redactor/redactor.js"));

            bundles.Add(new ScriptBundle("~/bundles/codemirror")
                .Include("~/Scripts/codemirror-2.37/lib/codemirror.js")
                .Include("~/Scripts/codemirror-2.37/mode/javascript/javascript.js")
                );

            bundles.Add(new StyleBundle("~/Content/codemirror")
                .Include("~/Scripts/codemirror-2.37/lib/codemirror.css")
                .Include("~/Scripts/codemirror-2.37/theme/ambiance.css"));

            bundles.Add(new StyleBundle("~/Content/campaignunsubscribe")
                .Include("~/Content/css/bootstrap.css")
                .Include("~/Content/css/flat-ui.css")
                .Include("~/Content/css/smarttouch.css")
                .Include("~/Content/css/smarttouchicons.css")
                .Include("~/Content/css/flat-ui.css"));


            bundles.Add(new StyleBundle("~/Content/kendocss")
                .Include("~/Content/kendoui/kendo.common.min.css")
                .Include("~/Content/kendoui/kendo.default.min.css"));

            bundles.Add(new StyleBundle("~/Content/kendodatavizcss")
               .Include("~/Content/kendoui/kendo.metro.min.css")
               .Include("~/Content/kendoui/kendo.dataviz.min.css")
               .Include("~/Content/kendoui/kendo.dataviz.default.min.css")
               .Include("~/Content/kendoui/kendo.dataviz.metro.min.css"));

            bundles.Add(new StyleBundle("~/Content/smarttouchframework")
                .Include("~/Content/styles/bootstrap.css")
                .Include("~/Content/styles/flat-ui.css"));

            bundles.Add(new StyleBundle("~/Content/smarttouchcss")
                .Include("~/Content/styles/smarttouch.css")
                .Include("~/Content/styles/smarttouchicons.css"));

            bundles.Add(new StyleBundle("~/Content/campaignstyles")
                .Include("~/Content/Prettify/Themes/tomorrow.css")
                .Include("~/Content/jquery-ui.min.css"));

            bundles.Add(new StyleBundle("~/Content/bootstraptourcss")
                .Include("~/Content/bootstrap-tour.css"));

            bundles.Add(new StyleBundle("~/Content/Redactor")
                .Include("~/Scripts/redactor/redactor.css"));

            bundles.Add(new ScriptBundle("~/bundles/leadscoreviewmodel")
                .Include("~/Scripts/ViewModels/LeadScoreViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/leadscorelistviewmodel")
                .Include("~/Scripts/ViewModels/LeadScoreListViewModel.js")
                .Include("~/Scripts/ViewModels/LeadScoreViewModel.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/exportviewmodel")
                .Include("~/Scripts/ViewModels/ExportPersonViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/tagviewmodel")
                .Include("~/Scripts/ViewModels/TagViewModel.js")
                //.Include("~/Scripts/Kendoui/jszip.min.js")
                //.Include("~/Scripts/Kendoui/kendo.excel.min.js")
              .Include("~/Scripts/kendo-excel-all.js")
              .Include("~/Scripts/kendo-excel-jszip.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/webvisitreportviewmodel")
           .Include("~/Scripts/ViewModels/WebVisitReportViewModel.js")
                //.Include("~/Scripts/Kendoui/jszip.min.js")
                //.Include("~/Scripts/Kendoui/kendo.excel.min.js")
              .Include("~/Scripts/kendo-excel-all.js")
              .Include("~/Scripts/kendo-excel-jszip.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/apptourcms")
                .Include("~/Scripts/ViewModels/ApplicationTourViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/serviceProviderViewModel")
              .Include("~/Scripts/ViewModels/ServiceProviderViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/appTour")
                .Include("~/Scripts/bootstrap-tour.js")
                .Include("~/Scripts/ViewModels/AppTourViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/marketingmessages")
                .Include("~/Scripts/ViewModels/MarketingMessageViewModel.js")
                .Include("~/Scripts/ViewModels/MarketingMessageContentViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/imagedomainvm")
           .Include("~/Scripts/ViewModels/ImageDomainListViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/emailvalidatorvm")
             .Include("~/Scripts/ViewModels/EmailValidatorViewModel.js")
             .Include("~/Scripts/ViewModels/TagViewModel.js")
             .Include("~/Scripts/ViewModels/AdvancedSearchViewModel.js"));

            bundles.Add(new ScriptBundle("~/bundles/tourByContactsvm")
              .Include("~/Scripts/ViewModels/TourByContactsReportViewModel.js")
                 .Include("~/Scripts/kendo-excel-all.js")
                 .Include("~/Scripts/kendo-excel-jszip.js")
                 );
            bundles.Add(new ScriptBundle("~/bundles/formsubmissionsvm")
                .Include("~/Scripts/kendo-excel-jszip.js")
                .Include("~/Scripts/Kendoui/kendo.all.min.js"));

            /*
             * Page wise bundles
             * To reduce number of requests
             * */

            var layoutJqueryKnockoutScriptBundle = ClubBundleFiles(bundles, new List<string>()
            {
                "~/bundles/modernizr","~/bundles/jqueryui","~/bundles/knockout","~/bundles/knockout2","~/bundles/utilities"
            }, new ScriptBundle("~/bundles/knockoutjs"));


            bundles.Add((ScriptBundle)layoutJqueryKnockoutScriptBundle);

            var layoutScriptBottomBundle = ClubBundleFiles(bundles, new List<string>()
            {
             "~/bundles/appTour", "~/bundles/quicklinksvm","~/bundles/redactor","~/bundles/smarttouchframework","~/bundles/viewmodels","~/bundles/application"
            }, new ScriptBundle("~/bundles/layoutbottom"));

            var layoutStyleBundle = ClubBundleFiles(bundles, new List<string>()
            {
                "~/Content/kendocss","~/Content/kendodatavizcss","~/Content/bootstraptourcss","~/Content/smarttouchframework","~/Content/smarttouchcss"
            }, new StyleBundle("~/Content/layoutstyles"));


            bundles.Add((ScriptBundle)layoutScriptBottomBundle);
            bundles.Add((StyleBundle)layoutStyleBundle);


            BundleTable.EnableOptimizations = true;
        }
        private static IEnumerable<string> GetFiles(BundleCollection bundles, string bundleName)
        {
            return new BundleResolver(bundles).GetBundleContents(bundleName);
        }
        private static Bundle ClubBundleFiles(BundleCollection bundles, IEnumerable<string> bundlePaths, Bundle bundle)
        {
            bundlePaths.Each(b =>
            {
                GetFiles(bundles, b).Each(f =>
                {
                    bundle.Include(f);
                });
            });
            return bundle;
        }
    }
}
