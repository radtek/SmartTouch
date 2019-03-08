using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.WebService.Helpers;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.Contacts;
using Newtonsoft.Json;
using SmartTouch.CRM.WebService.Models;
using LandmarkIT.Enterprise.Extensions;
using System.Linq;
using SmartTouch.CRM.Entities;
using System.Web.Script.Serialization;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using System.Web;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using System.ComponentModel;
using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;
using System.Globalization;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating contacts controller for contacts module
    /// </summary>
    public class ContactsController : SmartTouchApiController
    {
        private readonly IContactService contactService;
        private readonly IActionService actionService;
        private readonly ICommunicationService communicationservice;
        private readonly IContactRelationshipService contactRelationshipService;
        private readonly IDropdownValuesService dropdownValuesService;
        private readonly IUserService userService;

        /// <summary>
        /// Creating constructor for contacts controller for accessing
        /// </summary>
        /// <param name="contactService"></param>
        /// <param name="actionService"></param>
        /// <param name="communicationservice"></param>
        /// <param name="contactRelationshipService"></param>
        /// <param name="dropdownValuesService"></param>
        public ContactsController(IContactService contactService, IActionService actionService, ICommunicationService communicationservice, IContactRelationshipService contactRelationshipService, IDropdownValuesService dropdownValuesService, IUserService userService)
        {
            this.contactService = contactService;
            this.actionService = actionService;
            this.communicationservice = communicationservice;
            this.contactRelationshipService = contactRelationshipService;
            this.dropdownValuesService = dropdownValuesService;
            this.userService = userService;
        }

        /// <summary>
        /// Gets All types of contacts in the system.
        /// </summary>
        /// <param name="query">Name of the contact. It works as a search string, First Name, Last Name for persons and Company Name for companies.</param> 
        /// <param name="limit">Limit of the contacts for page in grid.</param>
        /// <param name="pageNumber">Page number in contacts grid.</param>
        /// <returns>contacts based on parametrs</returns>   
        [HttpGet]
        public HttpResponseMessage Search(string query, int limit, int pageNumber)
        {
            ServiceResponseBase response = contactService.GetAllContacts<ContactListEntry>(new SearchContactsRequest()
            {
                Query = query,
                Limit = limit,
                PageNumber = pageNumber,
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            });

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// ContactIndexing.
        /// </summary>
        /// <param name="contactIds">contact ids.</param>    
        /// <returns></returns>           
        [AllowAnonymous]
        [Route("ContactIndexing")]
        public HttpResponseMessage ContactIndexing(int[] contactIds)
        {
            ContactIndexingResponce response = contactService.ContactIndexing(new ContactIndexingRequest()
            {
                ContactIds = contactIds,
                Ids = contactIds.ToLookup(o => o, o => { return true; })
            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// ImportDataUpdate.
        /// </summary>
        /// <param name="contactIds">contact ids.</param>    
        /// <param name="tagIds">tagids.</param>    
        /// <returns></returns>             
        [AllowAnonymous]
        [Route("ImportDataUpdate")]
        public HttpResponseMessage ImportDataUpdate(int[] contactIds, int[] tagIds)
        {
            ImportDataUpdateResponce response = contactService.ImportDataUpdate(new ImportDataUpdateRequest()
            {
                ContactIds = contactIds,
                TagIds = tagIds
            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Persons in the system.
        /// </summary>
        /// <param name="name">Name of the contact. It works as a search string, First Name, Last Name of persons.</param>
        /// <param name="limit">Limit of the contacts for page in grid.</param>
        /// <param name="pageNumber">Page number in contacts grid.</param>
        /// <returns>persons based on parametrs</returns>
        [Route("Persons")]
        public HttpResponseMessage SearchPersons(string name, int limit, int pageNumber)
        {
            ServiceResponseBase response = contactService.GetPersons<ContactListEntry>(new SearchContactsRequest()
            {
                Query = name,
                Limit = limit,
                PageNumber = pageNumber,
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Companies in the system.
        /// </summary>
        /// <param name="name">Name of the contact. It works as a search string, Company Name of companies.</param>
        /// <param name="limit">Limit of the contacts for page in grid.</param>
        /// <param name="pageNumber">Page number in contacts grid.</param>
        /// <returns>Companies based on parameters</returns>
        [Route("Companies")]
        public HttpResponseMessage SearchCompanies(string name, int limit, int pageNumber)
        {
            ServiceResponseBase response = contactService.GetCompanies<ContactListEntry>(new SearchContactsRequest()
            {
                Query = name,
                Limit = limit,
                PageNumber = pageNumber,
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Search Company names.
        /// </summary>
        /// <param name="query">String query.</param>
        /// <param name="accountId">Id of the Account.</param>
        /// <returns>Company Names by accountId</returns>
        [Route("Company/Name")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage SearchCompanyNames(string query, int accountId)
        {
            AutoCompleteSearchResponse response = contactService.SearchCompanyByName(new AutoCompleteSearchRequest() { Query = query, AccountId = accountId });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Search contact emails.
        /// </summary>
        /// <param name="query">String query.</param>
        /// <returns>Contact EmailId </returns>
        [Route("Contact/EmailId")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage SearchContactEmailId(string query)
        {
            AutoCompleteSearchResponse response = contactService.SearchContactWithEmailId(new AutoCompleteSearchRequest()
            {
                Query = query,
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Search contact emails.
        /// </summary>
        /// <param name="query">String query.</param>
        /// <returns>Contact Phone Number details </returns>
        [Route("Contact/Phone")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage SearchContactPhone(string query)
        {
            AutoCompleteSearchResponse response = contactService.SearchContactWithPhone(new AutoCompleteSearchRequest() { Query = query, AccountId = this.AccountId, RequestedBy = this.UserId, RoleId = this.RoleId });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Search Titles.
        /// </summary>
        /// <param name="query">String query.</param>
        /// <returns>Contact tiles</returns>
        [Route("Person/Title")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage SearchContactTitles(string query)
        {
            AutoCompleteSearchResponse response = contactService.SearchContactTitles(new AutoCompleteSearchRequest() { Query = query, AccountId = this.AccountId });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Search Titles.
        /// </summary>
        /// <param name="query">String query.</param>
        /// <param name="accountId">Account Id.</param>
        /// <returns>Contact Full Names</returns>
        [Route("Contact/FullName")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage SearchContactFullNames(string query, int accountId)
        {
            AutoCompleteSearchResponse response = contactService.SearchContactFullName(
                new AutoCompleteSearchRequest() { Query = query, AccountId = this.AccountId, RoleId = this.RoleId, RequestedBy = this.UserId });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Person by id.
        /// </summary>
        /// <param name="id">The Id value that uniquely identifies a person.</param>
        /// <returns>Person Details</returns>
        [Route("Person")]
        public HttpResponseMessage GetPerson(int id)
        {
            ServiceResponseBase response = contactService.GetPerson(new GetPersonRequest(id)
            {
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Company by id.
        /// </summary>
        /// <param name="id">The Id value that uniquely identifies a company.</param>
        /// <returns>Company Details</returns>
        [Route("Company")]
        public HttpResponseMessage GetCompany(int id)
        {
            ServiceResponseBase response = contactService.GetCompany(new GetCompanyRequest(id)
            {
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Inserts a new person.
        /// </summary>
        /// <param name="PersonViewModel">Properties of a new person</param>
        /// <returns>Person Insertion Details</returns>
        [Route("Person")]
        [HttpPost]
        public HttpResponseMessage PostPerson(PersonViewModel viewmodel)
        {
            viewmodel.FirstName = !string.IsNullOrEmpty(viewmodel.FirstName) ? viewmodel.FirstName.Trim() : viewmodel.FirstName;
            viewmodel.LastName = !string.IsNullOrEmpty(viewmodel.LastName) ? viewmodel.LastName.Trim() : viewmodel.LastName;
            if (viewmodel.Phones.IsAny())
            {
                viewmodel.Phones.Each(pn =>
                {
                    pn.Number = !string.IsNullOrEmpty(pn.Number) ? pn.Number.Trim() : pn.Number;
                });
            }

            if (viewmodel.CustomFields.IsAny())
            {
                viewmodel.CustomFields.Each(cm =>
                {
                    cm.Value = !string.IsNullOrEmpty(cm.Value) ? cm.Value.Trim() : cm.Value;
                });
            }

            var json = new JavaScriptSerializer().Serialize(viewmodel);
            APILeadSubmissionViewModel apiLeadSubmissionViewModel = new APILeadSubmissionViewModel();
            apiLeadSubmissionViewModel.ContactID = null;
            apiLeadSubmissionViewModel.AccountID = this.AccountId;
            apiLeadSubmissionViewModel.SubmittedData = json;
            apiLeadSubmissionViewModel.SubmittedOn = DateTime.Now.ToUniversalTime();
            apiLeadSubmissionViewModel.IsProcessed = (byte)SubmittedFormStatus.ReadyToProcess;
            apiLeadSubmissionViewModel.Remarks = null;
            apiLeadSubmissionViewModel.OwnerID = this.UserId;
            apiLeadSubmissionViewModel.FormID = viewmodel.FormId;
            apiLeadSubmissionViewModel.IPAddress = this.GetUserIp(Request);
            bool allowSubmission = true;
            InsertAPILeadSubmissionResponse response = new InsertAPILeadSubmissionResponse();
            string primaryEmail = viewmodel.Emails.IsAny() ? viewmodel.Emails.Where(e => e.IsPrimary == true).Select(s => s.EmailId).FirstOrDefault() : string.Empty;
            short leadSourceId = viewmodel.SelectedLeadSource.IsAny() ? viewmodel.SelectedLeadSource.Select(s => s.DropdownValueID).FirstOrDefault() : (short)0;

            if (!string.IsNullOrEmpty(viewmodel.FirstName) && !string.IsNullOrEmpty(viewmodel.LastName))
            {
                if (leadSourceId == 0)
                {
                    response.Exception = new UnsupportedOperationException("Lead Source is mandatory.");
                    allowSubmission = false;

                }
            }
            else if (!string.IsNullOrEmpty(primaryEmail))
            {
                if (leadSourceId == 0)
                {
                    response.Exception = new UnsupportedOperationException("Lead Source is mandatory.");
                    allowSubmission = false;
                }
            }
            else
            {
                response.Exception = new UnsupportedOperationException("Email/First Name & Last Name, Lead Source are mandatory.");
                allowSubmission = false;
            }

            if (allowSubmission)
                response = contactService.InsertAPILeadSubmissionData(new InsertAPILeadSubmissionRequest() { ApiLeadSubmissionViewModel = apiLeadSubmissionViewModel });

            if (response.Exception != null)
            {
                var message = response.Exception.Message.Replace("[|", "").Replace("|]", "");
                response.Exception = new UnsupportedOperationException(message);
            }
            return Request.BuildResponse(response);
        }

        private string GetUserIp(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = request.Properties["MS_HttpContext"] as HttpContextBase;
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            return null;
        }

        /// <summary>
        /// Inserts a new company.
        /// </summary>
        /// <param name="viewModel">Properties of a new company</param>
        /// <returns>Company Insertion Details</returns>
        [Route("Company")]
        [HttpPost]
        public HttpResponseMessage PostCompany(CompanyViewModel viewModel)
        {
            viewModel.FirstContactSource = Entities.ContactSource.API;
            InsertCompanyResponse response = contactService.InsertCompany(new InsertCompanyRequest() { CompanyViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Updates a Person.
        /// </summary>
        /// <param name="viewModel">Properties of a new person</param>
        /// <returns>Person Updated Details</returns>
        [Route("Person")]
        [HttpPut]
        public HttpResponseMessage PutPerson(PersonViewModel viewModel)
        {
            UpdatePersonResponse response = contactService.UpdatePerson(new UpdatePersonRequest()
            {
                PersonViewModel = viewModel,
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Updates a Company.
        /// </summary>
        /// <param name="viewModel">Properties  of a company</param>
        /// <returns>Company Updated Details</returns>
        [Route("Company")]
        [HttpPut]
        public HttpResponseMessage PutCompany(CompanyViewModel viewModel)
        {
            UpdateCompanyResponse response = contactService.UpdateCompany(new UpdateCompanyRequest()
            {
                CompanyViewModel = viewModel,
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Deletes a contact.
        /// </summary>
        /// <param name="id">Id of the Contact.</param>
        /// <returns> Conatct Deletion Details</returns>
        [Route("Contacts")]
        [HttpDelete]
        public HttpResponseMessage Delete(int id)
        {
            DeactivateContactRequest request = new DeactivateContactRequest(id)
            {
                AccountId = this.AccountId,
                RequestedBy = this.UserId,
                RoleId = this.RoleId
            };

            DeactivateContactResponse response = contactService.Deactivate(request);
            return Request.BuildResponse(response);
        }

        #region Action

        /// <summary>
        /// Insert a action.
        /// </summary>
        /// <param name="viewModel">View Model of the Action</param>
        /// <returns></returns>
        [Route("Action/InsertAction")]
        [HttpPost]
        public HttpResponseMessage PostAction(ActionViewModel viewModel)
        {
            InsertActionResponse response = actionService.InsertAction(new InsertActionRequest() { ActionViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Update a action.
        /// </summary>
        /// <param name="viewModel">View Model of the Action</param>
        /// <returns></returns>
        [Route("Action/UpdateAction")]
        [HttpPost]
        public HttpResponseMessage PutAction(ActionViewModel viewModel)
        {
            UpdateActionResponse response = actionService.UpdateAction(new UpdateActionRequest() { ActionViewModel = viewModel });
            return Request.BuildResponse(response);
        }


        /// <summary>
        /// Get a action.
        /// </summary>
        /// <param name="actionId">Id of the Action</param>
        /// <returns></returns>
        [Route("Action/GetAction")]
        public HttpResponseMessage GetAction(int actionId)
        {
            GetActionResponse response = actionService.GetAction(new GetActionRequest() { Id = actionId });
            return Request.BuildResponse(response);
        }


        /// <summary>
        /// Get a actions of Contact.
        /// </summary>
        /// <param name="contactId">Id of the contact</param>
        /// <returns></returns>
        [Route("Action/GetContactAction")]
        public HttpResponseMessage GetContactActions(int contactId)
        {
            GetActionListResponse response = actionService.GetContactActions(new GetActionListRequest() { Id = contactId });
            return Request.BuildResponse(response);
        }
        /// <summary>
        /// Delete the action
        /// </summary>
        /// <param name="actionid">Action Id.</param> 
        /// <param name="contactId">Id of the contact related to action</param>       
        /// <returns>HttpResponseMessage</returns>   
        [Route("ActionsDelete")]
        [HttpDelete]
        public HttpResponseMessage DeleteAction(int actionid, int contactId)
        {
            DeactivateActionContactResponse response = actionService.
                ContactDeleteForAction(new DeactivateActionContactRequest() { ActionId = actionid, ContactId = contactId });
            return Request.BuildResponse(response);
        }
        #endregion


        /// <summary>
        /// Delete the index of the contact
        /// </summary>
        /// <param name="index">Index of the contact</param>
        /// <returns>Deleting Contact Index Details</returns>
        [Route("DeleteIndex")]
        public HttpResponseMessage DeleteIndex(string index)
        {
            DeleteIndexResponse response = contactService.DeleteIndex(new DeleteIndexRequest() { Name = index });
            return Request.BuildResponse(response);
        }
        /// <summary>
        /// Get the timeline of the contact
        /// </summary>
        /// <param name="Id">Id of the contact</param>
        /// <param name="limit">Limit of the contacts</param>
        /// <param name="pageNumber">PageNumber of the contact</param>
        /// <returns>Contact Timeline Data</returns>
        [Route("Timeline")]
        [HttpGet]
        public async Task<HttpResponseMessage> Timelines(int Id, int limit, int pageNumber)
        {
            ServiceResponseBase response = await contactService.GetTimeLinesDataAsync(new GetTimeLineRequest() { ContactID = Id, Limit = limit, PageNumber = pageNumber });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get contact web visits 
        /// </summary>
        /// <param name="contactId">Id of the contact</param>
        /// <returns>Contact Web Visits</returns>
        [Route("webvisits")]
        [HttpGet]
        public HttpResponseMessage GetContactWebVisits(int contactId)
        {
            GetContactWebVisitReportResponse response = new GetContactWebVisitReportResponse();
            response.WebVisits = contactService.GetContactWebVisits(new GetContactWebVisitReportRequest() { ContactId = contactId }).WebVisits;
            return Request.BuildResponse(response);
        }

        [Route("contactsummary")]
        /// <summary>
        /// Get contact summary
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        public HttpResponseMessage GetContactSummary(int contactId)
        {
            GetContactSummaryResponse response = new GetContactSummaryResponse();
            response.ContactSummary = contactService.GetContactSummary(new GetContactSummaryRequest() { ContactId = contactId }).ContactSummary;
            response.ContactSummaryDetails = JsonConvert.DeserializeObject<IEnumerable<ContactSummaryViewModel>>(response.ContactSummary);
            if (response.ContactSummaryDetails.IsAny())
            {
                response.ContactSummaryDetails.Each(d =>
                {
                    string Details = string.Empty;
                    d.SummaryDetails = d.NoteDetails;
                    if (d.NoteDetails.Length > 75)
                    {
                        Details = d.NoteDetails.Substring(0, 74);
                        d.NoteDetails = Details + "...";
                    }
                });
            }
            return Request.BuildResponse(response);

        }
        [Route("getschema")]
        [HttpGet]
        public HttpResponseMessage GetSchema()
        {
            var inputFormat = new PersonViewModelType();
            inputFormat.Properties.Add(new InputFormat("First Name", "Text", "FirstName"));
            inputFormat.Properties.Add(new InputFormat("Last Name", "Text", "LastName"));
            inputFormat.Properties.Add(new InputFormat("Company Name", "Text", "CompanyName"));
            inputFormat.Properties.Add(new InputFormat("Title", "Text", "Title"));
            inputFormat.Properties.Add(new InputFormat("Lead Source", "Dropdown", "SelectedLeadSource", true, "DropdownValueViewModel", dropdownId: 5));
            //inputFormat.Properties.Add(new InputFormat("Do Not Email", "Boolean", "DoNotEmail"));
            inputFormat.Properties.Add(new InputFormat("ContactID", "Number", "ContactID"));
            inputFormat.Properties.Add(new InputFormat("Contact Image Url", "Text", "ContactImageUrl"));
            //inputFormat.Properties.Add(new InputFormat("Profile Image Key", "Guid", "ProfileImageKey"));
            inputFormat.Properties.Add(new InputFormat("Account ID", "Number", "AccountID"));
            inputFormat.Properties.Add(new InputFormat("Communication", "", "Communication", false, "CommunicationViewModel"));
            inputFormat.Properties.Add(new InputFormat("Addresses", "", "Addresses", false, "AddressViewModel"));
            inputFormat.Properties.Add(new InputFormat("Phones", "", "Phones", true, "Phone"));
            inputFormat.Properties.Add(new InputFormat("Emails", "", "Emails", true, "Email"));
            inputFormat.Properties.Add(new InputFormat("SocialMediaUrls", "", "SocialMediaUrls", true, "Url"));
            //inputFormat.Properties.Add(new InputFormat("TagsList", "", "TagsList", true, "Tag"));
            inputFormat.Properties.Add(new InputFormat("Custom Fields", "", "CustomFields", true, "ContactCustomFieldMapViewModel"));
            //inputFormat.Properties.Add(new InputFormat("Life Cycle Stage", "Dropdown", "LifeCycleStage", false, "DropdownValueViewModel",dropdownId:2));
            inputFormat.Properties.Add(new InputFormat("Communities", "Dropdown", "Communities", true, "DropdownValueViewModel", dropdownId: 6));
            //inputFormat.Properties.Add(new InputFormat("First Contact Source", "Text", "FirstContactSource", value : "5"));
            inputFormat.Properties.Add(new InputFormat("FormId", "Number", "FormId"));


            inputFormat.ObjectTypes = new List<InputObjectFormat>
            {
                new InputObjectFormat
                {
                    ObjectType = "CommunicationViewModel",
                    Properties =new List<InputFormat>
                    {
                        new InputFormat("CommunicationID", "", "CommunicationID"),
                        new InputFormat("Secondary Emails", "Text", "SecondaryEmails"),
                        new InputFormat("Facebook Url", "Text", "FacebookUrl"),
                        new InputFormat("Twitter Url", "Text", "TwitterUrl"),
                        new InputFormat("Google Plus Url", "Text", "GooglePlusUrl"),
                        new InputFormat("LinkedIn Url", "Text", "LinkedInUrl"),
                        new InputFormat("Blog Url", "Text", "BlogUrl"),
                        new InputFormat("WebSite Url", "Text", "WebSiteUrl"),
                    }
                },
                new InputObjectFormat
                {
                    ObjectType = "AddressViewModel",
                    Properties =new List<InputFormat>
                    {
                        new InputFormat("AddressID", "", "AddressID"),
                        new InputFormat("AddressTypeID", "Dropdown", "AddressTypeID", dropdownId: 2),
                        new InputFormat("Address Line1", "Text", "AddressLine1"),
                        new InputFormat("Address Line2", "Text", "AddressLine2"),
                        new InputFormat("City", "Text", "City"),
                        new InputFormat("State", "", "State", true, "State"),
                        new InputFormat("Country", "", "Country", true, "Country"),
                        new InputFormat("ZipCode", "Text", "ZipCode"),
                        new InputFormat("IsDefault", "Boolean", "IsDefault"),
                    }
                },
                new InputObjectFormat
                {
                    ObjectType = "Phone",
                    Properties =new List<InputFormat>
                    {
                        new InputFormat("PhoneType", "Dropdown", "PhoneType", dropdownId: 1),
                        new InputFormat("IsPrimary", "Boolean", "IsPrimary"),
                        new InputFormat("Number", "Text", "Number")
                    }
                },
                new InputObjectFormat
                {
                    ObjectType = "Email",
                    Properties =new List<InputFormat>
                    {
                        new InputFormat("EmailId", "Text", "EmailId"),
                        new InputFormat("IsPrimary", "Boolean", "IsPrimary")
                    }
                },
                new InputObjectFormat
                {
                    ObjectType = "Url",
                    Properties =new List<InputFormat>
                    {
                        new InputFormat("URL", "Text", "URL")
                    }
                },
                new InputObjectFormat
                {
                    ObjectType = "Tag",
                    Properties =new List<InputFormat>
                    {
                        new InputFormat("TagName", "Text", "TagName")
                    }
                },
                new InputObjectFormat
                {
                    ObjectType = "ContactCustomFieldMapViewModel",
                    Properties =new List<InputFormat>
                    {
                        new InputFormat("CustomFieldId", "Number", "CustomFieldId"),
                        new InputFormat("Value", "Text", "Value")
                    }
                },new InputObjectFormat
                {
                    ObjectType = "DropdownValueViewModel",
                    Properties =new List<InputFormat>
                    {
                        new InputFormat("DropdownValueID", "Number", "DropdownValueID")
                    }
                }
            };

            return Request.BuildResponse(new GetSchemaResponse { Schema = JsonConvert.SerializeObject(inputFormat) });
        }

        /// <summary>
        /// Saving Relationship to Contact.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("AddRelation")]
        [HttpPost]
        public HttpResponseMessage AddRelation(RelationshipViewModel viewModel)
        {
            SaveRelationshipResponse response = new SaveRelationshipResponse();
            SaveRelationshipRequest request = new SaveRelationshipRequest()
            {
                RelationshipViewModel = viewModel,
                UserId = this.UserId,
                SelectAllSearchCriteria = null,
                AdvancedSearchCritieria = null,
                RequestedBy = this.UserId,
                RoleId = this.RoleId,
                AccountId = this.AccountId,
                DrillDownContactIds = new int[] { }
            };
            if (viewModel != null && viewModel.Relationshipentry.IsAny())
                response = contactRelationshipService.SaveRelationshipMap(request);

            return Request.BuildResponse(response);
        }


        /// <summary>
        /// Change Owner to Contact.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("ChangeOwner")]
        [HttpPost]
        public HttpResponseMessage ChangeOwerForContact(ChangeOwnerViewModel viewModel)
        {
            ChangeOwnerResponce response = contactService.ChangeOwner(new ChangeOwnerRequest()
            {
                ChangeOwnerViewModel = viewModel,
                RequestedBy = this.UserId,
                AccountId = this.AccountId,
                RoleId = this.RoleId,
                ModuleId = (byte)AppModules.Contacts
            });

            return Request.BuildResponse(response);

        }

        /// <summary>
        /// For User Created and Assigned Actions.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("GetCreatedActions")]
        [HttpPost]
        public HttpResponseMessage GetCreatedActions(GridEntryViewModel viewModel)
        {
            IEnumerable<ActionViewModel> actions = null;
            var Field = !string.IsNullOrEmpty(viewModel.SortField) ? viewModel.SortField : GetPropertyName<ActionViewModel, DateTime?>(r => r.ActionDate);
            var direction = !string.IsNullOrEmpty(viewModel.SortDirection) ? viewModel.SortDirection : "DESC";
            string userName = string.Empty;
            GetActionListResponse response = actionService.GetUserCreatedActions(new GetActionListRequest()
            {
                RequestedBy = viewModel.UserId == null ? this.UserId : viewModel.UserId,
                UserIds = viewModel.UserId == null ? new int[] { this.UserId } : new int[] { viewModel.UserId.Value },
                AccountId = this.AccountId,
                IsStAdmin = this.IsSTAdmin,
                PageNumber = viewModel.PageNumber,
                Limit = viewModel.PazeSize,
                Name = viewModel.SearchBy,
                Filter = string.IsNullOrEmpty(viewModel.FilterBy) ? "2" : viewModel.FilterBy,
                SortDirection = direction == "DESC" ? System.ComponentModel.ListSortDirection.Descending : System.ComponentModel.ListSortDirection.Ascending,
                SortField = Field,
                IsDashboard = viewModel.IsFromDashboard,
                StartDate = !string.IsNullOrEmpty(viewModel.StartDate) ? Convert.ToDateTime(viewModel.StartDate) : DateTime.MinValue,
                EndDate = !string.IsNullOrEmpty(viewModel.EndDate) ? Convert.ToDateTime(viewModel.EndDate) : DateTime.MinValue,
                FilterByActionType = viewModel.ShowingType
            });
            if (response != null)
            {
                response.ActionListViewModel.ToList().ForEach(a => a.ActionDateTime = a.ActionDateTime.ToUtc());
                response.ActionListViewModel.Each(a =>
                {
                    if (a.UserName != null && a.UserName.Length > 75)
                    {
                        userName = a.UserName.Substring(0, 74);
                        a.UserName = userName + "...";
                    }
                });
            }

            actions = response.ActionListViewModel;
            return Request.BuildResponse(response);
        }


        /// <summary>
        ///  Gets the users list.
        /// </summary>
        /// <returns></returns>
        [Route("GetUsersList")]
        [HttpGet]
        public HttpResponseMessage GetUsersList()
        {
            GetUsersResponse response = contactService.GetUsers(new GetUsersRequest()
            {
                AccountID = this.AccountId,
                UserId = 0,
                IsSTadmin = this.IsSTAdmin
            });

            return Request.BuildResponse(response);

        }

        /// <summary>
        /// For Contact Time Line Data
        /// </summary>
        /// <param name="timelineViewModel"></param>
        /// <returns></returns>
        [Route("TimeLineData")]
        [HttpPost]
        public async Task<HttpResponseMessage> TimeLineData(TimeLineViewModel timelineViewModel)
        {
            GetTimeLineResponse response = new GetTimeLineResponse();
            var timezone = this.TimeZone;
            response = await contactService.GetTimeLinesDataAsync(new GetTimeLineRequest()
            {
                AccountId = this.AccountId,
                ContactID = timelineViewModel.ContactID,
                OpportunityID = timelineViewModel.OpportunityID,
                TimeZone = timezone,
                Limit = 20,
                PageNumber = timelineViewModel.PageNumber,
                // Module = ReadCookie("module"),
                //Period = ReadCookie("period"),
                PageName = timelineViewModel.PageName,
                Activities = timelineViewModel.Activities,
                DateFormat = this.DateFormat,
                FromDate = timelineViewModel.CustomStartDate,
                ToDate = timelineViewModel.CustomEndDate
            });

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Custom Fields 
        /// </summary>
        /// <returns></returns>
        [Route("GetCustomFields")]
        [HttpGet]
        public HttpResponseMessage GetCustomFieldsByAccountId()
        {
            GetAllCustomFieldTabsResponse response = contactService.GetCustomFieldTabs(new GetAllCustomFieldTabsRequest(this.AccountId));
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("GetContactWebVisits")]
        [HttpPost]
        public HttpResponseMessage GetContactWebVisists(GetContactWebVisitsSummaryRequest request)
        {
            GetContactWebVisitsSummaryResponse response = contactService.GetContactWebVisitsSummary(request);
            if (response.WebVisits.IsAny())
            {
                response.WebVisits.Each(wv =>
                {
                    wv.VisitedOn = wv.VisitedOn.ToUtc().ToUtcBrowserDatetime();
                });

            }

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Getting Conacts of logged in Account.
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        [Route("GetContacts")]
        [HttpPost]
        public HttpResponseMessage ContactsViewRead(GridEntryViewModel viewmodel)
        {
            dynamic result = default(dynamic);
     
            result = getContacts<ContactGridEntry>(viewmodel.PageNumber, viewmodel.PazeSize, viewmodel.SearchBy, viewmodel.ShowingType,viewmodel.FilterBy,viewmodel.DrildownType,viewmodel.ContactIds);

            ContactsGridEntryResponse response = new ContactsGridEntryResponse();
            response.Contacts = result.Contacts;
            response.TotalHits = result.TotalHits;

            return Request.BuildResponse(response);
        }

         private dynamic getContacts<T>(int page, int pageSize, string name, string type, string sort,string drilldownType,List<int> ContactIds,string sortDirection = "DESC") where T : IShallowContact
        {

            int[] ContactIDsList = null;
            string sortingValue = null;
            if (type == "4" && sort == "1")
                sortingValue = "1";
            else if (type == "4" && sort == "2")
                sortingValue = "2";
            else if (type == "4" && sort == "3")
                sortingValue = "3";
            int ShowingFieldValue = 0;
            drilldownType = string.IsNullOrEmpty(drilldownType) ? "" : drilldownType;
            if ((string.IsNullOrEmpty(sort) || sort.Equals("0")) && string.IsNullOrEmpty(name) && (!type.Equals("4")))
                sort = "1";
            else if ((string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(name)) || type.Equals("4"))
                sort = "0";

            if (type == "3" && drilldownType == String.Empty && !ContactIds.IsAny())
            {
                ShowingFieldValue = 4;
                GetRecentViwedContactsResponse response = GetUserCreatedContacts(null);
                if (!response.ContactIdList.IsAny())
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                }
            }
            else if (type == "4" && drilldownType == String.Empty && !ContactIds.IsAny())
            {
                var sortedValue = "";
                if (sortingValue == "1")
                    sortedValue = sortingValue == "1" ? "1" : null;
                else if (sortingValue == "2")
                    sortedValue = "2";
                else if (sortingValue == "3")
                    sortedValue = "3";
                Logger.Current.Informational(" Type filter value " + type + " and sort value value  " + sortedValue);
                ShowingFieldValue = 5;
                GetRecentViwedContactsResponse response = GetRecentViewedContacts(null, sortedValue);
                if (response.ContactIdList == null || response.ContactIdList.Count() < 0)
                {
                    ContactIDsList = new List<int>().ToArray();
                }
                else
                {
                    ContactIDsList = response.ContactIdList.ToArray();
                    Logger.Current.Informational(" recentily viewed Contacts count :" + ContactIDsList.Count());
                }
            }
            else if(drilldownType == "Untouched" && !ContactIds.IsAny())
            {
                DateTime endDate = Convert.ToDateTime(DateTime.UtcNow.Date.AddHours(23).AddMinutes(59));
                GetUntouchedContactsResponse response = contactService.GetUntouchedContactsIds(new GetUntouchedContactsRequest()
                {
                    AccountId = this.AccountId,
                    RequestedBy= this.UserId,
                    RoleId =this.RoleId,
                    StartDate = ToUserUtcDateTime(DateTime.UtcNow.AddDays(-30).Date),
                    EndDate = ToUserUtcDateTime(endDate.Date)
                });

                if (response.ContactIds.IsAny())
                    ContactIDsList = response.ContactIds.ToArray();
                else
                    ContactIDsList = new List<int>().ToArray();

            }
            else if (ContactIds.IsAny())
            {
                ContactIDsList = ContactIds.ToArray();
            }

            SearchContactsResponse<T> searchresponse = new SearchContactsResponse<T>();
            if ((string.Equals(type, "2") || string.Equals(type, "3") || string.Equals(type, "4")) && (drilldownType == "Untouched" || ContactIds.IsAny()))
            {
                var p = page;
                var contacts = ContactIDsList;
                if (string.Equals(type, "4"))
                {
                    p = 1;
                    if (string.IsNullOrEmpty(name))
                        contacts = ContactIDsList.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
                    else
                        contacts = ContactIDsList.ToArray();
                    Logger.Current.Informational("Giving contactIds to elastic search: " + contacts.Count());
                }
                searchresponse = contactService.GetAllContacts<T>(new SearchContactsRequest()
                {
                    Query = name,
                    Limit = pageSize,
                    PageNumber = p,
                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                    AccountId = this.AccountId,
                    ContactIDs = contacts,
                    ShowingFieldType = (ContactShowingFieldType)ShowingFieldValue,
                    RequestedBy = this.UserId,
                    SortField = "",
                    IsResultsGrid = true,
                    RoleId = this.RoleId
                });
            }
            else if (type == "0" && (drilldownType == String.Empty || drilldownType == "Untouched" || ContactIds.IsAny()))
                searchresponse = contactService.GetPersons<T>(new SearchContactsRequest()
                {
                    Query = name,
                    Limit = pageSize,
                    PageNumber = page,
                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                    AccountId = this.AccountId,
                    RequestedBy = this.UserId,
                    RoleId = this.RoleId,
                    ContactIDs = ContactIDsList,
                    SortField = "",
                    SortDirection = sortDirection == "ASC" ? ListSortDirection.Ascending:ListSortDirection.Descending,
                    IsResultsGrid = true
                });
            else if (type == "1" && (drilldownType == String.Empty || drilldownType == "Untouched" || ContactIds.IsAny()))
                searchresponse = contactService.GetCompanies<T>(new SearchContactsRequest()
                {
                    Query = name,
                    Limit = pageSize,
                    PageNumber = page,
                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                    AccountId = this.AccountId,
                    RequestedBy = this.UserId,
                    RoleId = this.RoleId,
                    ContactIDs = ContactIDsList,
                    SortField = "",
                    SortDirection = sortDirection == "ASC" ? ListSortDirection.Ascending : ListSortDirection.Descending,
                });
            else if (type == "4" && (drilldownType == String.Empty || drilldownType == "Untouched" || ContactIds.IsAny()))
                searchresponse = contactService.GetCompanies<T>(new SearchContactsRequest()
                {
                    Query = name,
                    Limit = pageSize,
                    PageNumber = page,
                    SortFieldType = (ContactSortFieldType)Convert.ToInt16(sort),
                    AccountId = this.AccountId,
                    RequestedBy = this.UserId,
                    RoleId = this.RoleId,
                    ContactIDs = ContactIDsList,
                    SortField = "",
                    SortDirection = sortDirection == "ASC" ? ListSortDirection.Ascending : ListSortDirection.Descending,
                });


            if (searchresponse.Contacts != null)
            {

                var lifecycleStages = dropdownValuesService.GetDropdownValue(new GetDropdownValueRequest() { DropdownID = (int)DropdownFieldTypes.LifeCycle, AccountId = this.AccountId }).DropdownValues; //dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                var leadSources = dropdownValuesService.GetDropdownValue(new GetDropdownValueRequest() { DropdownID = (int)DropdownFieldTypes.LeadSources, AccountId = this.AccountId }).DropdownValues;
                foreach (var contact in searchresponse.Contacts)
                {
                    object obj = (object)contact;
                    var lifeCycleStageProp = obj.GetType().GetProperty("LifecycleStage");
                    var lifeCycleNameProp = obj.GetType().GetProperty("LifecycleName");
                    var lastTouchedProp = obj.GetType().GetProperty("LastTouched");
                    var lastContactedDateProp = obj.GetType().GetProperty("LastContactedDate");
                    var lastTouchedThroughProp = obj.GetType().GetProperty("LastTouchedThrough");
                    var firstLeadSourceProp = obj.GetType().GetProperty("FirstLeadSourceId");
                    var firstLeadSourceNameProp = obj.GetType().GetProperty("FirstLeadSource");
                    var leadSourceProp = obj.GetType().GetProperty("LeadSourceIds");
                    var leadSorceNameProp = obj.GetType().GetProperty("LeadSource");
                    if(leadSourceProp.GetValue(obj, null) != null)
                    {
                        var leadsourceIds = (short[])leadSourceProp.GetValue(obj, null);
                        var leadSourceNames = leadSources.DropdownValuesList.Where(e => leadsourceIds.Contains( e.DropdownValueID)).Select(s => s.DropdownValue).ToList();
                        leadSorceNameProp.SetValue(obj, string.Join(",", leadSourceNames.ToArray()));
                    }
                    lifeCycleNameProp.SetValue(obj, lifecycleStages.DropdownValuesList.Where(e => e.DropdownValueID == (short)lifeCycleStageProp.GetValue(obj, null)).Select(s => s.DropdownValue).FirstOrDefault());
                    firstLeadSourceNameProp.SetValue(obj, leadSources.DropdownValuesList.Where(e => e.DropdownValueID == (short)firstLeadSourceProp.GetValue(obj, null)).Select(s => s.DropdownValue).FirstOrDefault());
                    if (lastContactedDateProp.GetValue(obj, null) != null && typeof(T).Equals(typeof(ContactGridEntry)))
                    {
                        lastTouchedProp.SetValue(obj, ((DateTime)lastContactedDateProp.GetValue(obj, null)).ToJSDate().ToString(this.DateFormat, CultureInfo.InvariantCulture) + " (" + lastTouchedThroughProp.GetValue(obj, null) + ")");
                    }

                }
            }
            List<int?> Companyids = new List<int?>();
            if (typeof(T).Equals(typeof(ContactGridEntry)))
            {
                var contacts = (IEnumerable<ContactGridEntry>)searchresponse.Contacts;
                Companyids = contacts.Where(p => p.ContactType == 1 && p.CompanyID != null).Select(p => p.CompanyID).ToList();
                IEnumerable<Contact> companies = contactService.GetAllContactsByCompanyIds(Companyids, this.AccountId);
                foreach (var person in contacts.Where(p => p.ContactType == 1 && p.CompanyID != null))
                {
                    foreach (var company in companies)
                    {
                        if (person.CompanyID == company.Id && company.IsDeleted == false)
                            person.CompanyName = company.CompanyName;
                    }
                }
                foreach (var person in contacts.Where(p => p.ContactType == 1))
                {
                    string firstname = "";
                    string lastname = "";
                    if (!string.IsNullOrEmpty(person.FirstName))
                    {
                        if (person.FirstName.Length > 35)
                        {
                            firstname = person.FirstName.Substring(0, 34);
                            firstname = firstname + "...";
                        }
                        else
                        {
                            firstname = person.FirstName;
                        }
                    }
                    if (!string.IsNullOrEmpty(person.LastName))
                    {
                        if (person.LastName.Length > 35)
                        {
                            lastname = person.LastName.Substring(0, 34);
                            lastname = lastname + "...";
                        }
                        else
                        {
                            lastname = person.LastName;
                        }
                    }
                    if (!string.IsNullOrEmpty(person.FirstName) && !string.IsNullOrEmpty(person.LastName) && (person.FirstName.Length > 35 || person.LastName.Length > 35))
                    {
                        person.Name = firstname + " " + lastname;
                    }
                    person.FullName = person.FirstName + " " + person.LastName;
                }
            }
            return new
            {
                Contacts = (dynamic)searchresponse.Contacts,
                TotalHits = (int)searchresponse.TotalHits
            };
        }

        /// <summary>
        ///  Gets the user created contacts.
        /// </summary>
        /// <param name="contactIDsList"></param>
        /// <returns></returns>
        public GetRecentViwedContactsResponse GetUserCreatedContacts(int[] contactIDsList)
        {
            int userId = this.UserId;
            GetRecentViwedContactsRequest request = new GetRecentViwedContactsRequest();
            request.ActivityName = UserActivityType.Create;
            request.ModuleName = AppModules.Contacts;
            request.UserId = userId;
            request.ContactIDs = contactIDsList;
            request.AccountId = this.AccountId;
            GetRecentViwedContactsResponse response = contactService.GetContactByUserId(request);
            return response;
        }

        /// <summary>
        /// Gets the recent viewed contacts.
        /// </summary>
        /// <param name="contactIDsList"></param>
        /// <param name="sortValue"></param>
        /// <returns></returns>
        public GetRecentViwedContactsResponse GetRecentViewedContacts(int[] contactIDsList, string sortValue)
        {
            int userId = this.UserId;
            int accountId = this.AccountId;
            GetRecentViwedContactsRequest request = new GetRecentViwedContactsRequest();
            request.ActivityName = UserActivityType.Read;
            request.ModuleName = AppModules.Contacts;
            request.UserId = userId;
            request.ContactIDs = contactIDsList;
            request.sort = sortValue;
            request.AccountId = accountId;
            GetRecentViwedContactsResponse response = userService.GetRecentlyViewedContacts(request);
            return response;
        }

        /// <summary>
        /// For Getting My Communications Drilldown ContactIds.
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="period"></param>
        /// <param name="activityType"></param>
        /// <returns></returns>
        [Route("MyCommunicationContacts")]
        [HttpGet]
        public HttpResponseMessage GetMyCommunicationContacts(string activity, string period, string activityType)
        {
            DateTime startDate = DateTime.UtcNow;
            DateTime endDate = DateTime.UtcNow;
            if (period == "0")
                startDate = ToUserUtcDateTime(startDate.AddDays(-7).Date);
            else
                startDate = ToUserUtcDateTime(startDate.AddDays(-30).Date);
            GetMyCommunicationContactsResponse response = userService.GetMyCommunicationContacts(new GetMyCommunicationContactsRequest()
            {
                UserId = this.UserId,
                AccountId = this.AccountId,
                StartDate = startDate,
                EndDate = endDate,
                Activity = activity,
                ActivityType = activityType
            });

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Updating Contact Customfield value.
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="fieldId"></param>
        /// <param name="newValue"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        [Route("UpdateCustomfieldValue")]
        [HttpPost]
        public HttpResponseMessage UpdateCustomFieldValue(int contactId, int fieldId, string newValue, short inputType)
        {
            Logger.Current.Verbose(string.Format("Updating contact {0} field {1} with value {2}", contactId, fieldId, newValue));
            UpdateContactCustomFieldResponse response = new UpdateContactCustomFieldResponse();
            try
            {
                response = contactService.UpdateContactCustomField(new UpdateContactCustomFieldRequest()
                {
                    ContactId = contactId,
                    FieldId = fieldId,
                    Value = newValue,
                    AccountId = this.AccountId,
                    inputType = inputType,
                    DateFormat = this.DateFormat
                });
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(ex.Message);
            }

            return Request.BuildResponse(response);
        }

        private static DateTime ToUserUtcDateTime(DateTime d)
        {
            return d.ToJsonSerailizedDate().ToUserUtcDateTime();
        }


    }

    public class PersonAPIRequest
    {
        public PersonViewModel ViewModel { get; set; }
        public int FormId { get; set; }
    }
}