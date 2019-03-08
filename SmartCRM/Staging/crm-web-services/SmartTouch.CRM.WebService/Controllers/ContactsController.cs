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
        /// <summary>
        /// Creating constructor for contacts controller for accessing
        /// </summary>
        /// <param name="contactService">contactService </param>
        /// <param name="actionService">actionService</param>
        /// <param name="communicationservice">communicationService</param>
        public ContactsController(IContactService contactService, IActionService actionService, ICommunicationService communicationservice)
        {
            this.contactService = contactService;
            this.actionService = actionService;
            this.communicationservice = communicationservice;
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

            var json= new JavaScriptSerializer().Serialize(viewmodel);
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
            string primaryEmail = viewmodel.Emails.IsAny()? viewmodel.Emails.Where(e => e.IsPrimary == true).Select(s => s.EmailId).FirstOrDefault():string.Empty;
            short leadSourceId = viewmodel.SelectedLeadSource.IsAny() ? viewmodel.SelectedLeadSource.Select(s => s.DropdownValueID).FirstOrDefault() :(short)0;

            if(!string.IsNullOrEmpty(viewmodel.FirstName) && !string.IsNullOrEmpty(viewmodel.LastName))
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
            response.ContactSummary = contactService.GetContactSummary(new GetContactSummaryRequest() { ContactId = contactId }).ContactSummary ;
            response.ContactSummaryDetails = JsonConvert.DeserializeObject<IEnumerable<ContactSummaryViewModel>>(response.ContactSummary);
            if (response.ContactSummaryDetails.IsAny())
            {
                response.ContactSummaryDetails.Each(d =>
                {
                    string Details = string.Empty;
                    d.SummaryDetails = d.NoteDetails;
                    if(d.NoteDetails.Length > 75)
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
            inputFormat.Properties.Add(new InputFormat("Communities", "Dropdown", "Communities", true, "DropdownValueViewModel",dropdownId:6));
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
    }

    public class PersonAPIRequest
    {
        public PersonViewModel ViewModel { get; set; }
        public int FormId { get; set; }
    }
}