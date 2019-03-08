using Microsoft.AspNet.Identity;
using SimpleInjector;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Plugins.Utilities.ImplicitSync;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.ApplicationServices.Messaging.Geo;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;

namespace SmartTouch.CRM.Plugins.Controllers
{
    public class WASyncWSController : ApiController
    {
        private IContactService contactService;
        private IGeoService geoService;
        private ISearchService<Contact> searchService;
        private IActionService actionService;
        private ITourService tourService;
        private IUserService userService;
        private GetCountriesAndStatesResponse countriesAndStates;
        private IMessageService messageService;
        private IIndexingService indexingService;
        private IDropdownValuesService dropdownValuesService;

        public WASyncWSController()
        {
            //Uncomment the following line if using designed components 
            //InitializeComponent(); 
            Container container = new Container();
            IoC.Configure(container);
            contactService = IoC.Container.GetInstance<IContactService>();
            geoService = IoC.Container.GetInstance<IGeoService>();
            searchService = IoC.Container.GetInstance<ISearchService<Contact>>();
            actionService = IoC.Container.GetInstance<IActionService>();
            tourService = IoC.Container.GetInstance<ITourService>();
            userService = IoC.Container.GetInstance<IUserService>();
            messageService = IoC.Container.GetInstance<IMessageService>();
            indexingService = IoC.Container.GetInstance<IIndexingService>();
            countriesAndStates = geoService.GetCountriesAndStates(new ApplicationServices.Messaging.Geo.GetCountriesAndStatesRequest());
            dropdownValuesService = IoC.Container.GetInstance<IDropdownValuesService>();

        }

        [Route("login")]
        public ResponseXml Login(XDocument login)
        {
            ResponseXml xmlResp = new ResponseXml(string.Empty, "Login");
            string session = Guid.NewGuid().ToString("N");
            WASyncHandler handler = new WASyncHandler(session);

            string credentials = Uri.UnescapeDataString(login.Descendants("UserName").FirstOrDefault().Value.Trim());
            Logger.Current.Informational("Credentials after conversion: " + credentials);

            string domainUrl = credentials.Substring(0, credentials.IndexOf("|")).Trim();
            Logger.Current.Informational("DomainUrl: " + domainUrl);

            string username = credentials.Substring(credentials.IndexOf("|") + 1, credentials.Length - credentials.IndexOf("|") - 1).Trim();
            Logger.Current.Informational("Username: " + username);
            string password = login.Descendants("Password").FirstOrDefault().Value;

            PasswordHasher passwordHasher = new PasswordHasher();
            login.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value = session;
            UserViewModel user = handler.ISLogin(domainUrl, username, password, new Tuple<IUserService>(userService));

            if (passwordHasher.VerifyHashedPassword(user.Password, password) == PasswordVerificationResult.Failed)
                throw new Exception("Unknown error");
            xmlResp.ISResponse.InnerXml = "<SessionKey>" + session + "</SessionKey>";
            var existingUser = ConcurrentUsers.ConcurrentUser.Where(c => c.User.Id == user.UserID.ToString()).FirstOrDefault();
            if (existingUser != null)
            {
                ConcurrentUsers.ConcurrentUser.Remove(existingUser);
            }
            ConcurrentUsers.ConcurrentUser.Add(new ConcurrentUser()
            {
                User = new User()
                {
                    Id = user.UserID.ToString(),
                    AccountID = user.AccountID,
                    RoleID = user.RoleID,
                    Email = new Email() { EmailId = username }
                },
                SessionKey = session,
                SessionStartTime = DateTime.Now.ToUniversalTime(),
                DropdownValues = dropdownValuesService.GetAllByAccountID("", user.AccountID).DropdownValuesViewModel
            });


            return xmlResp;
        }

        [Route("LoginImpersonated")]

