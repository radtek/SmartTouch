using SimpleInjector;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Plugins.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.ApplicationServices;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ImplicitSync;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using SmartTouch.CRM.SearchEngine.Search;
using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using System.Configuration;
using SmartTouch.CRM.ApplicationServices.Messaging.Geo;
using System.Runtime.Serialization;
using LandmarkIT.Enterprise.CommunicationManager.Repositories;
using System.Globalization;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.ApplicationServices.Messaging.Tour;
using SmartTouch.CRM.MessageQueues;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Identity;
using SmartTouch.CRM.Domain.Workflows;
using HtmlAgilityPack;
using Microsoft.Office.Interop.Outlook;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.Plugins.Utilities.ImplicitSync
{
    public class WAException : System.Exception
    {
        public WAException(int status)
            : this(status, null)
        {
        }

        public WAException(int status, string message)
            : base(message)
        {
            Status = status;
        }

        public int Status { get; set; }
    }

    /// <summary>
    /// Summary description for WASyncHandler
    /// </summary>
    /// <exception cref="WAException">
    ///		All the functions can throw WAException with status code and optional custom error message 
    ///		that will be returned to client, if no error string is provided the error message will be 
    ///		retrieved via ISGetErrorMessages function.
    ///	</exception>
    public class WASyncHandler
    {

        /// <summary>
        /// If SessionIDManagerParam class is registered in web.config and used
        /// the context.Session object will be automatically initialized and be valid for every login session.
        /// The context.Session.SessionID will be equal to WA's session id.
        /// Also the context.Session[""] arguments can be used per user login session.
        /// If not, the sessions should be handled by manually.
        /// Please see <sessionState> option in web.config.
        /// </summary>
        /// <param name="sessionID"></param>
        public WASyncHandler(string sessionID)
        {
            session_ = sessionID;
        }

        /// <returns>true on success, otherwise false or Exception should be thrown</returns>
        public UserViewModel ISLogin(string domainUrl, string username, string password, Tuple<IUserService> services)
        {
            Logger.Current.Verbose("Request received by WASyncHandler/IsLogin method with parameters: " + domainUrl + ", " + username);
            var userService = services.Item1;
            GetUserRequest request = new GetUserRequest(1) { UserName = username, DomainUrl = domainUrl };
            GetUserResponse response = userService.GetUserByUserName(request);
            return response.User;
        }


        /// <returns>true on success, otherwise false or Exception should be thrown</returns>
        public bool ISLoginImpersonated(string username, string password, string impersonatedUserAccountName, string impersonatedUserEmail, Tuple<IUserService> services)
        {
            Logger.Current.Verbose("Request received by WASyncHandler/ISLoginImpersonated. AccountName: " + impersonatedUserAccountName + ", Email: " + impersonatedUserEmail);
            var userService = services.Item1;
            GetLoginInfoByUsernameRequest request = new GetLoginInfoByUsernameRequest() { UserName = username };
            GetLoginInfoByUsernameResponse response = userService.GetSuperAdminByEmail(request);
            if (string.IsNullOrEmpty(response.UserInfo.Value))
                throw new System.Exception("User is not impersonated");

            PasswordHasher passwordHasher = new PasswordHasher();
            return passwordHasher.VerifyHashedPassword(response.UserInfo.Value, password) == PasswordVerificationResult.Success;
        }


        /// <summary>
        /// Logout the user
        /// </summary>
        /// <param name="session"></param>
        public void ISLogout(string session, ConcurrentUser concurrentUser)
        {
            var tempUsers = ConcurrentUsers.ConcurrentUser;
            if (concurrentUser != null && ConcurrentUsers.ConcurrentUser.Any())
            {
                Logger.Current.Verbose(string.Format("Logged in user: " , concurrentUser.SessionKey));
                tempUsers.Remove(concurrentUser);
            }
            else
                Logger.Current.Verbose(string.Format("User with {0} not found.", session));

            ConcurrentUsers.ConcurrentUser = tempUsers;

            Logger.Current.Verbose("Logged in users count: " + ConcurrentUsers.ConcurrentUser.Count());
        }


        /// <returns>
        /// XmlElement in the following format
        ///		<configuration>
        ///			<general>
        ///				<syncEmail>1</syncEmail>
        ///				<uploadEmailAttachments>1</uploadEmailAttachments>
        ///				<syncContacts>1</syncContacts>
        ///				<syncOutlookToWebContacts>1</syncOutlookToWebContacts>
        ///				<syncWebToOutlookContacts>1</syncWebToOutlookContacts>
        ///				<syncTasks>1</syncTasks>
        ///				<syncOutlookToWebTasks>1</syncOutlookToWebTasks>
        ///				<syncWebToOutlookTasks>1</syncWebToOutlookTasks>
        ///				<syncCalendars>1</syncCalendars>
        ///				<syncOutlookToWebCalendars>1</syncOutlookToWebCalendars>
        ///				<syncWebToOutlookCalendars>1</syncWebToOutlookCalendars>
        ///				<syncItems>SyncCategorizedItems</syncItems>
        ///				<allowPeriodicSync>1</allowPeriodicSync>
        ///				<defaultSyncInterval>1</defaultSyncInterval>
        ///				<IsCategoryRequired>1</IsCategoryRequired>
        ///			</general>
        ///			<toolbar>
        ///				<showLoginMenu>1</showLoginMenu>
        ///				<loginURL>http://www.implicitsync.com</loginURL>
        ///				<showHelpMenu>1</showHelpMenu>
        ///				<helpURL>http://www.implicitsync.com</helpURL>
        ///				<showSynchronizeNowButton>1</showSynchronizeNowButton>
        ///				<showUploadEmailButton>1</showUploadEmailButton>
        ///			</toolbar>
        ///			<folders>
        ///				<syncFolders>AppFolders</syncFolders>
        ///				<rootFolderPath>\\Personal Folders</rootFolderPath>
        ///				<rootFolderName>Implicit Sync</rootFolderName>
        ///				<contactsFolderName>Contacts</contactsFolderName>
        ///				<calendarsFolderName>Calendar</calendarsFolderName>
        ///				<tasksFolderName>Tasks</tasksFolderName>
        ///			</folders>
        ///		</configuration>
        /// </returns>
        public XmlElement ISGetConfig()
        {
            Logger.Current.Verbose("In IsGetConfig()");
            var configLocation = ConfigurationManager.AppSettings["CONFIG_FILE_PATH"].ToString() + "config.xml";
            XmlTextReader reader = new XmlTextReader(configLocation);
            reader.WhitespaceHandling = WhitespaceHandling.None;
            XmlDocument xmlDoc = new XmlDocument();
            //Load the file into the XmlDocument
            xmlDoc.Load(reader);
            //Close off the connection to the file.
            reader.Close();
            //Add and item representing the document to the listbox

            //Find the root nede, and add it togather with its childeren
            XmlNode xnod = xmlDoc.DocumentElement;

            return (XmlElement)xnod;
        }


        /// <returns>
        /// XmlElement in the following format
        /// <Categories>
        ///		<refreshCategoryListInterval>0</refreshCategoryListInterval>
        ///		<noOfCategoryFields>N (N=0-10)</noOfCategoryFields>
        ///		<allowCategoryListRefresh>1</allowCategoryListRefresh> 
        ///		<allowCategoryListAdd>1</allowCategoryListAdd> 
        ///		<autoComplete>1</autoComplete>
        ///		<categoryFields>
        ///			<categoryField1 Label="Label1" Mandatory="1" Parent="0"></categoryField1>
        ///			<categoryField2 Label="Label2" Mandatory="1" Parent="1"></categoryField2>
        ///			<categoryField3 Label="Label3" Mandatory="0" Parent="1"></categoryField3>
        ///			<categoryField4 Label="Label4" Mandatory="0" Parent="0"></categoryField4>
        ///			<categoryField5 Label="Label5" Mandatory="1" Parent="0"></categoryField6>
        ///		</categoryFields>
        ///	</Categories>
        /// </returns>
        public XmlElement ISGetCategoryList()
        {
            Logger.Current.Verbose("In ISGetCategoryList()");
            var categoryLocation = ConfigurationManager.AppSettings["CONFIG_FILE_PATH"].ToString() + "Categories.xml";
            XmlTextReader reader = new XmlTextReader(categoryLocation);
            reader.WhitespaceHandling = WhitespaceHandling.None;
            XmlDocument xmlDoc = new XmlDocument();
            //Load the file into the XmlDocument
            xmlDoc.Load(reader);
            //Close off the connection to the file.
            reader.Close();
            //Add and item representing the document to the listbox

            //Find the root nede, and add it togather with its childeren
            XmlNode xnod = xmlDoc.DocumentElement;

            return (XmlElement)xnod;
        }


        /// <param name="categoryID">Category ID</param>
        /// <param name="catTimeStamp">Return categories that were added after catTimeStamp date</param>
        /// <returns>
        ///		<CategoryList>
        ///			<Parent ParentID="">
        ///				<value></value>
        ///			</Parent>
        ///			<Parent ParentID="">
        ///			</Parent>
        ///		</CategoryList>
        /// </returns>
        /// <example>
        /// {
        ///		XmlDocument doc = new XmlDocument();
        ///		doc.LoadXml( "<CategoryList><Parent ParentID=\"\"><value>test</value></Parent></CategoryList>" );
        ///		return doc.DocumentElement;
        ///	}
        /// </example>
        public XmlElement ISGetNewCategories(int categoryID, DateTime? catTimeStamp)
        {
            Logger.Current.Verbose("In ISGetNewCategories()");
            var categoryLocation = ConfigurationManager.AppSettings["CONFIG_FILE_PATH"].ToString() + "CategoryList.xml";

            XmlTextReader reader = new XmlTextReader(categoryLocation);
            reader.WhitespaceHandling = WhitespaceHandling.None;
            XmlDocument xmlDoc = new XmlDocument();
            //Load the file into the XmlDocument
            xmlDoc.Load(reader);
            //Close off the connection to the file.
            reader.Close();
            //Add and item representing the document to the listbox

            //Find the root nede, and add it togather with its childeren
            XmlNode xnod = xmlDoc.DocumentElement;

            return (XmlElement)xnod;
        }


        /// <param name="categoryID">Category ID</param>
        /// <param name="catTimeStamp">Return categories that were modified after catTimeStamp date</param>
        /// <returns>
        ///		<CategoryList>
        ///			<Parent ParentID="">
        ///				<value old=""></value>
        ///			</Parent>
        ///			...
        ///		</CategoryList>
        /// </returns>
        public XmlElement ISGetModifiedCategories(int categoryID, DateTime? catTimeStamp)
        {
            Logger.Current.Verbose("In ISGetModifiedCategories()");
            var categoryLocation = ConfigurationManager.AppSettings["CONFIG_FILE_PATH"].ToString() + "CategoryList.xml";
            XmlTextReader reader = new XmlTextReader(categoryLocation);
            reader.WhitespaceHandling = WhitespaceHandling.None;
            XmlDocument xmlDoc = new XmlDocument();
            //Load the file into the XmlDocument
            xmlDoc.Load(reader);
            //Close off the connection to the file.
            reader.Close();
            //Add and item representing the document to the listbox

            //Find the root nede, and add it togather with its childeren
            XmlNode xnod = xmlDoc.DocumentElement;

            return (XmlElement)xnod;
        }


        /// <param name="categoryID">Category ID</param>
        /// <param name="catTimeStamp">Return categories that were deleted after catTimeStamp date</param>
        /// <returns>
        ///		<CategoryList>
        ///			<Parent ParentID="">
        ///				<value></value>
        ///			</Parent>
        ///			<Parent ParentID="">
        ///			</Parent>
        ///		</CategoryList>
        /// </returns>
        public XmlElement ISGetDeletedCategories(int categoryID, DateTime? catTimeStamp)
        {
            Logger.Current.Verbose("In ISGetDeletedCategories()");
            XmlDocument xmlDoc = new XmlDocument();
            //Load the file into the XmlDocument

            //Find the root nede, and add it togather with its childeren
            XmlNode xnod = xmlDoc.DocumentElement;

            return (XmlElement)xnod;
        }


        /// <param name="xmlCategory">
        ///	category element in the following format
        ///		<Category CategoryNumber="" ParentValue="" ChildValue=""></Category>
        /// </param>
        public void ISAddToCategoryList(XmlElement xmlCategory)
        {
            Logger.Current.Verbose("In ISAddToCategoryList()");
            throw new WAException(-1, "Not implemented");
        }


        /// <param name="xmlContact">
        /// contact element in the following format
        ///		<Contact FolderPath="">
        ///			<value OutlookKey=""></value>
        ///			...
        ///		</Contact>
        /// </param>
        /// <returns>Unique key for the contact item</returns>
        /// session, contact, contactService, geoService, cacheService, searchService, countriesAndStates, dropdownValues
        public string ISAddContact(string session,ConcurrentUser concurrentUser, XmlElement xmlContact
            , Tuple<ISearchService<Contact>, IContactService, GetCountriesAndStatesResponse> services)
        {

            Logger.Current.Verbose("In ISAddContact()");
            var searchService = services.Item1;
            var contactService = services.Item2;
            var countriesAndStates = services.Item3;
            var dropdownValues = concurrentUser.DropdownValues;
            PersonViewModel personViewModel = new PersonViewModel();
            processContact(personViewModel, xmlContact, concurrentUser, dropdownValues, countriesAndStates);

            Contact Person = Mapper.Map<PersonViewModel, Person>(personViewModel);
            Person person = Person as Person;

            SearchParameters parameters = new SearchParameters() { AccountId = person.AccountID };
            SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
            IEnumerable<Contact> duplicateContacts = contactService.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person }).Contacts;
            duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts != null ? duplicateContacts.Count() : 0 : 0 };
            if (duplicateResult.TotalHits == 0)
            {
                Logger.Current.Informational("Attempting to insert Contact: " + personViewModel.FirstName);
                personViewModel.FirstContactSource = ContactSource.EmailSync;
                personViewModel.IncludeInReports = true;
                InsertPersonResponse response = contactService.InsertPerson(new InsertPersonRequest()
                {
                    PersonViewModel = personViewModel,
                    AccountId = concurrentUser.User.AccountID,
                    RequestedBy = int.Parse(concurrentUser.User.Id),
                    RoleId = concurrentUser.User.RoleID
                });
                return response.PersonViewModel.ContactID.ToString();
            }
            else
            {
                Logger.Current.Informational("Attempting to update Contact: " + personViewModel.ContactID);
                personViewModel.ContactID = duplicateResult.Results.FirstOrDefault().Id;
                personViewModel.OwnerId = duplicateResult.Results.FirstOrDefault().OwnerId ?? personViewModel.OwnerId;
                personViewModel.IncludeInReports = true;
                UpdatePersonResponse response = contactService.UpdatePerson(new UpdatePersonRequest()
                {
                    RequestedFrom = RequestOrigin.Outlook,
                    PersonViewModel = personViewModel,
                    AccountId = concurrentUser.User.AccountID,
                    RequestedBy = int.Parse(concurrentUser.User.Id),
                    RoleId = concurrentUser.User.RoleID
                });
                return response.PersonViewModel.ContactID.ToString();
            }

        }


        /// <param name="xmlAppointment">
        /// appointment element in the following format
        ///		<Appointment FolderPath="">
        ///			<value OutlookKey=""></value>
        ///			...
        ///		</Appointment>
        /// </param>
        /// <returns>Unique key for the appointment item</returns>
        public string ISAddAppointment(string session, ConcurrentUser concurrentUser, XmlElement xmlTour, Tuple<IContactService, ITourService> services)
        {

            var contactService = services.Item1;
            var tourService = services.Item2;

            var dropdownValues = concurrentUser.DropdownValues;

            TourViewModel tourViewModel = new TourViewModel();
            processTour(tourViewModel, xmlTour, concurrentUser, dropdownValues, contactService);

            InsertTourResponse response = tourService.InsertTour(new InsertTourRequest()
            {
                TourViewModel = tourViewModel,
                RequestedFrom = RequestOrigin.Outlook,
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID,
            });
            return response.TourViewModel.TourID.ToString();
        }

        /// <summary>
        /// Extracts the required values from the XMLElement passed.
        /// </summary>
        /// <param name="tourViewModel"></param>
        /// <param name="xmlTour"></param>
        /// <param name="concurrentUser"></param>
        /// <param name="dropdownValues"></param>
        /// <param name="contactService"></param>
        private void processTour(TourViewModel tourViewModel, XmlElement xmlTour, ConcurrentUser concurrentUser, IEnumerable<DropdownViewModel> dropdownValues, IContactService contactService)
        {
            Logger.Current.Informational("Request received to process tour " + tourViewModel.TourID);
            var xDoc = XDocument.Parse(xmlTour.OuterXml);
            var tourElements = (from t in xDoc.Descendants("value")
                                let outlookKeyAttribute = t.Attribute("OutlookKey").Value
                                let outlookKeyAttributeText = t.Value
                                select new { Attribute = outlookKeyAttribute, Text = outlookKeyAttributeText }
                                    ).ToDictionary(mc => mc.Attribute, mc => mc.Text);


            tourViewModel.AccountId = concurrentUser.User.AccountID;
            tourViewModel.CreatedBy = tourViewModel.TourID > 0 ? tourViewModel.CreatedBy : int.Parse(concurrentUser.User.Id);
            tourViewModel.LastUpdatedBy = int.Parse(concurrentUser.User.Id);
            var subject = GetValue("Subject", tourElements);
            var body = GetValue("Body", tourElements);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(body);
            var bodyNode = htmlDocument.DocumentNode.SelectNodes("//body");
            if (bodyNode != null && bodyNode.FirstOrDefault() != null)
            {
                body = htmlDocument.DocumentNode.SelectNodes("//body").FirstOrDefault().InnerText;
                tourViewModel.TourDetails = "Subject: " + subject + "\r\n\r\n" + "Body: " + body;
            }
            else
                tourViewModel.TourDetails = "Subject: " + subject + "\r\n\r\n" + "Body: " + "";

            IFormatProvider culture = new System.Globalization.CultureInfo("en-us");
            tourViewModel.TourDetails = tourViewModel.TourDetails.Replace("&nbsp;", "");
            var tourDate = GetValue("StartUTC", tourElements);
            tourViewModel.TourDate = DateTime.ParseExact(tourDate, "yyyyMMddHHmmss", culture);
            short defaultTourTypeID = (byte)TourType.First;
            try
            {
                var tourDropDown = dropdownValues.Where(d => d.DropdownID == 8).Select(d => d.DropdownValuesList).FirstOrDefault();
                if(tourDropDown != null)
                    defaultTourTypeID = tourDropDown.Where(t=>t.IsDefault == true && t.IsDeleted == false).Select(dv => dv.DropdownValueID).FirstOrDefault();
            }
            catch
            {
                Logger.Current.Informational("Default tour type not found for account: " + tourViewModel.AccountId);
            }
            tourViewModel.TourType = tourViewModel.TourID == 0 ? defaultTourTypeID : tourViewModel.TourType;

            if (tourViewModel.TourID == 0)
                tourViewModel.CreatedOn = tourViewModel.TourDate;
            tourViewModel.LastUpdatedOn = DateTime.UtcNow;


            var reminderSet = GetValue("ReminderSet", tourElements);
            if (reminderSet == bool.TrueString.ToString())
            {
                var reminderMinutesBeforeStart = GetValue("ReminderMinutesBeforeStart", tourElements);
                tourViewModel.ReminderDate = tourViewModel.TourDate.AddMinutes(-int.Parse(reminderMinutesBeforeStart));
                var currentDateTime = DateTime.Now.ToUniversalTime().AddMinutes(-5);
                if (tourViewModel.ReminderDate < currentDateTime)
                {
                    tourViewModel.ReminderDate = currentDateTime.AddMinutes(2);
                }
                var selectedReminderTypes = new List<ReminderType>();
                selectedReminderTypes.Add(ReminderType.PopUp);
                tourViewModel.SelectedReminderTypes = selectedReminderTypes;
            }



            tourViewModel.Community = GetValue("Location", tourElements);
            if (!string.IsNullOrEmpty(tourViewModel.Community))
            {
                var communities = dropdownValues.Single(c => c.DropdownID == (byte)DropdownFieldTypes.Community);
                var tourCommunity = communities.DropdownValuesList.FirstOrDefault(c => c.DropdownValue.ToLower() == tourViewModel.Community.ToLower());
                if (tourCommunity == null)
                    throw new System.Exception("Community: " + tourViewModel.Community + " not found");
                tourViewModel.CommunityID = tourCommunity.DropdownValueID;
            }

            var associatedContacts = GetValue("LinkedContacts", tourElements) + "," + GetValue("RequiredAttendees", tourElements)
                + "," + GetValue("OptionalAttendees", tourElements) + "," + GetValue("Resources", tourElements);

            if (!string.IsNullOrEmpty(associatedContacts))
            {
                IList<string> contactEmails = associatedContacts.Split(new char[] { ',', ';' }).ToList();
                contactEmails.Remove("");
                contactEmails.Remove(GetValue("Organizer", tourElements));
                if (contactEmails != null && contactEmails.Any())
                {
                    FindContactsByPrimaryEmailsRequest request = new FindContactsByPrimaryEmailsRequest()
                    {
                        Emails = contactEmails,
                        AccountId = concurrentUser.User.AccountID,
                        RequestedBy = int.Parse(concurrentUser.User.Id),
                        RoleId = concurrentUser.User.RoleID
                    };
                    var contactIds = contactService.FindContactsByPrimaryEmails(request).ContactIDs;
                    IList<ContactEntry> contactEntries = new List<ContactEntry>();
                    foreach (var contactId in contactIds)
                    {
                        contactEntries.Add(new ContactEntry() { Id = contactId });
                    };
                    tourViewModel.Contacts = (contactEntries != null && contactEntries.Any()) ? contactEntries : tourViewModel.Contacts;
                }
            }

            tourViewModel.OwnerIds = tourViewModel.OwnerIds.IsAny() ? tourViewModel.OwnerIds : new List<int>() { tourViewModel.CreatedBy };
        }

        private string GetValue(string key, Dictionary<string, string> keyValuePair)
        {
            if (keyValuePair.ContainsKey(key))
                return Uri.UnescapeDataString(keyValuePair[key].ToString());
            return string.Empty;
        }


        /// <param name="xmlTask">
        /// task element in the following format
        ///		<Task FolderPath="">
        ///			<value OutlookKey=""></value>
        ///			...
        ///		</Task>
        /// </param>
        /// <returns>Unique key for the task item</returns>
        public string ISAddTask(string session,ConcurrentUser concurrentUser ,XmlElement xmlTask,
            Tuple<IActionService, IContactService, IUserService> services)
        {

            var actionService = services.Item1;
            var contactService = services.Item2;
            var userService = services.Item3;

            ActionViewModel actionViewModel = new ActionViewModel();
            actionViewModel = processTask(actionViewModel, xmlTask, concurrentUser, contactService, userService);
            InsertActionResponse response = actionService.InsertAction(new InsertActionRequest()
            {
                ActionViewModel = actionViewModel,
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID,
                RequestedFrom = RequestOrigin.Outlook
            });
            return response.ActionViewModel.ActionId.ToString();
        }


        ActionViewModel processTask(ActionViewModel actionViewModel, XmlElement xmlTask, ConcurrentUser concurrentUser
            , IContactService contactService, IUserService userService)
        {
            var dropdownValues = concurrentUser.DropdownValues;

            var actionTypeDropdown = dropdownValues.Where(c => c.DropdownID == (byte)DropdownFieldTypes.ActionType).FirstOrDefault();
            var defaultActionType = actionTypeDropdown.DropdownValuesList.Where(c => c.IsDefault == true).Select(c => c.DropdownValueID).FirstOrDefault();
            var currentUser = concurrentUser.User;
            Logger.Current.Informational("Request received to process task " + actionViewModel.ActionId);
            actionViewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            actionViewModel.CreatedBy = actionViewModel.ActionId > 0 ? actionViewModel.CreatedBy : int.Parse(concurrentUser.User.Id);
            actionViewModel.LastUpdatedBy = int.Parse(concurrentUser.User.Id);
            actionViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            actionViewModel.SelectedReminderTypes = new List<ReminderType>();
            actionViewModel.TagsList = new List<TagViewModel>();
            actionViewModel.AccountId = concurrentUser.User.AccountID;
            actionViewModel.ActionType = actionViewModel.ActionId == 0 ? defaultActionType : actionViewModel.ActionType;
            var xDoc = XDocument.Parse(xmlTask.OuterXml);
            var taskElements = (from t in xDoc.Descendants("value")
                                let outlookKeyAttribute = t.Attribute("OutlookKey").Value
                                let outlookKeyAttributeText = t.Value
                                select new { Attribute = outlookKeyAttribute, Text = outlookKeyAttributeText }
                                    ).ToDictionary(te => te.Attribute, te => te.Text);

            var subject = GetValue("Subject", taskElements).Trim();
            var body = GetValue("Body", taskElements).Trim();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(body.Replace("&nbsp;", ""));
            if(htmlDocument.DocumentNode.SelectNodes(("//body")).IsAny())
                body = htmlDocument.DocumentNode.SelectNodes(("//body")).FirstOrDefault().InnerText;
            var actionDetails = "Subject: " + subject + "\r\n\r\n" + (!string.IsNullOrEmpty(body) ? ("Body: " + body) : "");

            actionViewModel.ActionMessage = actionDetails;

            var isCompleted = GetValue("Status", taskElements);
            actionViewModel.IsCompleted = isCompleted == "2" ? true : false;

            //string reminderDate = GetValue("DueDate", taskElements);
            //var userTimeZone = userService.GetUserTimeZoneByUserID(new GetUserTimeZoneRequest() { RequestedBy = int.Parse(currentUser.Id), AccountId = currentUser.AccountID }).TimeZone;
            //var timeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);

            IFormatProvider culture = new System.Globalization.CultureInfo("en-us");

            var startDate = GetValue("StartDate", taskElements);
            //actionViewModel.ActionStartTime = actionViewModel.ActionStartTime ?? (string.IsNullOrEmpty(startDate) ? DateTime.Now : DateTime.ParseExact(startDate, "yyyyMMdd", culture));
            if (!string.IsNullOrEmpty(startDate))
            {
                var outlookDate = DateTime.ParseExact(startDate, "yyyyMMdd", culture);
                if (actionViewModel.ActionStartTime != null && actionViewModel.ActionStartTime.HasValue)
                {
                    var actionDate = actionViewModel.ActionStartTime.Value;
                    actionViewModel.ActionStartTime = new DateTime(outlookDate.Year, outlookDate.Month, outlookDate.Day, actionDate.Hour, actionDate.Minute, actionDate.Second);
                }
                else
                    actionViewModel.ActionStartTime = outlookDate;

            }
            actionViewModel.ActionStartTime = actionViewModel.ActionStartTime ?? DateTime.Now;
            var dueDate = GetValue("StartDate", taskElements);
            //actionViewModel.ActionEndTime = actionViewModel.ActionEndTime ?? (string.IsNullOrEmpty(dueDate) ? DateTime.Now : DateTime.ParseExact(dueDate, "yyyyMMdd", culture));
            if (!string.IsNullOrEmpty(dueDate))
            {
                var outlookDate = DateTime.ParseExact(dueDate, "yyyyMMdd", culture);
                if (actionViewModel.ActionEndTime != null && actionViewModel.ActionEndTime.HasValue)
                {
                    var actionDate = actionViewModel.ActionEndTime.Value;
                    actionViewModel.ActionEndTime = new DateTime(outlookDate.Year, outlookDate.Month, outlookDate.Day, actionDate.Hour, actionDate.Minute, actionDate.Second);
                }
                else
                    actionViewModel.ActionEndTime = outlookDate;
            }
            actionViewModel.ActionEndTime = actionViewModel.ActionEndTime ?? DateTime.Now;
            actionViewModel.ActionDate = actionViewModel.ActionStartTime;
            var contactsFromExchange = GetValue("Contacts", taskElements).Trim();
            IEnumerable<string> contactList = contactsFromExchange.Split(';').ToList();

            FindContactsOfUserByFirstAndLastNameResponse actionContactIds
                = contactService.FindContactsOfUserByFirstAndLastName(new FindContactsOfUserByFirstAndLastNameRequest()
                {
                    ContactNames = contactList,
                    RequestedBy = int.Parse(concurrentUser.User.Id),
                    AccountId = concurrentUser.User.AccountID
                });

            var associatedContacts = GetValue("LinkedContacts", taskElements) + "," + GetValue("Contacts", taskElements);

            if (!string.IsNullOrEmpty(associatedContacts))
            {
                IList<string> contactEmails = associatedContacts.Split(',').ToList();
                contactEmails.Remove("");
                if (contactEmails != null && contactEmails.Any())
                {
                    FindContactsByPrimaryEmailsRequest request = new FindContactsByPrimaryEmailsRequest()
                    {
                        Emails = contactEmails,
                        AccountId = concurrentUser.User.AccountID,
                        RequestedBy = int.Parse(concurrentUser.User.Id),
                        RoleId = concurrentUser.User.RoleID
                    };
                    var contactIds = contactService.FindContactsByPrimaryEmails(request).ContactIDs;
                    var finalContactIds = contactIds.ToList();
                    finalContactIds.AddRange(actionContactIds.ContactIds);
                    // finalContactIds = finalContactIds.Distinct();
                    IList<ContactEntry> contactEntries = new List<ContactEntry>();
                    foreach (var contactId in finalContactIds.Distinct())
                    {
                        contactEntries.Add(new ContactEntry() { Id = contactId });
                    };
                    actionViewModel.Contacts = (contactEntries != null && contactEntries.Any()) ? contactEntries : actionViewModel.Contacts;
                }
            }


            actionViewModel.OutlookSync = new CRMOutlookSyncViewModel()
            {
                LastSyncDate = DateTime.Now.ToUniversalTime(),
                LastSyncedBy = int.Parse(concurrentUser.User.Id),
                OutlookKey = GetValue("EntryID", taskElements),
                SyncStatus = OutlookSyncStatus.InSync
            };
            return actionViewModel;

        }


        /// <param name="key">Unique key of the contact item</param>
        /// <param name="xmlContact">
        /// contact element in the following format
        ///		<Contact FolderPath="">
        ///			<value OutlookKey=""></value>
        ///			...
        ///		</Contact>
        /// </param>
        /// <returns>true on success, false or exception on error</returns>
        public bool ISModifyContact(string session, ConcurrentUser concurrentUser, string key, XmlElement xmlContact
            , Tuple<IContactService, GetCountriesAndStatesResponse> services)
        {
            var contactService = services.Item1;
            var countriesAndStates = services.Item2;
            var dropdownValues = concurrentUser.DropdownValues;


            GetPersonRequest getPersonRequest = new GetPersonRequest(int.Parse(key))
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID
            };
            PersonViewModel personViewModel = contactService.GetPerson(getPersonRequest).PersonViewModel;
            personViewModel.IncludeInReports = true;
            if (personViewModel != null)
            {
                processContact(personViewModel, xmlContact, concurrentUser, dropdownValues, countriesAndStates);
                UpdatePersonResponse response = contactService.UpdatePerson(new UpdatePersonRequest()
                {
                    PersonViewModel = personViewModel,
                    AccountId = concurrentUser.User.AccountID,
                    RequestedBy = int.Parse(concurrentUser.User.Id),
                    RoleId = concurrentUser.User.RoleID,
                    RequestedFrom = RequestOrigin.Outlook
                });
                return response.PersonViewModel != null ? true : false;
            }
            else
            {
                personViewModel.FirstContactSource = ContactSource.EmailSync;
                InsertPersonResponse response = contactService.InsertPerson(new InsertPersonRequest()
                {
                    PersonViewModel = personViewModel,
                    AccountId = concurrentUser.User.AccountID,
                    RequestedBy = int.Parse(concurrentUser.User.Id),
                    RoleId = concurrentUser.User.RoleID,
                    RequestedFrom = RequestOrigin.Outlook
                });
                return response.PersonViewModel != null ? true : false;
            }
        }


        private void processContact(PersonViewModel personViewModel, XmlElement xmlContact, ConcurrentUser concurrentUser, IEnumerable<DropdownViewModel> dropdownValues, GetCountriesAndStatesResponse countriesAndStates)
        {
            Logger.Current.Informational("Request received to process contact " + personViewModel.ContactID);

            var xDoc = XDocument.Parse(xmlContact.OuterXml);
            var contactElements = (from t in xDoc.Descendants("value")
                                   let outlookKeyAttribute = t.Attribute("OutlookKey").Value
                                   let outlookKeyAttributeText = t.Value
                                   select new { Attribute = outlookKeyAttribute, Text = outlookKeyAttributeText }
                                       ).ToDictionary(ce => ce.Attribute, ce => ce.Text);

            personViewModel.FirstName = GetValue("FirstName", contactElements);
            personViewModel.LastName = GetValue("LastName", contactElements);
            personViewModel.Title = GetValue("Title", contactElements);
            personViewModel.CompanyName = GetValue("CompanyName", contactElements);
            personViewModel.ContactType = ContactType.Person.ToString();
            if (personViewModel.SocialMediaUrls == null || !personViewModel.SocialMediaUrls.Any())
            {
                personViewModel.SocialMediaUrls = new List<Url>();
                personViewModel.SocialMediaUrls.Add(new Url() { MediaType = "Website" });
            }
            var websiteUrl = personViewModel.SocialMediaUrls.Where(c => c.MediaType == "Website").FirstOrDefault();
            var outlookWebsiteUrl = GetValue("WebPage", contactElements);
            websiteUrl.URL = outlookWebsiteUrl;


            personViewModel.Emails = personViewModel.Emails ?? new List<Email>();
            

            if (personViewModel.ContactID == 0)
            {
                personViewModel.CreatedBy = int.Parse(concurrentUser.User.Id);
                personViewModel.OwnerId = int.Parse(concurrentUser.User.Id);
                personViewModel.AccountID = concurrentUser.User.AccountID;
                personViewModel.ReferenceId = Guid.NewGuid();
                personViewModel.CreatedOn = DateTime.Now.ToUniversalTime();
                var leadSourceDropdown = concurrentUser.DropdownValues
                .Where(c => c.DropdownID == (byte)DropdownFieldTypes.LeadSources)
                .Select(c => c.DropdownValuesList.Where(d => d.IsDefault == true))
                .First();
                var defaultLeadSource = leadSourceDropdown.Where(c => c.IsDefault).First();

                var contactLeadSource = new List<DropdownValueViewModel>();
                contactLeadSource.Add(defaultLeadSource);
                personViewModel.SelectedLeadSource = contactLeadSource;
            }


            var lifecycleStageDropDown = dropdownValues.Single(c => c.DropdownID == (byte)DropdownFieldTypes.LifeCycle);
            var defaultLifecycleStage = lifecycleStageDropDown.DropdownValuesList.Single(e => e.IsDefault == true);
            if (personViewModel.ContactID == 0)
                personViewModel.LifecycleStage = defaultLifecycleStage.DropdownValueID;
            var addressDropdown = dropdownValues.Single(c => c.DropdownID == (byte)DropdownFieldTypes.AddressType);
            var phoneDropdown = dropdownValues.Single(c => c.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType);

            syncContactAddresses(personViewModel, contactElements, addressDropdown, countriesAndStates);
            syncContactEmails(personViewModel, contactElements, concurrentUser);
            syncContactPhones(personViewModel, contactElements, phoneDropdown, concurrentUser);

            personViewModel.LastUpdatedBy = int.Parse(concurrentUser.User.Id);
            personViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();

            personViewModel.OutlookSync = new CRMOutlookSyncViewModel()
            {
                LastSyncDate = DateTime.Now.ToUniversalTime(),
                LastSyncedBy = int.Parse(concurrentUser.User.Id),
                OutlookKey = GetValue("EntryID", contactElements),
                SyncStatus = OutlookSyncStatus.InSync
            };
        }


        private void syncContactPhones(PersonViewModel personViewModel, Dictionary<string, string> contactElements, DropdownViewModel phoneDropdown, ConcurrentUser concurrentUser)
        {
            personViewModel.Phones = personViewModel.Phones ?? new List<Phone>();
            var phoneTypes = contactElements.Where(d => d.Key.Contains("TelephoneNumber"))
                .Select(d => d.Key.Substring(0, d.Key.IndexOf("TelephoneNumber"))).ToList();
            foreach (string phoneType in phoneTypes.Distinct())
            {
                var thisPhoneType = (phoneType == "Work" || phoneType == "Business") ? "Work" : phoneType;
                var dropdown = phoneDropdown.DropdownValuesList.Where(a => a.DropdownValue == thisPhoneType).FirstOrDefault();
                if (dropdown != null)
                {
                    var phoneTypeName = dropdown.DropdownValue;

                    var existingPhone = personViewModel.Phones.Where(a => a.PhoneTypeName == phoneTypeName).FirstOrDefault();
                    var phoneNumber = Uri.UnescapeDataString(contactElements.Single(c => c.Key == phoneType + "TelephoneNumber").Value);
                    if (existingPhone == null && !string.IsNullOrEmpty(phoneNumber))
                    {
                        var newPhoneNumber = new Phone()
                        {
                            PhoneTypeName = phoneTypeName,
                            PhoneType = dropdown.DropdownValueID
                        };
                        personViewModel.Phones.Add(newPhoneNumber);
                    }
                    existingPhone = personViewModel.Phones.Where(a => a.PhoneTypeName == phoneTypeName).FirstOrDefault();
                    if (existingPhone != null)
                    {
                        existingPhone.Number = string.IsNullOrEmpty(phoneNumber) ? existingPhone.Number : phoneNumber;
                        existingPhone.IsPrimary = personViewModel.Phones.Count() == 1 ? true : existingPhone.IsPrimary;
                        existingPhone.AccountID = concurrentUser.User.AccountID;
                        existingPhone.IsDeleted = false;
                    }
                }
            }
            var contactPhones = personViewModel.Phones.ToList();

            if (contactPhones != null && contactPhones.Any())
            {
                var accountDefault = phoneDropdown.DropdownValuesList.Where(c => c.IsDefault == true).FirstOrDefault();
                var defaultPhone = contactPhones.Where(c => c.IsPrimary == true).FirstOrDefault() ?? contactPhones.Where(c => c.PhoneType == accountDefault.DropdownValueTypeID).FirstOrDefault();
                if (defaultPhone == null)
                {
                    defaultPhone = contactPhones.FirstOrDefault();
                    defaultPhone.IsPrimary = true;
                }
                if (contactPhones.Where(c => c.IsPrimary == true).Count() > 1)  // Making sure the contact has only one primary phone number
                {
                    contactPhones.ForEach(c => { c.IsPrimary = false; });
                    defaultPhone.IsPrimary = true;
                }
                
            }

            foreach (Phone phone in contactPhones)
            {
                if (!phoneTypes.Contains(phone.PhoneTypeName))
                    personViewModel.Phones.Remove(phone);
            };
        }


        private void syncContactEmails(PersonViewModel personViewModel, Dictionary<string, string> contactElements, ConcurrentUser concurrentUser)
        {
            Logger.Current.Verbose("Processing Contact Emails. ContactId: " + personViewModel.ContactID);
            for (int i = 1; i <= 3; i++)
            {
                var emailAddress = GetValue("Email" + i + "Address", contactElements);

                var contactPrimaryEmail = personViewModel.Emails.Where(e => e.IsPrimary == true).FirstOrDefault();
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    var existingEmail = personViewModel.Emails.Where(e => e.EmailId == emailAddress).FirstOrDefault();
                    if (existingEmail == null)
                    {
                        personViewModel.Emails.Add(new Email()
                        {
                            EmailId = emailAddress,
                            AccountID = concurrentUser.User.AccountID,
                            ContactID = personViewModel.ContactID,
                            EmailStatusValue = EmailStatus.NotVerified,
                            IsDeleted = false
                        });
                    }
                }
                if (contactPrimaryEmail == null && personViewModel.Emails.Count() > 0)
                    personViewModel.Emails.FirstOrDefault().IsPrimary = true;
                    
            }
        }


        private void syncContactAddresses(PersonViewModel personViewModel, Dictionary<string, string> contactElements, DropdownViewModel addressDropdown
            , GetCountriesAndStatesResponse countriesAndStates)
        {
            personViewModel.Addresses = personViewModel.Addresses ?? new List<AddressViewModel>();
            var addressTypes = contactElements.Where(d => d.Key.Contains("AddressCountry"))
                .Select(d => d.Key.Substring(0, d.Key.IndexOf("AddressCountry"))).ToList();
            foreach (string addressType in addressTypes.Distinct())
            {
                var dropdown = addressDropdown.DropdownValuesList.Where(a => a.DropdownValue == addressType).FirstOrDefault();
                if (dropdown != null)
                {
                    var country = GetValue(addressType + "AddressCountry", contactElements);
                    country = country == "United States of America" ? "United States" : country;
                    var countryCode = countriesAndStates.Countries.Where(c => c.Name == country).FirstOrDefault();

                    var state = GetValue(addressType + "AddressState", contactElements);
                    var stateCode = countriesAndStates.States.Where(c => c.Name == state).FirstOrDefault();
                    var zipCode = GetValue(addressType + "AddressPostalCode", contactElements);
                    var address = personViewModel.Addresses.Where(a => a.AddressType == dropdown.DropdownValue).FirstOrDefault();

                    if (zipCode != null || (stateCode != null && countryCode != null))
                    {
                        if (address == null)
                        {
                            personViewModel.Addresses.Add(new AddressViewModel()
                            {
                                AddressType = dropdown.DropdownValue,
                                AddressTypeID = dropdown.DropdownValueID
                            });
                            address = personViewModel.Addresses.Where(a => a.AddressType == dropdown.DropdownValue).FirstOrDefault();
                        }
                        address.ZipCode = zipCode;

                        address.AddressLine1 = GetValue(addressType + "AddressStreet", contactElements);
                        var indexOfLineDelimiter1 = address.AddressLine1.IndexOf("\r\n");
                        var indexOfLineDelimiter2 = address.AddressLine1.IndexOf("\n");

                        if (indexOfLineDelimiter1 != -1)
                        {
                            address.AddressLine2 = address.AddressLine1.Replace("\r\n", "").Substring(indexOfLineDelimiter1);
                            address.AddressLine1 = address.AddressLine1.Substring(0, indexOfLineDelimiter1);
                        }
                        else if (indexOfLineDelimiter2 != -1)
                        {
                            address.AddressLine2 = address.AddressLine1.Replace("\n", "").Substring(indexOfLineDelimiter2);
                            address.AddressLine1 = address.AddressLine1.Substring(0, indexOfLineDelimiter2);
                        }
                        address.Country = new Country() { Code = countryCode != null ? countryCode.Code : "" };
                        address.State = new State() { Code = stateCode != null ? stateCode.Code : "" };
                        address.City = GetValue(addressType + "AddressCity", contactElements);
                        address.IsDefault = personViewModel.Addresses.Count() == 1 ? true : address.IsDefault;

                        Logger.Current.Informational("Added " + dropdown.DropdownValue + " address for contact " + personViewModel.ContactID);
                    }
                }
            }
            var contactAddresses = personViewModel.Addresses.ToList();

            if (contactAddresses != null && contactAddresses.Any())
            {
                var defaultAddress = contactAddresses.Where(c => c.IsDefault == true).FirstOrDefault();
                if (defaultAddress == null)
                {
                    defaultAddress = contactAddresses.FirstOrDefault();
                    defaultAddress.IsDefault = true;
                }
            }

            foreach (AddressViewModel address in contactAddresses)
            {
                if (!addressTypes.Contains(address.AddressType))
                    personViewModel.Addresses.Remove(address);
            };
        }


        /// <param name="key">Unique key of the appointment item</param>
        /// <param name="xmlAppointment">
        /// appointment element in the following format
        ///		<Appointment FolderPath="">
        ///			<value OutlookKey=""></value>
        ///			...
        ///		</Appointment>
        /// </param>
        /// <returns>true on success, false or exception on error</returns>
        public bool ISModifyAppointment(string session, ConcurrentUser concurrentUser, string key, XmlElement xmlTour
            , Tuple<ITourService, IContactService> services)
        {
            var tourService = services.Item1;
            var contactService = services.Item2;
            var dropdownValues = concurrentUser.DropdownValues;

            TourViewModel tourViewModel = tourService.GetTour(int.Parse(key)).TourViewModel;
            if (tourViewModel != null)
            {
                processTour(tourViewModel, xmlTour, concurrentUser, dropdownValues, contactService);
                UpdateTourResponse response = tourService.UpdateTour(new UpdateTourRequest()
                {
                    TourViewModel = tourViewModel,
                    RequestedFrom = RequestOrigin.Outlook,
                    AccountId = concurrentUser.User.AccountID,
                    RequestedBy = int.Parse(concurrentUser.User.Id),
                    RoleId = concurrentUser.User.RoleID,

                });
                return response.TourViewModel != null ? true : false;
            }
            else
            {
                InsertTourResponse response = tourService.InsertTour(new InsertTourRequest()
                {
                    TourViewModel = tourViewModel,
                    RequestedFrom = RequestOrigin.Outlook,
                    AccountId = concurrentUser.User.AccountID,
                    RequestedBy = int.Parse(concurrentUser.User.Id),
                    RoleId = concurrentUser.User.RoleID
                });
                return response.TourViewModel != null ? true : false;
            }
        }


        /// <param name="key">Unique key of the task item</param>
        /// <param name="xmlTask">
        /// task element in the following format
        ///		<Task FolderPath="">
        ///			<value OutlookKey=""></value>
        ///			...
        ///		</Task>
        /// </param>
        /// <returns>true on success, false or exception on error</returns>
        public bool ISModifyTask(string session,ConcurrentUser concurrentUser, string key, XmlElement xmlTask, Tuple<IActionService, IContactService, IUserService> services)
        {

            var actionService = services.Item1;
            var contactService = services.Item2;
            var userService = services.Item3;
            ActionViewModel actionViewModel = actionService.GetAction(new GetActionRequest()
            {
                Id = int.Parse(key),
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID
            }).ActionViewModel;

            if (actionViewModel != null)
            {
                actionViewModel = processTask(actionViewModel, xmlTask, concurrentUser, contactService, userService);
                UpdateActionResponse response = actionService.UpdateAction(new UpdateActionRequest()
                {
                    ActionViewModel = actionViewModel,
                    AccountId = concurrentUser.User.AccountID,
                    RequestedBy = int.Parse(concurrentUser.User.Id),
                    RoleId = concurrentUser.User.RoleID,
                    RequestedFrom = RequestOrigin.Outlook
                });
                return response.ActionViewModel != null ? true : false;
            }
            else
            {
                InsertActionResponse response = actionService.InsertAction(new InsertActionRequest()
                {
                    ActionViewModel = actionViewModel,
                    AccountId = concurrentUser.User.AccountID,
                    RequestedBy = int.Parse(concurrentUser.User.Id),
                    RoleId = concurrentUser.User.RoleID,
                    RequestedFrom = RequestOrigin.Outlook
                });
                return response.ActionViewModel != null ? true : false;
            }
        }


        /// <param name="key">Unique key of the contact item</param>
        /// <returns>true on success, false or exception on error</returns>
        public bool ISRemoveContact(string session, ConcurrentUser concurrentUser, string key, Tuple<IContactService> services)
        {
            var contactService = services.Item1;
            DeactivateContactResponse response = contactService.Deactivate(new DeactivateContactRequest(int.Parse(key))
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID
            });
            if (response.Exception == null)
                return true;
            else
                return false;
        }


        /// <param name="key">Unique key of the appointment item</param>
        /// <returns>true on success, false or exception on error</returns>
        public bool ISRemoveAppointment(string session, ConcurrentUser concurrentUser, string key, Tuple<ITourService> services)
        {
            var tourService = services.Item1;

            DeleteTourResponse response = tourService.DeleteTour(int.Parse(key), int.Parse(concurrentUser.User.Id), 0);
            if (response.Exception == null)
                return true;
            else
                return false;
        }


        /// <param name="key">Unique key of the task item</param>
        /// <returns>true on success, false or exception on error</returns>
        public bool ISRemoveTask(string session,ConcurrentUser concurrentUser, string key, Tuple<IActionService> services)
        {
            var actionService = services.Item1;
            DeleteActionResponse response = actionService.DeleteAction(new DeleteActionRequest()
            {
                ActionId = int.Parse(key),
                DeleteForAll = true,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                AccountId = concurrentUser.User.AccountID,
                RoleId = concurrentUser.User.RoleID
            });
            if (response.Exception == null)
                return true;
            else
                return false;
        }


        /// <param name="timeStamp">Return contacts that were added after timeStamp date</param>
        /// <returns>
        /// contact item elements in the following format
        ///		<Contacts>
        ///			<Contact Key="" FolderPath="">
        ///				<value OutlookKey=""></value>
        ///				...
        ///			</Contact>
        ///			...
        ///		</Contacts>
        /// </returns>
        public XmlElement ISGetNewContacts(string session, ConcurrentUser concurrentUser, DateTime? timeStamp, int? maxNumRecords, bool firstSync
           , Tuple<IContactService, GetCountriesAndStatesResponse> services)
        {
            Logger.Current.Verbose("WASyncHandler/ISGetNewContacts");
            var contactService = services.Item1;
            var dropdownValues = concurrentUser.DropdownValues;
            GetContactsToSyncRequest request = new GetContactsToSyncRequest()
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                MaxNumRecords = maxNumRecords,
                TimeStamp = timeStamp,
                FirstSync = firstSync,
                RoleId = concurrentUser.User.RoleID,
                OperationType = CRUDOperationType.Create
            };
            GetContactsToSyncResponse response = contactService.GetContactsToSync(request);
            XmlDocument doc = createContactXMLElement(response.CRMOutlookSyncMappings, dropdownValues, contactService, concurrentUser.User);

            return (XmlElement)doc.FirstChild;
        }


        /// <param name="xmlContacts"> contact item elements in the following format
        ///		<Contacts>
        ///			<Contact Key="">
        ///				<value OutlookKey="EntryID"></value>
        ///			</Contact>
        ///			...
        ///		</Contacts>
        /// </param>
        public void ISGetNewContactsResult(XmlElement xmlContacts, Tuple<IContactService> services)
        {
            var contactService = services.Item1;
            UpdateSyncedEntitiesRequest request = new UpdateSyncedEntitiesRequest();

            var xDoc = XDocument.Parse(xmlContacts.OuterXml);
            var contactElements = (from t in xDoc.Descendants("Contact")
                                   let outlookKeyAttribute = t.Attribute("Key").Value
                                   select new { Attribute = int.Parse(outlookKeyAttribute), Text = "" }
                                       ).ToDictionary(ce => ce.Attribute, ce => ce.Text);

            request.SyncedEntities = contactElements;
            UpdateSyncedEntitiesResponse response = contactService.UpdateSyncedEntities(request);
        }


        /// <param name="timeStamp">Return appointments that were added after timeStamp date</param>
        /// <returns>
        /// appointment item elements in the following format
        ///		<Appointments>
        ///			<Appointment Key="" FolderPath="">
        ///				<value OutlookKey=""></value>
        ///				...
        ///			</Appointment>
        ///			...
        ///		</Appointments>
        /// </returns>
        public XmlElement ISGetNewAppointments(string session,ConcurrentUser concurrentUser, DateTime? timeStamp, int? maxNumRecords, bool firstSync, Tuple<ITourService> services)
        {
            Logger.Current.Verbose("Fetching New Appointments");
            var tourService = services.Item1;

            var dropdownValues = concurrentUser.DropdownValues;

            GetToursToSyncRequest request = new GetToursToSyncRequest()
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID,
                MaxNumRecords = maxNumRecords,
                TimeStamp = timeStamp,
                FirstSync = firstSync,
                OperationType = CRUDOperationType.Update
            };
            GetToursToSyncResponse response = tourService.GetToursToSync(request);
            XmlDocument doc = createAppoinmentXMLElement(response, dropdownValues);

            return (XmlElement)doc.FirstChild;
        }


        /// <param name="xmlAppointments"> appointment item elements in the following format
        ///		<Appointments>
        ///			<Appointment Key="">
        ///				<value OutlookKey="EntryID"></value>
        ///			</Appointment>
        ///			...
        ///		</Appointments>
        /// </param>
        public void ISGetNewAppointmentsResult(XmlElement xmlAppointments, Tuple<IContactService> services)
        {
            var contactService = services.Item1;

            var xDoc = XDocument.Parse(xmlAppointments.OuterXml);
            var dictionarytourElements = new Dictionary<int, string>();
            var tourElements = (from t in xDoc.Descendants("Appointment")
                                          let outlookKeyAttribute = t.Attribute("Key").Value
                                          select new { Attribute = int.Parse(outlookKeyAttribute), Text = "" }
                                       );
            foreach(var element in tourElements)
            {
                if(!dictionarytourElements.ContainsKey(element.Attribute))
                {
                    dictionarytourElements.Add(element.Attribute, element.Text);
                }
                else
                {
                    var ex = new System.Exception();
                    Logger.Current.Error("Ignore: Eror while tour processing",ex);
                    ex.Data.Clear();
                    ex.Data.Add("Tour XML", xmlAppointments);
                }
            }

            UpdateSyncedEntitiesRequest request = new UpdateSyncedEntitiesRequest();
            request.SyncedEntities = dictionarytourElements;
            UpdateSyncedEntitiesResponse response = contactService.UpdateSyncedEntities(request);
        }


        /// <param name="timeStamp">Return tasks that were added after timeStamp date</param>
        /// <returns>
        /// task item elements in the following format
        ///		<Tasks>
        ///			<Task Key="" FolderPath="">
        ///				<value OutlookKey=""></value>
        ///				...
        ///			</Task>
        ///			...
        ///		</Tasks>
        /// </returns>
        /// 
        public XmlElement ISGetNewTasks(string session, ConcurrentUser concurrentUser, DateTime? timeStamp, int? maxNumRecords, bool firstSync, Tuple<IActionService> services)
        {
            var actionService = services.Item1;

            var dropdownValues = concurrentUser.DropdownValues;


            GetTasksToSyncRequest request = new GetTasksToSyncRequest()
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID,
                MaxNumRecords = maxNumRecords,
                TimeStamp = timeStamp,
                FirstSync = firstSync
            };
            GetTasksToSyncResponse response = actionService.GetNewActionsToSync(request);
            XmlDocument doc = createTaskXMLElement(response.CRMOutlookSyncMappings, dropdownValues);

            return (XmlElement)doc.FirstChild;
        }


        XmlDocument createContactXMLElement(IEnumerable<CRMOutlookSyncViewModel> contacts, IEnumerable<DropdownViewModel> dropdownValues, IContactService contactService, User currentUser)
        {
            XDocument doc = new XDocument();
            var syncCategory = System.Configuration.ConfigurationManager.AppSettings["SYNC_CATEGORY"];

            doc.Add(new XElement("Contacts"));
            foreach (CRMOutlookSyncViewModel contact in contacts)
            {
                PersonViewModel person = contact.Contact;
                XElement xContactElement = new XElement("Contact", new XAttribute("Key", contact.EntityID));
                xContactElement.Add(new XAttribute("FolderPath", "Contacts"));
                XElement xEntryID = new XElement("value", new XAttribute("OutlookKey", "EntryID"));
                xEntryID.SetValue(contact.OutlookKey ?? ""); xContactElement.Add(xEntryID);

                XElement xFirstName = new XElement("value", new XAttribute("OutlookKey", "FirstName"));
                xFirstName.SetValue(contact.Contact.FirstName ?? ""); xContactElement.Add(xFirstName);

                XElement xLastName = new XElement("value", new XAttribute("OutlookKey", "LastName"));
                xLastName.SetValue(contact.Contact.LastName ?? ""); xContactElement.Add(xLastName);

                var addressDropdown = dropdownValues.Where(d => d.DropdownID == (short)DropdownFieldTypes.AddressType).FirstOrDefault();

                //var addressTypes = person.Addresses.Select(a => a.AddressTypeID).ToList();
                //var addressTypeNames = addressDropdown.DropdownValuesList.Where(a => addressTypes.Contains(a.DropdownValueID)).Select(c => c.DropdownValue).ToList();
                if (person.Addresses != null && person.Addresses.Any())
                {
                    foreach (var address in person.Addresses)
                    {
                        var addressName = addressDropdown.DropdownValuesList.Where(a => a.DropdownValueID == address.AddressTypeID)
                            .Select(c => c.DropdownValue).FirstOrDefault();

                        XElement xAddressCity = new XElement("value", new XAttribute("OutlookKey", addressName + "AddressCity"));
                        xAddressCity.SetValue(address.City ?? "");
                        xContactElement.Add(xAddressCity);

                        XElement xAddressCountry = new XElement("value", new XAttribute("OutlookKey", addressName + "AddressCountry"));
                        xAddressCountry.SetValue(address.Country.Name ?? "");
                        xContactElement.Add(xAddressCountry);

                        XElement xAddressPostalCode = new XElement("value", new XAttribute("OutlookKey", addressName + "AddressPostalCode"));
                        xAddressPostalCode.SetValue(address.ZipCode ?? "");
                        xContactElement.Add(xAddressPostalCode);

                        XElement xAddressState = new XElement("value", new XAttribute("OutlookKey", addressName + "AddressState"));
                        xAddressState.SetValue(address.State.Name ?? "");
                        xContactElement.Add(xAddressState);

                        XElement xAddressStreet = new XElement("value", new XAttribute("OutlookKey", addressName + "AddressStreet"));
                        xAddressStreet.SetValue((address.AddressLine1 ?? "") + "\r\n" + (address.AddressLine2 ?? ""));
                        xContactElement.Add(xAddressStreet);
                    }
                }
                if (person.Phones != null && person.Phones.Any())
                {
                    foreach (var phone in person.Phones)
                    {
                        XElement xTelephoneNumber = new XElement("value", new XAttribute("OutlookKey", phone.PhoneTypeName + "TelephoneNumber"));
                        xTelephoneNumber.SetValue(phone != null ? phone.Number ?? "" : "");
                        xContactElement.Add(xTelephoneNumber);
                    }
                }

                XElement xCompanyName = new XElement("value", new XAttribute("OutlookKey", "CompanyName"));
                if (contact.Contact.CompanyID != null && contact.Contact.CompanyID > 0)
                {
                    Logger.Current.Verbose("Attempting to fetch company details for CompanyId: " + contact.Contact.CompanyID);
                    GetCompanyRequest companyRequest = new GetCompanyRequest((int)contact.Contact.CompanyID)
                    {
                        RequestedBy = int.Parse(currentUser.Id),
                        AccountId = currentUser.AccountID,
                        RoleId = currentUser.RoleID
                    };
                    GetCompanyResponse company = null;
                    try
                    {
                        company = contactService.GetCompany(companyRequest);
                    }
                    catch(System.Exception ex)
                    {
                        Logger.Current.Error(ex.Message, ex);
                    }
                    
                    if (company != null && company.CompanyViewModel != null && company.CompanyViewModel.CompanyName != null)
                    {
                        xCompanyName.SetValue(company.CompanyViewModel.CompanyName);
                    }
                    else
                    {
                        xCompanyName.SetValue("");
                    }
                    xContactElement.Add(xCompanyName);
                }
                XElement xCustomerID = new XElement("value", new XAttribute("OutlookKey", "CustomerID"));
                xCustomerID.SetValue(person.ContactID.ToString() ?? "");
                xContactElement.Add(xCustomerID);

                var emails = person.Emails;

                XElement xEmail1Address = new XElement("value", new XAttribute("OutlookKey", "Email1Address"));
                xEmail1Address.SetValue(emails.Where(e => e.IsPrimary).Select(e => e.EmailId).FirstOrDefault() ?? ""); xContactElement.Add(xEmail1Address);

                if (emails.Count() > 1)
                {
                    XElement xEmail2Address = new XElement("value", new XAttribute("OutlookKey", "Email2Address"));
                    xEmail2Address.SetValue(emails.Where(e => !e.IsPrimary).Select(e => e.EmailId).FirstOrDefault() ?? ""); xContactElement.Add(xEmail2Address);
                }
                if (emails.Count() > 2)
                {
                    XElement xEmail3Address = new XElement("value", new XAttribute("OutlookKey", "Email3Address"));
                    xEmail3Address.SetValue(emails.Where(e => !e.IsPrimary).Select(e => e.EmailId).Skip(1).FirstOrDefault() ?? ""); xContactElement.Add(xEmail3Address);
                }
                XElement xTitle = new XElement("value", new XAttribute("OutlookKey", "Title"));
                xTitle.SetValue(person.Title ?? ""); xContactElement.Add(xTitle);
                XElement xCategories = new XElement("value", new XAttribute("OutlookKey", "Categories"));
                xCategories.SetValue(syncCategory ?? "SmartTouch"); xContactElement.Add(xCategories);
                doc.Elements("Contacts").FirstOrDefault().Add(xContactElement);
            }


            var xmlDocument = new XmlDocument();
            using (var xmlReader = doc.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }

            return xmlDocument;
        }


        XmlDocument createAppoinmentXMLElement(GetToursToSyncResponse response, IEnumerable<DropdownViewModel> dropdownValues)
        {
            XDocument doc = new XDocument();
            var syncCategory = System.Configuration.ConfigurationManager.AppSettings["SYNC_CATEGORY"];

            doc.Add(new XElement("Appointments"));
            foreach (CRMOutlookSyncViewModel appointment in response.CRMOutlookSyncMappings)
            {
                TourViewModel tour = appointment.Tour;
                var communities = dropdownValues.Where(d => d.DropdownID == (byte)DropdownFieldTypes.Community).Select(c => c.DropdownValuesList).FirstOrDefault();
                var selectedCommunity = communities.Where(c => c.DropdownValueID == appointment.Tour.CommunityID).Select(t => t.DropdownValue).FirstOrDefault();
                XElement xTaskElement = new XElement("Appointment", new XAttribute("Key", appointment.EntityID));
                xTaskElement.Add(new XAttribute("FolderPath", "Calendar"));

                XElement xEntryID = new XElement("value", new XAttribute("OutlookKey", "EntryID"));
                xEntryID.SetValue(appointment.OutlookKey ?? ""); xTaskElement.Add(xEntryID);

                XElement xSubject = new XElement("value", new XAttribute("OutlookKey", "Subject"));
                XElement xBody = new XElement("value", new XAttribute("OutlookKey", "Body"));
                if (appointment.Tour.TourDetails != null)
                {
                    var appointmentDetails = appointment.Tour.TourDetails.Replace("\n", "").Replace("\r", "");
                    //var subjectIndex = appointmentDetails.ToLower().IndexOf("subject:");
                    var bodyIndex = appointmentDetails.ToLower().IndexOf("body:");
                    var subject = "";
                    var body = "";
                    if (bodyIndex != -1)
                    {
                        subject = appointmentDetails.Substring(0, bodyIndex).Replace("Subject:", "");
                        subject = Regex.Replace(subject, "Subject:", "", RegexOptions.IgnoreCase);
                        body = appointmentDetails.Substring(bodyIndex).Replace("Body:", "");
                        body = Regex.Replace(body, "Body:", "", RegexOptions.IgnoreCase);
                    }
                    else if (appointmentDetails.Length > 120)
                    {
                        subject = appointmentDetails.Substring(0, 120).Replace("Subject:", "");
                        subject = subject + "...";
                        body = appointmentDetails.Replace("Body:", "");
                    }
                    else
                    {
                        subject = appointmentDetails;
                        body = appointmentDetails;
                    }
                    xSubject.SetValue(subject.Trim()); xTaskElement.Add(xSubject);
                    xBody.SetValue(body.Trim()); xTaskElement.Add(xBody);
                }


                XElement xLocation = new XElement("value", new XAttribute("OutlookKey", "Location"));
                xLocation.SetValue(selectedCommunity); xTaskElement.Add(xLocation);

                var tourDate = appointment.Tour.TourDate;
                var appointmentDate = tourDate.ToString("yyyyMMddHHmmss");

                XElement xAppointmentDate = new XElement("value", new XAttribute("OutlookKey", "StartUTC"));
                xAppointmentDate.SetValue(appointmentDate); xTaskElement.Add(xAppointmentDate);

                var appointmentEndDate = tourDate.AddMinutes(30).ToString("yyyyMMddHHmmss");

                XElement xAppointmentEndDate = new XElement("value", new XAttribute("OutlookKey", "EndUTC"));
                xAppointmentEndDate.SetValue(appointmentEndDate); xTaskElement.Add(xAppointmentEndDate);

                if (tour.SelectedReminderTypes != null && tour.SelectedReminderTypes.Any())
                {
                    var tourRemindOnDate = appointment.Tour.ReminderDate;
                    //var outlookRemindOnDate = ((DateTime)tourRemindOnDate).ToString("yyyyMMddHHmmss");
                    TimeSpan reminderMinutesBeforeStart = (TimeSpan)(tour.TourDate - tour.ReminderDate);

                    XElement xReminderDate = new XElement("value", new XAttribute("OutlookKey", "ReminderMinutesBeforeStart"));
                    xReminderDate.SetValue(reminderMinutesBeforeStart.TotalMinutes.ToString()); xTaskElement.Add(xReminderDate);

                    XElement xReminderSet = new XElement("value", new XAttribute("OutlookKey", "ReminderSet"));
                    xReminderSet.SetValue("True"); xTaskElement.Add(xReminderSet);
                }
                else
                {
                    XElement xReminderSet = new XElement("value", new XAttribute("OutlookKey", "ReminderSet"));
                    xReminderSet.SetValue("False"); xTaskElement.Add(xReminderSet);
                }


                XElement xStatus = new XElement("value", new XAttribute("OutlookKey", "Status"));
                xStatus.SetValue(appointment.Tour.IsCompleted); xTaskElement.Add(xStatus);

                XElement xCategories = new XElement("value", new XAttribute("OutlookKey", "Categories"));
                xCategories.SetValue(syncCategory ?? "SmartTouch"); xTaskElement.Add(xCategories);
                //XElement xBody = new XElement("value", new XAttribute("OutlookKey", "Body"));
                //xBody.SetValue(appointment.Tour.TourDetails ?? "None"); xTaskElement.Add(xBody);

                //XElement xLinkedContacts = new XElement("value", new XAttribute("OutlookKey", "LinkedContacts"));
                //xLinkedContacts.SetValue(""); xTaskElement.Add(xLinkedContacts);

                //XElement xRequiredAttendees = new XElement("value", new XAttribute("OutlookKey", "RequiredAttendees"));
                //xRequiredAttendees.SetValue(""); xTaskElement.Add(xRequiredAttendees);

                doc.Elements("Appointments").FirstOrDefault().Add(xTaskElement);
            }


            var xmlDocument = new XmlDocument();
            using (var xmlReader = doc.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }

            return xmlDocument;
        }


        XmlDocument createTaskXMLElement(IEnumerable<CRMOutlookSyncViewModel> tasks, IEnumerable<DropdownViewModel> dropdownValues)
        {
            XDocument doc = new XDocument();
            var syncCategory = System.Configuration.ConfigurationManager.AppSettings["SYNC_CATEGORY"];
            doc.Add(new XElement("Tasks"));
            foreach (CRMOutlookSyncViewModel task in tasks)
            {
                //ActionViewModel action = task.Action;
                XElement xTaskElement = new XElement("Task", new XAttribute("Key", task.EntityID));
                xTaskElement.Add(new XAttribute("FolderPath", "Tasks"));

                XElement xEntryID = new XElement("value", new XAttribute("OutlookKey", "EntryID"));
                xEntryID.SetValue(task.OutlookKey ?? ""); xTaskElement.Add(xEntryID);

                XElement xSubject = new XElement("value", new XAttribute("OutlookKey", "Subject"));
                XElement xBody = new XElement("value", new XAttribute("OutlookKey", "Body"));

                if (task.Action.ActionMessage != null)
                {
                    var actionMessage = task.Action.ActionMessage.Replace("\n", "").Replace("\r", "");
                    //var subjectIndex = actionMessage.ToLower().IndexOf("subject:");
                    var bodyIndex = actionMessage.ToLower().IndexOf("body:");
                    var subject = "";
                    var body = "";
                    if (bodyIndex != -1)
                    {
                        subject = actionMessage.Substring(0, bodyIndex).Replace("Subject:", "").Trim();
                        subject = Regex.Replace(subject, "Subject:", "", RegexOptions.IgnoreCase);
                        body = actionMessage.Substring(bodyIndex).Replace("Body:", "").Trim();
                        body = Regex.Replace(body, "Body:", "", RegexOptions.IgnoreCase);
                    }
                    else if (actionMessage.Length > 120)
                    {
                        subject = actionMessage.Substring(0, 120).Replace("Subject:", "");
                        subject = subject + "...";
                        body = actionMessage.Replace("Body:", "");
                    }
                    else
                    {
                        subject = actionMessage;
                        body = actionMessage;
                    }
                    xSubject.SetValue(subject.Trim()); xTaskElement.Add(xSubject);
                    xBody.SetValue(body.Trim()); xTaskElement.Add(xBody);
                }

                if (task.Action.RemindOn != null)
                {
                    XElement xReminderSet = new XElement("value", new XAttribute("OutlookKey", "ReminderSet"));
                    xReminderSet.SetValue("True"); xTaskElement.Add(xReminderSet);
                    var remindOnDate = (DateTime)task.Action.RemindOn;

                    XElement xDueDate = new XElement("value", new XAttribute("OutlookKey", "DueDate"));
                    xDueDate.SetValue(remindOnDate.ToString("yyyyMMdd")); xTaskElement.Add(xDueDate);

                    XElement xReminderTime = new XElement("value", new XAttribute("OutlookKey", "ReminderTime"));
                    xReminderTime.SetValue(remindOnDate.ToString("yyyyMMddHHmmss")); xTaskElement.Add(xReminderTime);
                }

                XElement xStatus = new XElement("value", new XAttribute("OutlookKey", "Complete"));
                XElement xCompleted = new XElement("value", new XAttribute("OutlookKey", "PercentComplete"));
                XElement xDateCompleted = new XElement("value", new XAttribute("OutlookKey", "DateCompleted"));

                if (task.Action.IsCompleted)
                {
                    xStatus.SetValue(OlTaskStatus.olTaskComplete); xTaskElement.Add(xStatus);
                    xCompleted.SetValue(100); xTaskElement.Add(xCompleted);
                    xDateCompleted.SetValue(DateTime.Now.ToString("yyyyMMddHHmm")); xTaskElement.Add(xDateCompleted);

                }
                else
                {
                    xStatus.SetValue(OlTaskStatus.olTaskInProgress); xTaskElement.Add(xStatus);
                    xCompleted.SetValue(0); xTaskElement.Add(xCompleted);
                    //xDateCompleted.SetValue(DateTime.Now.ToString("yyyyMMddHHmm")); xTaskElement.Add(xDateCompleted);
                }
                XElement xCategories = new XElement("value", new XAttribute("OutlookKey", "Categories"));
                xCategories.SetValue(syncCategory ?? "SmartTouch"); xTaskElement.Add(xCategories);

                doc.Elements("Tasks").FirstOrDefault().Add(xTaskElement);

                XElement xStartDate = new XElement("value", new XAttribute("OutlookKey", "StartDate"));
                xStartDate.SetValue(task.Action.ActionDate.Value.ToString("yyyyMMdd")); xTaskElement.Add(xStartDate);

                XElement xEndDate = new XElement("value", new XAttribute("OutlookKey", "DueDate"));
                xEndDate.SetValue(task.Action.ActionDate.Value.ToString("yyyyMMdd")); xTaskElement.Add(xEndDate);

            }

            var xmlDocument = new XmlDocument();
            using (var xmlReader = doc.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }

            return xmlDocument;
        }


        /// <param name="xmlTasks"> task item elements in the following format
        ///		<Tasks>
        ///			<Task Key="">
        ///				<value OutlookKey="EntryID"></value>
        ///			</Task>
        ///			...
        ///		</Tasks>
        /// </param>
        public void ISGetNewTasksResult(XmlElement xmlTasks, Tuple<IContactService> services)
        {
            UpdateSyncedEntitiesRequest request = new UpdateSyncedEntitiesRequest();

            var xDoc = XDocument.Parse(xmlTasks.OuterXml);
            var taskElements = (from t in xDoc.Descendants("Task")
                                let outlookKeyAttribute = t.Attribute("Key").Value
                                select new { Attribute = int.Parse(outlookKeyAttribute), Text = "" }
                                       ).ToDictionary(ce => ce.Attribute, ce => ce.Text);
            var contactService = services.Item1;
            request.SyncedEntities = taskElements;
            UpdateSyncedEntitiesResponse response = contactService.UpdateSyncedEntities(request);
        }


        /// <param name="timeStamp">Return contacts that were modified after timeStamp date</param>
        /// <returns>
        /// contact item elements in the following format
        ///		<Contacts>
        ///			<Contact Key="" FolderPath="">
        ///				<value OutlookKey=""></value>
        ///				...
        ///			</Contact>
        ///			...
        ///		</Contacts>
        /// </returns>
        public XmlElement ISGetModifiedContacts(string session,ConcurrentUser concurrentUser,  DateTime? timeStamp, int? maxNumRecords, Tuple<IContactService> services)
        {
             
            var contactService = services.Item1;

            var dropdownValues = concurrentUser.DropdownValues;


            GetContactsToSyncRequest request = new GetContactsToSyncRequest()
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                MaxNumRecords = maxNumRecords,
                TimeStamp = timeStamp
            };
            GetContactsToSyncResponse response = contactService.GetContactsToSync(request);
            XmlDocument doc = createContactXMLElement(response.CRMOutlookSyncMappings, dropdownValues, contactService, concurrentUser.User);

            return (XmlElement)doc.FirstChild;
        }


        /// <param name="xmlContacts"> contact item elements in the following format
        ///		<Contacts>
        ///			<Contact Key="">
        ///				<value OutlookKey="EntryID"></value>
        ///			</Contact>
        ///			...
        ///		</Contacts>
        /// </param>
        public void ISGetModifiedContactsResult(XmlElement xmlContacts)
        {
            //throw new WAException(-1, "Not implemented");
        }


        /// <param name="timeStamp">Return appointments that were modified after timeStamp date</param>
        /// <returns>
        /// appointment item elements in the following format
        ///		<Appointments>
        ///			<Appointment Key="" FolderPath="">
        ///				<value OutlookKey=""></value>
        ///				...
        ///			</Appointment>
        ///			...
        ///		</Appointments>
        /// </returns>
        public XmlElement ISGetModifiedAppointments(string session, ConcurrentUser concurrentUser , DateTime? timeStamp, int? maxNumRecords, bool firstSync, Tuple<ITourService> services)
        {
            var tourService = services.Item1;

            var dropdownValues = concurrentUser.DropdownValues;


            GetToursToSyncRequest request = new GetToursToSyncRequest()
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                MaxNumRecords = maxNumRecords,
                TimeStamp = timeStamp,
                FirstSync = firstSync,
                OperationType = CRUDOperationType.Update
            };
            GetToursToSyncResponse response = tourService.GetToursToSync(request);
            XmlDocument doc = createAppoinmentXMLElement(response, dropdownValues);

            return (XmlElement)doc.FirstChild;
        }


        /// <param name="xmlAppointments"> appointment item elements in the following format
        ///		<Appointments>
        ///			<Appointment Key="">
        ///				<value OutlookKey="EntryID"></value>
        ///			</Appointment>
        ///			...
        ///		</Appointments>
        /// </param>
        public void ISGetModifiedAppointmentsResult(XmlElement xmlAppointments, Tuple<IContactService> services)
        {
            var contactService = services.Item1;

            var xDoc = XDocument.Parse(xmlAppointments.OuterXml);
            var tourElements = (from t in xDoc.Descendants("value")
                                let outlookKeyAttribute = t.Attribute("Key").Value
                                let outlookKeyAttributeText = t.Value
                                select new { Attribute = int.Parse(outlookKeyAttribute), Text = outlookKeyAttributeText }
                                       ).ToDictionary(ce => ce.Attribute, ce => ce.Text);

            UpdateSyncedEntitiesRequest request = new UpdateSyncedEntitiesRequest();

            request.SyncedEntities = tourElements;
            UpdateSyncedEntitiesResponse response = contactService.UpdateSyncedEntities(request);
        }


        /// <param name="timeStamp">Return tasks that were modified after timeStamp date</param>
        /// <returns>
        /// task item elements in the following format
        ///		<Tasks>
        ///			<Task Key="" FolderPath="">
        ///				<value OutlookKey=""></value>
        ///				...
        ///			</Task>
        ///			...
        ///		</Tasks>
        /// </returns>
        public XmlElement ISGetModifiedTasks(string session,ConcurrentUser concurrentUser, DateTime? timeStamp, int? maxNumRecords, bool firstSync, Tuple<IActionService> services)
        {
            var actionService = services.Item1;

            var dropdownValues = concurrentUser.DropdownValues;
            GetTasksToSyncRequest request = new GetTasksToSyncRequest()
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID,
                MaxNumRecords = maxNumRecords,
                TimeStamp = timeStamp,
                FirstSync = firstSync
            };
            GetTasksToSyncResponse response = actionService.GetModifiedActionsToSync(request);
            XmlDocument doc = createTaskXMLElement(response.CRMOutlookSyncMappings, dropdownValues);

            return (XmlElement)doc.FirstChild;
        }


        /// <param name="xmlTasks"> task item elements in the following format
        ///		<Tasks>
        ///			<Task Key="">
        ///				<value OutlookKey="EntryID"></value>
        ///			</Task>
        ///			...
        ///		</Tasks>
        /// </param>
        public void ISGetModifiedTasksResult(XmlElement xmlTasks, Tuple<IContactService> services)
        {

            var contactService = services.Item1;
            UpdateSyncedEntitiesRequest request = new UpdateSyncedEntitiesRequest();
            var xDoc = XDocument.Parse(xmlTasks.OuterXml);
            var taskElements = (from t in xDoc.Descendants("value")
                                let outlookKeyAttribute = t.Attribute("Key").Value
                                let outlookKeyAttributeText = t.Value
                                select new { Attribute = int.Parse(outlookKeyAttribute), Text = outlookKeyAttributeText }
                                       ).ToDictionary(ce => ce.Attribute, ce => ce.Text);

            request.SyncedEntities = taskElements;
            UpdateSyncedEntitiesResponse response = contactService.UpdateSyncedEntities(request);
        }


        /// <param name="timeStamp">Return contacts that were deleted after timeStamp date</param>
        /// <returns>
        /// contact item elements in the following format
        ///		<Contacts>
        ///			<Contact Key="" />
        ///			...
        ///		</Contacts>
        /// </returns>
        public XmlElement ISGetDeletedContacts(string session,  ConcurrentUser concurrentUser , DateTime? timeStamp, int? maxNumRecords, Tuple<IContactService> services)
        {
            var contactService = services.Item1;

            GetContactsToSyncRequest request = new GetContactsToSyncRequest()
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID,
                MaxNumRecords = maxNumRecords,
                TimeStamp = timeStamp,
                IsDeleted = true
            };
            GetContactsToSyncResponse response = contactService.GetDeletedContactsToSync(request);
            XmlDocument doc = createDeletedContactXMLElement(response.DeletedContacts);

            return (XmlElement)doc.FirstChild;
        }


        /// <param name="xmlContacts"> contact item elements in the following format
        ///		<Contacts>
        ///			<Contact Key="">
        ///				<value OutlookKey="EntryID"></value>
        ///			</Contact>
        ///			...
        ///		</Contacts>
        /// </param>
        public void ISGetDeletedContactsResult(XmlElement xmlContacts)
        {
        }


        /// <param name="timeStamp">Return appointments that were deleted after timeStamp date</param>
        /// <returns>
        /// appointment item elements in the following format
        ///		<Appointments>
        ///			<Appointment Key="" />
        ///			...
        ///		</Appointments>
        /// </returns>
        public XmlElement ISGetDeletedAppointments(string session, ConcurrentUser concurrentUser , DateTime? timeStamp, int? maxNumRecords, Tuple<ITourService> services)
        {
            var tourService = services.Item1;

            GetToursToSyncRequest request = new GetToursToSyncRequest()
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID,
                MaxNumRecords = maxNumRecords,
                TimeStamp = timeStamp,
                IsDeleted = true,
                OperationType = CRUDOperationType.Delete
            };
            GetToursToSyncResponse response = tourService.GetDeletedToursToSync(request);
            XmlDocument doc = createDeletedAppointmentXMLElement(response.DeletedTours);

            return (XmlElement)doc.FirstChild;
        }


        XmlDocument createDeletedContactXMLElement(IEnumerable<int> deletedCotnacts)
        {
            XDocument doc = new XDocument();
            doc.Add(new XElement("DeletedContacts"));
            foreach (int contactId in deletedCotnacts)
            {
                XElement xContactElement = new XElement("Contact", new XAttribute("Key", contactId));
                doc.Elements("DeletedContacts").FirstOrDefault().Add(xContactElement);
            }
            var xmlDocument = new XmlDocument();
            using (var xmlReader = doc.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }


        XmlDocument createDeletedAppointmentXMLElement(IEnumerable<int> deletedAppointments)
        {
            XDocument doc = new XDocument();
            doc.Add(new XElement("DeletedAppointments"));
            foreach (int tourId in deletedAppointments)
            {
                XElement xTourElement = new XElement("Appointment", new XAttribute("Key", tourId));
                doc.Elements("DeletedAppointments").FirstOrDefault().Add(xTourElement);
            }
            var xmlDocument = new XmlDocument();
            using (var xmlReader = doc.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }


        XmlDocument createDeletedTaskXMLElement(IEnumerable<int> deletedTasks)
        {
            XDocument doc = new XDocument();
            doc.Add(new XElement("DeletedTasks"));
            foreach (int taskId in deletedTasks)
            {
                XElement xTaskElement = new XElement("Task", new XAttribute("Key", taskId));
                doc.Elements("DeletedTasks").FirstOrDefault().Add(xTaskElement);
            }
            var xmlDocument = new XmlDocument();
            using (var xmlReader = doc.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }


        /// <param name="xmlAppointments"> appointment item elements in the following format
        ///		<Appointments>
        ///			<Appointment Key="">
        ///				<value OutlookKey="EntryID"></value>
        ///			</Appointment>
        ///			...
        ///		</Appointments>
        /// </param>
        public void ISGetDeletedAppointmentsResult(XmlElement xmlAppointments)
        {
        }


        /// <param name="timeStamp">Return tasks that were deleted after timeStamp date</param>
        /// <returns>
        /// task item elements in the following format
        ///		<Tasks>
        ///			<Task Key="" />
        ///			...
        ///		</Tasks>
        /// </returns>
        public XmlElement ISGetDeletedTasks(string session, ConcurrentUser concurrentUser , DateTime? timeStamp, int? maxNumRecords, Tuple<IActionService> services)
        {
            var actionService = services.Item1;

            GetTasksToSyncRequest request = new GetTasksToSyncRequest()
            {
                AccountId = concurrentUser.User.AccountID,
                RequestedBy = int.Parse(concurrentUser.User.Id),
                RoleId = concurrentUser.User.RoleID,
                MaxNumRecords = maxNumRecords,
                TimeStamp = timeStamp,
                IsDeleted = true
            };
            GetTasksToSyncResponse response = actionService.GetDeletedTasksToSync(request);
            XmlDocument doc = createDeletedTaskXMLElement(response.DeletedActions);

            return (XmlElement)doc.FirstChild;
        }


        /// <param name="xmlTasks"> task item elements in the following format
        ///		<Tasks>
        ///			<Task Key="">
        ///				<value OutlookKey="EntryID"></value>
        ///			</Task>
        ///			...
        ///		</Tasks>
        /// </param>
        public void ISGetDeletedTasksResult(XmlElement xmlTasks)
        {
        }


        /// <param name="xmlEmail">
        /// email data in the following format
        ///		<EmailMessage>
        ///			<uploadMail>
        ///				<from></from>
        ///				<to></to>
        ///				<cc></cc>
        ///				<subject></subject>
        ///				<message></message>
        ///				<sentDate></sentDate>
        ///				<categories>
        ///					<value webKey=""></value>
        ///				</categories>
        ///			</uploadMail>
        ///		</EmailMessage>
        /// </param>
        /// <returns>email unique key</returns>
        public string ISUploadEmail(XDocument uploadXML, Tuple<IContactService, IMessageService, IIndexingService, IUserService> services)
        {
            Logger.Current.Verbose("In ISUploadEmail");
            var contactService = services.Item1;
            var messageService = services.Item2;
            var indexingService = services.Item3;
            var userService = services.Item4;
            XmlDocument doc = new XmlDocument();

            string session = uploadXML.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;
            var existingUser = ConcurrentUsers.ConcurrentUser.Where(c => c != null && c.SessionKey == session).Any() ?
                ConcurrentUsers.ConcurrentUser.Where(c => c != null && c.SessionKey == session).FirstOrDefault() : new ConcurrentUser();
            if (existingUser.User == null)
            {
                Logger.Current.Informational("User with session not found. Session Key: " + session);
                return Guid.NewGuid().ToString();
            }
            var accountId = existingUser.User.AccountID;
            EmailInfo emailinfo = new EmailInfo();
            try
            {
                System.Exception itemException = null;
                doc.LoadXml(uploadXML.ToString());
                XmlNodeList emailNodes = doc.DocumentElement.ChildNodes[0].ChildNodes[0].ChildNodes;
                Logger.Current.Verbose("Identifying the nodes");
                foreach (XmlElement item in emailNodes)
                {
                    try
                    {
                        if (item.Name == "to")
                        {
                            var emaildata = System.Web.HttpUtility.UrlDecode(item.InnerXml);
                            if (!string.IsNullOrEmpty(emaildata))
                                emailinfo.To = emaildata.Split(';').Where(i => !string.IsNullOrEmpty(i)).Distinct();
                            
                        }
                        else if (item.Name == "cc")
                        {
                            var emaildata = System.Web.HttpUtility.UrlDecode(item.InnerXml);
                            if (!string.IsNullOrEmpty(emaildata))
                                emailinfo.CC = emaildata.Split(';').Where(i => !string.IsNullOrEmpty(i)).Distinct();
                        }
                        else if (item.Name == "bcc")
                        {
                            var emaildata = System.Web.HttpUtility.UrlDecode(item.InnerXml);
                            if (!string.IsNullOrEmpty(emaildata))
                                emailinfo.BCC = emaildata.Split(';').Where(i => !string.IsNullOrEmpty(i)).Distinct();
                        }
                        else if (item.Name == "message")
                        {
                            var emaildata = System.Web.HttpUtility.UrlDecode(item.InnerXml);
                            if (!string.IsNullOrEmpty(emaildata))
                                emailinfo.Body = emaildata;
                        }
                        else if (item.Name == "sentDateUTC")
                        {
                            string datevalue = item.InnerXml;
                            string format = "yyyyMMddHHmmss";
                            DateTime SentUTCDate = DateTime.ParseExact(datevalue, format,
                                                             CultureInfo.InvariantCulture);
                            emailinfo.SentDate = SentUTCDate;
                        }
                        else if (item.Name == "EmailType")
                        {
                            emailinfo.EmailType = item.InnerXml;
                        }
                        else if (item.Name == "subject")
                        {
                            emailinfo.Subject = System.Web.HttpUtility.UrlDecode(item.InnerXml);
                            Logger.Current.Informational("Subject : " + emailinfo.Subject);

                        }
                        else if (item.Name == "from")
                        {
                            emailinfo.FromEmail = System.Web.HttpUtility.UrlDecode(item.InnerXml); //existingUser.User.Email.EmailId;
                            Logger.Current.Informational("From : " + emailinfo.FromEmail);
                        }
                    }
                    catch (WAException ex)
                    {
                        item.SetAttribute("RetVal", "-1");
                        item.SetAttribute("ErrorString", ex.Message);
                        itemException = ex;
                    }
                    catch (System.Exception ex)
                    {
                        item.SetAttribute("RetVal", "1");
                        item.SetAttribute("ErrorString", ex.Message);
                        itemException = ex;
                    }
                    while (item.HasChildNodes)
                        item.RemoveChild(item.FirstChild);
                    
                }
                Logger.Current.Verbose("Idenfied all nodes");
            }
            catch (System.Exception ex)
            {
                Logger.Current.Error(ex.ToString());
            }
            var from = new List<string>();
            from.Add(emailinfo.FromEmail);
            var mailFromContacts = contactService.FindContactsByEmail(new FindContactsByEmailRequest() { Email = emailinfo.FromEmail, AccountId = accountId });
            var recepientEmails = emailinfo.To.Concat(emailinfo.CC ?? new List<string>()).Concat(emailinfo.BCC ?? new List<string>());
            Logger.Current.Verbose("MailFromContacts");
            var recepients = userService.FindUsersByEmails(new FindUsersByEmailsRequest() { UserEmails = recepientEmails, AccountId = accountId }).Users;
            if (mailFromContacts.ContactIDs != null && mailFromContacts.ContactIDs.Any() && recepients != null && recepients.Any())
            {
                Logger.Current.Informational("Found contacts in recipients list");
                //Same email can be used by different contacts as secondary email. Hence, adding a record for all contacts.
                foreach (var contactId in mailFromContacts.ContactIDs)
                {
                    InserImplicitSyncEmailInfoRequest receivedEmailRequest = new InserImplicitSyncEmailInfoRequest();
                    receivedEmailRequest.EmailInfo = emailinfo;
                    receivedEmailRequest.SentByContactID = contactId;
                    receivedEmailRequest.Users = recepients;
                    InserImplicitSyncEmailInfoResponse receivedEmailResponse = userService.TrackReceivedEmail(receivedEmailRequest);
                }
                return Guid.NewGuid().ToString();
            }
            else
            {
                Logger.Current.Informational("No contacts found in recipients list");
                OutlookEmailSentRequest req = new OutlookEmailSentRequest()
                {
                    SentDate = emailinfo.SentDate
                };
                OutllokEmailSentResposne resp = contactService.isOutlookEmailAlreadySynced(req);

                //if (resp.isEmailAlreadySent)
                //  throw new System.Exception("This email is already synced");

                int accountID = existingUser.User.AccountID;
                int userID = int.Parse(existingUser.User.Id);
                List<string> emailsToUpload = new List<string>();
                if (emailinfo.To != null)
                    emailsToUpload.AddRange(emailinfo.To);
                if (emailinfo.CC != null)
                    emailsToUpload.AddRange(emailinfo.CC);
                if (emailinfo.BCC != null)
                    emailsToUpload.AddRange(emailinfo.BCC);

                GetOutlookEmailInformationResponse emailInformationResponse = contactService
                    .GetEmailInformation(new GetOutlookEmailInformationRequest() { EmailsToUpload = emailsToUpload, AccountId = accountID }); // 
                if (emailInformationResponse.OutlookInformation != null && emailInformationResponse.OutlookInformation.Any())
                {
                    Logger.Current.Informational("Outlook information is not null");
                    InserImplicitSyncEmailInfoRequest request = new InserImplicitSyncEmailInfoRequest() { EmailInfo = emailinfo };
                    InserImplicitSyncEmailInfoResponse response = contactService.InsertImplicitSyncEmailUpload(request);

                    contactService.InsertOutlookEmailAuditInformation(new InsertOutlookEmailAuditInformationRequest()
                    {
                        AccountId = accountID,
                        RequestedBy = userID,
                        Guid = response.ResponseGuid,
                        SentUTCDate = emailinfo.SentDate,
                        Emails = emailInformationResponse.OutlookInformation
                    });
                    Logger.Current.Informational("Inserted outlook audit info");

                    var messages = new List<TrackMessage>();
                    foreach (var item in emailInformationResponse.OutlookInformation)
                    {
                        var message = new TrackMessage()
                        {
                            EntityId = item.ContactEmailID,
                            AccountId = accountID,
                            ContactId = item.ContactID,
                            LeadScoreConditionType = (int)LeadScoreConditionType.AnEmailSent
                        };
                        messages.Add(message);
                    }
                    messageService.SendMessages(new ApplicationServices.Messaging.Messages.SendMessagesRequest()
                    {
                        Messages = messages
                    });
                    Logger.Current.Informational("Getting contact list");
                    var contactList = emailInformationResponse.OutlookInformation.Select(x => x.ContactID).ToList();
                    IEnumerable<Contact> documents = contactService.GetAllContactsByIds(new GetContactsByIDsRequest() { ContactIDs = contactList }).Contacts; // contactRepository.FindAll(contactList);
                    IEnumerable<ContactCreatorInfo> creatorInfos = contactService.GetContactCreatorsInfo(new GetContactCreatorsInfoRequest() { ContactIDs = contactList }).GetContactCreatorsInfo;
                    foreach (var contact in documents)
                    {
                        var creatorInfo = creatorInfos.SingleOrDefault(c => c.ContactId == contact.Id);
                        contact.CreatedOn = creatorInfo.CreatedOn.Value;
                        indexingService.IndexContact(contact);
                    }
                    return response.ResponseGuid.ToString();
                }
                else
                {
                    throw new System.Exception("No valid contact emails in CC, BCC or TO in the email recieved from " + emailinfo.FromEmail);
                }
            }
        }


        /// <summary>
        /// upload an attachment for previously uploaded email
        /// </summary>
        /// <param name="emailKey">unique key of the email</param>
        /// <param name="att">Attachment binary stream</param>
        public void ISUploadAttachment(string emailKey, string filename, Stream att)
        {
            throw new WAException(-1, "Not implemented");
        }


        /// <summary>
        /// Discard previously uploaded email and attachments
        /// </summary>
        /// <param name="emailKey">Unique key of the email</param>
        public void ISCancelUploadEmail(string emailKey)
        {
            throw new WAException(-1, "Not implemented");
        }


        public DateTime ISGetServerTime()
        {
            return DateTime.Now.ToUniversalTime();
        }


        /// <summary>
        /// Enumerates and returns all files on the server
        /// </summary>
        /// <returns>
        /// all file items in the following format
        ///		<Files>
        ///			<File Key="" LastModifiedUTC="yyyyMMddHHmmss" />
        ///			...
        ///		</Files>
        /// </returns>
        public XmlElement ISEnumerateFiles()
        {
            throw new WAException(-1, "Not implemented");
        }


        /// <summary>
        /// Get file information and content from the server
        /// </summary>
        /// <param name="filekey">unique key of the file</param>
        /// <returns>
        /// file item in the following format
        ///		<File Key="">
        ///			<FileName></FileName>
        ///			<Description></Description>
        ///			<Data><![CDATA[Base64 encoded file contents]]>
        ///			<Categories>
        ///				<Category1></Category1>
        ///				...
        ///			</Categories>
        ///		</File>
        /// </returns>
        public XmlElement ISGetFile(string filekey)
        {
            throw new WAException(-1, "Not implemented");
        }


        /// <summary>
        /// Insert file on the server
        /// </summary>
        /// <param name="xmlFile">
        /// file data in the following format
        ///		<File>
        ///			<FileName></FileName>
        ///			<Description></Description>
        ///			<Data><![CDATA[Base64 encoded file contents]]>
        ///			<Categories>
        ///				<Category1></Category1>
        ///				...
        ///			</Categories>
        ///		</File>
        /// </param>
        /// <returns>file unique key</returns>
        public string ISInsertFile(XmlElement xmlFile, out DateTime lastModifiedUTC)
        {
            throw new WAException(-1, "Not implemented");
        }


        /// <summary>
        /// Update file on the server
        /// </summary>
        /// <param name="xmlFile">
        /// file data in the following format
        ///		<File Key="">
        ///			<FileName></FileName>
        ///			<Description></Description>
        ///			<Data><![CDATA[Base64 encoded file contents]]>
        ///			<Categories>
        ///				<Category1></Category1>
        ///				...
        ///			</Categories>
        ///		</File>
        /// </param>
        /// <returns>file unique key</returns>
        public void ISUpdateFile(XmlElement xmlFile, out DateTime lastModifiedUTC)
        {
            throw new WAException(-1, "Not implemented");
        }


        /// <summary>
        /// Delete file from the server
        /// </summary>
        /// <param name="filekey">file key to be deleted</param>
        public void ISDeleteFile(string filekey)
        {
            throw new WAException(-1, "Not implemented");
        }


        public void ISDetachFile(string filekey)
        {
            throw new WAException(-1, "Not implemented");
        }


        private string session_;
    }

}