using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using LandmarkIT.Enterprise.Utilities.Excel;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Communication;
using System.Diagnostics;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using System.Web.Script.Serialization;
using System.Collections;
using SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList;
using System.Configuration;
using System.Reflection;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;

namespace SmartTouch.CRM.LeadAdapters.Providers
{
    public class ExcelLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IMailGunService mailGunService;
        ILeadAdaptersRepository repository;
        IImportDataRepository importDataRepository;
        IIndexingService indexingService;
        ISearchService<Contact> searchService;
        ICustomFieldService customFieldService;
        ISuppressionListService suppressionListService;
        IDropdownValuesService dropdownService;
        List<string> newFiles = new List<string>();
        List<RawContactViewModel> rawContacts = new List<RawContactViewModel>();
        List<ImportCustomData> contactCustomData = new List<ImportCustomData>();
        List<ImportPhoneData> contactPhoneData = new List<ImportPhoneData>();
        List<string> filesUnderProcess = new List<string>();
        int accountId, jobId;

        public ExcelLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
             IImportDataRepository importDataRepository, ISearchService<Contact> searchService, IUnitOfWork unitOfWork,
            ICustomFieldService customFieldService, ISuppressionListService suppressionListService, ICachingService cacheService, ICommunicationService communicationService, IMailGunService mailGunService,
            IContactService contactService, IDropdownValuesService dropdownService)
            : base(accountId, leadAdapterAndAccountMapID, LeadAdapterTypes.Import, leadAdaptersRepository, importDataRepository, searchService, unitOfWork,
            customFieldService, cacheService, serviceProviderRepository, mailGunService, contactService)
        {
            this.repository = leadAdaptersRepository;
            this.importDataRepository = importDataRepository;
            this.customFieldService = customFieldService;
            this.suppressionListService = suppressionListService;
            this.indexingService = new IndexingService();
            this.searchService = searchService;
            this.mailGunService = mailGunService;
            this.dropdownService = dropdownService;
            leadAdapterAndAccountMap = repository.GetLeadAdapterByID(leadAdapterAndAccountMapID);
            this.accountId = accountId;
        }

        public override void Initialize()
        {
            //Task.Factory.StartNew(ProcessFiles);
            this.ProcessFiles();
        }

        /// <summary>
        /// processing the Imported files
        /// </summary>
        protected override void ProcessFiles()
        {
            Logger.Current.Informational("file path: " + leadAdapterAndAccountMap.LocalFilePath);

            try
            {
                if (!string.IsNullOrEmpty(leadAdapterAndAccountMap.LocalFilePath) && Directory.Exists(leadAdapterAndAccountMap.LocalFilePath))
                {
                    var filesList = Directory.GetFiles(leadAdapterAndAccountMap.LocalFilePath);

                    foreach (var fileName in filesList)
                    {
                        Logger.Current.Informational("Reading data from file and preparing contacts " + fileName);
                        var sw = new Stopwatch();
                        sw.Start();
                        readDataFromFile(fileName);
                        sw.Stop();
                        Logger.Current.Informational(string.Format("Time taken to read file {0} {1}", fileName, sw.Elapsed));
                        if (!filesUnderProcess.Contains(fileName))
                            newFiles.Add(fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while reading data from file/while preparing contacts from data in the file ", ex);
            }

            foreach (string fileName in newFiles)
            {
                try
                {
                    Logger.Current.Informational("Entered into the processing files " + fileName);
                    if (!System.IO.File.Exists(fileName))
                    {
                        filesUnderProcess.Remove(fileName);
                        continue;
                    }
                    string filenamewithoutextension = Path.GetFileNameWithoutExtension(fileName);

                    ImportDataSettings importdatasettings = importDataRepository.getImportdataSetting(filenamewithoutextension);
                    if (importdatasettings == null)
                        continue;
                    bool UpdateOnDuplicate = default(bool);
                    DuplicateLogic duplicateLogic = default(DuplicateLogic);

                    if (importdatasettings != null)
                    {
                        duplicateLogic = (DuplicateLogic)importdatasettings.DuplicateLogic;
                        UpdateOnDuplicate = importdatasettings.UpdateOnDuplicate;
                    }

                    importDataRepository.UpdateLeadAdapterJobLogsWithProcessedFileName(jobId, Path.GetFileName(fileName));
                    var contacts = this.GetContactData(fileName, duplicateLogic, UpdateOnDuplicate);
                    //this.MailgunVerification(contacts.ToList());
                    this.SuppressedEmailCheck(contacts.ToList());
                    this.ContactsBulkinsert(contacts.ToList());
                }
                catch (Exception ex)
                {
                    importDataRepository.UpdateLeadAdapterStatus(jobId, LeadAdapterJobStatus.Failed);
                    Logger.Current.Error("An error occured in ExcelLeadAdapterProvider repeating for looop ", ex);
                }
                finally
                {
                    if (Directory.Exists(leadAdapterAndAccountMap.ArchivePath))
                        File.Move(fileName, Path.Combine(leadAdapterAndAccountMap.ArchivePath, Path.GetFileName(fileName)));
                    else if (System.IO.File.Exists(fileName))
                        File.Delete(fileName);

                    filesUnderProcess.Remove(fileName);
                }
            }
        }

        private void SuppressedEmailCheck(List<RawContact> contacts)
        {
            try
            {
                Logger.Current.Informational("Suppression Email Verification Started: " + DateTime.UtcNow);
                CheckSuppressionEmailsResponse suppressedEmailCheckResponse = suppressionListService.CheckSuppressionEmails(new CheckSuppressionEmailsRequest { Emails = contacts.Where(w => w.PrimaryEmail != null).Select(c => c.PrimaryEmail),
                    AccountId = accountId });
                IEnumerable<string> suppressedEmails = suppressedEmailCheckResponse.SuppressedEmails.Select(e => e.ToString()).ToList();
                if (suppressedEmails != null)
                {
                    contacts.ForEach(a =>
                    {
                        if (suppressedEmails.Contains(a.PrimaryEmail))
                        {
                            a.EmailStatus = (byte)EmailStatus.Suppressed;
                        }
                        else
                        {
                            a.EmailStatus = (byte)EmailStatus.NotVerified;
                        }
                    });
                }

                Logger.Current.Informational("Suppression Email Verification Ended: " + DateTime.UtcNow);

            }
            catch (Exception ex)
            {
                Logger.Current.Error("error while checking for suppression list in account " + accountId, ex);
            }
        }

        private void readDataFromFile(string fileName)
        {
            try
            {
                var pathToSaveFile = Path.Combine(ConfigurationManager.AppSettings["LEADADAPTER_PHYSICAL_PATH"].ToString(), accountId.ToString());
                var destinationPath = Path.Combine(pathToSaveFile, fileName);

                var data = new DataSet();
                ReadCSV readCSV = new ReadCSV();
                ReadExcel readExcel = new ReadExcel();
                var jobLogDetails = new List<LeadAdapterJobLogDetails>();
                Guid uniqueidentifier = Guid.NewGuid();
                rawContacts = new List<RawContactViewModel>();
                var settings = importDataRepository.getImportdataSetting(Path.GetFileNameWithoutExtension(fileName));
                bool UpdateOnDuplicate = settings.UpdateOnDuplicate;// request.ImportDataListViewModel.UpdateOnDuplicate;
                byte duplicateLogic = settings.DuplicateLogic;// request.ImportDataListViewModel.DuplicateLogic;

                var fileInfo = new FileInfo(destinationPath);
                if (fileInfo.Extension == ".csv")
                    data = readCSV.GetDataSetFromCSVfileUsingCSVHelper(destinationPath);
                else
                    data = readExcel.ToDataSet(destinationPath, true);
                //var jobLog = new LeadAdapterJobLogs();
                var customFieldsViewModel = customFieldService.GetAllCustomFields(new ApplicationServices.Messaging.CustomFields.GetAllCustomFieldsRequest(accountId));
                var fields = customFieldsViewModel.CustomFields;
                jobId = settings.LeadAdaperJobID.Value;
                int importedBy = repository.ImpotedLeadByJobId(jobId);
                var columnMappings = importDataRepository.GetColumnMappings(jobId);
                var importcolumns = mapColumns(columnMappings);

                var mappedcustomfields = fields.Join(importcolumns, i => i.FieldId.ToString(), j => j.ContactFieldName, (i, j) => new ImportDataViewModel
                {
                    SheetColumnName = j.SheetColumnName,
                    PreviewData = i.Title,
                    FieldType = i.FieldInputTypeId,
                    FieldID = i.FieldId
                }).ToList();

                IEnumerable<DropdownValueViewModel> phonefields = dropdownService.GetDropdownValue(new ApplicationServices.Messaging.DropdownValues.GetDropdownValueRequest()
                {
                    AccountId = accountId,
                    DropdownID = 1
                }).DropdownValues.DropdownValuesList;

                var mappedphonefields = phonefields.Join(importcolumns.Where(i => i.IsDropDownField == true),
                   i => i.DropdownValueID.ToString(), j => j.ContactFieldName, (i, j) => new ImportDataViewModel
                   {
                       SheetColumnName = j.SheetColumnName,
                       PreviewData = i.DropdownValueID.ToString(),
                       ContactFieldName = j.ContactFieldName
                   }).ToList();
                var mappingsfromview = importcolumns.Where(i => i.IsDropDownField == false).ToList();

                var rows = data.Tables[0].Rows;
                Logger.Current.Informational("Cast to DataRow");
                var filteredRows = rows.Cast<DataRow>().Where(row => row.ItemArray.Any(field => !(field is System.DBNull)));
                Logger.Current.Informational("Preparing RawContactViewModel");
                int importid = 1;
                foreach (DataRow personRow in filteredRows)
                {

                    var guid = Guid.NewGuid();
                    //var jobLogDetail = new LeadAdapterJobLogDetails
                    //{
                    //    SubmittedData = string.Empty,
                    //    ReferenceId = guid,
                    //    LeadAdapterRecordStatusID = (int)LeadAdapterRecordStatus.Undefined,
                    //    Remarks = string.Empty,
                    //    CreatedBy = leadAdapterAndAccountMap.CreatedBy,
                    //    CreatedDateTime = DateTime.UtcNow
                    //};
                    //jobLogDetails.Add(jobLogDetail);
                    RawContactViewModel person = getPerson(personRow, mappingsfromview, accountId, UpdateOnDuplicate,
                                                           importid, guid, mappedcustomfields, mappedphonefields, jobId, importedBy);
                    rawContacts.Add(person);
                    Logger.Current.Informational("Preparing Person " + importid);
                    importid++;
                }

                //jobLog.LeadAdapterJobLogDetails = jobLogDetails;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception looging while reading data from file:" + ex);
                Logger.Current.Informational("Logging File and jobid are:" + fileName + "," + jobId);

            }

        }

        private static string GetNonNumericData(string input)
        {
            Regex regex = new Regex("[^a-zA-Z0-9]");
            string output = regex.Replace(input, "");
            if (output.Length > 20)
                return "";
            return regex.Replace(input, "");
        }

        private static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                switch (prop.PropertyType.Name)
                {
                    case "String":
                        dataTable.Columns.Add(prop.Name, typeof(string));
                        break;
                    case "Byte":
                        dataTable.Columns.Add(prop.Name, typeof(byte));
                        break;
                    case "DateTime":
                        dataTable.Columns.Add(prop.Name, typeof(DateTime));
                        break;
                    case "Int32":
                        dataTable.Columns.Add(prop.Name, typeof(Int32));
                        break;
                    default:
                        dataTable.Columns.Add(prop.Name);
                        break;
                }
            }

            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        private RawContactViewModel getPerson(DataRow personRow, IList<ImportDataViewModel> mappings, int accountID, bool IsUpdate, int importId,
                                      Guid guid, IList<ImportDataViewModel> mappedcustomfields, IList<ImportDataViewModel> mappedphonefields, int jobId, int proccessedBy)
        {
            RawContactViewModel person = new RawContactViewModel();

            person.AccountID = accountID;
            person.ImportDataId = importId;
            person.ReferenceId = guid;
            StringBuilder customField = new StringBuilder();
            StringBuilder phoneField = new StringBuilder();
            //contactCustomData = new List<ImportCustomData>();
            //contactPhoneData = new List<ImportPhoneData>();
            StringBuilder jsondata = new StringBuilder();

            //firstname
            var firstNameColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.FirstNameField)));
            string firstname = string.Empty;
            firstname = firstNameColumn != null ? personRow[firstNameColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(firstname))
            {
                person.FirstName = firstname;
                jsondata.Append(firstNameColumn.SheetColumnName + " = " + firstname + "; ");
            }

            //lastname
            var lastNameColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.LastNameField)));
            string lastname = string.Empty;
            lastname = lastNameColumn != null ? personRow[lastNameColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(lastname))
            {
                person.LastName = lastname;
                jsondata.Append(lastNameColumn.SheetColumnName + " = " + lastname + "; ");
            }

            //primaryEmail
            var primaryEmailColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.PrimaryEmail)));
            string primaryemail = string.Empty;
            primaryemail = primaryEmailColumn != null ? personRow[primaryEmailColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(primaryemail))
            {
                person.PrimaryEmail = primaryemail;
                jsondata.Append(primaryEmailColumn.SheetColumnName + " = " + primaryemail + "; ");

            }

            if (mappedphonefields.Any())
            {

                foreach (var field in mappedphonefields)
                {
                    ImportPhoneData phoneData = new ImportPhoneData();
                    phoneData.ReferenceID = guid;
                    string phonenumber = GetNonNumericData(personRow[field.SheetColumnName].ToString());
                    if (!string.IsNullOrEmpty(phonenumber) && IsValidPhoneNumberLength(phonenumber.ToString()))
                    {
                        // phoneField.Append("~" + field.PreviewData + "|" + phonenumber);
                        phoneData.PhoneType = Convert.ToInt32(field.PreviewData);
                        phoneData.PhoneNumber = phonenumber.ToString();
                        contactPhoneData.Add(phoneData);
                        jsondata.Append(field.SheetColumnName + " = " + phonenumber.ToString() + "; ");
                    }

                }

            }

            if (customField.Length > 0)
                customField = customField.Remove(0, 1);

            if (phoneField.Length > 0)
                phoneField = phoneField.Remove(0, 1);

            if (mappedcustomfields.Any())
            {

                foreach (var field in mappedcustomfields)
                {
                    ImportCustomData customData = new ImportCustomData();
                    customData.FieldID = field.FieldID;
                    customData.FieldTypeID = (int?)field.FieldType;
                    customData.ReferenceID = guid;

                    if (field.FieldType == FieldType.date || field.FieldType == FieldType.datetime)
                    {
                        double value = default(double);
                        if (double.TryParse(personRow[field.SheetColumnName].ToString(), out value))
                        {
                            DateTime conv = DateTime.FromOADate(value);
                            customField.Append("~" + field.FieldID + "##$##" + (byte)field.FieldType + "|" + conv.ToString("MM/dd/yyyy hh:mm tt"));
                            customData.FieldValue = conv.ToString("MM/dd/yyyy hh:mm tt");
                        }
                        else
                        {
                            DateTime converteddate;
                            if (DateTime.TryParse(personRow[field.SheetColumnName].ToString(), out converteddate))
                            {
                                customField.Append("~" + field.FieldID + "##$##" + (byte)field.FieldType + "|" + converteddate.ToString("MM/dd/yyyy hh:mm tt"));
                                customData.FieldValue = converteddate.ToString("MM/dd/yyyy hh:mm tt");
                            }
                        }
                    }
                    else if (field.FieldType == FieldType.time)
                    {
                        double value = default(double);
                        if (double.TryParse(personRow[field.SheetColumnName].ToString(), out value))
                        {
                            DateTime conv = DateTime.FromOADate(value);
                            customField.Append("~" + field.FieldID + "##$##" + (byte)field.FieldType + "|" + conv.ToString("hh:mm tt"));
                            customData.FieldValue = conv.ToString("hh:mm tt");
                        }
                        else
                        {
                            DateTime converteddate;
                            if (DateTime.TryParse(personRow[field.SheetColumnName].ToString(), out converteddate))
                            {
                                customField.Append("~" + field.FieldID + "##$##" + (byte)field.FieldType + "|" + converteddate.ToString("hh:mm tt"));
                                customData.FieldValue = converteddate.ToString("hh:mm tt");
                            }
                        }
                    }
                    else if (field.FieldType == FieldType.number)
                    {
                        double number;
                        if (double.TryParse(personRow[field.SheetColumnName].ToString(), out number))
                        {
                            customField.Append("~" + field.FieldID + "##$##" + (byte)field.FieldType + "|" + number.ToString());
                            customData.FieldValue = number.ToString();
                        }
                    }
                    else if (field.FieldType == FieldType.url)
                    {
                        if (IsValidURL(personRow[field.SheetColumnName].ToString().Trim()))
                        {
                            customField.Append("~" + field.FieldID + "##$##" + (byte)field.FieldType + "|" + personRow[field.SheetColumnName].ToString().Trim());
                            customData.FieldValue = personRow[field.SheetColumnName].ToString().Trim();
                        }
                    }
                    else
                    {
                        if (personRow[field.SheetColumnName].ToString() != "")
                        {
                            customField.Append("~" + field.FieldID + "##$##" + (byte)field.FieldType + "|" + personRow[field.SheetColumnName].ToString());
                            customData.FieldValue = personRow[field.SheetColumnName].ToString();
                        }
                    }
                    contactCustomData.Add(customData);
                }

            }

            //company
            var companyColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.CompanyNameField)));
            string company = string.Empty;
            company = companyColumn != null ? personRow[companyColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(company))
            {
                person.CompanyName = company;
                jsondata.Append(companyColumn.SheetColumnName + " = " + company + "; ");
            }

            //title
            var titleColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.TitleField)));
            string title = string.Empty;
            title = titleColumn != null ? personRow[titleColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(title))
            {
                person.Title = title;
                jsondata.Append(titleColumn.SheetColumnName + " = " + title + "; ");
            }

            //donotemail
            var doNotEmailColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.DonotEmail)));
            string donotemail = string.Empty;
            donotemail = doNotEmailColumn != null ? personRow[doNotEmailColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(donotemail))
            {
                if (donotemail.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    person.DoNotEmail = true;
                else
                    person.DoNotEmail = false;
                jsondata.Append(doNotEmailColumn.SheetColumnName + " = " + person.DoNotEmail + "; ");
            }

            //LifecycleStage
            var lifecycleStageColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.LifecycleStageField)));
            string lifecyclestage = string.Empty;
            lifecyclestage = lifecycleStageColumn != null ? personRow[lifecycleStageColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(lifecyclestage))
            {
                person.LifecycleStage = lifecyclestage;
                jsondata.Append(lifecycleStageColumn.SheetColumnName + " = " + lifecyclestage + "; ");
            }


            //PartnerType
            var partnerTypeColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.PartnerTypeField)));
            string partnertype = string.Empty;
            partnertype = partnerTypeColumn != null ? personRow[partnerTypeColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(partnertype))
            {
                person.PartnerType = partnertype;
                jsondata.Append(partnerTypeColumn.SheetColumnName + " = " + partnertype + "; ");
            }

            //SecondaryEmails
            var secondaryEmailsColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.SecondaryEmail)));
            string secondaryemail = string.Empty;
            secondaryemail = secondaryEmailsColumn != null ? personRow[secondaryEmailsColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(secondaryemail))
            {
                person.SecondaryEmails = secondaryemail;
                jsondata.Append(secondaryEmailsColumn.SheetColumnName + " = " + secondaryemail + "; ");
            }

            //FacebookUrl
            var facebookUrlColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.FacebookUrl)));
            string facebookurl = string.Empty;
            facebookurl = facebookUrlColumn != null ? personRow[facebookUrlColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(facebookurl)
                && IsFacebookURLValid(facebookurl))
            {
                person.FacebookUrl = facebookurl;
                jsondata.Append(facebookUrlColumn.SheetColumnName + " = " + facebookurl + "; ");
            }

            //TwitterUrl
            var twitterUrlColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.TwitterUrl)));
            string twitterurl = string.Empty;
            twitterurl = twitterUrlColumn != null ? personRow[twitterUrlColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(twitterurl)
                && IsTwitterURLValid(twitterurl))
            {
                person.TwitterUrl = twitterurl;
                jsondata.Append(twitterUrlColumn.SheetColumnName + " = " + twitterurl + "; ");
            }

            //LinkedInUrl
            var linkedInUrlColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.LinkedInUrl)));
            string linkedinurl = string.Empty;
            linkedinurl = linkedInUrlColumn != null ? personRow[linkedInUrlColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(linkedinurl)
                && IsLinkedInURLValid(linkedinurl))
            {
                person.LinkedInUrl = linkedinurl;
                jsondata.Append(linkedInUrlColumn.SheetColumnName + " = " + linkedinurl + "; ");
            }

            //BlogUrl
            var blogUrlColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.BlogUrl)));
            string blogurl = string.Empty;
            blogurl = blogUrlColumn != null ? personRow[blogUrlColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(blogurl) && IsValidURL(blogurl))
            {
                person.BlogUrl = blogurl;
                jsondata.Append(blogUrlColumn.SheetColumnName + " = " + blogurl + "; ");
            }


            //WebSiteUrl
            var webSiteUrlColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.WebsiteUrl)));
            string websiteurl = string.Empty;
            websiteurl = webSiteUrlColumn != null ? personRow[webSiteUrlColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(websiteurl) && IsValidURL(websiteurl))
            {
                person.WebSiteUrl = websiteurl;
                jsondata.Append(webSiteUrlColumn.SheetColumnName + " = " + websiteurl + "; ");
            }

            //GooglePlusUrl
            var googlePlusUrlColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.GooglePlusUrl)));
            string googleplusurl = string.Empty;
            googleplusurl = googlePlusUrlColumn != null ? personRow[googlePlusUrlColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(googleplusurl)
                && IsGooglePlusURLValid(googleplusurl))
            {
                person.GooglePlusUrl = googleplusurl;
                jsondata.Append(googlePlusUrlColumn.SheetColumnName + " = " + googleplusurl + "; ");
            }

            //AddressLine1
            var addressLine1Column = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.AddressLine1Field)));
            string addressline1 = string.Empty;
            addressline1 = addressLine1Column != null ? personRow[addressLine1Column.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(addressline1))
            {
                person.AddressLine1 = addressline1;
                jsondata.Append(addressLine1Column.SheetColumnName + " = " + addressline1 + "; ");
            }

            //AddressLine2
            var addressLine2Column = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.AddressLine2Field)));
            string addressline2 = string.Empty;
            addressline2 = addressLine2Column != null ? personRow[addressLine2Column.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(addressline2))
            {
                person.AddressLine2 = addressline2;
                jsondata.Append(addressLine2Column.SheetColumnName + " = " + addressline2 + "; ");
            }

            //City
            var cityColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.CityField)));
            string city = string.Empty;
            city = cityColumn != null ? personRow[cityColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(city))
            {
                person.City = city;
                jsondata.Append(cityColumn.SheetColumnName + " = " + city + "; ");
            }

            //ZipCode
            var zipCodeColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.ZipCodeField)));
            string zipcode = string.Empty;
            zipcode = zipCodeColumn != null ? personRow[zipCodeColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(zipcode))
            {
                person.ZipCode = zipcode.Replace(" ", "").Trim();
                jsondata.Append(zipCodeColumn.SheetColumnName + " = " + zipcode.Replace(" ", "").Trim() + "; ");
            }

            //state
            var stateColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.StateField)));
            string state = string.Empty;
            state = stateColumn != null ? personRow[stateColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(state))
            {
                person.State = state;
                jsondata.Append(stateColumn.SheetColumnName + " = " + state + "; ");
            }

            // leadsource
            var leadsourcemapping = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.LeadSource)));
            string leadsource = string.Empty;
            leadsource = leadsourcemapping != null ? personRow[leadsourcemapping.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(leadsource))
            {
                person.LeadSource = leadsource;
                jsondata.Append(leadsourcemapping.SheetColumnName + " = " + leadsource + "; ");
            }

            //country
            var countryColumn = mappings.SingleOrDefault(c => c.ContactFieldName.Equals(Convert.ToString((int)ContactFields.CountryField)));
            string country = string.Empty;
            country = countryColumn != null ? personRow[countryColumn.SheetColumnName].ToString() : null;
            if (!string.IsNullOrEmpty(country))
            {
                var countryvalue = country.Replace(" ", string.Empty).ToLower();
                if (countryvalue == "usa" || countryvalue == "us" || countryvalue == "unitedstates" || countryvalue == "unitedstatesofamerica")
                    person.Country = "US";
                else if (countryvalue == "ca" || countryvalue == "canada")
                    person.Country = "CA";
                else
                    person.Country = country;
                jsondata.Append(countryColumn.SheetColumnName + " = " + person.Country + "; ");
            }


            // setting is deleted 
            person.IsDeleted = false;
            // setting custom fileds
            person.CustomField = customField.ToString();
            person.PhoneData = phoneField.ToString();
            person.JobID = jobId;
            person.ContactStatusID = 1;
            person.ContactTypeID = 1;
            person.OwnerID = proccessedBy;
            person.LeadAdapterRowData = jsondata.ToString();

            return person;
        }

        private bool IsFacebookURLValid(string facebookUrl)
        {
            bool result = facebookUrl.ToLower().Contains("facebook.com") ? true : false;
            return result;
        }

        private bool IsTwitterURLValid(string twitterUrl)
        {
            bool result = twitterUrl.ToLower().Contains("twitter.com") ? true : false;
            return result;
        }

        private bool IsLinkedInURLValid(string linkedInUrl)
        {
            bool result = linkedInUrl.ToLower().Contains("linkedin.com") ? true : false;
            return result;
        }

        private bool IsGooglePlusURLValid(string googlePlusUrl)
        {
            bool result = googlePlusUrl.ToLower().Contains("plus.google.com") ? true : false;
            return result;
        }

        private void ContactsBulkinsert(List<RawContact> contacts)
        {
            try
            {
                Logger.Current.Informational("Entered into insert bulk contact data");
                ImportContactsData contactsdata = new ImportContactsData();
                contactsdata.ContactData = contacts;
                contactsdata.ContactCustomData = contactCustomData;
                contactsdata.ContactPhoneData = contactPhoneData;
                importDataRepository.InsertImportsData(contactsdata);
                Logger.Current.Informational("Completed insert bulk contact data");
                //importDataRepository.InsertImportCustomFieldData(contactCustomData);
                //Logger.Current.Informational("Completed insert bulk contact custom data");
                //importDataRepository.InsertImportPhoneData(contactPhoneData);
                //Logger.Current.Informational("Completed insert bulk contact phone data");
                importDataRepository.InsertImportContactEmailStatuses(contacts);
                Logger.Current.Informational("Completed insert bulk contact email data");
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while inserting bulk contact data", ex);
                throw new UnsupportedOperationException("Error while inserting Bulk Contact Data", ex);
            }
        }

        /// <summary>
        /// Mailgun verification for bulk contacts
        /// Not using mailgun verification in imports 11/2/2016
        /// </summary>
        private void MailgunVerification(List<RawContact> contacts)
        {
            Logger.Current.Informational("Inside mailgun verification method");
            var mailGunList = new List<IEnumerable<string>>();
            var interimMailList = new List<string>();
            var mailLengthList = contacts
                                .Where(c => !string.IsNullOrEmpty(c.PrimaryEmail))
                                .Select(c => new { Email = c.PrimaryEmail, Length = (c.PrimaryEmail.Length + 1) }) //adding + 1 to consider comma in final string
                                .Distinct();
            Logger.Current.Informational("magilLength List is prepared");
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
            Logger.Current.Informational("mails broken into smaller lists");
            foreach (var mailsToCheck in mailGunList)
            {
                Logger.Current.Informational("MailGun Verification Started: " + DateTime.UtcNow + ", iteration:" + mailGunList.IndexOf(mailsToCheck) + "/" + mailGunList.Count);
                var emails = string.Join(",", mailsToCheck);
                GetRestResponse response = mailGunService.BulkEmailValidate(new GetRestRequest() { Email = emails });
                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic restResponse = js.Deserialize<dynamic>(response.RestResponse.Content);

                if (restResponse != null)
                {
                    string[] valid = ((IEnumerable)restResponse["parsed"]).Cast<object>().Select(x => x.ToString()).ToArray();
                    string[] notvalid = ((IEnumerable)restResponse["unparseable"]).Cast<object>().Select(x => x.ToString()).ToArray();

                    contacts.Join(valid, c => c.PrimaryEmail.ToLower(), v => v.ToLower(), (c, v) => c).ForEach(p => p.EmailStatus = (byte)EmailStatus.Verified);
                    contacts.Join(notvalid, c => c.PrimaryEmail.ToLower(), v => v.ToLower(), (c, v) => c).ForEach(p => p.EmailStatus = (byte)EmailStatus.HardBounce);
                }
                else
                {
                    contacts.ForEach(p => p.EmailStatus = (byte)EmailStatus.NotVerified);
                }
                Logger.Current.Informational("MailGun Verification Ended: " + DateTime.UtcNow + ", iteration:" + mailGunList.IndexOf(mailsToCheck) + "/" + mailGunList.Count);
                Logger.Current.Informational("Suppression Email Verification Started: " + DateTime.UtcNow + ", iteration:" + mailGunList.IndexOf(mailsToCheck) + "/" + mailGunList.Count);

                CheckSuppressionEmailsResponse suppressedEmailCheckResponse = suppressionListService.CheckSuppressionEmails(new CheckSuppressionEmailsRequest { Emails = mailsToCheck, AccountId = accountId });
                IEnumerable<string> suppressedEmails = suppressedEmailCheckResponse.SuppressedEmails.Select(e => e.ToString()).ToList();
                if (suppressedEmails != null)
                {
                    contacts.ForEach(a =>
                    {
                        if (suppressedEmails.Contains(a.PrimaryEmail))
                        {
                            a.EmailStatus = (byte)EmailStatus.Suppressed;
                        }
                    });
                }

                Logger.Current.Informational("Suppression Email Verification Ended: " + DateTime.UtcNow + ", iteration:" + mailGunList.IndexOf(mailsToCheck) + "/" + mailGunList.Count);
            }
            importDataRepository.InsertImportContactEmailStatuses(contacts);
        }

        /// <summary>
        /// Reading contact object from excel to domain class
        /// </summary>
        public List<RawContact> GetContactData(string fileName, DuplicateLogic duplicateLogic, bool updateOnDuplicate)
        {
            Logger.Current.Informational("Entered into Getdata method");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var persons = new List<RawContact>();

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
                if (rawContacts.Any())
                {
                    //Process for each person
                    //var contactElements = dataTable.Rows;
                    //var filteredRows = contactElements.Cast<DataRow>().Where(row => row.ItemArray.Any(field => !(field is System.DBNull)));

                    //assign serial id for all contacts
                    var i = 0;
                    rawContacts.ForEach(r => r.SerialId = i++);

                    var dupContactsByEmail = rawContacts.Where(r => !string.IsNullOrEmpty(r.PrimaryEmail))
                                                            .GroupBy(r => r.PrimaryEmail, StringComparer.OrdinalIgnoreCase)
                                                            .Where(x => x.Count() > 1)
                                                            .Select(g => new { Key = g.Key, Serials = g.Select(h => h.SerialId) });
                    dupContactsByEmail.ForEach(d =>
                    {
                        var sid = d.Serials.FirstOrDefault();
                        rawContacts.Join(d.Serials, r => r.SerialId, s => s, (r, s) => r).ForEach(r =>
                        {
                            r.SerialId = sid;
                        });
                    });

                    if (duplicateLogic == DuplicateLogic.EmailAddressAndFullNameAndCompany)
                    {
                        var dupContactsByName = rawContacts.Where(r => string.IsNullOrEmpty(r.PrimaryEmail) && !string.IsNullOrEmpty(r.FirstName) && !string.IsNullOrEmpty(r.LastName))
                                                    .GroupBy(r => new ContactName { FirstName = r.FirstName,LastName= r.LastName }, new CustomComparer())
                                                    .Where(x => x.Count() > 1)
                                                    .Select(g => new { Key = g.Key, Serials = g.Select(h => h.SerialId) });

                        dupContactsByName.ForEach(d =>
                        {
                            var sid = d.Serials.FirstOrDefault();
                            rawContacts.Join(d.Serials, r => r.SerialId, s => s, (r, s) => r).ForEach(r =>
                            {
                                r.SerialId = sid;
                            });
                        });
                    }

                    foreach (var contactElement in rawContacts)
                    {
                        var guid = contactElement.ReferenceId;

                        RawContact data = null;

                        //if (duplicateLogic == DuplicateLogic.OnlyEmailAddress)
                        //{
                        //    data = persons.FirstOrDefault(p => p.SerialId == contactElement.SerialId);
                        //    if (!String.IsNullOrEmpty(contactElement.PrimaryEmail))
                        //    {
                        //        data = persons.FirstOrDefault(p => p.PrimaryEmail.Length == contactElement.PrimaryEmail.Length &&
                        //                                    string.Compare(p.PrimaryEmail, contactElement.PrimaryEmail, true) == 0);
                        //    }

                        //}
                        //else
                        //{
                        //    if (!String.IsNullOrEmpty(contactElement.PrimaryEmail))
                        //    {
                        //        data = persons.FirstOrDefault(p => p.PrimaryEmail.Length == contactElement.PrimaryEmail.Length &&
                        //                                            string.Compare(p.PrimaryEmail, contactElement.PrimaryEmail, true) == 0);
                        //    }

                        //    else if (!String.IsNullOrEmpty(contactElement.FirstName) && !String.IsNullOrEmpty(contactElement.LastName))
                        //    {
                        //        data = persons.FirstOrDefault(p => p.FirstName.Length == contactElement.FirstName.Length &&
                        //                                            p.LastName.Length == contactElement.LastName.Length &&
                        //                                            string.Compare(p.FirstName, contactElement.FirstName, true) == 0 &&
                        //                                            string.Compare(p.LastName, contactElement.LastName, true) == 0);
                        //    }


                        //}
                        data = persons.FirstOrDefault(p => p.SerialId == contactElement.SerialId);

                        if (data != null && updateOnDuplicate == true)
                        {
                            RawContact person = MergeDuplicateData(data, contactElement, guid);
                            //person.IsDuplicate = true;
                        }
                        else if (!(data != null && updateOnDuplicate == false))
                        {
                            var person = new RawContact
                            {
                                FirstName = contactElement.FirstName,
                                LastName = contactElement.LastName,
                                CompanyName = contactElement.CompanyName,
                                Title = contactElement.Title,
                                LeadSource = contactElement.LeadSource,
                                DoNotEmail = contactElement.DoNotEmail,
                                HomePhone = null,
                                MobilePhone = null,
                                WorkPhone = null,
                                AccountID = contactElement.AccountID,
                                PrimaryEmail = contactElement.PrimaryEmail,
                                EmailStatus = (byte)EmailStatus.NotVerified,    //Default Status for emails
                                SecondaryEmails = contactElement.SecondaryEmails,
                                FacebookUrl = contactElement.FacebookUrl,
                                TwitterUrl = contactElement.TwitterUrl,
                                GooglePlusUrl = contactElement.GooglePlusUrl,
                                LinkedInUrl = contactElement.LinkedInUrl,
                                BlogUrl = contactElement.BlogUrl,
                                WebSiteUrl = contactElement.WebSiteUrl,
                                AddressLine1 = contactElement.AddressLine1,
                                AddressLine2 = contactElement.AddressLine2,
                                City = contactElement.City,
                                State = contactElement.State,
                                Country = contactElement.Country,
                                ZipCode = contactElement.ZipCode,
                                IsDefault = contactElement.IsDefault,
                                LifecycleStage = contactElement.LifecycleStage,
                                PartnerType = contactElement.PartnerType,
                                LeadAdapterRecordStatusId = 1,
                                ReferenceId = guid,
                                ContactID = 0,
                                IsDeleted = contactElement.IsDeleted,
                                CustomFieldsData = contactElement.CustomField,
                                PhoneData = contactElement.PhoneData,
                                JobID = contactElement.JobID,// Convert.ToInt32(Convert.ToString(contactElement["JobID"])),
                                OwnerID = contactElement.OwnerID,// Convert.ToInt32(Convert.ToString(contactElement["OwnerID"])),
                                ContactTypeID = contactElement.ContactTypeID,// Convert.ToByte(Convert.ToString(contactElement["ContactTypeID"])),
                                ContactStatusID = contactElement.ContactStatusID,// Convert.ToByte(Convert.ToString(contactElement["ContactStatusID"])),
                                EmailExists = false,
                                LeadAdapterRowData = contactElement.LeadAdapterRowData,
                                SerialId = contactElement.SerialId
                            };

                            person.ValidEmail = ValidateEmail(person);

                            //RawContact duplicate_data = null;
                            //if (!String.IsNullOrEmpty(Convert.ToString(person.PrimaryEmail)))
                            //    duplicate_data = persons.Where(p => string.Compare(p.PrimaryEmail, person.PrimaryEmail, true) == 0).FirstOrDefault();
                            //else
                            //    duplicate_data = persons.Where(p => string.Compare(p.FirstName, person.FirstName, true) == 0 && string.Compare(p.LastName, person.LastName, true) == 0).FirstOrDefault();

                            //if (duplicate_data != null)
                            //    person.IsDuplicate = true;

                            persons.Add(person);
                        }
                    }
                }
                stopwatch.Stop();

                var elapsed_time = stopwatch.Elapsed;
                Logger.Current.Informational("Time Taken to complete GetData method :" + elapsed_time);
                return persons;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured in GetData Method in the Excel Lead Adapter Provider: ", ex);
            }
            return persons;

        }

        RawContact MergeDuplicateData(RawContact contact, RawContactViewModel contactElement, Guid referenceId)
        {
            contact.FirstName = contactElement.FirstName ?? contact.FirstName;
            contact.LastName = contactElement.LastName ?? contact.LastName;
            contact.CompanyName = contactElement.CompanyName ?? contact.CompanyName;
            contact.Title = contactElement.Title ?? contact.Title;
            contact.LeadSource = contactElement.LeadSource ?? contact.LeadSource;
            contact.DoNotEmail = contactElement.DoNotEmail;
            contact.PrimaryEmail = contactElement.PrimaryEmail ?? contact.PrimaryEmail;
            contact.SecondaryEmails = contactElement.SecondaryEmails ?? contact.SecondaryEmails;
            contact.FacebookUrl = contactElement.FacebookUrl ?? contact.FacebookUrl;
            contact.TwitterUrl = contactElement.TwitterUrl ?? contact.TwitterUrl;
            contact.GooglePlusUrl = contactElement.GooglePlusUrl ?? contact.GooglePlusUrl;
            contact.LinkedInUrl = contactElement.LinkedInUrl ?? contact.LinkedInUrl;
            contact.BlogUrl = contactElement.BlogUrl ?? contact.BlogUrl;
            contact.WebSiteUrl = contactElement.WebSiteUrl ?? contact.WebSiteUrl;
            contact.AddressLine1 = contactElement.AddressLine1 ?? contact.AddressLine1;
            contact.AddressLine2 = contactElement.AddressLine2 ?? contact.AddressLine2;
            contact.City = contactElement.City ?? contact.City;
            contact.State = contactElement.State ?? contact.State;
            contact.Country = contactElement.Country ?? contact.Country;
            contact.ZipCode = contactElement.ZipCode ?? contact.ZipCode;
            contact.IsDefault = contactElement.IsDefault;
            contact.LifecycleStage = contactElement.LifecycleStage ?? contact.LifecycleStage;
            contact.PartnerType = contactElement.PartnerType ?? contact.PartnerType;
            contact.LeadAdapterRecordStatusId = 1;
            contact.ContactID = 0;
            contact.IsDeleted = contactElement.IsDeleted;
            contact.CustomFieldsData = contactElement.CustomField ?? contact.CustomFieldsData;
            contact.PhoneData = contactElement.PhoneData ?? contact.PhoneData;
            contact.JobID = contactElement.JobID;
            contact.OwnerID = contactElement.OwnerID;
            contact.ContactTypeID = contactElement.ContactTypeID;
            contact.ContactStatusID = contactElement.ContactStatusID;
            contact.EmailExists = false;
            contact.LeadAdapterRowData = contactElement.LeadAdapterRowData ?? string.Empty;
            contact.ValidEmail = true;

            importDataRepository.MergeDuplicateImportData(contact.CustomFieldsData, contact.PhoneData, referenceId, contact.ReferenceId);

            return contact;
        }

        public override ImportContactsData GetContacts(string fileName, IEnumerable<FieldViewModel> customFields, int jobId, IEnumerable<DropdownValueViewModel> DropdownFields)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks given object is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private T CheckIfNull<T>(object input)
        {
            if (input == null)
                return default(T);
            return (T)input;
        }

        private IEnumerable<ImportDataViewModel> mapColumns(IEnumerable<ImportColumnMappings> columnMappings)
        {
            List<ImportDataViewModel> vm = new List<ImportDataViewModel>();
            if (columnMappings != null && columnMappings.Any())
            {
                foreach (var map in columnMappings)
                {
                    ImportDataViewModel viewModel = new ImportDataViewModel() { ContactFieldName = map.ContactFieldName, IsCustomField = map.IsCustomField, IsDropDownField = map.IsDropDownField, SheetColumnName = map.SheetColumnName };
                    vm.Add(viewModel);
                }
            }
            return vm;
        }

        public override Dictionary<string, string> GetFieldMappings()
        {
            return new Dictionary<string, string>();
        }

        public override string GetRootNode()
        {
            throw new NotImplementedException();
        }
    }


    public class CustomComparer : IEqualityComparer<ContactName>
    {
        public bool Equals(ContactName x, ContactName y)
        {
            return String.Equals(x.FirstName, y.FirstName, StringComparison.CurrentCultureIgnoreCase)
                   && String.Equals(x.LastName, y.LastName, StringComparison.CurrentCultureIgnoreCase);
                   
        }

        public int GetHashCode(ContactName obj)
        {
            return string.Concat(obj.FirstName.ToLower(),
                                 obj.LastName.ToLower()).GetHashCode();
        }
    }

    public class ContactName
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