        public ResponseXml LoginImpersonated(XDocument login)
        {
            ResponseXml xmlResp = new ResponseXml(string.Empty, "Login");
            string session = Guid.NewGuid().ToString("N");
            WASyncHandler handler = new WASyncHandler(session);

            try
            {
                string username = Uri.UnescapeDataString(login.Descendants("UserName").FirstOrDefault().Value.Trim());
                string password = login.Descendants("Password").FirstOrDefault().Value;
                string impersonatedUserInfo = login.Descendants("ImpersonatedUserName").FirstOrDefault().Value;

                var indexOfDelimiter = impersonatedUserInfo.IndexOf("/");

                string impersonatedUserAccountName = impersonatedUserInfo.Substring(0, indexOfDelimiter).Trim();
                Logger.Current.Informational("DomainUrl: " + impersonatedUserAccountName);

                string impersonatedUserEmail = impersonatedUserInfo.Substring(indexOfDelimiter + 1, impersonatedUserInfo.Length - indexOfDelimiter - 1).Trim();
                Logger.Current.Informational("Username: " + username);

                bool isLoginImpersonated = handler
                    .ISLoginImpersonated(username, password, impersonatedUserAccountName, impersonatedUserEmail, new Tuple<IUserService>(userService));
                if (!isLoginImpersonated)
                    throw new Exception("Incorrect credentials");

                GetUserRequest impersonatedUserRequest = new GetUserRequest(1) { UserName = impersonatedUserEmail, DomainUrl = impersonatedUserAccountName };
                GetUserResponse impersonatedUserResponse = userService.GetUserByUserName(impersonatedUserRequest);
                if (impersonatedUserResponse.User != null)
                {
                    var user = impersonatedUserResponse.User;
                    ConcurrentUsers.ConcurrentUser.Add(new ConcurrentUser()
                    {
                        User = new User()
                        {
                            Id = user.UserID.ToString(),
                            AccountID = user.AccountID,
                            RoleID = user.RoleID,
                            Email = new Email() { EmailId = username }
                        },
                        SessionKey = session,
                        SessionStartTime = DateTime.Now.ToUniversalTime(),
                        DropdownValues = dropdownValuesService.GetAllByAccountID("", user.AccountID).DropdownValuesViewModel
                    });
                    xmlResp.ISResponse.InnerXml = "<SessionKey>" + session + "</SessionKey>";
                }
                else
                    throw new Exception("User not found with email " + impersonatedUserInfo);
            }
            catch (WAException ex)
            {
                Logger.Current.Error("Exception occured LoginImpersonated. Error: ", ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured LoginImpersonated. Error: ", ex);

                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }

        private ResponseXml HandleAction(ResponseXml xmlResp, string session, Action method)
        {
            try
            {
                method();
            }
            catch (WAException ex)
            {
                Logger.Current.Error(string.Format("Exception occured in {0}. Error: ", session), ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Current.Error(string.Format("Exception occured in {0}. Error: ", session), ex);

                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml Logout(XDocument logout)
        {
            string session = logout.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;
            Logger.Current.Verbose(string.Format("Removing user from list with session : {0}", session));
            ConcurrentUser concurrentUser = ConcurrentUsers.ConcurrentUser.Where(c => c != null && c.SessionKey == session).FirstOrDefault();
            //if (concurrentUser == null)
            //    throw new System.Exception(string.Format("User with session {0} not found", session));
            ResponseXml xmlResp = new ResponseXml(string.Empty, "Logout");
            WASyncHandler handler = new WASyncHandler(session);
            return HandleAction(xmlResp, "Log Out", () => handler.ISLogout(session, concurrentUser));
        }


        #region GetConfig
        public ResponseXml GetConfig(XDocument getConfigXml)
        {
            string session = getConfigXml.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;
            ResponseXml xmlResp = new ResponseXml(session, "GetConfig");
            WASyncHandler handler = new WASyncHandler(session);
            return HandleAction(xmlResp, "GetConfig", () => xmlResp.ISResponse.InnerXml = handler.ISGetConfig().OuterXml);
        }
        #endregion


        public ResponseXml GetCategoryList(XDocument getCategoryXml)
        {
            string session = getCategoryXml.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;
            ResponseXml xmlResp = new ResponseXml(session, "GetCategoryList");
            WASyncHandler handler = new WASyncHandler(session);
            return HandleAction(xmlResp, "GetCategoryList", () =>
            {
                xmlResp.ISResponse.InnerXml = handler.ISGetCategoryList().OuterXml;
            });
        }


        public ResponseXml AddToCategoryList(string session, string CategoryXML)
        {
            ResponseXml xmlResp = new ResponseXml(session, "AddToCategoryList");
            WASyncHandler handler = new WASyncHandler(session);
            XmlDocument doc = new XmlDocument();
            return HandleAction(xmlResp, "AddToCategoryList", () =>
            {
                doc.LoadXml(CategoryXML);
                handler.ISAddToCategoryList(doc.DocumentElement);
            });
        }


        public ResponseXml SyncOutlookToWebContacts(XDocument ContactsXML)
        {
            ResponseXml xmlResp = new ResponseXml("", "SyncOutlookToWebContacts");
            WASyncHandler handler = new WASyncHandler("");
            XmlDocument doc = new XmlDocument();
            string session = ContactsXML.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;

            try
            {
                ConcurrentUser concurrentUser = ConcurrentUsers.ConcurrentUser.Where(c => c != null && c.SessionKey == session).FirstOrDefault();
                if (concurrentUser == null)
                    throw new System.Exception(string.Format("User with session {0} not found", session));
                Exception itemException = null;
                doc.LoadXml(ContactsXML.ToString());
                XmlNodeList items = doc.SelectNodes("//Item");

                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement contact = doc.CreateElement("Contact");
                        contact.InnerXml = item.InnerXml;
                        contact.SetAttribute("FolderPath", item.GetAttribute("FolderPath"));

                        switch (item.GetAttribute("SyncAction").ToLower())
                        {
                            case "add":
                                {
                                    string key = handler.ISAddContact(session, concurrentUser, contact
                                        , new Tuple<ISearchService<Contact>, IContactService, GetCountriesAndStatesResponse>(searchService, contactService, countriesAndStates));
                                    if (string.IsNullOrEmpty(key))
                                        throw new Exception("Invalid key returned");
                                    item.SetAttribute("Key", key);
                                }
                                break;
                            case "modify":
                                {
                                    if (!handler.ISModifyContact(session, concurrentUser, item.Attributes["Key"].Value, contact, new Tuple<IContactService, GetCountriesAndStatesResponse>(contactService, countriesAndStates)))
                                        throw new Exception();
                                }
                                break;
                            case "remove":
                                {
                                    if (!handler.ISRemoveContact(session, concurrentUser, item.Attributes["Key"].Value, new Tuple<IContactService>(contactService)))
                                        throw new Exception();
                                }
                                break;
                            default:
                                {
                                    throw new Exception("Invalid SyncAction for item with \"" + item.Attributes["Key"].Value + "\" key");
                                }
                        }
                        item.SetAttribute("RetVal", "0");
                    }
                    catch (WAException ex)
                    {
                        Logger.Current.Error("Exception occured in SyncOutlookToWebContacts1. Error: ", ex);

                        item.SetAttribute("RetVal", "-1");
                        item.SetAttribute("ErrorString", ex.Message);
                        itemException = ex;
                    }
                    catch (Exception ex)
                    {
                        if(ex.Message.Contains("User with session"))
                            Logger.Current.Informational("Exception occured in SyncOutlookToWebContacts1. message: " + ex.Message+ "stacktrace :" + ex.StackTrace);
                        else
                            Logger.Current.Error("Exception occured in SyncOutlookToWebContacts1. Error: ", ex);

                        item.SetAttribute("RetVal", "1");
                        item.SetAttribute("ErrorString", ex.Message);
                        itemException = ex;
                    }
                    while (item.HasChildNodes)
                        item.RemoveChild(item.FirstChild);
                }

                xmlResp.ISResponse.AppendChild(xmlResp.ISResponse.OwnerDocument.CreateElement("ContactsResponse"));
                xmlResp.ISResponse.FirstChild.InnerXml = doc.DocumentElement.InnerXml;

                if (itemException != null)
                    throw itemException;
            }
            catch (WAException ex)
            {
                Logger.Current.Error("Exception occured in SyncOutlookToWebContacts2. Error: ", ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured in SyncOutlookToWebContacts2. Error: ", ex);

                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml SyncOutlookToWebCalendar(XDocument CalendarXML)
        {
            ResponseXml xmlResp = new ResponseXml("", "SyncOutlookToWebCalendar");
            WASyncHandler handler = new WASyncHandler("");
            XmlDocument doc = new XmlDocument();
            string session = CalendarXML.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;

            try
            {
                ConcurrentUser concurrentUser = ConcurrentUsers.ConcurrentUser.Where(c => c != null && c.SessionKey == session).FirstOrDefault();
                if (concurrentUser == null)
                    throw new System.Exception(string.Format("User with session {0} not found", session));
                Exception itemException = null;
                doc.LoadXml(CalendarXML.ToString());
                XmlNodeList items = doc.SelectNodes("//Item");

                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement appt = doc.CreateElement("Appointment");
                        appt.InnerXml = item.InnerXml;
                        appt.SetAttribute("FolderPath", item.GetAttribute("FolderPath"));

                        switch (item.GetAttribute("SyncAction").ToLower())
                        {
                            case "add":
                                {
                                    string key = handler.ISAddAppointment(session, concurrentUser, appt
                                        , new Tuple<IContactService, ITourService>(contactService, tourService));
                                    if (string.IsNullOrEmpty(key))
                                        throw new Exception("Invalid key returned");
                                    item.SetAttribute("Key", key);
                                }
                                break;
                            case "modify":
                                {
                                    if (!handler.ISModifyAppointment(session, concurrentUser, item.Attributes["Key"].Value, appt
                                        , new Tuple<ITourService, IContactService>(tourService, contactService)))
                                        throw new Exception();
                                }
                                break;
                            case "remove":
                                {
                                    if (!handler.ISRemoveAppointment(session, concurrentUser, item.Attributes["Key"].Value
                                        , new Tuple<ITourService>(tourService)))
                                        throw new Exception();
                                }
                                break;
                            default:
                                {
                                    throw new Exception("Invalid SyncAction for item with \"" + item.Attributes["Key"].Value + "\" key");
                                }
                        }
                        item.SetAttribute("RetVal", "0");
                    }
                    catch (WAException ex)
                    {
                        Logger.Current.Error("Exception occured while syncing AddAppointment. Error: ", ex);

                        item.SetAttribute("RetVal", "-1");
                        item.SetAttribute("ErrorString", ex.Message);
                        itemException = ex;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("User with session"))
                            Logger.Current.Informational("Exception occured while syncing AddAppointment. message: " + ex.Message + "stacktrace :" + ex.StackTrace);
                        else
                            Logger.Current.Error("Exception occured while syncing AddAppointment. Error: ", ex);
                        item.SetAttribute("RetVal", "1");
                        item.SetAttribute("ErrorString", ex.Message);
                        itemException = ex;
                    }
                    while (item.HasChildNodes)
                        item.RemoveChild(item.FirstChild);
                }

                xmlResp.ISResponse.AppendChild(xmlResp.ISResponse.OwnerDocument.CreateElement("CalendarResponse"));
                xmlResp.ISResponse.FirstChild.InnerXml = doc.DocumentElement.InnerXml;

                if (itemException != null)
                    throw itemException;
            }
            catch (WAException ex)
            {
                Logger.Current.Error("Exception occured in AddAppointment2. Error: ", ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured in AddAppointment2. Error: ", ex);

                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml SyncOutlookToWebTasks(XDocument xmlOutlookToWebTasks)
        {
            Logger.Current.Verbose("WASyncWSController/SyncOutlookToWebTasks");
            XmlDocument doc = new XmlDocument();

            string session = xmlOutlookToWebTasks.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;
            ResponseXml xmlResp = new ResponseXml(session, "SyncWebToOutlookTasks");
            WASyncHandler handler = new WASyncHandler(session);

            try
            {
                ConcurrentUser concurrentUser = ConcurrentUsers.ConcurrentUser.Where(c => c != null && c.SessionKey == session).FirstOrDefault();
                if (concurrentUser == null)
                    throw new System.Exception(string.Format("User with session {0} not found", session));
                Exception itemException = null;
                doc.LoadXml(xmlOutlookToWebTasks.ToString());
                XmlNodeList items = doc.SelectNodes("//Item");
                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement task = doc.CreateElement("Task");
                        task.InnerXml = item.InnerXml;
                        task.SetAttribute("FolderPath", item.GetAttribute("FolderPath"));

                        switch (item.GetAttribute("SyncAction").ToLower())
                        {
                            case "add":
                                {
                                    string key = handler.ISAddTask(session, concurrentUser, task,
                                        new Tuple<IActionService, IContactService, IUserService>(actionService, contactService, userService));
                                    if (string.IsNullOrEmpty(key))
                                        throw new Exception("Invalid key returned");
                                    item.SetAttribute("Key", key);
                                }
                                break;
                            case "modify":
                                {
                                    if (!handler.ISModifyTask(session, concurrentUser, item.Attributes["Key"].Value, task
                                       , new Tuple<IActionService, IContactService, IUserService>(actionService, contactService, userService)))
                                        throw new Exception();
                                }
                                break;
                            case "remove":
                                {
                                    if (!string.IsNullOrEmpty(item.Attributes["Key"].Value) && !handler.ISRemoveTask(session, concurrentUser, item.Attributes["Key"].Value, new Tuple<IActionService>(actionService)))
                                        throw new Exception();
                                }
                                break;
                            default:
                                {
                                    throw new Exception("Invalid SyncAction for item with \"" + item.Attributes["Key"].Value + "\" key");
                                }
                        }
                        item.SetAttribute("RetVal", "0");
                    }
                    catch (WAException ex)
                    {
                        Logger.Current.Error("Exception occured in SyncOutlookToWebTasks1. Error: ", ex);

                        item.SetAttribute("RetVal", "-1");
                        item.SetAttribute("ErrorString", ex.Message);
                        itemException = ex;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("User with session"))
                            Logger.Current.Informational("Exception occured in SyncOutlookToWebTasks1. message: " + ex.Message + "stacktrace :" + ex.StackTrace);
                        else
                            Logger.Current.Error("Exception occured in SyncOutlookToWebTasks1. Error: ", ex);
                        item.SetAttribute("RetVal", "1");
                        item.SetAttribute("ErrorString", ex.Message);
                        itemException = ex;
                    }
                    while (item.HasChildNodes)
                        item.RemoveChild(item.FirstChild);
                }

                xmlResp.ISResponse.AppendChild(xmlResp.ISResponse.OwnerDocument.CreateElement("TasksResponse"));
                xmlResp.ISResponse.FirstChild.InnerXml = doc.DocumentElement.InnerXml;

                if (itemException != null)
                    throw itemException;
            }
            catch (WAException ex)
            {
                Logger.Current.Error("Exception occured in SyncOutlookToWebTasks2. Error: ", ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured in SyncOutlookToWebTasks2. Error: ", ex);

                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml SyncWebToOutlookContacts(XDocument xmlContacts)
        {
            Logger.Current.Verbose("WASyncWSController/SyncWebToOutlookContacts");
            XmlDocument doc = new XmlDocument();
            var context = HttpContext.Current;
            DateTime? time = null;

            string session = xmlContacts.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;

            if (!string.IsNullOrEmpty(context.Request.Params["ContactTimeStamp"]) &&
                context.Request.Params["ContactTimeStamp"] != "0")
            {
                try
                {
                    time = DateTime.ParseExact(context.Request.Params["ContactTimeStamp"], "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
                }
                catch
                {
                    throw new Exception("Invalid time stamp");
                }
            }

            int? maxitems = null;
            int? maxNumRecords = int.Parse(context.Request.Params["MaxNumRecords"]);
            if (!string.IsNullOrEmpty(context.Request.Params["MaxNumRecords"]) &&
                context.Request.Params["MaxNumRecords"] != "0")
            {
                try
                {
                    maxitems = Int32.Parse(context.Request.Params["MaxNumRecords"]);
                }
                catch
                {
                    throw new Exception("Invalid argument MaxNumRecords");
                }
            }

            bool firstSync = !string.IsNullOrEmpty(context.Request.Params["FirstSync"]) && context.Request.Params["FirstSync"] != "0" && context.Request.Params["FirstSync"].ToLower() != "false";

            doc.LoadXml("<Contacts><count NumberOfItems=\"\"/></Contacts>");

            ResponseXml xmlResp = new ResponseXml(session, "SyncWebToOutlookContacts");
            WASyncHandler handler = new WASyncHandler(session);


            try
            {
                ConcurrentUser concurrentUser = ConcurrentUsers.ConcurrentUser.Where(c => c != null && c.SessionKey == session).FirstOrDefault();
                if (concurrentUser == null)
                    throw new System.Exception(string.Format("User with session {0} not found", session));

                if (maxNumRecords != null && maxNumRecords.Value < 0)
                    throw new Exception("Invalid argument. maxNumRecords must be greater than 0");
                if (maxNumRecords != null && maxNumRecords.Value == 0)
                    maxNumRecords = null;

                doc.LoadXml("<Contacts><count NumberOfItems=\"\"/></Contacts>");

                XmlElement contacts = handler.ISGetNewContacts(session, concurrentUser, time, maxNumRecords, firstSync,
                    new Tuple<IContactService, GetCountriesAndStatesResponse>(contactService, countriesAndStates));
                if (contacts != null)
                {
                    Logger.Current.Verbose("Requested WASyncWSController/SyncWebToOutlookContacts/IsGetNewContacts");
                    foreach (XmlNode item in contacts.ChildNodes)
                    {
                        XmlElement node = doc.CreateElement("Item");
                        node.InnerXml = item.InnerXml;
                        node.SetAttribute("Key", item.Attributes["Key"].Value);
                        node.SetAttribute("Guid", item.Attributes["Key"].Value);
                        if (item.Attributes["FolderPath"] != null)
                            node.SetAttribute("FolderPath", item.Attributes["FolderPath"].Value);
                        node.SetAttribute("SyncAction", "Add");
                        doc.DocumentElement.FirstChild.AppendChild(node);
                    }
                    if (maxNumRecords != null)
                        maxNumRecords -= contacts.ChildNodes.Count;
                }

                if (maxNumRecords == null || maxNumRecords > 0)
                {
                    contacts = handler.ISGetModifiedContacts(session, concurrentUser, time, maxNumRecords, new Tuple<IContactService>(contactService));
                    if (contacts != null)
                    {
                        Logger.Current.Verbose("Requested WASyncWSController/SyncWebToOutlookContacts/ISGetModifiedContacts");

                        foreach (XmlNode item in contacts.ChildNodes)
                        {
                            XmlElement node = doc.CreateElement("Item");
                            node.InnerXml = item.InnerXml;
                            node.SetAttribute("Key", item.Attributes["Key"].Value);
                            node.SetAttribute("Guid", item.Attributes["Key"].Value);
                            if (item.Attributes["FolderPath"] != null)
                                node.SetAttribute("FolderPath", item.Attributes["FolderPath"].Value);
                            node.SetAttribute("SyncAction", "Modify");
                            doc.DocumentElement.FirstChild.AppendChild(node);
                        }
                        if (maxNumRecords != null)
                            maxNumRecords -= contacts.ChildNodes.Count;
                    }
                }

                if (maxNumRecords == null || maxNumRecords > 0)
                {
                    contacts = handler.ISGetDeletedContacts(session, concurrentUser, time, maxNumRecords, new Tuple<IContactService>(contactService));
                    if (contacts != null)
                    {
                        Logger.Current.Verbose("Requested WASyncWSController/SyncWebToOutlookContacts/ISGetDeletedContacts");

                        foreach (XmlNode item in contacts.ChildNodes)
                        {
                            XmlElement node = doc.CreateElement("Item");
                            node.InnerXml = item.InnerXml;
                            node.SetAttribute("Key", item.Attributes["Key"].Value);
                            node.SetAttribute("Guid", item.Attributes["Key"].Value);
                            node.SetAttribute("SyncAction", "Remove");
                            doc.DocumentElement.FirstChild.AppendChild(node);
                        }
                    }
                }

                doc.DocumentElement.FirstChild.Attributes["NumberOfItems"].Value = doc.DocumentElement.FirstChild.ChildNodes.Count.ToString();
                xmlResp.ISResponse.InnerXml = doc.DocumentElement.OuterXml;
            }
            catch (WAException ex)
            {
                Logger.Current.Error("Exception occured in SyncWebToOutlookContacts. Error: ", ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("User with session"))
                    Logger.Current.Informational("Exception occured in SyncWebToOutlookContacts. message: " + ex.Message + "stacktrace :" + ex.StackTrace);
                else
                    Logger.Current.Error("Exception occured in SyncWebToOutlookContacts. Error: ", ex);

                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml SyncWebToOutlookContactsResult(XDocument xmlContacts)
        {
            string session = xmlContacts.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;
            ResponseXml xmlResp = new ResponseXml(session, "SyncOutlookToWebContactsResult");
            WASyncHandler handler = new WASyncHandler(session);
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(xmlContacts.ToString());
                XmlElement contacts = doc.CreateElement("Contacts");
                XmlNodeList items = doc.SelectNodes("//Item[translate(@SyncAction, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'add']");
                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement contact = doc.CreateElement("Contact");
                        contact.InnerXml = item.InnerXml;
                        contact.SetAttribute("Key", item.GetAttribute("Key"));
                        contacts.AppendChild(contact);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Exception" + ex);
                    }
                }
                handler.ISGetNewContactsResult(contacts, new Tuple<IContactService>(contactService));

                contacts = doc.CreateElement("Contacts");
                items = doc.SelectNodes("//Item[translate(@SyncAction, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'modify']");
                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement contact = doc.CreateElement("Contact");
                        contact.InnerXml = item.InnerXml;
                        contact.SetAttribute("Key", item.GetAttribute("Key"));
                        contacts.AppendChild(contact);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Exception" + ex);
                    }
                }
                handler.ISGetModifiedContactsResult(contacts);

                contacts = doc.CreateElement("Contacts");
                items = doc.SelectNodes("//Item[translate(@SyncAction, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'remove']");
                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement contact = doc.CreateElement("Contact");
                        contact.InnerXml = item.InnerXml;
                        contact.SetAttribute("Key", item.GetAttribute("Key"));
                        contacts.AppendChild(contact);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Exception" + ex);
                    }
                }
                handler.ISGetDeletedContactsResult(contacts);
            }
            catch (WAException ex)
            {
                Logger.Current.Error("Exception occured in SyncWebToOutlookContactsResults. Error: ", ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured in SyncWebToOutlookContactsResults. Error: ", ex);

                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml SyncWebToOutlookCalendar(XDocument xmlWebToOutlookCalendar)
        {
            Logger.Current.Verbose("WASyncWSController/SyncWebToOutlookCalendar");
            string session = xmlWebToOutlookCalendar.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;
            ResponseXml xmlResp = new ResponseXml(session, "SyncWebToOutlookCalendar");
            WASyncHandler handler = new WASyncHandler(session);
            XmlDocument doc = new XmlDocument();
            DateTime? timeStamp = null;
            ConcurrentUsers.ConcurrentUser.Remove(null);
            var context = HttpContext.Current;
            try
            {
                ConcurrentUser concurrentUser = ConcurrentUsers.ConcurrentUser.Where(c => c != null && c.SessionKey == session).FirstOrDefault();
                if (concurrentUser == null)
                    throw new Exception(string.Format("User with session {0} not found", session));
                int? maxNumRecords = int.Parse(context.Request.Params["MaxNumRecords"]);
                if (maxNumRecords != null && maxNumRecords.Value < 0)
                    throw new Exception("Invalid argument. maxNumRecords must be greater than 0");
                if (maxNumRecords != null && maxNumRecords.Value == 0)
                    maxNumRecords = null;
                bool firstSync = !string.IsNullOrEmpty(context.Request.Params["FirstSync"]) && context.Request.Params["FirstSync"] != "0"
                    && context.Request.Params["FirstSync"].ToLower() != "false";

                doc.LoadXml("<Calendar><count NumberOfItems=\"\"/></Calendar>");

                XmlElement appts = handler.ISGetNewAppointments(session, concurrentUser, timeStamp, maxNumRecords, firstSync, new Tuple<ITourService>(tourService));
                if (appts != null)
                {
                    foreach (XmlNode item in appts.ChildNodes)
                    {
                        XmlElement node = doc.CreateElement("Item");
                        node.InnerXml = item.InnerXml;
                        node.SetAttribute("Key", item.Attributes["Key"].Value);
                        node.SetAttribute("Guid", item.Attributes["Key"].Value);
                        if (item.Attributes["FolderPath"] != null)
                            node.SetAttribute("FolderPath", item.Attributes["FolderPath"].Value);
                        node.SetAttribute("SyncAction", "Add");
                        doc.DocumentElement.FirstChild.AppendChild(node);
                    }
                    if (maxNumRecords != null)
                        maxNumRecords -= appts.ChildNodes.Count;
                }

                if (maxNumRecords == null || maxNumRecords > 0)
                {
                    appts = handler.ISGetModifiedAppointments(session, concurrentUser, timeStamp, maxNumRecords, firstSync
                        , new Tuple<ITourService>(tourService));
                    if (appts != null)
                    {
                        foreach (XmlNode item in appts.ChildNodes)
                        {
                            XmlElement node = doc.CreateElement("Item");
                            node.InnerXml = item.InnerXml;
                            node.SetAttribute("Key", item.Attributes["Key"].Value);
                            node.SetAttribute("Guid", item.Attributes["Key"].Value);
                            if (item.Attributes["FolderPath"] != null)
                                node.SetAttribute("FolderPath", item.Attributes["FolderPath"].Value);
                            node.SetAttribute("SyncAction", "Modify");
                            doc.DocumentElement.FirstChild.AppendChild(node);
                        }
                        if (maxNumRecords != null)
                            maxNumRecords -= appts.ChildNodes.Count;
                    }
                }

                if (maxNumRecords == null || maxNumRecords > 0)
                {
                    appts = handler.ISGetDeletedAppointments(session, concurrentUser, timeStamp, maxNumRecords, new Tuple<ITourService>(tourService));
                    if (appts != null)
                    {
                        foreach (XmlNode item in appts.ChildNodes)
                        {
                            XmlElement node = doc.CreateElement("Item");
                            node.InnerXml = item.InnerXml;
                            node.SetAttribute("Key", item.Attributes["Key"].Value);
                            node.SetAttribute("Guid", item.Attributes["Key"].Value);
                            node.SetAttribute("SyncAction", "Remove");
                            doc.DocumentElement.FirstChild.AppendChild(node);
                        }
                    }
                }

                doc.DocumentElement.FirstChild.Attributes["NumberOfItems"].Value = doc.DocumentElement.FirstChild.ChildNodes.Count.ToString();
                xmlResp.ISResponse.InnerXml = doc.DocumentElement.OuterXml;
            }
            catch (WAException ex)
            {
                Logger.Current.Error("Exception occured in SyncWebToOutlookCalendar. Error: ", ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("User with session"))
                    Logger.Current.Informational("Exception occured in SyncWebToOutlookCalendar. message: " + ex.Message + "stacktrace :" + ex.StackTrace);
                else
                    Logger.Current.Error("Exception occured in SyncWebToOutlookCalendar. Error: ", ex);
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml SyncWebToOutlookCalendarResult(XDocument xmlAppointments)
        {
            Logger.Current.Verbose("WASyncWSController/SyncWebToOutlookCalendar");
            string session = xmlAppointments.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;

            ResponseXml xmlResp = new ResponseXml(session, "SyncWebToOutlookCalendar");
            WASyncHandler handler = new WASyncHandler(session);
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(xmlAppointments.ToString());
                XmlElement appts = doc.CreateElement("Appointments");
                XmlNodeList items = doc.SelectNodes("//Item[translate(@SyncAction, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'add']");
                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement appt = doc.CreateElement("Appointment");
                        appt.InnerXml = item.InnerXml;
                        appt.SetAttribute("Key", item.GetAttribute("Key"));
                        appts.AppendChild(appt);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Exception" + ex);
                    }
                }
                handler.ISGetNewAppointmentsResult(appts, new Tuple<IContactService>(contactService));

                appts = doc.CreateElement("Appointments");
                items = doc.SelectNodes("//Item[translate(@SyncAction, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'modify']");
                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement appt = doc.CreateElement("Appointment");
                        appt.InnerXml = item.InnerXml;
                        appt.SetAttribute("Key", item.GetAttribute("Key"));
                        appts.AppendChild(appt);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Exception" + ex);
                    }
                }
                handler.ISGetModifiedAppointmentsResult(appts, new Tuple<IContactService>(contactService));

                appts = doc.CreateElement("Appointments");
                items = doc.SelectNodes("//Item[translate(@SyncAction, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'remove']");
                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement appt = doc.CreateElement("Appointment");
                        appt.InnerXml = item.InnerXml;
                        appt.SetAttribute("Key", item.GetAttribute("Key"));
                        appts.AppendChild(appt);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Exception" + ex);
                    }
                }
                handler.ISGetDeletedAppointmentsResult(appts);
            }
            catch (WAException ex)
            {
                Logger.Current.Error("Exception occured in SyncWebToOutlookCalendarResult. Error: ", ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured in SyncWebToOutlookCalendarResult. Error: ", ex);

                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml SyncWebToOutlookTasks(XDocument xmlWebToOutlookTasks)
        {

            Logger.Current.Verbose("WASyncWSController/SyncWebToOutlookContacts");
            XmlDocument doc = new XmlDocument();
            var context = HttpContext.Current;
            DateTime? time = null;

            string session = xmlWebToOutlookTasks.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;
            ResponseXml xmlResp = new ResponseXml(session, "SyncWebToOutlookTasks");
            WASyncHandler handler = new WASyncHandler(session);
            try
            {
                ConcurrentUser concurrentUser = ConcurrentUsers.ConcurrentUser.Where(c => c != null && c.SessionKey == session).FirstOrDefault();
                if (concurrentUser == null)
                    throw new System.Exception(string.Format("User with session {0} not found", session));
                int? maxitems = null;
                int? maxNumRecords = int.Parse(context.Request.Params["MaxNumRecords"]);
                if (!string.IsNullOrEmpty(context.Request.Params["MaxNumRecords"]) &&
                    context.Request.Params["MaxNumRecords"] != "0")
                {
                    try
                    {
                        maxitems = Int32.Parse(context.Request.Params["MaxNumRecords"]);
                    }
                    catch
                    {
                        throw new Exception("Invalid argument MaxNumRecords");
                    }
                }
                bool firstSync = !string.IsNullOrEmpty(context.Request.Params["FirstSync"]) && context.Request.Params["FirstSync"] != "0" && context.Request.Params["FirstSync"].ToLower() != "false";

                doc.LoadXml("<Tasks><count NumberOfItems=\"\"/></Tasks>");

                XmlElement tasks = handler.ISGetNewTasks(session, concurrentUser, time, maxNumRecords, firstSync, new Tuple<IActionService>(actionService));
                if (tasks != null)
                {
                    foreach (XmlNode item in tasks.ChildNodes)
                    {
                        XmlElement node = doc.CreateElement("Item");
                        node.InnerXml = item.InnerXml;
                        node.SetAttribute("Key", item.Attributes["Key"].Value);
                        node.SetAttribute("Guid", item.Attributes["Key"].Value);
                        if (item.Attributes["FolderPath"] != null)
                            node.SetAttribute("FolderPath", item.Attributes["FolderPath"].Value);
                        node.SetAttribute("SyncAction", "Add");
                        doc.DocumentElement.FirstChild.AppendChild(node);
                    }
                    if (maxNumRecords != null)
                        maxNumRecords -= tasks.ChildNodes.Count;
                }

                if (maxNumRecords == null || maxNumRecords > 0)
                {
                    tasks = handler.ISGetModifiedTasks(session, concurrentUser, time, maxNumRecords, firstSync
                        , new Tuple<IActionService>(actionService));
                    if (tasks != null)
                    {
                        foreach (XmlNode item in tasks.ChildNodes)
                        {
                            XmlElement node = doc.CreateElement("Item");
                            node.InnerXml = item.InnerXml;
                            node.SetAttribute("Key", item.Attributes["Key"].Value);
                            node.SetAttribute("Guid", item.Attributes["Key"].Value);
                            if (item.Attributes["FolderPath"] != null)
                                node.SetAttribute("FolderPath", item.Attributes["FolderPath"].Value);
                            node.SetAttribute("SyncAction", "Modify");
                            doc.DocumentElement.FirstChild.AppendChild(node);
                        }
                        if (maxNumRecords != null)
                            maxNumRecords -= tasks.ChildNodes.Count;
                    }
                }

                if (maxNumRecords == null || maxNumRecords > 0)
                {
                    tasks = handler.ISGetDeletedTasks(session, concurrentUser, time, maxNumRecords, new Tuple<IActionService>(actionService));
                    if (tasks != null)
                    {
                        foreach (XmlNode item in tasks.ChildNodes)
                        {
                            XmlElement node = doc.CreateElement("Item");
                            node.InnerXml = item.InnerXml;
                            node.SetAttribute("Key", item.Attributes["Key"].Value);
                            node.SetAttribute("Guid", item.Attributes["Key"].Value);
                            node.SetAttribute("SyncAction", "Remove");
                            doc.DocumentElement.FirstChild.AppendChild(node);
                        }
                    }
                }

                doc.DocumentElement.FirstChild.Attributes["NumberOfItems"].Value = doc.DocumentElement.FirstChild.ChildNodes.Count.ToString();
                xmlResp.ISResponse.InnerXml = doc.DocumentElement.OuterXml;
            }
            catch (WAException ex)
            {
                Logger.Current.Error("Exception occured in SyncWebToOutlookTasks. Error: ", ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("User with session"))
                    Logger.Current.Informational("Exception occured in SyncWebToOutlookTasks. message: " + ex.Message + "stacktrace :" + ex.StackTrace);
                else
                    Logger.Current.Error("Exception occured in SyncWebToOutlookTasks. Error: ", ex);

                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml SyncWebToOutlookTasksResult(XDocument xmlTasks)
        {
            string session = xmlTasks.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;
            ResponseXml xmlResp = new ResponseXml(session, "SyncOutlookToWebContactsResult");
            WASyncHandler handler = new WASyncHandler(session);
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlTasks.ToString());
                XmlElement tasks = doc.CreateElement("Tasks");
                XmlNodeList items = doc.SelectNodes("//Item[translate(@SyncAction, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'add']");
                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement task = doc.CreateElement("Task");
                        task.InnerXml = item.InnerXml;
                        task.SetAttribute("Key", item.GetAttribute("Key"));
                        tasks.AppendChild(task);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Exception" + ex);
                    }
                }
                handler.ISGetNewTasksResult(tasks, new Tuple<IContactService>(contactService));

                tasks = doc.CreateElement("Tasks");
                items = doc.SelectNodes("//Item[translate(@SyncAction, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'modify']");
                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement task = doc.CreateElement("Task");
                        task.InnerXml = item.InnerXml;
                        task.SetAttribute("Key", item.GetAttribute("Key"));
                        tasks.AppendChild(task);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Exception" + ex);
                    }
                }
                handler.ISGetModifiedTasksResult(tasks, new Tuple<IContactService>(contactService));

                tasks = doc.CreateElement("Tasks");
                items = doc.SelectNodes("//Item[translate(@SyncAction, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'remove']");
                foreach (XmlElement item in items)
                {
                    try
                    {
                        XmlElement task = doc.CreateElement("Task");
                        task.InnerXml = item.InnerXml;
                        task.SetAttribute("Key", item.GetAttribute("Key"));
                        tasks.AppendChild(task);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Exception" + ex);
                    }
                }
                handler.ISGetDeletedTasksResult(tasks);
            }
            catch (WAException ex)
            {
                Logger.Current.Error("Exception occured in SyncWebToOutlookTasksResult. Error: ", ex);

                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured in SyncWebToOutlookTasksResult. Error: ", ex);

                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml UploadEmail(XDocument uploadXML)
        {
            Logger.Current.Verbose("Uploading Email");
            ResponseXml xmlResp = new ResponseXml("", "UploadEmail");
            WASyncHandler handler = new WASyncHandler("");
            Exception itemException = null;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(uploadXML.ToString());
                string emailkey = handler.ISUploadEmail(uploadXML
                    , new Tuple<IContactService, IMessageService, IIndexingService, IUserService>(contactService, messageService, indexingService, userService));
                if (string.IsNullOrEmpty(emailkey))
                    throw new Exception("Error in UploadEmail. Invalid email key generated");
                xmlResp.ISResponse.InnerXml = "<EmailKey>" + emailkey + "</EmailKey>";
                var nodes = doc.SelectNodes("//EmailMessage");
                foreach (XmlElement item in nodes)
                {
                    item.SetAttribute("Key", emailkey);
                }
                Logger.Current.Verbose("Completed. Returning response");
                xmlResp.ISResponse.FirstChild.InnerXml = doc.DocumentElement.InnerXml;
            }
            catch (WAException ex)
            {
                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
                itemException = ex;
            }
            catch (Exception ex)
            {
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
                itemException = ex;
            }
            return xmlResp;
        }


