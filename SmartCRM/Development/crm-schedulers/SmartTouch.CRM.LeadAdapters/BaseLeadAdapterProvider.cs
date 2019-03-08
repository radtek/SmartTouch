using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using LandmarkIT.Enterprise.Utilities.Excel;
using LandmarkIT.Enterprise.Utilities.Common;
using System.Text;

namespace SmartTouch.CRM.LeadAdapters
{
    public abstract class BaseLeadAdapterProvider : ILeadAdapterProvider
    {
        IMailGunService mailGunService;
        IContactService contactService;
        ILeadAdaptersRepository repository;
        IImportDataRepository importDataRepository;
        IServiceProviderRepository serviceProviderRepository;
        ICustomFieldService customFieldService;
        ICachingService cacheService;
        readonly IUnitOfWork unitOfWork;
        protected LeadAdapterAndAccountMap leadAdapterAndAccountMap = default(LeadAdapterAndAccountMap);
        protected int LeadAdapterAccountMapID = default(int);
        BlockingCollection<string> newFiles = new BlockingCollection<string>(new ConcurrentQueue<string>());
        IList<string> filesUnderProcess = new List<string>();
        protected string supportEmailId = ConfigurationManager.AppSettings["SupportEmailId"];
        protected int AccountID;
        LeadAdapterTypes leadAdapterType;
        private Dictionary<string, string> _fieldMappings = new Dictionary<string, string>();

        #region ContactData
        private IList<string> hashes = new List<string>();
        private List<RawContact> persons = new List<RawContact>();
        
        private List<ImportCustomData> personCustomFieldData = new List<ImportCustomData>();
        private List<ImportPhoneData> personPhoneData = new List<ImportPhoneData>();
        #endregion

        public BaseLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, LeadAdapterTypes leadAdapterType
            , ILeadAdaptersRepository leadAdaptersRepository, IImportDataRepository importDataRepository, ISearchService<Contact> searchService,
            IUnitOfWork unitOfWork, ICustomFieldService customFieldService, ICachingService cacheService, IServiceProviderRepository serviceProviderRepository, IMailGunService mailGunService,
            IContactService contactService)
        {
            this.AccountID = accountId;
            this.repository = leadAdaptersRepository;
            this.serviceProviderRepository = serviceProviderRepository;
            this.importDataRepository = importDataRepository;
            this.customFieldService = customFieldService;
            this.unitOfWork = unitOfWork;
            this.mailGunService = mailGunService;
            this.contactService = contactService;
            this.leadAdapterType = leadAdapterType;
            this.cacheService = cacheService;
            LeadAdapterAccountMapID = leadAdapterAndAccountMapID;
            leadAdapterAndAccountMap = repository.GetLeadAdapterByID(LeadAdapterAccountMapID);
            _fieldMappings = GetFieldMappings();
        }



