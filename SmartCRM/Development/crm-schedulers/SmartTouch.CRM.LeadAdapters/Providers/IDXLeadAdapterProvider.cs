using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using System.Configuration;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using System.Web.Script.Serialization;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using System.Threading.Tasks;
using System.Collections;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;


namespace SmartTouch.CRM.LeadAdapters.Providers
{
    public class IDXLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IMailGunService mailGunService;
        IContactService contactService;
        ISearchService<Contact> searchService;
        IImportDataRepository importDataRepository;
        public IDXLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
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

   
        public static string GetNonNumericData(string input)
        {
            Regex regex = new Regex("[^a-zA-Z0-9]");
            string output = regex.Replace(input, "");
            if (output.Length > 20)
                return "";
            return regex.Replace(input, "");
        }

        public static HttpWebRequest CreateWebRequest(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";

            return webRequest;
        }

        public override ImportContactsData GetContacts(string fileName, IEnumerable<FieldViewModel> customFields, int jobId, IEnumerable<DropdownValueViewModel> DropdownFields)
        {
            List<RawContact> persons = new List<RawContact>();
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

            string soapResult = string.Empty;           
            try
            {           
                var ftpManager = new FtpService();
                var ftpcontent = ftpManager.GetService(leadAdapterAndAccountMap.RequestGuid);
                var cid = default(int);
                int.TryParse(ftpcontent.UserName, out cid);
                DateTime lastProcessed = leadAdapterAndAccountMap.LastProcessed == null ? DateTime.Now.ToUniversalTime().AddDays(-90) : leadAdapterAndAccountMap.LastProcessed.Value;
                HttpWebRequest request = CreateWebRequest(ftpcontent.Host);
                XmlDocument soapEnvelopeXml = new XmlDocument();
                List<string> hashes = new List<string>();
                soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                  <soap:Body><listLeads xmlns='http://tempuri.org/'><cid xsi:type="" xsd:int"">" + cid + @"</cid><password xsi:type="" xsd:string"">" + ftpcontent.Password + "</password>" +
                    "</listLeads></soap:Body></soap:Envelope>");
                LeadAdapterRecordStatus status = LeadAdapterRecordStatus.Undefined;

                using (Stream stream = request.GetRequestStream())
                {
                    soapEnvelopeXml.Save(stream);
                }

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();
                        //Console.WriteLine(soapResult);
                    }
                }

                int AccountID = leadAdapterAndAccountMap.AccountID;
                if (!string.IsNullOrWhiteSpace(soapResult))
                {
                    int startIndex = soapResult.IndexOf("<return xsi:type=\"xsd:string\">") + "<return xsi:type=\"xsd:string\">".Length;
                    int endIndex = soapResult.IndexOf("</return>");
                    string newString = soapResult.Substring(startIndex, endIndex - startIndex);

                    string[] leadsList = Regex.Split(newString, "\r\n|\r|\n");

                    /*The lead's ID 0 | 
                     * The lead's name |
                     * The lead's primary email address |
                     * Agent Owner (by IDX assigned Agent ID) | 
                     * The date the lead subscribed |
                     * The opt-in status of the lead (y = opted in) | 
                     * The disabled status of the lead (y = disabled) | 
                     * If the lead account is disabled | 
                     * If the lead can login |
                     * If the lead unsubscribed from email updates |
                     * If the lead wants plain text of HTML emails |
                     * The area code of the lead | 
                     * The first 3 digits of the lead phone number |
                     * The final 4 digits of the lead phone number | 
                     * The lead street address | 
                     * The lead city | 
                     * The lead state |                      
                     * The lead zipcode |
                     * The last lead login date |
                     * The last lead update date | 
                     * The lead category | 
                     * If the lead has been flagged (starred)*/


                    leadsList = leadsList.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    customFields = customFields.Where(i => i.IsLeadAdapterField && i.StatusId == FieldStatus.Active && i.LeadAdapterType == (byte)LeadAdapterTypes.IDX);

                    foreach (var contact in leadsList)
                    {
                        var guid = Guid.NewGuid();
                     
                        var contactDetails = contact.Split('|');
                        var disabledstatus = contactDetails[6];
                        var accountdisabled = contactDetails[7];

                        if (disabledstatus == "y" || accountdisabled == "y" ||
                            DateTime.Compare(lastProcessed, DateTime.Parse(contactDetails[5], CultureInfo.InvariantCulture)) > 0)
                            continue;

                        string fullName = string.Empty;
                        string firstName = string.Empty;
                        string lastName = string.Empty;
                        if (contactDetails.Length > 1)
                            fullName = contactDetails[1];

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
                        IList<ImportCustomData> contactCustomData = new List<ImportCustomData>();
                        IList<ImportPhoneData> contactPhoneData = new List<ImportPhoneData>();
                        ///////////////////////////////////
                        var oldNewValues = new Dictionary<string, dynamic> { };
                        StringBuilder hash = new StringBuilder();
                        string primaryemail = contactDetails.Length > 2 ? contactDetails[2] : string.Empty;
                        IList<Email> emails = new List<Email>();

                        if (!string.IsNullOrEmpty(primaryemail))
                        {

                            Email email = new Email()
                            {
                                EmailId = primaryemail,
                                AccountID = AccountID,
                                IsPrimary = true
                            };
                            emails.Add(email);
                            hash.Append("-").Append(primaryemail);
                        }
                        else
                        {
                            hash.Append("-").Append("na@na.com");
                        }

                        Person person1 = new Person()
                        {
                            FirstName = firstName,
                            LastName = lastName,
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
                        hash.Append("-").Append(string.Empty);

                        bool IsNotValidContact = false;
                        bool isDuplicateFromFile = false;

                        if (!IsNotValidContact)
                        {
                            bool duplicateemailcount = hashes.Where(h => h.Contains(primaryemail)).Any();
                            if (!string.IsNullOrEmpty(primaryemail) && duplicateemailcount)
                                isDuplicateFromFile = true;
                            else if (!string.IsNullOrEmpty(primaryemail) && !duplicateemailcount)
                                isDuplicateFromFile = false;
                            else if (string.IsNullOrEmpty(primaryemail) && hashes.Where(h => h.Contains(hash.ToString())).Any())
                                isDuplicateFromFile = true;
                            else
                                isDuplicateFromFile = false;
                        }

                        SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
                        if (!IsNotValidContact && isDuplicateFromFile == false)
                        {
                            SearchParameters parameters = new SearchParameters() { AccountId = AccountID };
                            IEnumerable<Contact> duplicateContacts = contactService.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person1 }).Contacts;
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


                        ///////////////////////////////////////////////////////////////
                      
                        StringBuilder CustomFields = new StringBuilder();
                        ImportCustomData customData = new ImportCustomData();
                        customData.ReferenceID = guid;
                      
                        string elementvalue = string.Empty;
                        string leadsid = contactDetails[0];
                        if (!string.IsNullOrEmpty(leadsid))
                        {
                            var customfield = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == "lead'sid(idx)").FirstOrDefault();
                            if (customfield != null)
                            {
                                var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customfield.FieldId).FirstOrDefault();
                                if (customfielddata != null)
                                    elementvalue = customfielddata.Value;
                                if (CustomFields.Length == 0)
                                    CustomFields.Append(customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + leadsid);
                                else
                                    CustomFields.Append("~" + customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + leadsid);
                            }

                            customData.FieldID = customfield.FieldId;
                            customData.FieldTypeID = (int?)customfield.FieldInputTypeId;
                            customData.FieldValue = leadsid;                 
                            contactCustomData.Add(customData);

                            oldNewValues.Add("lead's ID", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = leadsid });
                        }
                        //if (!string.IsNullOrEmpty(fullName))
                        //{
                        //    string oldname = string.Empty;
                        //    if (!string.IsNullOrEmpty(duplicateperson.))
                        //        oldname += duplicateperson.FirstName + " ";
                        //    if (!string.IsNullOrEmpty(duplicateperson.LastName))
                        //        oldname += duplicateperson.LastName;
                        //    oldNewValues.Add("The lead's name", new { OldValue = oldname, NewValue = fullName });
                        //}

                        //if (!string.IsNullOrEmpty(primaryemail))
                        //{
                        //    string oldemail = string.Empty;
                        //    if (duplicateperson.Emails != null && duplicateperson.Emails.Count(i => i.IsPrimary) > 0)
                        //        oldemail = duplicateperson.Emails.SingleOrDefault(i => i.IsPrimary).EmailId;
                        //    oldNewValues.Add("The lead's primary email address", new { OldValue = oldemail, NewValue = primaryemail });
                        //}
                        string agentowner = contactDetails[3];
                        if (!string.IsNullOrEmpty(agentowner))
                        {
                          
                            var customfield = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == "agentowner(idx)").FirstOrDefault();
                            if (customfield != null)
                            {
                                var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customfield.FieldId).FirstOrDefault();
                                if (customfielddata != null)
                                    elementvalue = customfielddata.Value;
                                if (CustomFields.Length == 0)
                                    CustomFields.Append(customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + agentowner);
                                else
                                    CustomFields.Append("~" + customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + agentowner);
                            }
                            customData.FieldID = customfield.FieldId;
                            customData.FieldTypeID = (int?)customfield.FieldInputTypeId;
                            customData.FieldValue = agentowner;
                            contactCustomData.Add(customData);

                            oldNewValues.Add("Agent Owner", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = agentowner });
                        }

                        string leadsubscribeddate = contactDetails[5];
                        if (!string.IsNullOrEmpty(leadsubscribeddate))
                        {
                            var customfield = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == "thedatetheleadsubscribed(idx)").FirstOrDefault();
                            if (customfield != null)
                            {
                                var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customfield.FieldId).FirstOrDefault();
                                if (customfielddata != null)
                                    elementvalue = customfielddata.Value;
                                if (CustomFields.Length == 0)
                                    CustomFields.Append(customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + leadsubscribeddate);
                                else
                                    CustomFields.Append("~" + customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + leadsubscribeddate);
                            }
                            customData.FieldID = customfield.FieldId;
                            customData.FieldTypeID = (int?)customfield.FieldInputTypeId;
                            customData.FieldValue = leadsubscribeddate;
                            contactCustomData.Add(customData);

                            oldNewValues.Add("The date the lead subscribed", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = leadsubscribeddate });
                        }

                        string optinstatusoflead = contactDetails[6];
                        if (!string.IsNullOrEmpty(optinstatusoflead))
                        {
                            var customfield = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == "theopt-instatusofthelead(idx)").FirstOrDefault();
                            if (customfield != null)
                            {
                                var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customfield.FieldId).FirstOrDefault();
                                if (customfielddata != null)
                                    elementvalue = customfielddata.Value;
                                if (CustomFields.Length == 0)
                                    CustomFields.Append(customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + optinstatusoflead);
                                else
                                    CustomFields.Append("~" + customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + optinstatusoflead);
                            }
                            customData.FieldID = customfield.FieldId;
                            customData.FieldTypeID = (int?)customfield.FieldInputTypeId;
                            customData.FieldValue = optinstatusoflead;
                            contactCustomData.Add(customData);
                            oldNewValues.Add("The opt-in status of the lead", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = optinstatusoflead });
                        }

                        string donotemail = contactDetails[9];
                        if (!string.IsNullOrEmpty(donotemail))
                        {
                            var customfield = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == "iftheleadunsubscribedfromemailupdates(idx)").FirstOrDefault();
                            if (customfield != null)
                            {
                                var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customfield.FieldId).FirstOrDefault();
                                if (customfielddata != null)
                                    elementvalue = customfielddata.Value;
                              
                                if (CustomFields.Length == 0)
                                    CustomFields.Append(customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + donotemail);
                                else
                                    CustomFields.Append("~" + customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + donotemail);
                            }
                            customData.FieldID = customfield.FieldId;
                            customData.FieldTypeID = (int?)customfield.FieldInputTypeId;
                            customData.FieldValue = donotemail;
                            contactCustomData.Add(customData);
                            oldNewValues.Add("If the lead unsubscribed from email updates", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = donotemail });
                        }

                        StringBuilder phonedata = new StringBuilder();
                        string areacode = contactDetails.Length > 11 ? contactDetails[11] : string.Empty;
                        string first3digitsofphone = contactDetails.Length > 12 ? contactDetails[12] : string.Empty;
                        string last4digitsofphone = contactDetails.Length > 13 ? contactDetails[13] : string.Empty;

                        string Phone = string.Concat(areacode, first3digitsofphone, last4digitsofphone);
                        if (!string.IsNullOrEmpty(Phone))
                        {
                            //string nonnumericphonenumber = GetNonNumericData(Phone);
                            DropdownValueViewModel drpvalue = DropdownFields.Where(i => i.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
                            var mobilephone = default(Phone);
                            ImportPhoneData phoneData = new ImportPhoneData();
                            phoneData.ReferenceID = guid;
                            if (drpvalue != null && !string.IsNullOrEmpty(Phone))
                            {                               
                                    string nonnumericstring = GetNonNumericData(Phone);
                                    if (IsValidPhoneNumberLength(nonnumericstring))
                                    {
                                        phonedata.Append(drpvalue.DropdownValueID.ToString() + "|" + nonnumericstring);
                                        phoneData.PhoneType = (int?)drpvalue.DropdownValueID;
                                        phoneData.PhoneNumber = nonnumericstring;
                                        contactPhoneData.Add(phoneData);
                                    }                                                     
                            }

                            if (mobilephone != null)
                            {
                                elementvalue = mobilephone.Number;
                            }
                            oldNewValues.Add("Phone number", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = Phone });
                        }

                        string leadsstreetadress = contactDetails[14];
                        oldNewValues.Add("The lead street address", new { OldValue = string.Empty, NewValue = leadsstreetadress });
                        string leadscity = contactDetails[15];
                        oldNewValues.Add("The lead city", new { OldValue = string.Empty, NewValue = leadscity });
                        string leadsstate = contactDetails[16];
                        oldNewValues.Add("The lead state", new { OldValue = string.Empty, NewValue = leadsstate });
                        string leadszipcode = contactDetails[17];
                        oldNewValues.Add("The lead zipcode", new { OldValue = string.Empty, NewValue = leadszipcode });
                    
                        string leadlastlogindate = contactDetails[18];
                        if (!string.IsNullOrEmpty(leadlastlogindate))
                        {
                            var customfield = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == "thelastleadlogindate(idx)").FirstOrDefault();
                            if (customfield != null)
                            {
                                var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customfield.FieldId).FirstOrDefault();
                                if (customfielddata != null)
                                    elementvalue = customfielddata.Value;
                                if (CustomFields.Length == 0)
                                    CustomFields.Append(customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + leadlastlogindate);
                                else
                                    CustomFields.Append("~" + customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + leadlastlogindate);
                            }
                            customData.FieldID = customfield.FieldId;
                            customData.FieldTypeID = (int?)customfield.FieldInputTypeId;
                            customData.FieldValue = leadlastlogindate;
                            contactCustomData.Add(customData);

                            oldNewValues.Add("The last lead login date", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = leadlastlogindate });
                        }

                        string leadlastupdatedate = contactDetails[19];
                        if (!string.IsNullOrEmpty(leadlastupdatedate))
                        {
                            var customfield = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == "thelastleadupdatedate(idx)").FirstOrDefault();
                            if (customfield != null)
                            {
                                var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customfield.FieldId).FirstOrDefault();
                                if (customfielddata != null)
                                    elementvalue = customfielddata.Value;
                                if (CustomFields.Length == 0)
                                    CustomFields.Append(customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + leadlastupdatedate);
                                else
                                    CustomFields.Append("~" + customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + leadlastupdatedate);
                            }
                            customData.FieldID = customfield.FieldId;
                            customData.FieldTypeID = (int?)customfield.FieldInputTypeId;
                            customData.FieldValue = leadlastupdatedate;
                            contactCustomData.Add(customData);

                            oldNewValues.Add("The last lead update date", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = leadlastupdatedate });
                        }

                        string leadcategory = contactDetails[20];
                        if (!string.IsNullOrEmpty(leadcategory))
                        {
                            var customfield = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == "theleadcategory(idx)").FirstOrDefault();
                            if (customfield != null)
                            {
                                var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customfield.FieldId).FirstOrDefault();
                                if (customfielddata != null)
                                    elementvalue = customfielddata.Value;
                                if (CustomFields.Length == 0)
                                    CustomFields.Append(customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + leadcategory);
                                else
                                    CustomFields.Append("~" + customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + leadcategory);
                            }
                            customData.FieldID = customfield.FieldId;
                            customData.FieldTypeID = (int?)customfield.FieldInputTypeId;
                            customData.FieldValue = leadcategory;
                            contactCustomData.Add(customData);

                            oldNewValues.Add("The lead category", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = leadcategory });
                        }

                        string isleadflagged = contactDetails[21];
                        if (!string.IsNullOrEmpty(isleadflagged))
                        {
                            var customfield = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == "iftheleadhasbeenflagged(idx)").FirstOrDefault();
                            if (customfield != null)
                            {
                                var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customfield.FieldId).FirstOrDefault();
                                if (customfielddata != null)
                                    elementvalue = customfielddata.Value;
                                if (CustomFields.Length == 0)
                                    CustomFields.Append(customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + isleadflagged);
                                else
                                    CustomFields.Append("~" + customfield.FieldId + "##$##" + (byte)customfield.FieldInputTypeId + "|" + isleadflagged);
                            }
                            customData.FieldID = customfield.FieldId;
                            customData.FieldTypeID = (int?)customfield.FieldInputTypeId;
                            customData.FieldValue = isleadflagged;
                            contactCustomData.Add(customData);
                            oldNewValues.Add("If the lead has been flagged", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = isleadflagged });
                        }
                        contactCustomData.Add(customData);
                        var person = new RawContact
                        {
                            ReferenceId = guid,
                            AccountID = leadAdapterAndAccountMap.AccountID,
                            FirstName = firstName,
                            LastName = lastName,
                            PrimaryEmail = primaryemail,
                            AddressLine1 = leadsstreetadress,
                            City = leadscity,
                            State = leadsstate,
                            PhoneData = phonedata.ToString(),
                            
                            CustomFieldsData = CustomFields.ToString(),
                            ZipCode = contactDetails.Length > 17 ? contactDetails[17] : string.Empty
                        };                      
                        person.ReferenceId = guid;
                        person.AccountID = AccountID;                      
                        person.LeadAdapterRecordStatusId = (byte)status;
                        person.JobID = jobId;
                        person.ContactStatusID = 1;
                        person.ContactTypeID = 1;
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        person.LeadAdapterSubmittedData = js.Serialize(oldNewValues);
                        person.LeadAdapterRowData = contact;
                        personCustomFieldData.AddRange(contactCustomData);
                        personPhoneData.AddRange(contactPhoneData);

                        person.ValidEmail = ValidateEmail(person);

                        RawContact duplicate_data = null;
                        if (!String.IsNullOrEmpty(Convert.ToString(person.PrimaryEmail)))
                            duplicate_data = persons.Where(p => string.Compare(p.PrimaryEmail, Convert.ToString(person.PrimaryEmail), true) == 0).FirstOrDefault();
                        else
                            duplicate_data = persons.Where(p => string.Compare(p.FirstName, Convert.ToString(person.FirstName), true) == 0 && string.Compare(p.LastName, Convert.ToString(person.LastName), true) == 0).FirstOrDefault();

                        if (duplicate_data != null)
                        {
                            person.IsDuplicate = true;
                            //RawContact updatedperson = MergeDuplicateData(duplicate_data, person, guid);
                            //duplicate_data = updatedperson;
                        }
                        
                        persons.Add(person);
                    }

                }
            }
            catch (WebException ex)
            {              

                Logger.Current.Error("An error occurred in idx get data method(web exception): " + ex);
                Logger.Current.Error("The invalid xml file content: " + soapResult);

            }
            catch (XmlException ex)
            {              
                Logger.Current.Error("An error occurred in idx get data method(web exception): " + ex);
                Logger.Current.Error("The invalid xml file content: " + soapResult);               
            }
            catch (Exception ex)
            {

                Logger.Current.Error("An error occurred in idx get data method: " + ex);              
            }
            data.ContactPhoneData = personPhoneData;
            data.ContactCustomData = personCustomFieldData;
            data.ContactData = persons;
            return data;
        }
    }
}