        public ResponseXml UploadAttachment(string session, string Emailkey, string FileName, byte[] file)
        {
            ResponseXml xmlResp = new ResponseXml(session, "UploadAttachment");
            WASyncHandler handler = new WASyncHandler(session);

            try
            {
                if (string.IsNullOrEmpty(Emailkey) || string.IsNullOrEmpty(FileName))
                    throw new Exception("Invalid input params");

                handler.ISUploadAttachment(Emailkey, FileName, new System.IO.MemoryStream(file));
            }
            catch (WAException ex)
            {
                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml CancelUploadEmail(string session, string Emailkey)
        {
            ResponseXml xmlResp = new ResponseXml(session, "CancelUploadEmail");
            WASyncHandler handler = new WASyncHandler(session);

            try
            {
                if (string.IsNullOrEmpty(Emailkey))
                    throw new Exception("Invalid input params");

                handler.ISCancelUploadEmail(Emailkey);
            }
            catch (WAException ex)
            {
                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml GetServerTime(XDocument xDocument)
        {
            string session = xDocument.Descendants("Session").FirstOrDefault().Attribute("SessionKey").Value;
            ResponseXml xmlResp = new ResponseXml(session, "GetServerTime");
            WASyncHandler handler = new WASyncHandler(session);

            try
            {
                xmlResp.ISResponse.InnerXml = "<ServerTime>" + handler.ISGetServerTime().ToString("yyyyMMddHHmmss") + "</ServerTime>";
            }
            catch (WAException ex)
            {
                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml EnumerateFiles(string session)
        {
            ResponseXml xmlResp = new ResponseXml(session, "EnumerateFiles");
            WASyncHandler handler = new WASyncHandler(session);

            try
            {
                xmlResp.ISResponse.InnerXml = handler.ISEnumerateFiles().OuterXml;
            }
            catch (WAException ex)
            {
                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml GetFile(string session, string filekey)
        {
            ResponseXml xmlResp = new ResponseXml(session, "GetFile");
            WASyncHandler handler = new WASyncHandler(session);

            try
            {
                xmlResp.ISResponse.InnerXml = handler.ISGetFile(filekey).OuterXml;
            }
            catch (WAException ex)
            {
                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml InsertFile(string session, string FileXML)
        {
            ResponseXml xmlResp = new ResponseXml(session, "InsertFile");
            WASyncHandler handler = new WASyncHandler(session);
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(FileXML);
                DateTime lastModifiedUTC;
                string filekey = handler.ISInsertFile(doc.DocumentElement, out lastModifiedUTC);
                if (string.IsNullOrEmpty(filekey))
                    throw new Exception("Error in InsertFile. Invalid file key generated");
                xmlResp.ISResponse.InnerXml = string.Format("<File Key=\"{0}\" LastModifiedUTC=\"{1}\" />", filekey, lastModifiedUTC.ToString("yyyyMMddHHmmss"));
            }
            catch (WAException ex)
            {
                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml UpdateFile(string session, string FileXML)
        {
            ResponseXml xmlResp = new ResponseXml(session, "UpdateFile");
            WASyncHandler handler = new WASyncHandler(session);
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(FileXML);
                DateTime lastModifiedUTC;
                handler.ISUpdateFile(doc.DocumentElement, out lastModifiedUTC);
                xmlResp.ISResponse.InnerXml = string.Format("<File LastModifiedUTC=\"{0}\" />", lastModifiedUTC.ToString("yyyyMMddHHmmss"));
            }
            catch (WAException ex)
            {
                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }
            return xmlResp;
        }


        public ResponseXml DeleteFile(string session, string filekey)
        {
            ResponseXml xmlResp = new ResponseXml(session, "DeleteFile");
            WASyncHandler handler = new WASyncHandler(session);

            try
            {
                handler.ISDeleteFile(filekey);
            }
            catch (WAException ex)
            {
                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }

            return xmlResp;
        }


        public ResponseXml DetachFile(string session, string filekey)
        {
            ResponseXml xmlResp = new ResponseXml(session, "DetachFile");
            WASyncHandler handler = new WASyncHandler(session);

            try
            {
                handler.ISDetachFile(filekey);
            }
            catch (WAException ex)
            {
                xmlResp.RetStatus = ex.Status;
                xmlResp.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                xmlResp.RetStatus = -1;
                xmlResp.ErrorString = ex.Message;
            }

            return xmlResp;
        }
    }

}