        /*In this method we will get all the files for the lead adapter and we will download the file to our machine
          in to the local folder of the lead adapter and we will get all the data that is embeded in the file 
         * and we will validate each record and also checking the duplicate status and adding the particular recrod 
         * and finally we will upload all the contacts and the joblogs and then we will move the file from local folder
         * to archive folder and we will delete the file from the ftp location.
         */
        public virtual void Process()
        {
            try
            {
                
                leadAdapterAndAccountMap = repository.GetLeadAdapterByID(LeadAdapterAccountMapID);
                ServiceProvider ServiceProviders = serviceProviderRepository
                  .GetServiceProviders(1, CommunicationType.Mail, MailType.TransactionalEmail);

                #region Read FTP
                var ftpManager = new FtpService();
                ftpManager.OnServiceFailure += ftpManager_OnServiceFailure;
                var filesList = ftpManager.GetFiles(leadAdapterAndAccountMap.RequestGuid, string.Empty, ServiceProviders.LoginToken,
                            leadAdapterAndAccountMap.LeadAdapterTypeID.ToString(), leadAdapterAndAccountMap.AccountName);

                foreach (var fileName in filesList)
                {
                    /*
                     * Read XML file from local path for Builder Number
                     * Get accounts related to builder number
                     * Copy file to all those accounts
                     * **/
                    try
                    {
                        var localFilePath = Path.Combine(leadAdapterAndAccountMap.LocalFilePath, fileName);
                        ftpManager.Download(leadAdapterAndAccountMap.RequestGuid, string.Empty, fileName, localFilePath);
                        List<LeadAdapterAndAccountMap> leadData = new List<LeadAdapterAndAccountMap>();

                        leadData.Add(leadAdapterAndAccountMap);

                        var nodes = GetNodes(localFilePath);
                        foreach (var node in nodes)
                        {
                            var attributes = node.GetAllAttributesAndElements();
                            var builderNumber = attributes[_fieldMappings.GetOrDefault("BuilderNumber")].Value;
                            IEnumerable<Guid> matchedGuids = ftpManager.FindMatchGuids(leadAdapterAndAccountMap.RequestGuid);
                            leadData.AddRange(repository.GetEmptyCommunities(builderNumber, leadAdapterAndAccountMap.LeadAdapterTypeID, matchedGuids));
                        }
                        if (File.Exists(localFilePath))
                            File.Delete(localFilePath);

                        foreach (LeadAdapterAndAccountMap leadAdapter in leadData.GroupBy(g => g.Id).Select(s => s.First()))
                        {
                            Logger.Current.Informational("Inserting joblog for account id : " + leadAdapter.AccountID);
                            var jobLog = new LeadAdapterJobLogs();
                            jobLog.LeadAdapterAndAccountMapID = leadAdapter.Id;
                            jobLog.LeadAdapterJobStatusID = LeadAdapterJobStatus.Undefined;
                            jobLog.Remarks = string.Empty;
                            jobLog.FileName = fileName;
                            List<LeadAdapterJobLogDetails> details = new List<LeadAdapterJobLogDetails>();
                            jobLog.LeadAdapterJobLogDetails = details;
                            var leadAdapterJobLogID = importDataRepository.InsertLeadAdapterjob(jobLog, new Guid(), false, false, leadAdapter.AccountID, leadAdapter.CreatedBy, 1, leadAdapter.CreatedBy,true,0);

                            var filePath = Path.Combine(leadAdapter.LocalFilePath, fileName);
                            var fileName_rename = Path.GetFileNameWithoutExtension(filePath) + "~" + leadAdapterJobLogID + Path.GetExtension(filePath);
                            localFilePath = Path.Combine(leadAdapter.LocalFilePath, fileName_rename);

                            ftpManager.Download(leadAdapter.RequestGuid, string.Empty, fileName, localFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("An error occured while accessing file downloading it to matched accounts, filename : " + fileName, ex);
                        continue;
                    }
                };
                #endregion

                #region Read Local Files
                readLocalFiles(ftpManager);
                #endregion
            }
            catch (IndexOutOfRangeException indexEx)
            {
                Logger.Current.Error("Index out of range exception, account name : " + leadAdapterAndAccountMap.AccountName, indexEx);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured when uploading the contacts in the lead adapter, account name : " + leadAdapterAndAccountMap.AccountName, ex);
                SendEmail("Exception details" + ex, "Exception while processing data from " + leadAdapterAndAccountMap.LeadAdapterTypeID.ToString() + " LeadAdapter");
            }
            finally
            {
                repository.UpdateProcessedDate(LeadAdapterAccountMapID);
            }
        }

        private void readLocalFiles(FtpService ftpManager)
        {
            DirectoryInfo d = new DirectoryInfo(leadAdapterAndAccountMap.LocalFilePath);
            FileInfo[] Files = d.GetFiles("*.xml");

            if (Files != null && Files.Count() > 0)
            {
                var response = customFieldService.GetAllCustomFields(new GetAllCustomFieldsRequest(leadAdapterAndAccountMap.AccountID));
                IEnumerable<FieldViewModel> customFields = response.CustomFields;
                var dropdownfeildsresposne = cacheService.GetDropdownValues(leadAdapterAndAccountMap.AccountID);

                IEnumerable<DropdownValueViewModel> phoneFields = dropdownfeildsresposne.Where(x => x.DropdownID == (short)DropdownFieldTypes.PhoneNumberType).Select(x => x.DropdownValuesList).FirstOrDefault();

                Logger.Current.Informational("Processing files for account : " + leadAdapterAndAccountMap.AccountName + " count of files : " + Files.LongLength);
                foreach (var file in Files)
                {
                    Logger.Current.Informational("Current Processed File Name: " + file.Name);
                    var localFilePath = Path.Combine(leadAdapterAndAccountMap.LocalFilePath, file.Name);

                    try
                    {
                        int jobid = Convert.ToInt32(Path.GetFileNameWithoutExtension(localFilePath).Split('~')[1]);
                        var fileName = Path.GetFileNameWithoutExtension(localFilePath).Split('~')[0] + Path.GetExtension(localFilePath);

                        /* Get contacts */
                        var contacts = new ImportContactsData();
                        contacts = GetContacts(localFilePath, customFields, jobid, phoneFields);
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
                        /* File move to archive after processing */

                        var fileExtension = Path.GetExtension(localFilePath);
                        var newFileName = Guid.NewGuid() + fileExtension;
                        importDataRepository.UpdateLeadAdapterJobLogsWithProcessedFileName(jobid, newFileName);
                        string archivedfile = string.Empty;
                        if (Directory.Exists(leadAdapterAndAccountMap.ArchivePath) && System.IO.File.Exists(localFilePath))
                        {
                            archivedfile = Path.Combine(leadAdapterAndAccountMap.ArchivePath, newFileName);
                            File.Move(localFilePath, archivedfile);
                        }
                        else if (File.Exists(localFilePath))
                        {
                            File.Delete(localFilePath);
                        }

                        bool isFolderExist = ftpManager.CreateFTPDirectory(leadAdapterAndAccountMap.RequestGuid);
                        if (isFolderExist)
                            ftpManager.MoveFile(leadAdapterAndAccountMap.RequestGuid, fileName, archivedfile, jobid);
                        else
                            ftpManager.Delete(leadAdapterAndAccountMap.RequestGuid, string.Empty, new List<string> { fileName });
                    }
                    catch (IndexOutOfRangeException indexExc)
                    {
                        Logger.Current.Error("Index out of range exception while accessing jobid and filename, account name : " + leadAdapterAndAccountMap.AccountName, indexExc);
                        var fileExtension = Path.GetExtension(localFilePath);
                        var newFileName = Guid.NewGuid() + fileExtension;

                        string archivedfile = string.Empty;
                        archivedfile = Path.Combine(leadAdapterAndAccountMap.ArchivePath, newFileName);
                        File.Move(localFilePath, archivedfile);           //These are old files
                        continue;
                    }
                }
                Logger.Current.Informational("Files processed succcessfully for account : " + leadAdapterAndAccountMap.AccountName + " count of files : " + Files.LongLength);
            }
        }

        /// <summary>
        /// Saving bulk contacts from imported file to temp table
        /// </summary>
        public void ContactsBulkinsert(ImportContactsData contacts)
        {
            Logger.Current.Informational("Request received for contacts bulk insert");
            importDataRepository.InsertImportsData(contacts);
            //  tagService.addLeadAdapterToTopic(leadAdapterAndAccountMap.Id, contacts, leadAdapterAndAccountMap.AccountID);
        }

        /// <summary>
        /// Mailgun verification for bulk contacts
        /// </summary>
        public void MailgunVerification(List<RawContact> contacts)
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

        public bool IsValidPhoneNumberLength(string phoneNumber)
        {
            string pattern = @"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(phoneNumber);
        }

        public bool IsValidURL(string URL)
        {
            string pattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";
            return Regex.IsMatch(URL, pattern);
        }

        private void ftpManager_OnServiceFailure(object sender, ServiceEventArgs eventArgs)
        {
            try
            {
                leadAdapterAndAccountMap.LeadAdapterErrorStatusID = LeadAdapterErrorStatus.Error;
                leadAdapterAndAccountMap.LeadAdapterServiceStatusID = (LeadAdapterServiceStatus)eventArgs.ServiceStatus;
                leadAdapterAndAccountMap.LastProcessed = DateTime.Now.ToUniversalTime();
                repository.Update(leadAdapterAndAccountMap);
                unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured in ftpManager on service failure method:  ", ex);
            }
        }

        private IEnumerable<Node> GetNodes(string fileName)
        {
            var xml = new ReadXML(fileName).Read();
            return xml[GetRootNode()];
        }

        public virtual ImportContactsData GetContacts(string fileName, IEnumerable<FieldViewModel> customFields, int jobId, IEnumerable<DropdownValueViewModel> dropdownFields)
        {
            ImportContactsData data = new ImportContactsData();
            customFields = customFields.Where(i => i.IsLeadAdapterField && i.LeadAdapterType == (byte)leadAdapterType && i.StatusId == FieldStatus.Active);
            var nodes = GetNodes(fileName);
            persons = new List<RawContact>();
            personPhoneData = new List<ImportPhoneData>();
            personCustomFieldData = new List<ImportCustomData>();
            try
            {
                foreach (var node in nodes)
                {
                    NodeAttributes attributes = node.GetAllAttributesAndElements();
                    processContacts(node, attributes, customFields, jobId, dropdownFields, fileName);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("File", fileName);
                ex.Data.Add("Job", jobId);
                Logger.Current.Error("Error while reading contacts in Lead Adapter..", ex);
            }
            data.ContactCustomData = personCustomFieldData;
            data.ContactPhoneData = personPhoneData;
            data.ContactData = persons;
            return data;
        }

        public virtual void Initialize()
        {
            Task.Factory.StartNew(ProcessFiles);
            this.ProcessFiles();
        }

        protected virtual void ProcessFiles()
        {
            //for future use
        }

        public virtual bool CheckIfStandardField(NodeAttribute attribute, string field)
        {
            return (attribute.Name.ToLower() == _fieldMappings.GetOrDefault(field).NullSafeToLower());
        }

        private static string GetNonNumericData(string input)
        {
            Regex regex = new Regex("[^a-zA-Z0-9]");
            string output = regex.Replace(input, "");
            if (output.Length > 20)
                return "";
            return regex.Replace(input, "");
        }
        private void processContacts(Node node, NodeAttributes attributes,
            IEnumerable<FieldViewModel> customFields, int jobId,
            IEnumerable<DropdownValueViewModel> dropdownFields, string fileName)
        {
            #region Declarations
            StringBuilder hash = new StringBuilder();
            IList<Email> emails = new List<Email>();
            var guid = Guid.NewGuid();
            RawContact contact = new RawContact();
            IList<ImportCustomData> contactCustomData = new List<ImportCustomData>();
            IList<ImportPhoneData> contactPhoneData = new List<ImportPhoneData>();
            StringBuilder customFieldData = new StringBuilder();
            bool isDuplicateFromFile = false;
            LeadAdapterRecordStatus status = LeadAdapterRecordStatus.Undefined;
            SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
            Dictionary<string, dynamic> oldNewValues = new Dictionary<string, dynamic> { };
            #endregion

            #region BuilderNumber Validation
            string builderNumber = attributes[_fieldMappings.GetOrDefault("BuilderNumber")].Value;
            bool builderNumberPass = leadAdapterAndAccountMap.BuilderNumber.ToLower().Split(',').Contains(builderNumber.ToLower());
            #endregion

            #region Community Number Validation
            string communityNumber = attributes[_fieldMappings.GetOrDefault("CommunityNumber")].Value;
            bool communityNumberPass = true;
            #endregion

            #region ContactsProcessing
            string firstName = attributes[_fieldMappings.GetOrDefault("FirstName")].Value;
            string lastName = attributes[_fieldMappings.GetOrDefault("LastName")].Value;
            string primaryEmail = attributes[_fieldMappings.GetOrDefault("Email")].Value;
            string companyName = string.Empty;

            #region HashPreparation
            Action<string> HashAppend = (n) =>
            {
                if (string.IsNullOrEmpty(n))
                    hash.Append("-").Append(string.Empty);
                else
                    hash.Append("-").Append(n);
            };

            if (!string.IsNullOrEmpty(primaryEmail))
            {

                Email _primaryemail = new Email()
                {
                    EmailId = primaryEmail,
                    AccountID = leadAdapterAndAccountMap.AccountID,
                    IsPrimary = true
                };
                emails.Add(_primaryemail);
                hash.Append("-").Append(primaryEmail);
            }
            else
            {
                hash.Append("-").Append("na@na.com");
            }

            HashAppend(firstName);
            HashAppend(lastName);
            HashAppend(companyName);
            #endregion

            Person person = new Person()
            {
                FirstName = firstName,
                LastName = lastName,
                CompanyName = companyName,
                Emails = emails,
                AccountID = AccountID
            };


            if (builderNumberPass && communityNumberPass)
            {
                bool duplicatEemailCount = hashes.Any(h => !string.IsNullOrEmpty(primaryEmail) && h.Contains(primaryEmail));
                if (duplicatEemailCount ||
                    (string.IsNullOrEmpty(primaryEmail) && hashes.Where(h => h.Contains(hash.ToString())).Any()))
                    isDuplicateFromFile = true;
                else if (!duplicatEemailCount)
                    isDuplicateFromFile = false;
            }

            hashes.Add(hash.ToString());


            if (builderNumberPass && communityNumberPass)
            {
                SearchParameters parameters = new SearchParameters() { AccountId = AccountID };
                IEnumerable<Contact> duplicateContacts = contactService.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person }).Contacts;
                duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };
            }

            if (!builderNumberPass)
                status = LeadAdapterRecordStatus.BuilderNumberFailed;
            else if (!communityNumberPass)
                status = LeadAdapterRecordStatus.CommunityNumberFailed;
            else if (isDuplicateFromFile)
                status = LeadAdapterRecordStatus.DuplicateFromFile;
            else if (duplicateResult.TotalHits > 0)
            {
                status = LeadAdapterRecordStatus.Updated;
                guid = duplicateResult.Results.FirstOrDefault().ReferenceId;
            }
            else
                status = LeadAdapterRecordStatus.Added;


            Contact duplicatePerson = default(Person);

            if (status == LeadAdapterRecordStatus.Updated)
                duplicatePerson = duplicateResult.Results.FirstOrDefault();
            else
                duplicatePerson = new Person();

            Func<NodeAttribute, string, bool> checkIfStandardField = (name, field) =>
             {
                 return (name.Name.ToLower() == _fieldMappings.GetOrDefault(field).NullSafeToLower());
             };

            try
            {
                foreach (var attribute in attributes)
                {
                    var name = attribute.Name.ToLower();
                    var value = attribute.Value;

                    ImportCustomData customData = new ImportCustomData();
                    try
                    {
                        var elementValue = string.Empty;
                        if (checkIfStandardField(attribute, "FirstName"))
                        {
                            elementValue = ((Person)duplicatePerson).FirstName == null ? string.Empty : ((Person)duplicatePerson).FirstName;
                            contact.FirstName = value;
                        }
                        else if (checkIfStandardField(attribute, "LastName"))
                        {
                            elementValue = ((Person)duplicatePerson).LastName;
                            contact.LastName = value;
                        }
                        else if (checkIfStandardField(attribute, "Email"))
                        {
                            Email primaryemail = duplicatePerson.Emails.IsAny() ? duplicatePerson.Emails.Where(i => i.IsPrimary == true).FirstOrDefault() : null;
                            if (primaryemail != null)
                                elementValue = primaryemail.EmailId;
                            contact.PrimaryEmail = value;
                        }
                        else if (checkIfStandardField(attribute, "Company"))
                        {
                            // get company dynamic
                            elementValue = duplicatePerson.CompanyName;
                            contact.CompanyName = value;
                        }
                        else if (checkIfStandardField(attribute, "PhoneNumber") || checkIfStandardField(attribute, "Phone"))
                        {
                            DropdownValueViewModel dropdownValue = dropdownFields.Where(i => i.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
                            var mobilephone = default(Phone);
                            ImportPhoneData phoneData = new ImportPhoneData();
                            phoneData.ReferenceID = guid;
                            if (dropdownValue != null)
                            {
                                if (!string.IsNullOrEmpty(value))
                                {
                                    string phoneNumber = GetNonNumericData(value);
                                    if (IsValidPhoneNumberLength(phoneNumber))
                                    {
                                        contact.PhoneData = dropdownValue.DropdownValueID.ToString() + "|" + phoneNumber;
                                        phoneData.PhoneType = (int?)dropdownValue.DropdownValueID;
                                        phoneData.PhoneNumber = phoneNumber;
                                        contactPhoneData.Add(phoneData);
                                    }
                                }
                                mobilephone = duplicatePerson.Phones.IsAny() ? duplicatePerson.Phones.Where(i => i.PhoneType == dropdownValue.DropdownValueID).FirstOrDefault() : null;
                            }

                            if (mobilephone != null)
                            {
                                elementValue = mobilephone.Number;
                            }
                        }
                        else if (checkIfStandardField(attribute, "Country"))
                        {
                            var countryvalue = value.Replace(" ", string.Empty).ToLower();
                            if (countryvalue == "usa" || countryvalue == "us" || countryvalue == "unitedstates" || countryvalue == "unitedstatesofamerica")
                                contact.Country = "US";
                            else if (countryvalue == "ca" || countryvalue == "canada")
                                contact.Country = "CA";
                            else
                                contact.Country = value;
                        }
                        else if (checkIfStandardField(attribute, "StreetAddress"))
                        {
                            contact.AddressLine1 = value;
                        }
                        else if (checkIfStandardField(attribute, "City"))
                        {
                            contact.City = value;
                        }
                        else if (checkIfStandardField(attribute, "State"))
                        {
                            contact.State = value;
                        }
                        else if (checkIfStandardField(attribute, "PostalCode"))
                        {
                            contact.ZipCode = value;
                        }
                        else
                        {
                            var customField = customFields.Where(i => i.Title.Replace(" ", string.Empty).ToLower() == (name + "(" + leadAdapterType.ToString().ToLower() + ")")).FirstOrDefault();
                            if (customField != null)
                            {
                                var customfielddata = duplicatePerson.CustomFields == null ? null : duplicatePerson.CustomFields.Where(i => i.CustomFieldId == customField.FieldId).FirstOrDefault();
                                if (customfielddata != null)
                                    elementValue = customfielddata.Value;
                                customData.FieldID = customField.FieldId;
                                customData.FieldTypeID = (int?)customField.FieldInputTypeId;
                                customData.ReferenceID = guid;

                                if (customField.FieldInputTypeId == FieldType.date || customField.FieldInputTypeId == FieldType.datetime || customField.FieldInputTypeId == FieldType.time)
                                {

                                    DateTime converteddate;
                                    if (DateTime.TryParse(value, out converteddate))
                                    {
                                        customFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + converteddate.ToString("MM/dd/yyyy hh:mm tt"));
                                        customData.FieldValue = converteddate.ToString("MM/dd/yyyy hh:mm tt");
                                    }
                                }
                                else if (customField.FieldInputTypeId == FieldType.number)
                                {
                                    double number;
                                    if (double.TryParse(value, out number))
                                    {
                                        customFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + number.ToString());
                                        customData.FieldValue = number.ToString();
                                    }
                                }
                                else if (customField.FieldInputTypeId == FieldType.url)
                                {
                                    if (IsValidURL(value.Trim()))
                                    {
                                        customFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + value.Trim());
                                        customData.FieldValue = value.Trim();
                                    }
                                }
                                else
                                {
                                    customFieldData.Append("~" + customField.FieldId + "##$##" + (byte)customField.FieldInputTypeId + "|" + value.Trim());
                                    customData.FieldValue = value.Trim();
                                }
                                contactCustomData.Add(customData);
                            }
                        }
                        if (!oldNewValues.ContainsKey(attribute.Name))
                            oldNewValues.Add(attribute.Name, new { OldValue = string.IsNullOrEmpty(elementValue) ? string.Empty : elementValue, NewValue = value });
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("An exception occured in Genereating old new values element in Trulia : " + attribute.Name, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured in Genereating old new values element in Trulia : ", ex);
            }
            #endregion

            if (customFieldData.Length > 0)
                customFieldData.Remove(0, 1);
            contact.CustomFieldsData = customFieldData.ToString();

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
            contact.LeadAdapterRowData = node.Current != null ? node.Current.ToString() : string.Empty;
            personCustomFieldData.AddRange(contactCustomData);
            personPhoneData.AddRange(contactPhoneData);

            contact.ValidEmail = ValidateEmail(contact);

            RawContact duplicate_data = null;
            if (!string.IsNullOrEmpty(contact.PrimaryEmail))
                duplicate_data = persons.Where(p => string.Compare(p.PrimaryEmail, contact.PrimaryEmail, true) == 0).FirstOrDefault();
            else
                duplicate_data = persons.Where(p => string.Compare(p.FirstName, contact.FirstName, true) == 0 &&
                string.Compare(p.LastName, contact.LastName, true) == 0).FirstOrDefault();

            if (duplicate_data != null)
            {
                contact.IsDuplicate = true;
                //RawContact updatedperson = MergeDuplicateData(duplicate_data, contact, guid);
                //duplicate_data = updatedperson;
            }

            persons.Add(contact);
        }

        private bool ValidateEmail(RawContact c)
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
        }

