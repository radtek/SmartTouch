using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;

namespace SmartTouch.CRM.LeadAdapters.Providers
{
    public class BuzzBuzzHomesLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IContactService contactService;
        IMailGunService mailGunService;
        ISearchService<Contact> searchService;
        IImportDataRepository importDataRepository;
        public BuzzBuzzHomesLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
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

  
        private string ProcessWebRequest(string url)
        {
            string result = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = ".NET Framework Test Client";
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
                    //leadAdapterAndAccountMap.LeadAdapterErrorStatusID = LeadAdapterErrorStatus.Error;
                    //leadAdapterAndAccountMap.LeadAdapterServiceStatusID = LeadAdapterServiceStatus.InvalidCredentials;
                    //leadAdapterAndAccountMap.LastProcessed = DateTime.Now.ToUniversalTime();
                    //repository.Update(leadAdapterAndAccountMap);
                    //unitOfWork.Commit();
                }
                Logger.Current.Error("An error occurred in idx get data method(web exception): " + ex);
               // SendEmail("Exception details" + ex, "Exception while processing data from Buzz Buzz Home LeadAdapter");
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Logger.Current.Error("An error occurred in process web request method in buzz buzz homes: ", ex);
               // SendEmail("Exception details" + ex, "Exception while processing data from Buzz Buzz Home LeadAdapter");
            }
            return result;
        }

        //public override bool IsValidPhoneNumberLength(string phoneNumber)
        //{
        //    bool isValidPhone = false;
        //    string pattern = @"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$";
        //    Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
        //    bool result = isValidPhone = regex.IsMatch(phoneNumber);
        //    return result;
        //}

        public override ImportContactsData GetContacts(string fileName, IEnumerable<FieldViewModel> customFields, int jobId, IEnumerable<DropdownValueViewModel> DropdownFields)
        {

            var persons = new List<RawContact>();
            ImportContactsData data = new ImportContactsData();
            var personCustomFieldData = new List<ImportCustomData>();
            var personPhoneData = new List<ImportPhoneData>();
            string leadsString = string.Empty;
            IList<string> hashes = new List<string>();
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
                int AccountID = leadAdapterAndAccountMap.AccountID;              
                var ftpManager = new FtpService();
                var ftpcontent = ftpManager.GetService(leadAdapterAndAccountMap.RequestGuid);
                DateTime lastProcessed = leadAdapterAndAccountMap.LastProcessed == null ? DateTime.Now.ToUniversalTime().AddDays(-90) : leadAdapterAndAccountMap.LastProcessed.Value;
                string url = string.Format(ftpcontent.Host + "/{0}/{1}/{2}?date={3}&time={4}",
                                           ftpcontent.UserName, "xmlfeeds", ftpcontent.Password,
                                           lastProcessed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                           lastProcessed.ToString("HH:mm"));
                leadsString = this.ProcessWebRequest(url);              
                var xDocument = XDocument.Parse(leadsString);
                var developmentElements = xDocument.Descendants("development");
                var builderagency = xDocument.Descendants("developerbuilderagency");
                //string developerbuilderagency = builderagency != null ? builderagency.FirstOrDefault() == null ? "" : builderagency.FirstOrDefault().Value : "";
                LeadAdapterRecordStatus status = LeadAdapterRecordStatus.Undefined;
                customFields = customFields.Where(i => i.LeadAdapterType == (byte)LeadAdapterTypes.BuzzBuzzHomes && i.IsLeadAdapterField && i.StatusId == FieldStatus.Active);

                foreach (var developementElement in developmentElements)
                {
                    // developementElement attributes                    
                    //string developmentname = developementElement.Element("developmentname") != null ? developementElement.Element("developmentname").Value : "";
                    //string providerdevelopmentid = developementElement.Element("providerdevelopmentid") != null ? developementElement.Element("providerdevelopmentid").Value : "";
                    var leads = developementElement.Descendants("lead");

                  

                    foreach (var lead in leads)
                    {
                        // created the guid for reference in the contact
                        var guid = Guid.NewGuid();                      
                        RawContact contact = new RawContact();
                        IList<ImportCustomData> contactCustomData = new List<ImportCustomData>();
                        IList<ImportPhoneData> contactPhoneData = new List<ImportPhoneData>();
                        var oldNewValues = new Dictionary<string, dynamic> { };
                        StringBuilder CustomFieldData = new StringBuilder();

                        ////////////////////////////////////////////////
                        StringBuilder hash = new StringBuilder();

                        string firstName = string.Empty;
                        string lastName = string.Empty;

                        string fullName = lead.Element("name").Value;

                        string[] name = fullName.Split(' ');
                        if (name.Length == 1)
                        {
                            firstName = name[0];
                        }
                        else if (name.Length > 1)
                        {
                            for (var i = 0; i < name.Length; i++)
                            {
                                if (i == 0)
                                {
                                    firstName = name[i];
                                }
                                else
                                {
                                    lastName = name[i] + " ";
                                }
                            }
                        }
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
                            FirstName = firstName,
                            LastName = lastName,
                            CompanyName = CompanyName,
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

                        if (string.IsNullOrEmpty(CompanyName))
                            hash.Append("-").Append(string.Empty);
                        else
                            hash.Append("-").Append(CompanyName);



                        //var BrokenRules = person.GetBrokenRules();
                        bool IsNotValidContact = false;

                        bool isDuplicateFromFile = false;

                        if (!IsNotValidContact)
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
                        if (!IsNotValidContact && isDuplicateFromFile == false)
                        {
                            SearchParameters parameters = new SearchParameters() { AccountId = AccountID };
                            IEnumerable<Contact> duplicateContacts = contactService.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person }).Contacts;
                            duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };
                        }
                       
                        if (IsNotValidContact)
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
                        ///////////////////////////////////////////////////
                        try
                        {
                            XDocument doc = XDocument.Parse(lead.ToString());
                            var elements = doc.Root.DescendantNodes().OfType<XElement>();
                            foreach (XElement element in elements)
                            {
                                try
                                {
                                    var elementvalue = string.Empty;
                                    if (element.Name.LocalName.ToLower() == "name")
                                    {

                                        fullName = lead.Element("name").Value;

                                        string[] Name = fullName.Split(' ');

                                        if (Name.Length == 1)
                                        {
                                            firstName = Name[0];
                                        }
                                        else if (Name.Length > 1)
                                        {
                                            for (var i = 0; i < Name.Length; i++)
                                            {
                                                if (i == 0)
                                                {
                                                    firstName = Name[i];
                                                }
                                                else
                                                {
                                                    lastName = Name[i] + " ";
                                                }
                                            }
                                        }
                                        string previousfirstname = ((Person)duplicateperson).FirstName == null ? "" : ((Person)duplicateperson).FirstName;
                                        string previouslastname = ((Person)duplicateperson).LastName == null ? "" : ((Person)duplicateperson).LastName;

                                        elementvalue = previousfirstname + " " + previouslastname;
                                        contact.FirstName = firstName;
                                        contact.LastName = lastName;
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
                                        if (drpvalue != null && !string.IsNullOrEmpty(element.Value))
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

                                        if (mobilephone != null)
                                        {
                                            elementvalue = mobilephone.Number;
                                        }
                                    }
                                    else
                                    {
                                        var customField = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == (element.Name.LocalName.ToLower() + "(" + LeadAdapterTypes.BuzzBuzzHomes.ToString().ToLower() + ")")).FirstOrDefault();
                                        ImportCustomData customData = new ImportCustomData();
                                        customData.FieldID = customField.FieldId;
                                        customData.FieldTypeID = (int?)customField.FieldInputTypeId;
                                        customData.ReferenceID = guid;

                                        if (customField != null)
                                        {
                                            var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customField.FieldId).FirstOrDefault();
                                            if (customfielddata != null)
                                                elementvalue = customfielddata.Value;

                                            if (customField.FieldInputTypeId == FieldType.date || customField.FieldInputTypeId == FieldType.datetime || customField.FieldInputTypeId == FieldType.time)
                                            {
                                                DateTime converteddate;
                                                if (DateTime.TryParse(element.Value.Trim(), out converteddate))
                                                {
                                                   CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + converteddate.ToString("MM/dd/yyyy hh:mm tt"));
                                                    customData.FieldValue = converteddate.ToString("MM/dd/yyyy hh:mm tt");
                                                    contactCustomData.Add(customData);
                                                }
                                            }
                                            else if (customField.FieldInputTypeId == FieldType.number)
                                            {
                                                double number;
                                                if (double.TryParse(element.Value.Trim(), out number))
                                                {
                                                   CustomFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + number.ToString());
                                                    customData.FieldValue = number.ToString();
                                                    contactCustomData.Add(customData);
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
                                        oldNewValues.Add(element.Name.LocalName, new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = element.Value });
                                }
                                catch (Exception ex)
                                {
                                    Logger.Current.Error("An exception occured in Genereating old new values element in BuzzBuzz Homes : " + element.Name.LocalName, ex);
                                    continue;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Current.Error("An exception occured in Genereating old new values element in Buzzbuzz homes : ", ex);
                        }
                        if (CustomFieldData.Length > 0)
                            CustomFieldData.Remove(0, 1);

                        contact.CustomFieldsData = CustomFieldData.ToString();
                        contact.CustomFieldsData = CustomFieldData.ToString();
                        contact.ReferenceId = guid;
                        contact.AccountID = AccountID;
                        contact.LeadAdapterRecordStatusId = (byte)status;
                        contact.JobID = jobId;
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        contact.LeadAdapterSubmittedData = js.Serialize(oldNewValues);
                        contact.LeadAdapterRowData = lead.ToString();
                        contact.ContactStatusID = 1;
                        contact.ContactTypeID = 1;
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

  
        //public bool IsValidURL(string URL)
        //{
        //    string pattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";
        //    bool result = Regex.IsMatch(URL, pattern);
        //    return result;
        //}

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
