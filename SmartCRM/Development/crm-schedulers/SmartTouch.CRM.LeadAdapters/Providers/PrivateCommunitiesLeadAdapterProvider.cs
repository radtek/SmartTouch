using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace SmartTouch.CRM.LeadAdapters.Providers
{
    public class PrivateCommunitiesLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IMailGunService mailGunService;
        IContactService contactService;
        ICustomFieldService customFieldService;
        ICachingService cacheService;
        ILeadAdaptersRepository leadAdaptersRepository;
        IServiceProviderRepository serviceProviderRepository;
        IImportDataRepository importDataRepository;
        ISearchService<Contact> searchService;
        public PrivateCommunitiesLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
            IImportDataRepository importDataRepository, ISearchService<Contact> searchService, IUnitOfWork unitOfWork,
            ICustomFieldService customFieldService, ICachingService cacheService, ICommunicationService communicationService, IMailGunService mailGunService, IContactService contactService)
            : base(accountId, leadAdapterAndAccountMapID, LeadAdapterTypes.BDX, leadAdaptersRepository, importDataRepository, searchService, unitOfWork,
            customFieldService, cacheService, serviceProviderRepository, mailGunService, contactService)
        {
            this.mailGunService = mailGunService;
            this.searchService = searchService;
            this.contactService = contactService;
            this.cacheService = cacheService;
            this.leadAdaptersRepository = leadAdaptersRepository;
            this.serviceProviderRepository = serviceProviderRepository;
            this.customFieldService = customFieldService;
            this.importDataRepository = importDataRepository;
        }
    
        private string ProcessWebRequest(string url)
        {
            string result = string.Empty;
            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                var errorResponse = (FtpWebResponse)ex.Response;
                if (errorResponse.StatusCode == FtpStatusCode.NotLoggedIn)
                {
                }
                Logger.Current.Error("An error occurred in idx get data method(web exception): " + ex);
               // SendEmail("Exception details" + ex, "Exception while processing data from Private Communities LeadAdapter");
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Logger.Current.Error("An erro in process web request in private communities", ex);
               // SendEmail("Exception details" + ex, "Exception while processing data from Private Communities LeadAdapter");
            }
            return result;
        }

        public override ImportContactsData GetContacts(string fileName, IEnumerable<FieldViewModel> customFields, int jobId, IEnumerable<DropdownValueViewModel> DropdownFields)
        {
            var persons = new List<RawContact>();
            var personCustomFieldData = new List<ImportCustomData>();
            var personPhoneData = new List<ImportPhoneData>();
            ImportContactsData data = new ImportContactsData(); 
            string leadsString = string.Empty;
            var oldNewValues = new Dictionary<string, dynamic> { };
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

            try
            {
                Logger.Current.Verbose("Getting the data for BDX Lead adapter provider");

                var ftpManager = new FtpService();
                var ftpcontent = ftpManager.GetService(leadAdapterAndAccountMap.RequestGuid);
                DateTime lastProcessed = leadAdapterAndAccountMap.LastProcessed == null ? DateTime.Now.ToUniversalTime().AddDays(-90) : leadAdapterAndAccountMap.LastProcessed.Value;
               
                string url = string.Format(ftpcontent.Host + "?CommunityID={0}&Username={1}&Password={2}&StartDate={3}&EndDate={4}",
                                           leadAdapterAndAccountMap.BuilderNumber, ftpcontent.UserName, ftpcontent.Password,
                                           lastProcessed.ToString("MM-dd-yyyy hh:mm:ss", CultureInfo.InvariantCulture),
                                           DateTime.Now.ToUniversalTime().ToString("MM-dd-yyyy hh:mm:ss", CultureInfo.InvariantCulture));
                leadsString = this.ProcessWebRequest(url);

                string ext = leadsString.Trim('\n', '\r');
                string formattedXml = XElement.Parse(ext).ToString();
                var xDocument = XDocument.Parse(formattedXml);
                var leads = xDocument.Descendants("Lead");
                customFields = customFields.Where(i => i.IsLeadAdapterField && i.LeadAdapterType == (byte)LeadAdapterTypes.PrivateCommunities && i.StatusId == FieldStatus.Active);
                IList<string> hashes = new List<string>();
                LeadAdapterRecordStatus status = LeadAdapterRecordStatus.Undefined;

                int AccountID = leadAdapterAndAccountMap.AccountID;
                foreach (var lead in leads)
                {
                    var guid = Guid.NewGuid();
                    var propertyIntrest = lead.Descendants("PropertyInterest").FirstOrDefault();
                    var leadcontact = lead.Descendants("Contact").FirstOrDefault();

                    string BuilderNumber = propertyIntrest.Attribute("BuilderNumber") != null ? propertyIntrest.Attribute("BuilderNumber").Value : "";
                    bool builderNumberPass = leadAdapterAndAccountMap.BuilderNumber.ToLower().Split(',').Contains(BuilderNumber.ToLower());

                    string CommunityNumber = propertyIntrest.Attribute("CommunityNumber") != null ? propertyIntrest.Attribute("CommunityNumber").Value : "";
                    bool communityNumberPass = true;
                    //bool communityNumberPass = String.IsNullOrEmpty(leadAdapterAndAccountMap.CommunityNumber) ? true :
                    //    leadAdapterAndAccountMap.CommunityNumber.ToLower().Split(',').Contains(CommunityNumber.ToLower()) && !string.IsNullOrEmpty(CommunityNumber);
                    Logger.Current.Informational("Processing leads for account : " + leadAdapterAndAccountMap.AccountName + " IsCommunityPass : " + communityNumberPass);

                    string FirstName = leadcontact.Attribute("FirstName") != null ? leadcontact.Attribute("FirstName").Value : null;
                    string LastName = leadcontact.Attribute("LastName") != null ? leadcontact.Attribute("LastName").Value : null;
                    string PrimaryEmail = leadcontact.Attribute("Email") != null ? leadcontact.Attribute("Email").Value : null;
                    string CompanyName = string.Empty;

                    StringBuilder hash = new StringBuilder();
                    IList<Email> emails = new List<Email>();

                    if (!string.IsNullOrEmpty(PrimaryEmail))
                    {

                        Email primaryemail = new Email()
                        {
                            EmailId = PrimaryEmail,
                            AccountID = leadAdapterAndAccountMap.AccountID,
                            IsPrimary = true
                        };
                        emails.Add(primaryemail);
                        hash.Append("-").Append(PrimaryEmail);
                    }
                    else
                    {
                        hash.Append("-").Append("na@na.com");
                    }

                    Person person = new Person()
                    {
                        FirstName = FirstName,
                        LastName = LastName,
                        CompanyName = CompanyName,
                        Emails = emails,
                        AccountID = AccountID
                    };

                    if (string.IsNullOrEmpty(FirstName))
                        hash.Append("-").Append(string.Empty);
                    else
                        hash.Append("-").Append(FirstName);

                    if (string.IsNullOrEmpty(LastName))
                        hash.Append("-").Append(string.Empty);
                    else
                        hash.Append("-").Append(LastName);

                    if (string.IsNullOrEmpty(CompanyName))
                        hash.Append("-").Append(string.Empty);
                    else
                        hash.Append("-").Append(CompanyName);


                    bool IsNotValidContact = false;

                    bool isDuplicateFromFile = false;

                    if (!IsNotValidContact && builderNumberPass && communityNumberPass)
                    {
                        bool duplicateemailcount = hashes.Where(h => h.Contains(PrimaryEmail)).Any();
                        if (!string.IsNullOrEmpty(PrimaryEmail) && duplicateemailcount)
                            isDuplicateFromFile = true;
                        else if (!string.IsNullOrEmpty(PrimaryEmail) && !duplicateemailcount)
                            isDuplicateFromFile = false;
                        else if (string.IsNullOrEmpty(PrimaryEmail) && hashes.Where(h => h.Contains(hash.ToString())).Any())
                            isDuplicateFromFile = true;
                        else
                            isDuplicateFromFile = false;
                    }

                    SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
                    if (!IsNotValidContact && builderNumberPass && isDuplicateFromFile == false && communityNumberPass)
                    {
                        SearchParameters parameters = new SearchParameters() { AccountId = AccountID };
                        IEnumerable<Contact> duplicateContacts = contactService.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person }).Contacts;
                        duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };
                    }

                    if (!builderNumberPass)
                        status = LeadAdapterRecordStatus.BuilderNumberFailed;
                    else if (!communityNumberPass)
                        status = LeadAdapterRecordStatus.CommunityNumberFailed;
                    else if (IsNotValidContact)
                        status = LeadAdapterRecordStatus.ValidationFailed;
                    else if (isDuplicateFromFile)
                        status = LeadAdapterRecordStatus.DuplicateFromFile;
                    else if (duplicateResult.TotalHits > 0)
                    {
                        status = LeadAdapterRecordStatus.Updated;
                        guid = duplicateResult.Results.FirstOrDefault().ReferenceId;
                    }
                    else
                        status = LeadAdapterRecordStatus.Added;



                    Contact duplicateperson = default(Person);
                    //contact.LeadAdapterRecordStatusId = (byte)status;
                    if (status == LeadAdapterRecordStatus.Updated)
                        duplicateperson = duplicateResult.Results.FirstOrDefault();
                    else
                        duplicateperson = new Person();

                    ///////////////////////////////////////////////////////////



                    RawContact contact = new RawContact();
                    IList<ImportCustomData> contactCustomData = new List<ImportCustomData>();
                    IList<ImportPhoneData> contactPhoneData = new List<ImportPhoneData>();

                    StringBuilder CustomFieldData = new StringBuilder();
                    try
                    {
                        XDocument doc = XDocument.Parse(lead.ToString());
                        List<XMLTypeHolder> allattributes = new List<XMLTypeHolder>();
                        foreach (XElement node in doc.Nodes())
                        {
                            allattributes.AddRange(node.Attributes().Select(x => new XMLTypeHolder { NodeName = node.Name.LocalName, LocalName = x.Name.LocalName, Value = x.Value }).ToList());
                            if (node.HasElements)
                            {
                                foreach (XElement ele in node.Elements())
                                {
                                    allattributes.AddRange(ele.Attributes().Select(x => new XMLTypeHolder { NodeName = ele.Name.LocalName, LocalName = x.Name.LocalName, Value = x.Value }).ToList());
                                }
                            }
                        }

                        foreach (var attribute in allattributes)
                        {
                            ImportCustomData customData = new ImportCustomData();
                            try
                            {
                                var elementvalue = string.Empty;
                                if (attribute.LocalName.ToLower() == "firstname" && attribute.NodeName == "Contact")
                                {
                                    elementvalue = ((Person)duplicateperson).FirstName == null ? "" : ((Person)duplicateperson).FirstName;
                                    contact.FirstName = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "lastname" && attribute.NodeName == "Contact")
                                {
                                    elementvalue = ((Person)duplicateperson).LastName;
                                    contact.LastName = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "email" && attribute.NodeName == "Contact")
                                {
                                    Email primaryemail = duplicateperson.Emails == null ? null : duplicateperson.Emails.Where(i => i.IsPrimary == true).FirstOrDefault();
                                    if (primaryemail != null)
                                        elementvalue = primaryemail.EmailId;
                                    contact.PrimaryEmail = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "company" && attribute.NodeName == "Contact")
                                {
                                    elementvalue = duplicateperson.CompanyName;
                                    contact.CompanyName = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "phone" && attribute.NodeName == "Contact")
                                {
                                    DropdownValueViewModel drpvalue = DropdownFields.Where(i => i.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
                                    var mobilephone = default(Phone);
                                    ImportPhoneData phoneData = new ImportPhoneData();
                                    phoneData.ReferenceID = guid;
                                    if (drpvalue != null)
                                    {
                                        if (!string.IsNullOrEmpty(attribute.Value))
                                        {
                                            string nonnumericstring = GetNonNumericData(attribute.Value);
                                            if (IsValidPhoneNumberLength(nonnumericstring))
                                            {
                                                contact.PhoneData = drpvalue.DropdownValueID.ToString() + "|" + nonnumericstring;
                                                phoneData.PhoneType = (int?)drpvalue.DropdownValueID;
                                                phoneData.PhoneNumber = nonnumericstring;
                                                contactPhoneData.Add(phoneData);
                                            }
                                        }
                                        mobilephone = duplicateperson.Phones == null ? null : duplicateperson.Phones.Where(i => i.PhoneType == drpvalue.DropdownValueID).FirstOrDefault();
                                    }

                                    if (mobilephone != null)
                                    {
                                        elementvalue = mobilephone.Number;
                                    }
                                }
                                else if (attribute.LocalName.ToLower() == "country" && attribute.NodeName == "Contact")
                                {
                                    var countryvalue = attribute.Value.Replace(" ", string.Empty).ToLower();
                                    if (countryvalue == "usa" || countryvalue == "us" || countryvalue == "unitedstates" || countryvalue == "unitedstatesofamerica")
                                        contact.Country = "US";
                                    else if (countryvalue == "ca" || countryvalue == "canada")
                                        contact.Country = "CA";
                                    else
                                        contact.Country = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "title" && attribute.NodeName == "Contact")
                                {
                                    contact.Title = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "streetadress" && attribute.NodeName == "Contact")
                                {
                                    contact.AddressLine1 = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "city" && attribute.NodeName == "Contact")
                                {
                                    contact.City = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "state" && attribute.NodeName == "Contact")
                                {
                                    contact.State = attribute.Value;
                                }
                                else if (attribute.LocalName.ToLower() == "postalcode" && attribute.NodeName == "Contact")
                                {
                                    contact.ZipCode = attribute.Value;
                                }
                                else
                                {
                                    var customField = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == (attribute.LocalName.ToLower() + "(" + LeadAdapterTypes.PrivateCommunities.ToString().ToLower() + ")")).FirstOrDefault();
                                    if (customField != null)
                                    {
                                        var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customField.FieldId).FirstOrDefault();
                                        if (customfielddata != null)
                                            elementvalue = customfielddata.Value;

                                        customData.FieldID = customField.FieldId;
                                        customData.FieldTypeID = (int?)customField.FieldInputTypeId;
                                        customData.ReferenceID = guid;

                                        if (customField.FieldInputTypeId == FieldType.date || customField.FieldInputTypeId == FieldType.datetime || customField.FieldInputTypeId == FieldType.time)
                                        {
                                            DateTime converteddate;
                                            if (DateTime.TryParse(attribute.Value, out converteddate))
                                            {
                                                CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + converteddate.ToString("MM/dd/yyyy hh:mm tt"));
                                                customData.FieldValue = converteddate.ToString("MM/dd/yyyy hh:mm tt");         
                                            }
                                        }
                                        else if (customField.FieldInputTypeId == FieldType.number)
                                        {
                                            double number;
                                            if (double.TryParse(attribute.Value, out number))
                                            {
                                                CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + number.ToString());
                                                customData.FieldValue = number.ToString();   
                                            }
                                        }
                                        else if (customField.FieldInputTypeId == FieldType.url)
                                        {
                                            if (IsValidURL(attribute.Value.Trim()))
                                            {
                                                CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + attribute.Value.Trim());
                                                customData.FieldValue = attribute.Value.Trim(); 
                                            }
                                        }
                                        else
                                        {
                                            CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + attribute.Value.Trim());
                                            customData.FieldValue = attribute.Value.Trim(); 
                                        }
                                        contactCustomData.Add(customData);
                                    }
                                }
                                if (!oldNewValues.ContainsKey(attribute.LocalName))
                                    oldNewValues.Add(attribute.LocalName, new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = attribute.Value });
                            }
                            catch (Exception ex)
                            {
                                Logger.Current.Error("An exception occured in Genereating old new values attribute in Private Communities : " + attribute.LocalName, ex);
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("An exception occured in Genereating old new values attribute in Private Communities : ", ex);
                    }
                    if (CustomFieldData.Length > 0)
                        CustomFieldData.Remove(0, 1);
                    contact.CustomFieldsData = CustomFieldData.ToString();
                    contact.ReferenceId = guid;
                    contact.AccountID = AccountID;
                    contact.LeadAdapterRecordStatusId = (byte)status;
                    contact.JobID = jobId;
                    contact.IsCommunityNumberPass = communityNumberPass;
                    contact.IsBuilderNumberPass = builderNumberPass;
                    contact.ContactStatusID = 1;
                    contact.ContactTypeID = 1;
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    contact.LeadAdapterSubmittedData = js.Serialize(oldNewValues);
                    contact.LeadAdapterRowData = lead.ToString();
                    personCustomFieldData.AddRange(contactCustomData);
                    personPhoneData.AddRange(contactPhoneData);

                    contact.ValidEmail = ValidateEmail(contact);

                    RawContact duplicate_data = null;
                    if (!String.IsNullOrEmpty(Convert.ToString(contact.PrimaryEmail)))
                        duplicate_data = persons.Where(p => string.Compare(p.PrimaryEmail, Convert.ToString(contact.PrimaryEmail), true) == 0).FirstOrDefault();
                    else
                        duplicate_data = persons.Where(p => string.Compare(p.FirstName, Convert.ToString(contact.FirstName), true) == 0 && string.Compare(p.LastName, Convert.ToString(contact.LastName), true) == 0).FirstOrDefault();

                    if (duplicate_data != null)
                    {
                        contact.IsDuplicate = true;
                        //RawContact updatedperson = MergeDuplicateData(duplicate_data, contact, guid);
                        //duplicate_data = updatedperson;
                    }
                    
                    persons.Add(contact);
                }
                
            }
            catch (Exception ex)
            {              
                Logger.Current.Error("Exception occured while getting bdx files :" + ex);
            }
            data.ContactCustomData = personCustomFieldData;
            data.ContactPhoneData = personPhoneData;
            data.ContactData = persons;
            return data;            
        }

        public override void Process()
        {
            leadAdapterAndAccountMap = leadAdaptersRepository.GetLeadAdapterByID(LeadAdapterAccountMapID);

            ServiceProvider ServiceProviders = serviceProviderRepository
              .GetServiceProviders(1, CommunicationType.Mail, MailType.TransactionalEmail);

            var response = customFieldService.GetAllCustomFields(new GetAllCustomFieldsRequest(leadAdapterAndAccountMap.AccountID));
            IEnumerable<FieldViewModel> customFields = response.CustomFields;
            var dropdownfeildsresposne = cacheService.GetDropdownValues(leadAdapterAndAccountMap.AccountID);

            IEnumerable<DropdownValueViewModel> phoneFields = dropdownfeildsresposne.Where(x => x.DropdownID == (short)DropdownFieldTypes.PhoneNumberType).Select(x => x.DropdownValuesList).FirstOrDefault();

            var jobLogId = InsertJobLog(leadAdapterAndAccountMap);
            var contacts = this.GetContacts("lead_feed.xml", customFields, jobLogId, phoneFields);
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

        /// <summary>
        /// Saving bulk contacts from imported file to temp table
        /// </summary>
        private void ContactsBulkinsert(ImportContactsData contacts)
        {
            importDataRepository.InsertImportsData(contacts);
            //  tagService.addLeadAdapterToTopic(leadAdapterAndAccountMap.Id, contacts, leadAdapterAndAccountMap.AccountID);
        }

        /// <summary>
        /// Mailgun verification for bulk contacts
        /// </summary>
        private void MailgunVerification(List<RawContact> contacts)
        {
            var mailGunList = new List<IEnumerable<string>>();
            var interimMailList = new List<string>();
            var mailLengthList = contacts
                                .Where(c => !string.IsNullOrEmpty(c.PrimaryEmail))
                                .Select(c => new { Email = c.PrimaryEmail, Length = (c.PrimaryEmail.Length + 1) }) //adding + 1 to consider comma in final string
                                .Distinct();

            var maxSize = 4000;
            int sum = 0; int index = 0;
            foreach (var mail in mailLengthList)
            {
                index++;
                sum = sum + mail.Length;
                if (sum >= maxSize)
                {
                    mailGunList.Add(interimMailList);
                    interimMailList = new List<string>();
                    interimMailList.Add(mail.Email);
                    sum = mail.Length;
                }
                interimMailList.Add(mail.Email);
                if (index == mailLengthList.Count())
                    mailGunList.Add(interimMailList);
            }

            foreach (var mailsToCheck in mailGunList)
            {
                var emails = string.Join(",", mailsToCheck);
                GetRestResponse response = mailGunService.BulkEmailValidate(new GetRestRequest() { Email = emails });
                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic restResponse = js.Deserialize<dynamic>(response.RestResponse.Content);

                if (restResponse != null)
                {
                    string[] valid = ((IEnumerable)restResponse["parsed"]).Cast<object>().Select(x => x.ToString()).ToArray();
                    string[] notvalid = ((IEnumerable)restResponse["unparseable"]).Cast<object>().Select(x => x.ToString()).ToArray();

                    contacts = contacts.Join(valid, c => c.PrimaryEmail, v => v, (c, v) => c).ToList();
                    foreach (RawContact contact in contacts)
                    {
                        contact.EmailStatus = (byte)EmailStatus.Verified;
                    }

                    if (notvalid.IsAny())
                    {
                        contacts = contacts.Join(notvalid, c => c.PrimaryEmail, v => v, (c, v) => c).ToList();
                        foreach (RawContact contact in contacts)
                        {
                            contact.EmailStatus = (byte)EmailStatus.HardBounce;
                        }
                    }        
                             
        
                }
                else
                {
                    contacts.ForEach(p => p.EmailStatus = (byte)EmailStatus.NotVerified);
                }
            }
            importDataRepository.InsertImportContactEmailStatuses(contacts);
        }

        private int InsertJobLog(LeadAdapterAndAccountMap accountMap)
        {
            Logger.Current.Informational("Inserting joblog for account id : " + accountMap.AccountID);
            var jobLog = new LeadAdapterJobLogs();
            jobLog.LeadAdapterAndAccountMapID = accountMap.Id;
            jobLog.LeadAdapterJobStatusID = LeadAdapterJobStatus.Undefined;
            jobLog.Remarks = string.Empty;
            jobLog.FileName = "lead_feed.xml";
            List<LeadAdapterJobLogDetails> details = new List<LeadAdapterJobLogDetails>();
            jobLog.LeadAdapterJobLogDetails = details;
            return importDataRepository.InsertLeadAdapterjob(jobLog, new Guid(), false, false, accountMap.AccountID, accountMap.CreatedBy, 1, accountMap.CreatedBy,true,0);
        }
 
        public static string GetNonNumericData(string input)
        {
            Regex regex = new Regex("[^a-zA-Z0-9]");
            string output = regex.Replace(input, "");
            if (output.Length > 20)
                return "";
            return regex.Replace(input, "");
        }
    }
}