        public virtual Dictionary<string, string> GetFieldMappings()
        {
            var mappings = new Dictionary<string, string>();
            mappings.Add("BuilderNumber", "BuilderNumber");
            mappings.Add("BuilderName", "BuilderName");
            mappings.Add("CommunityNumber", "CommunityNumber");
            mappings.Add("CommunityName", "CommunityName");
            mappings.Add("StateName", "StateName");
            mappings.Add("MarketName", "MarketName");
            mappings.Add("Comments", "Comments");
            mappings.Add("FirstName", "FirstName");
            mappings.Add("LastName", "LastName");
            mappings.Add("PostalCode", "PostalCode");
            mappings.Add("Email", "Email");
            mappings.Add("Country", "Country");
            mappings.Add("LeadType", "LeadType");
            mappings.Add("Source", "Source");
            return mappings;
        }

        public virtual string GetRootNode()
        {
            return "Lead";
        }

        public void SendEmail(string bodyMessage, string subject)
        {
            try
            {
                ServiceProvider ServiceProviders = serviceProviderRepository
                   .GetServiceProviders(1, CommunicationType.Mail, MailType.TransactionalEmail);

                LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest sendMailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();

                Logger.Current.Verbose("Account Id in LeadAdapter:" + leadAdapterAndAccountMap.AccountID);
                Logger.Current.Verbose("Email Guid in LeadAdapter :" + ServiceProviders.LoginToken);

                string _supportEmailId = ConfigurationManager.AppSettings["SupportEmailId"];
                string machine = ConfigurationManager.AppSettings["MachineName"];
                string subjct = leadAdapterAndAccountMap.AccountName + " - " + subject;
                if (machine != "" && machine != null)
                    subjct = machine + " : " + subjct;

                var body = " Error Message     : " + subject + ".\r\n Account Name     : " + leadAdapterAndAccountMap.AccountName + ".\r\n LeadAdapter        :  " + leadAdapterAndAccountMap.LeadAdapterTypeID.ToString() + "  .\r\n Instance occured on  : " + DateTime.UtcNow + " (UTC).\r\n More Info            : " + bodyMessage;

                Logger.Current.Verbose("Sending Email in LeadAdapter Engine :" + _supportEmailId);
                _supportEmailId = _supportEmailId == null ? "smartcrm3@gmail.com" : _supportEmailId;
                List<string> To = new List<string>();
                To.Add(_supportEmailId);
                EmailAgent agent = new EmailAgent();
                sendMailRequest.TokenGuid = ServiceProviders.LoginToken;
                sendMailRequest.From = _supportEmailId;
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
                Logger.Current.Verbose("Sending Email in LeadAdapter Engine :" + _supportEmailId);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured while sending email: ", ex);
            }
        }
    }
}
