using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using System.Web.Script.Serialization;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using System.Text.RegularExpressions;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;

namespace SmartTouch.CRM.LeadAdapters.Providers
{
    public class NewHomeFeedLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IMailGunService mailGunService;
        IContactService contactService;
        ISearchService<Contact> searchService;
        IImportDataRepository importDataRepository;
        public NewHomeFeedLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
            IImportDataRepository importDataRepository, ISearchService<Contact> searchService, IUnitOfWork unitOfWork,
            ICustomFieldService customFieldService, ICachingService cacheService, ICommunicationService communicationService, IMailGunService mailGunService, IContactService contactService)
            : base(accountId, leadAdapterAndAccountMapID, LeadAdapterTypes.BDX, leadAdaptersRepository, importDataRepository, searchService, unitOfWork,
             customFieldService, cacheService, serviceProviderRepository, mailGunService, contactService)
        {
            this.mailGunService = mailGunService;
            this.searchService = searchService;
            this.contactService = contactService;
            this.importDataRepository = importDataRepository;

        }

        public override ImportContactsData GetContacts(string fileName, IEnumerable<FieldViewModel> customFields, int jobId, IEnumerable<DropdownValueViewModel> DropdownFields)
        {
            var persons = new List<RawContact>();
            var personCustomFieldData = new List<ImportCustomData>();
            var personPhoneData = new List<ImportPhoneData>();
            ImportContactsData data = new ImportContactsData();

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
                var xDocument = XDocument.Load(fileName);
                // getting all the leads in the file
                var leads = xDocument.Descendants("lead").Select(p => p).ToList();
                int AccountID = leadAdapterAndAccountMap.AccountID;
                IList<string> hashes = new List<string>();
                customFields = customFields.Where(i => i.IsLeadAdapterField && i.LeadAdapterType == (byte)LeadAdapterTypes.NewHomeFeed && i.StatusId == FieldStatus.Active);
                foreach (var lead in leads)
                {
                    // created the guid for reference in the contact
                    var guid = Guid.NewGuid();
                    string buildernumber = lead.Elements("buildernumber").FirstOrDefault() == null ? string.Empty : lead.Elements("buildernumber").First().Value;
                    bool builderNumberPass = leadAdapterAndAccountMap.BuilderNumber.ToLower().Split(',').Contains(buildernumber.ToLower());
                    bool communityNumberPass = true;
                    string CommunityNumber = lead.Elements("communitynumber").FirstOrDefault() == null ? string.Empty : lead.Elements("communitynumber").First().Value;
                    //bool communityNumberPass = String.IsNullOrEmpty(leadAdapterAndAccountMap.CommunityNumber) ? true :
                    //    leadAdapterAndAccountMap.CommunityNumber.ToLower().Split(',').Contains(CommunityNumber.ToLower()) && !string.IsNullOrEmpty(CommunityNumber);
                    Logger.Current.Informational("Processing leads for account : " + leadAdapterAndAccountMap.AccountName + " IsCommunityPass : " + communityNumberPass);

                    LeadAdapterRecordStatus status = LeadAdapterRecordStatus.Undefined;
                    RawContact contact = new RawContact();
                    IList<ImportCustomData> contactCustomData = new List<ImportCustomData>();
                    IList<ImportPhoneData> contactPhoneData = new List<ImportPhoneData>();
                    StringBuilder CustomFieldData = new StringBuilder();
                    var oldNewValues = new Dictionary<string, dynamic> { };

                    //////////////////////////////////////////
                    StringBuilder hash = new StringBuilder();

                    string FirstName = lead.Elements("firstname").FirstOrDefault() == null ? string.Empty : lead.Elements("firstname").First().Value;
                    string LastName = lead.Elements("lastname").FirstOrDefault() == null ? string.Empty : lead.Elements("lastname").First().Value;
                    string PrimaryEmail = lead.Elements("email").FirstOrDefault() == null ? string.Empty : lead.Elements("email").First().Value;
                    string CompanyName = lead.Elements("Company").FirstOrDefault() == null ? null : lead.Elements("Company").First().Value;


                    IList<Email> emails = new List<Email>();

                    if (!string.IsNullOrEmpty(PrimaryEmail))
                    {

                        Email primaryemail = new Email()
                        {
                            EmailId = PrimaryEmail,
                            AccountID = AccountID,
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



                    //var BrokenRules = person.GetBrokenRules();
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
                    if (status == LeadAdapterRecordStatus.Updated)
                        duplicateperson = duplicateResult.Results.FirstOrDefault();
                    else
                        duplicateperson = new Person();

                    //////////////////////////////////////////////////////
                    try
                    {
                        XDocument doc = XDocument.Parse(lead.ToString());
                        ImportCustomData customData = new ImportCustomData();
                        var elements = doc.Root.DescendantNodes().OfType<XElement>();
                        foreach (XElement element in elements)
                        {
                            try
                            {
                                var elementvalue = string.Empty;
                                if (element.Name.LocalName.ToLower() == "firstname")
                                {
                                    elementvalue = ((Person)duplicateperson).FirstName == null ? "" : ((Person)duplicateperson).FirstName;
                                    contact.FirstName = element.Value;
                                }
                                else if (element.Name.LocalName.ToLower() == "lastname")
                                {
                                    elementvalue = ((Person)duplicateperson).LastName;
                                    contact.LastName = element.Value;
                                }
                                else if (element.Name.LocalName.ToLower() == "email")
                                {
                                    Email primaryemail = duplicateperson.Emails == null ? null : duplicateperson.Emails.Where(i => i.IsPrimary == true).FirstOrDefault();
                                    if (primaryemail != null)
                                        elementvalue = primaryemail.EmailId;
                                    contact.PrimaryEmail = element.Value;
                                }
                                else if (element.Name.LocalName.ToLower() == "company")
                                {
                                    elementvalue = duplicateperson.CompanyName;
                                    contact.CompanyName = element.Value;
                                }
                                else if (element.Name.LocalName.ToLower() == "phone")
                                {
                                    DropdownValueViewModel drpvalue = DropdownFields.Where(i => i.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
                                    var mobilephone = default(Phone);
                                    ImportPhoneData phoneData = new ImportPhoneData();
                                    phoneData.ReferenceID = guid;
                                    if (drpvalue != null)
                                    {
                                        if (!string.IsNullOrEmpty(element.Value))
                                        {
                                            string nonnumericstring = GetNonNumericData(element.Value);
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
                                else
                                {
                                    var customField = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == (element.Name.LocalName.ToLower() + "(" + LeadAdapterTypes.NewHomeFeed.ToString().ToLower() + ")")).FirstOrDefault();
                                    if (customField != null)
                                    {
                                        customData = new ImportCustomData();
                                        var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customField.FieldId).FirstOrDefault();
                                        if (customfielddata != null)
                                            elementvalue = customfielddata.Value;

                                        customData.FieldID = customField.FieldId;
                                        customData.FieldTypeID = (int?)customField.FieldInputTypeId;
                                        customData.ReferenceID = guid;

                                        if (customField.FieldInputTypeId == FieldType.date || customField.FieldInputTypeId == FieldType.datetime || customField.FieldInputTypeId == FieldType.time)
                                        {

                                            DateTime converteddate;
                                            if (DateTime.TryParse(element.Value.Trim(), out converteddate))
                                            {
                                                CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + converteddate.ToString("MM/dd/yyyy hh:mm tt"));
                                                customData.FieldValue = converteddate.ToString("MM/dd/yyyy hh:mm tt");                                                
                                            }
                                        }
                                        else if (customField.FieldInputTypeId == FieldType.number)
                                        {
                                            double number;
                                            if (double.TryParse(element.Value.Trim(), out number))
                                            {
                                                CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + number.ToString());
                                                customData.FieldValue = number.ToString();          
                                            }
                                        }
                                        else if (customField.FieldInputTypeId == FieldType.url)
                                        {
                                            if (IsValidURL(element.Value.Trim()))
                                            {
                                                CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + element.Value.Trim());
                                                customData.FieldValue = element.Value.Trim(); 
                                            }
                                        }
                                        else
                                        {
                                            CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + element.Value.Trim());
                                            customData.FieldValue = element.Value.Trim(); 
                                        }
                                        contactCustomData.Add(customData);
                                    }
                                }
                                if (!oldNewValues.ContainsKey(element.Name.LocalName))
                                    oldNewValues.Add(element.Name.LocalName, new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = element.Value.Trim() });
                            }
                            catch (Exception ex)
                            {
                                Logger.Current.Error("An exception occured in Genereating old new values element in NewHomeFeed : " + element.Name.LocalName, ex);
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("An exception occured in Genereating old new values element in NewHomeFeed : ", ex);
                    }
                    if (CustomFieldData.Length > 0)
                        CustomFieldData.Remove(0, 1);

                    contact.CustomFieldsData = CustomFieldData.ToString();
                    contact.ReferenceId = guid;
                    contact.AccountID = AccountID;
                    contact.IsBuilderNumberPass = builderNumberPass;
                    contact.IsCommunityNumberPass = communityNumberPass;
                    contact.LeadAdapterRecordStatusId = (byte)status;
                    contact.JobID = jobId;
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
                        contact.IsDuplicate = true;

                    persons.Add(contact);
                }

            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occurred in zillow lead apapterprovider get data method", ex);
            }
            data.ContactCustomData = personCustomFieldData;
            data.ContactPhoneData = personPhoneData;
            data.ContactData = persons;
            return data;
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
