using LandmarkIT.Enterprise.CommunicationManager.Requests;
using RestSharp;
using xmlse = System.Xml.Serialization;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Fields;
using System.Web.Script.Serialization;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using LandmarkIT.Enterprise.Utilities.Logging;
using System.Globalization;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class FindSpamService :IFindSpamService
    {
        readonly IFormRepository formRepository;
        readonly IServiceProviderRepository serviceProvider;
        readonly IAccountRepository accountRepository;
        readonly ICustomFieldRepository customFieldRepository;
        // readonly IFormService formService;
        readonly IAccountService accountService;
        readonly IUrlService urlService;
        public FindSpamService(IFormRepository formRepository
          , IServiceProviderRepository serviceProvider, IAccountRepository accountRepository
            , ICustomFieldRepository customFieldRepository
          , IAccountService accountService, IUrlService urlService
            )
        {
            this.formRepository = formRepository;
            this.serviceProvider = serviceProvider;
            this.accountRepository = accountRepository;
            this.customFieldRepository = customFieldRepository;
            // this.formService = formService;
            this.accountService = accountService;
            this.urlService = urlService;
        }

        /// <summary>
        /// Spam Check
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="accountId"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public bool SpamCheck(IDictionary<string,string> fields, int accountId,string ipAddress, int formId, bool isFormSubmission, out string spamRemarks)
        {
            
            var isSpam = false;
            string remarks = string.Empty;
            spamRemarks = string.Empty;
            IDictionary<string, string> submittedFields = fields;
            var spamValidators = GetSpamValidators(accountId);
            var order = spamValidators.OrderBy(o => o.Order).ToList();
            try
            {
                foreach (var item in order)
                {
                    switch (item.Validator)
                    {
                        case "InternalSpamIPCheck":
                            isSpam = InternalSpamIPCheck(ipAddress, accountId, out remarks);
                            break;
                        case "SpamIPAPICheck":
                            isSpam = SpamIPApiCheck(ipAddress, accountId, out remarks);
                            break;
                        case "SubmissionIPLimit":
                            isSpam = IPSubmissionLimitCheck(accountId, item.Value, ipAddress, isFormSubmission, out remarks);
                            break;
                        case "EmailValidation":
                            isSpam = ValidateEmail(submittedFields, item.Value, out remarks);
                            break;
                        case "SpamKeyWordCheck":
                            isSpam = CheckForSpamKeywords(submittedFields, out remarks);
                            break;
                        case "RepeatedValueCheck":
                            isSpam = RepeatedValuesCheck(submittedFields, item.Value, out remarks);
                            break;
                        case "InvalidDatatypeCheck":
                            isSpam = ValidateDataTypes(submittedFields, accountId, out remarks);
                            break;
                        //case "AlphaNumeric":
                        //    isSpam = ValidateAlphaNumeric(submittedFields, out remarks);
                        //    break;
                    }

                    if (isSpam)
                    {
                        spamRemarks = remarks;
                        break;
                    }

              }
                
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error Logging while validating Spam Logic", ex);
            }

            return isSpam;
        }

        /// <summary>
        /// Get All Spam Validators
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        private IEnumerable<SpamValidator> GetSpamValidators(int accountId)
        {
            IEnumerable<SpamValidator> spamValidators = formRepository.GetAllSpamValidators(accountId);
            return spamValidators;
        }

        /// <summary>
        /// Internal Ip Address Checking
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        private bool InternalSpamIPCheck(string ipAddress, int accountId,out string remarks)
        {
            bool isSpam = false;
            remarks = string.Empty;
            IList<string> SpamIps =  new List<string>();
            string ipAdd = ipAddress;
            string newIP = ipAddress;
            if (ipAdd == "::1")
                newIP = "127.0.0.1";
            isSpam = formRepository.CheckForSpamIP(newIP, accountId, true);
            if (isSpam)
                remarks = "Internal Spam IP Check Failed";

            return isSpam;
        }

        /// <summary>
        /// API Spam Checking
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        private bool SpamIPApiCheck(string ipAddress, int accountId, out string remarks)
        {
            bool isSpam = false;
            remarks = string.Empty;
            string ipAdd = ipAddress;
            string newIP = ipAddress;
            if (ipAdd == "::1")
                newIP = "127.0.0.1";
           bool spam = formRepository.CheckForIPExclusion(newIP, accountId);
            if(!spam)
            {
                RestClient client = new RestClient();
                client.BaseUrl = new Uri("http://www.stopforumspam.com/api?ip=");
                RestRequest Request = new RestRequest();
                Request.AddParameter("ip", newIP);
                var response = client.Execute(Request);
                ApiResponse apiResponse = new ApiResponse();
                xmlse.XmlSerializer deserializer = new xmlse.XmlSerializer(typeof(ApiResponse));
                StringReader reader = new StringReader(response.Content);
                apiResponse = (ApiResponse)deserializer.Deserialize(reader);
                if (apiResponse.Apprears == "yes")
                    isSpam = false;//isSpam = true; temporary commented this flag need to revert change after days 'line commented by kiran on 11/05/2018 for process form submission'
                if (isSpam)
                    remarks = "Spam IP Check Failed";

            }
            
            return isSpam;
        }

        /// <summary>
        /// IP Submission Limit Checking
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="SubmissionLimit"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        private bool IPSubmissionLimitCheck(int accountId, string SubmissionLimit, string ipAddress,bool isFormSubmission, out string remarks)
        {
            string[] values = SubmissionLimit.Split('|');
            bool isSpam = false;
            remarks = string.Empty;
            int limit = Int32.Parse(values[0]);
            int timeLimit = Int32.Parse(values[1]);
            string ipAdd = ipAddress;
            string newIP = ipAddress;
            if (ipAdd == "::1")
                newIP = "127.0.0.1";
            bool spam = formRepository.CheckForIPExclusion(newIP, accountId);
            if(!spam)
            {
                int IPCount = formRepository.GettingFormIPSubmissionCount(accountId, ipAddress, timeLimit, isFormSubmission);
                if (IPCount > limit)
                {
                    isSpam = true;
                    remarks = "IP Submission Limit Exceeded.";
                }
            }
            
            return isSpam;
        }

        /// <summary>
        /// Email Validation with firstname and lastname
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="formatter"></param>
        /// <returns></returns>
        private bool ValidateEmail(IDictionary<string, string> fields, string formatter, out string remarks)
        {
            bool isSpam = false;
            remarks = string.Empty;
            Func<ContactFields, string> GetFieldValue = (cf) =>
            {
                var field = fields.Where(k => k.Key == ((byte)cf).ToString()).FirstOrDefault();
                if (!field.Equals(default(KeyValuePair<string, string>)))
                    return field.Value;
                else
                    return string.Empty;
            };
            string firstName = GetFieldValue(ContactFields.FirstNameField);
            string lastName = GetFieldValue(ContactFields.LastNameField);
            string[] formats = formatter.Split('|');
            string email = GetFieldValue(ContactFields.PrimaryEmail);
            string[] data = email.Split('@');
            if (data.Length > 1)
            {
               foreach (var item in formats)
               {
                  string format = item.Replace("{firstname}", firstName).Replace("{lastname}", lastName);
                  if (format.Equals(data[0], StringComparison.OrdinalIgnoreCase))
                  {
                            isSpam = true;
                            remarks = "Email Check Failed, Format matched " + item;
                            break;
                  }
               }
             }
            return isSpam;
        }

        /// <summary>
        /// Spam KeyWords Checking
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private bool CheckForSpamKeywords(IDictionary<string,string> fields, out string remarks)
        {
            /*
             * Firstname, lastname, email are exact match.
             * Rest of the fields should follow partial match.
             * */
            bool isSpam = false;
            remarks = string.Empty;
            IEnumerable<string> SpamKeyWords = formRepository.GetAllSpamKeyWords();
            foreach (KeyValuePair<string,string> keyValue in fields)
            {
                string key = keyValue.Key;
                string value = keyValue.Value;
                if ((key == ((byte)Entities.ContactFields.FirstNameField).ToString() ||
                    key == ((byte)Entities.ContactFields.LastNameField).ToString() ||
                    key == ((byte)Entities.ContactFields.PrimaryEmail).ToString())
                    && !string.IsNullOrEmpty(value))
                {
                    isSpam = SpamKeyWords.Where(s => s.Equals(value,StringComparison.OrdinalIgnoreCase)).Any();
                    if (isSpam)
                        break;
                }
                else if(!string.IsNullOrEmpty(value))
                {
                    foreach(var spamkey in SpamKeyWords)
                    {
                        isSpam = value.ToLower().Contains(spamkey.ToLower());
                        if (isSpam)
                            break;
                    }

                    if (isSpam)
                        break;
                }

            }

            if (isSpam)
                remarks = "Matching Spam Key Words.";


            return isSpam;
        }

        /// <summary>
        /// Checking Duplicate Values Check
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="maxLimit"></param>
        /// <returns></returns>
        private bool RepeatedValuesCheck(IDictionary<string,string> fields, string maxLimit, out string remarks)
        {
            bool isSpam = false;
            int limit = Int32.Parse(maxLimit);
            remarks = string.Empty;
            var duplicateValues = fields.GroupBy(x => x.Value).Select(x => new { Item = x.First(), Count = x.Count() }).ToList();
            foreach (var item in duplicateValues)
            {
                if (!string.IsNullOrEmpty(item.Item.Value))
                {
                    if(item.Count > limit)
                    {
                        isSpam = true;
                        remarks = "Repeated Values Detected.";
                        break;
                    }
                    
                }
            }
            return isSpam;
        }

        /// <summary>
        /// Validating Data types
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="accountId"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        private bool ValidateDataTypes(IDictionary<string,string> fields,int accountId, out string remarks)
        {
            bool isSpam = false;
            remarks = string.Empty;
            Field field = new Field();
            IEnumerable<Field> customFields = customFieldRepository.GetAllCustomFieldsByAccountId(accountId);
            foreach (KeyValuePair<string,string> keyValue in fields)
            {
                string key = keyValue.Key;
                string value = keyValue.Value;
                int id;
                bool res = int.TryParse(key, out id);
                if(res == true && id > 200) 
                    field = customFields.Where(c => c.Id == id).FirstOrDefault();
                    
                if (key == ((byte)Entities.ContactFields.FirstNameField).ToString() && !string.IsNullOrEmpty(value))
                {
                    Regex r = new Regex("^[a-zA-Z]+$");
                    if(!r.IsMatch(keyValue.Value))
                    {
                        isSpam = true;
                        remarks = "Invalid FirstName";
                        break;
                    }
                    
                }
                else if(key == ((byte)Entities.ContactFields.LastNameField).ToString() && !string.IsNullOrEmpty(value))
                {
                    Regex r = new Regex("^[a-zA-Z]+$");
                    if (!r.IsMatch(keyValue.Value))
                    {
                        isSpam = true;
                        remarks = "Invalid LastName";
                        break;
                    }
                }
                else if(key == ((byte)Entities.ContactFields.MobilePhoneField).ToString() && !string.IsNullOrEmpty(value))
                {
                    if (value.Length < 10 || value.Length > 15)
                    {
                        isSpam = false;
                        break;
                    }
                    else
                    {
                        Regex regex = new Regex("^\\s*(?:\\+?(\\d{1,3}))?[-. (]*(\\d{3})[-. )]*(\\d{3})[-. ]*(\\d{4})(?: *x(\\d+))?\\s*$");
                        Match matchPhoneNumber = regex.Match(keyValue.Value);
                        if(!matchPhoneNumber.Success)
                        {
                            isSpam = true;
                            remarks = "Invalid MobilePhone";
                            break;
                        }
                        
                    }

                }
                else if(field != null && field.FieldInputTypeId == FieldType.date && !string.IsNullOrEmpty(value))
                {
                    DateTime dt;
                    CultureInfo ci = CultureInfo.GetCultureInfo("en-us");
                    string[] formats = ci.DateTimeFormat.GetAllDateTimePatterns();
                    if (DateTime.TryParseExact(value, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                    {
                        Match matchResult = Regex.Match(dt.ToShortDateString(), "^([1-9]|0[1-9]|1[012])[- /.]([1-9]|0[1-9]|[12][0-9]|3[01])[- /.][0-9]{4}$");
                        if (matchResult.Success == false)
                        {
                            isSpam = true;
                            remarks = "Invalid Date";
                            break;
                        }
                        value = dt.ToString("yyyy-MM-dd");
                    }
                }
                else if(field != null && field.FieldInputTypeId == FieldType.time && !string.IsNullOrEmpty(value))
                {

                    Regex matchResult = new Regex(@"^(?:(?:0?[0-9]|1[0-2]):[0-5][0-9] [ap]m|(?:[01][0-9]|2[0-3]):[0-5][0-9])$", RegexOptions.IgnoreCase);
                    bool result = matchResult.IsMatch(keyValue.Value);
                    if (!result)
                    {
                        isSpam = true;
                        remarks = "Invalid Time";
                        break;
                    }
                }
            }

            return isSpam;
        }

        //private bool ValidateAlphaNumeric(IDictionary<string, string> fields, out string remarks)
        //{
        //    bool isSpam = false;
        //    remarks = string.Empty;
        //    if (fields.Any())
        //    {
        //        Func<ContactFields, string> GetFieldValue = (cf) =>
        //        {
        //            var field = fields.Where(k => k.Key == ((byte)cf).ToString()).FirstOrDefault();
        //            if (!field.Equals(default(KeyValuePair<string, string>)))
        //                return field.Value;
        //            else
        //                return string.Empty;
        //        };
        //        string firstName = GetFieldValue(ContactFields.FirstNameField);
        //        string lastName = GetFieldValue(ContactFields.LastNameField);
        //        Regex r = new Regex("^[a-zA-Z0-9]*$");
        //        if (!r.IsMatch(firstName)) {
        //            isSpam = true;
        //            remarks = "Only AlphaNumerics are allowed in FirstName";
        //        }
        //        if (!r.IsMatch(lastName))
        //        {
        //            isSpam = true;
        //            remarks = "Only AlphaNumerics are allowed in LastName";
        //        }
        //    }
        //    return isSpam;
        //}
    }
}
