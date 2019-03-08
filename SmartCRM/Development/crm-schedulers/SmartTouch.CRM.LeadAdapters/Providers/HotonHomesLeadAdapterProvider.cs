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
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Web.Script.Serialization;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using System.Text.RegularExpressions;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;


namespace SmartTouch.CRM.LeadAdapters.Providers
{
    public class HotonHomesLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IMailGunService mailGunService;
        IContactService contactService;
        ISearchService<Contact> searchService;
        IImportDataRepository importDataRepository;
        public HotonHomesLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
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
                var xDocument = XDocument.Load(fileName);
                customFields = customFields.Where(i => i.IsLeadAdapterField && i.LeadAdapterType == (byte)LeadAdapterTypes.HotonHomes && i.StatusId == FieldStatus.Active);
                IList<string> hashes = new List<string>();
                //Process for each person
                var leads = xDocument.Descendants("Lead");
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

                    LeadAdapterRecordStatus status = LeadAdapterRecordStatus.Undefined;
                    RawContact contact = new RawContact();
                    IList<ImportCustomData> contactCustomData = new List<ImportCustomData>();
                    IList<ImportPhoneData> contactPhoneData = new List<ImportPhoneData>();
                    StringBuilder CustomFieldData = new StringBuilder();


                    ////////////////////////////////////////////////
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

                    /////////////////////////////////////////////////////////////////

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
                                    // get company dynamic
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
                                    var customField = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == (attribute.LocalName.ToLower() + "(" + LeadAdapterTypes.HotonHomes.ToString().ToLower() + ")")).FirstOrDefault();
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
                                                contactCustomData.Add(customData);
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
                                if(!oldNewValues.ContainsKey(attribute.LocalName))
                                    oldNewValues.Add(attribute.LocalName, new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = attribute.Value });
                            }
                            catch (Exception ex)
                            {
                                Logger.Current.Error("An exception occured in Genereating old new values attribute in HotOnHomes : " + attribute.LocalName, ex);
                                continue;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("An exception occured in Genereating old new values attribute in HotOnHomes : ", ex);
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
