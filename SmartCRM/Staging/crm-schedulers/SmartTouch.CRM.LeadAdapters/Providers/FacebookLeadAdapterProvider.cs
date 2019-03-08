using Facebook;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SmartTouch.CRM.LeadAdapters.Providers
{
    public class FacebookLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IMailGunService mailGunService;
        IContactService contactService;
        ISearchService<Contact> searchService;
        ILeadAdaptersRepository leadAdaptersRepository;
        IImportDataRepository importDataRepository;
        IAccountRepository accountRepository;
        IEnumerable<FacebookLeadGen> facebookLeadGens;
        string FacebookAppID;
        string FacebookAppSecret;
        IEnumerable<DropdownValueViewModel> phoneDropdownFields;
        IEnumerable<FieldViewModel> customFields;
        ICachingService cacheService;
        ICustomFieldService customFieldService;
        IServiceProviderRepository serviceProviderRepository;

        public FacebookLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
            IImportDataRepository importDataRepository, ISearchService<Contact> searchService, IUnitOfWork unitOfWork,
            ICustomFieldService customFieldService, ICachingService cacheService, ICommunicationService communicationService, IMailGunService mailGunService, IContactService contactService, IAccountRepository accRepository)
            : base(accountId, leadAdapterAndAccountMapID, LeadAdapterTypes.BDX, leadAdaptersRepository, importDataRepository, searchService, unitOfWork,
                customFieldService, cacheService, serviceProviderRepository, mailGunService, contactService)
        {
            Logger.Current.Verbose("Enter into Facebook LeadAdapterProvider");
            this.mailGunService = mailGunService;
            this.searchService = searchService;
            this.contactService = contactService;
            this.leadAdaptersRepository = leadAdaptersRepository;
            this.importDataRepository = importDataRepository;
            this.accountRepository = accRepository;
            this.cacheService = cacheService;
            this.customFieldService = customFieldService;
            this.serviceProviderRepository = serviceProviderRepository;
        }

        public override void Initialize()
        {

        }

        public override void Process()
        {
            Logger.Current.Informational("Request received for processing Facebook Lead Adapter for account : " + AccountID);
            facebookLeadGens = leadAdaptersRepository.GetFacebookLeadGens(LeadAdapterAccountMapID);
            var account = leadAdaptersRepository.GetFacebookApp(AccountID);
            if (account != null)
            {
                FacebookAppID = account.FacebookAPPID;
                FacebookAppSecret = account.FacebookAPPSecret;
            }
            
            ProcessLeadGens(facebookLeadGens);
        }

        private void ProcessLeadGens(IEnumerable<FacebookLeadGen> leads)
        {
            if (leads != null && leads.Any())
            {
                Logger.Current.Informational("Processing leadgens");
                phoneDropdownFields = GetPhoneDropdownFields();
                customFields = GetCustomFields();
                
                List<FacebookLead> fbLeads = new List<FacebookLead>();
                foreach (var lead in leads)
                {
                    fbLeads.Add(GetData(lead));
                }

                var contacts = ProcessContacts(fbLeads);
                if (contacts.ContactData != null && contacts.ContactData.Any())
                {
                    Logger.Current.Informational("Got contacts for inserting : " + contacts.ContactData.Count);
                    /* Bulk Insert */
                    Task.Factory.StartNew(() => { MailgunVerification(contacts.ContactData.ToList()); }, TaskCreationOptions.LongRunning);
                    Task.Factory.StartNew(() => { ContactsBulkinsert(contacts); }, TaskCreationOptions.LongRunning);
                }
                else
                {
                    Logger.Current.Informational("No contacts for inserting, Account name : " + leadAdapterAndAccountMap.AccountName);
                }
            }
            else
                Logger.Current.Informational("No leadgens found for processing");
        }

        private FacebookLead GetData(FacebookLeadGen fbLead)
        {
            Logger.Current.Informational("Request received for fetching data from fb");
            string pageAccessToken = fbLead.PageAccessToken;   // GetExtendedPageAccessToken(fbLead.PageAccessToken);
            string leadGenID = fbLead.LeadGenID.ToString();
            var fb = new FacebookClient(pageAccessToken);
            string exceptionMessage = string.Empty;
            fb.AppId = FacebookAppID;
            fb.AppSecret = FacebookAppSecret;
            FacebookLead lead = new FacebookLead();
            try
            {
                var response = (JsonObject)fb.Get(leadGenID);
                if (response != null)
                {
                    var id = response["id"];
                    var date = response["created_time"];
                    var fieldDate = (JsonArray)response["field_data"];
                    List<FacebookFieldData> fieldValues = new List<FacebookFieldData>();
                    if (fieldDate != null)
                    {
                        foreach (var pair in fieldDate)
                        {
                            FacebookFieldData fieldData = new FacebookFieldData();
                            var name = ((JsonObject)pair)["name"];
                            var values = ((JsonObject)pair)["values"];
                            if (values != null)
                            {
                                var valuesObj = (JsonArray)values;
                                var val = valuesObj[0];
                                fieldData.Name = name.ToString();
                                fieldData.Values = new String[] { val.ToString() };
                            }
                            fieldValues.Add(fieldData);
                        }
                    }

                    lead.ID = id.ToString();
                    lead.CreatedTime = DateTime.Parse(date.ToString());
                    lead.FieldData = fieldValues;
                    fbLead.IsProcessed = true;
                    fbLead.Remarks = "Success";
                    fbLead.RawData = response.ToString();
                    leadAdaptersRepository.UpdateFacebookLeadGen(fbLead);
                }
            }
            catch (FacebookOAuthException ex)
            {
                Logger.Current.Error("An error occured while fetching lead data from Facebook", ex);
                exceptionMessage = ex.Message;
                leadAdaptersRepository.UpdateLeadAdapterStatus(leadAdapterAndAccountMap.Id, LeadAdapterErrorStatus.Error, LeadAdapterServiceStatus.TokenExpired);
                SendErrorEmail("Exception while processing data from " + leadAdapterAndAccountMap.LeadAdapterTypeID.ToString() + " LeadAdapter", "Exception details" + ex.Message);
            }
            catch (FacebookApiException ex)
            {
                Logger.Current.Error("An error occured while fetching lead data from Facebook", ex);
                exceptionMessage = ex.Message;
                leadAdaptersRepository.UpdateLeadAdapterStatus(leadAdapterAndAccountMap.Id, LeadAdapterErrorStatus.Running, LeadAdapterServiceStatus.InvalidRequest);
            }
            finally
            {
                fbLead.Remarks = exceptionMessage;
                fbLead.IsProcessed = true;
                leadAdaptersRepository.UpdateFacebookLeadGen(fbLead);  
            }
            return lead;
        }

        private ImportContactsData ProcessContacts(IEnumerable<FacebookLead> fbLeads)
        {
            ImportContactsData data = new ImportContactsData();
            if (fbLeads != null && fbLeads.Any() && fbLeads.Where(w => w.FieldData != null && w.FieldData.Any()).Any())
            {
                int jobLogId = this.InsertLeadAdapterJobLog();
                Logger.Current.Informational("Request received for processing fbleads to importcontactdata");
                var persons = new List<RawContact>();
                var personCustomFieldData = new List<ImportCustomData>();
                var personPhoneData = new List<ImportPhoneData>();

                Func<RawContact, bool> ValidateEmail = (c) =>
                {
                    /*
                     * If email is not empty or null then validate email
                     * If email is empty/or null then check for legth of first name and lastname
                     */
                    if ((!string.IsNullOrEmpty(c.PrimaryEmail) && c.IsValidEmail(c.PrimaryEmail)) ||
                        (string.IsNullOrEmpty(c.PrimaryEmail) && !string.IsNullOrEmpty(c.FirstName) && !string.IsNullOrEmpty(c.LastName)))
                    {
                        return true;
                    }
                    return false;
                };

                foreach (var lead in fbLeads)
                {
                    RawContact contact = new RawContact();
                    IList<ImportPhoneData> contactPhoneData = new List<ImportPhoneData>();
                    IList<ImportCustomData> contactCustomData = new List<ImportCustomData>();
                    List<FacebookAttribute> attributes = new List<FacebookAttribute>();

                    if (lead != null && lead.FieldData != null)
                    {
                        var guid = Guid.NewGuid();
                        LeadAdapterRecordStatus status = LeadAdapterRecordStatus.Undefined;
                        string firstName = string.Empty;
                        string lastName = string.Empty;
                        string company = string.Empty;
                        string primaryEmail = string.Empty;
                        string zipCode = string.Empty;
                        string phone = string.Empty;
                        string militaryStatus = string.Empty;
                        string addressline1 = string.Empty;
                        string state = string.Empty;
                        string city = string.Empty;

                        foreach (var field in lead.FieldData)
                        {
                            if (field.Name == "first_name" && field.Values != null)
                            {
                                attributes.Add(new FacebookAttribute() { LocalName = field.Name, Value = field.Values[0] });
                                firstName = field.Values[0];
                            }
                            else if (field.Name == "last_name" && field.Values != null)
                            {
                                attributes.Add(new FacebookAttribute() { LocalName = field.Name, Value = field.Values[0] });
                                lastName = field.Values[0];
                            }
                            else if (field.Name == "full_name" && field.Values != null)
                            {
                                string[] names = field.Values[0].Split();
                                if (names != null && names.Any())
                                {
                                    firstName = names[0];
                                    lastName = names[1];
                                }
                                attributes.Add(new FacebookAttribute() { LocalName = "first_name", Value = firstName });
                                attributes.Add(new FacebookAttribute() { LocalName = "last_name", Value = lastName });
                            }
                            else if (field.Name == "company_name" && field.Values != null)
                            {
                                attributes.Add(new FacebookAttribute() { LocalName = field.Name, Value = field.Values[0] });
                                company = field.Values[0];
                            }
                            else if (field.Name == "email" && field.Values != null)
                            {
                                attributes.Add(new FacebookAttribute() { LocalName = field.Name, Value = field.Values[0] });
                                primaryEmail = field.Values[0];
                            }
                            else if ((field.Name == "post_code" || field.Name == "zip_code") && field.Values != null)
                            {
                                attributes.Add(new FacebookAttribute() { LocalName = "post_code", Value = field.Values[0] });
                                zipCode = field.Values[0];
                            }
                            else if ((field.Name == "phone_number" || field.Name == "work_phone_number") && field.Values != null)
                            {
                                attributes.Add(new FacebookAttribute() { LocalName = "phone", Value = field.Values[0] });
                                phone = field.Values[0];
                            }
                            else if (field.Name == "military_status" && field.Values != null)
                            {
                                string value = new string(field.Values[0].Take(11).ToArray());
                                attributes.Add(new FacebookAttribute() { LocalName = "militarystatus", Value = value });
                                militaryStatus = value;
                            }
                            else if (field.Name == "street_address" && field.Values != null)
                            {
                                attributes.Add(new FacebookAttribute() { LocalName = field.Name, Value = field.Values[0] });
                                addressline1 = field.Values[0];
                            }
                            else if (field.Name == "city" && field.Values != null)
                            {
                                attributes.Add(new FacebookAttribute() { LocalName = field.Name, Value = field.Values[0] });
                                city = field.Values[0];
                            }
                            else if (field.Name == "state" && field.Values != null)
                            {
                                attributes.Add(new FacebookAttribute() { LocalName = field.Name, Value = field.Values[0] });
                                state = field.Values[0];
                            }
                        }

                        StringBuilder hash = new StringBuilder();
                        IList<Email> emails = new List<Email>();
                        IList<string> hashes = new List<string>();
                        if (!string.IsNullOrEmpty(primaryEmail))
                        {
                            Email primaryemail = new Email()
                            {
                                EmailId = primaryEmail,
                                AccountID = leadAdapterAndAccountMap.AccountID,
                                IsPrimary = true
                            };
                            emails.Add(primaryemail);
                            hash.Append("-").Append(primaryEmail);
                        }
                        else
                        {
                            hash.Append("-").Append("na@na.com");
                        }

                        Person person = new Person()
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            CompanyName = company,
                            Emails = emails,
                            AccountID = AccountID
                        };
                        if (string.IsNullOrEmpty(firstName))
                            hash.Append("-").Append(string.Empty);
                        else
                            hash.Append("-").Append(firstName);

                        if (string.IsNullOrEmpty(lastName))
                            hash.Append("-").Append(string.Empty);
                        else
                            hash.Append("-").Append(lastName);

                        if (string.IsNullOrEmpty(company))
                            hash.Append("-").Append(string.Empty);
                        else
                            hash.Append("-").Append(company);

                        SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
                        SearchParameters parameters = new SearchParameters() { AccountId = AccountID };
                        IEnumerable<Contact> duplicateContacts = contactService.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person }).Contacts;
                        duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };
                        
                        if (duplicateResult.TotalHits > 0)
                            status = LeadAdapterRecordStatus.Updated;
                        else
                            status = LeadAdapterRecordStatus.Added;

                        Contact duplicateperson = default(Person);
                        if (status == LeadAdapterRecordStatus.Updated)
                            duplicateperson = duplicateResult.Results.FirstOrDefault();
                        else
                            duplicateperson = new Person();

                        StringBuilder CustomFieldData = new StringBuilder();
                        var oldNewValues = new Dictionary<string, dynamic> { };
                        foreach (var attribute in attributes)
                        {
                            ImportCustomData customData = new ImportCustomData();
                            try
                            {
                                var elementvalue = string.Empty;
                                if (attribute.LocalName == "first_name")
                                {
                                    elementvalue = ((Person)duplicateperson).FirstName == null ? "" : ((Person)duplicateperson).FirstName;
                                    contact.FirstName = attribute.Value;
                                }
                                else if (attribute.LocalName == "last_name")
                                {
                                    elementvalue = ((Person)duplicateperson).LastName;
                                    contact.LastName = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "email")
                                {
                                    Email primaryemail = duplicateperson.Emails == null ? null : duplicateperson.Emails.Where(i => i.IsPrimary == true).FirstOrDefault();
                                    if (primaryemail != null)
                                        elementvalue = primaryemail.EmailId;
                                    contact.PrimaryEmail = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "company")
                                {
                                    elementvalue = duplicateperson.CompanyName;
                                    contact.CompanyName = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "post_code")
                                    contact.ZipCode = attribute.Value;
                                else if (attribute.LocalName.ToLower() == "street_address")
                                    contact.AddressLine1 = attribute.Value;
                                else if (attribute.LocalName.ToLower() == "city")
                                    contact.City = attribute.Value;
                                else if (attribute.LocalName.ToLower() == "state")
                                    contact.State = attribute.Value;
                                else if (attribute.LocalName.ToLower() == "phone")
                                {
                                    DropdownValueViewModel drpvalue = phoneDropdownFields.Where(i => i.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
                                    if (duplicateperson.Phones != null)
                                    {
                                        var mobilePhone = duplicateperson.Phones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
                                        if (mobilePhone != null)
                                            elementvalue = mobilePhone.Number;
                                    }
                                    ImportPhoneData phoneData = new ImportPhoneData();
                                    phoneData.ReferenceID = guid;

                                    string nonnumericstring = GetNonNumericData(attribute.Value);
                                    if (IsValidPhoneNumberLength(nonnumericstring))
                                    {
                                        contact.PhoneData = drpvalue.DropdownValueID.ToString() + "|" + nonnumericstring;
                                        phoneData.PhoneType = (int?)drpvalue.DropdownValueID;
                                        phoneData.PhoneNumber = nonnumericstring;
                                        contactPhoneData.Add(phoneData);
                                    }
                                }
                                else if (attribute.LocalName.ToLower() == "militarystatus")
                                {
                                    var customField = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == (attribute.LocalName.ToLower() + "(" + LeadAdapterTypes.Facebook.ToString().ToLower() + ")")).FirstOrDefault();
                                    if (customField != null)
                                    {
                                        var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customField.FieldId).FirstOrDefault();
                                        if (customfielddata != null)
                                            elementvalue = customfielddata.Value;

                                        customData.FieldID = customField.FieldId;
                                        customData.FieldTypeID = (int?)customField.FieldInputTypeId;
                                        customData.ReferenceID = guid;
                                        CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + attribute.Value.Trim());
                                        customData.FieldValue = attribute.Value.Trim();

                                        contactCustomData.Add(customData);

                                    }
                                }
                                if (!oldNewValues.ContainsKey(attribute.LocalName))
                                    oldNewValues.Add(attribute.LocalName, new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = attribute.Value.Trim() });

                                var brokenrules = contact.GetBrokenRules();                                                               
                                if (brokenrules != null && brokenrules.Any())
                                {
                                    status = LeadAdapterRecordStatus.ValidationFailed;
                                }
                                contact.ReferenceId = guid;
                                contact.AccountID = AccountID;
                                contact.IsBuilderNumberPass = true;
                                contact.IsCommunityNumberPass = true;
                                contact.LeadAdapterRecordStatusId = (byte)status;
                                contact.JobID = jobLogId;
                                contact.ContactStatusID = 1;
                                contact.ContactTypeID = 1;
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                string submittedData = js.Serialize(oldNewValues);
                                contact.LeadAdapterRowData = submittedData;
                                contact.LeadAdapterSubmittedData = submittedData;
                            }
                            catch (Exception Ex)
                            {
                                Logger.Current.Error("An error occured while processing contact fields, facebook lead adapter", Ex);
                            }
                        }

                    }
                    contact.ValidEmail = ValidateEmail(contact);
                    persons.Add(contact);
                    personPhoneData.AddRange(contactPhoneData);
                    personCustomFieldData.AddRange(contactCustomData);
                }
                data.ContactData = persons;
                data.ContactPhoneData = personPhoneData;
                data.ContactCustomData = personCustomFieldData;
            }
            return data;
        }

        private RawContact GetRawContact(FacebookLead lead)
        {
            RawContact contact = new RawContact();

            return contact;
        }

        private static string GetNonNumericData(string input)
        {
            Regex regex = new Regex("[^a-zA-Z0-9]");
            string output = regex.Replace(input, "");
            if (output.Length > 20)
                return "";
            return regex.Replace(input, "");
        }

        private int InsertLeadAdapterJobLog()
        {
            var jobLog = new LeadAdapterJobLogs();
            jobLog.LeadAdapterAndAccountMapID = leadAdapterAndAccountMap.Id;
            jobLog.LeadAdapterJobStatusID = LeadAdapterJobStatus.Undefined;
            jobLog.Remarks = string.Empty;
            jobLog.FileName = string.Empty;
            List<LeadAdapterJobLogDetails> details = new List<LeadAdapterJobLogDetails>();
            jobLog.LeadAdapterJobLogDetails = details;
            var leadAdapterJobLogID = importDataRepository.InsertLeadAdapterjob(jobLog, new Guid(), false, false, leadAdapterAndAccountMap.AccountID, leadAdapterAndAccountMap.CreatedBy, 1, leadAdapterAndAccountMap.CreatedBy,true);
            return leadAdapterJobLogID;
        }

        private string GetExtendedPageAccessToken(string pageAccessToken)
        {
            string extendedToken = pageAccessToken;
            if (!String.IsNullOrEmpty(pageAccessToken))
            {
                try
                {
                    FacebookClient client = new FacebookClient();
                    dynamic result = client.Get("/oauth/access_token", new
                    {
                        grant_type = "fb_exchange_token",
                        client_id = FacebookAppID,
                        client_secret = FacebookAppSecret,
                        fb_exchange_token = pageAccessToken
                    });

                    extendedToken = result.access_token;
                    if (!String.IsNullOrEmpty(extendedToken))
                        UpdateAccessToken(extendedToken);
                }
                catch (Exception Ex)
                {
                    Logger.Current.Error("An error occured while exchanging page access token for long lived token", Ex);
                }
            }
            return extendedToken;
        }

        private void UpdateAccessToken(string accessToken)
        {
            Logger.Current.Informational("Updating long-lived access token for LeadAdapterAccountMapID : " + LeadAdapterAccountMapID);
            leadAdaptersRepository.UpdateFacebookPageToken(accessToken, LeadAdapterAccountMapID);
        }

        private void SendErrorEmail(string subject, string bodyMessage)
        {
            try
            {
                string toEmail = accountRepository.GetAccountPrimaryEmail(AccountID);
                string fromEmail = ConfigurationManager.AppSettings["SupportEmailId"];

                ServiceProvider ServiceProviders = serviceProviderRepository
                       .GetServiceProviders(1, CommunicationType.Mail, MailType.TransactionalEmail);

                LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest sendMailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();

                Logger.Current.Verbose("Account Id in LeadAdapter:" + leadAdapterAndAccountMap.AccountID);
                Logger.Current.Verbose("Email Guid in LeadAdapter :" + ServiceProviders.LoginToken);
                string subjct = leadAdapterAndAccountMap.AccountName + " - " + subject;

                var body = " Error Message     : " + subject + ".\r\n Account Name     : " + leadAdapterAndAccountMap.AccountName + ".\r\n LeadAdapter        :  " + leadAdapterAndAccountMap.LeadAdapterTypeID.ToString() + "  .\r\n Instance occured on  : " + DateTime.UtcNow + " (UTC).\r\n More Info            : " + bodyMessage;

                List<string> To = new List<string>();
                To.Add(toEmail);
                EmailAgent agent = new EmailAgent();
                sendMailRequest.TokenGuid = ServiceProviders.LoginToken;
                sendMailRequest.From = fromEmail;
                sendMailRequest.IsBodyHtml = true;
                sendMailRequest.DisplayName = "";
                sendMailRequest.To = To;
                sendMailRequest.Subject = subjct;
                sendMailRequest.Body = body;
                sendMailRequest.RequestGuid = Guid.NewGuid();
                var varsendMailresponse = agent.SendEmail(sendMailRequest);
                if (varsendMailresponse.StatusID == LandmarkIT.Enterprise.CommunicationManager.Responses.CommunicationStatus.Success)
                {
                    Logger.Current.Informational("Support mail sent successfully");
                }
                Logger.Current.Verbose("Sending Email in LeadAdapter Engine :" + fromEmail);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured while sending email: ", ex);
            }
        }

        public override Domain.ImportData.ImportContactsData GetContacts(string fileName, IEnumerable<ApplicationServices.ViewModels.FieldViewModel> Fields, int jobId, IEnumerable<ApplicationServices.ViewModels.DropdownValueViewModel> DropdownFields)
        {
            return new ImportContactsData();
        }
        
        private IEnumerable<DropdownValueViewModel> GetPhoneDropdownFields()
        {
            var dropdownfeildsresposne = cacheService.GetDropdownValues(leadAdapterAndAccountMap.AccountID);
            IEnumerable<DropdownValueViewModel> phoneFields = dropdownfeildsresposne.Where(x => x.DropdownID == (short)DropdownFieldTypes.PhoneNumberType).Select(x => x.DropdownValuesList).FirstOrDefault();
            return phoneFields;
        }

        private IEnumerable<FieldViewModel> GetCustomFields()
        {
            var response = customFieldService.GetAllCustomFields(new GetAllCustomFieldsRequest(leadAdapterAndAccountMap.AccountID));
            IEnumerable<FieldViewModel> customFields = response.CustomFields;
            return customFields;
        }
    }

    public class FacebookAttribute
    {
        public string LocalName { get; set; }
        public string Value { get; set; }
    }
}
