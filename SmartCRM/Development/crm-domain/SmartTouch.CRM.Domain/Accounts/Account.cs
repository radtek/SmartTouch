using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using SmartTouch.CRM.Domain.Modules;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.WebAnalytics;
using System.Text;

namespace SmartTouch.CRM.Domain.Accounts
{
    public class Account : EntityBase<int>, IAggregateRoot
    {
        #region local variables
        private string accountName;
        private string firstName;
        private string lastName;
        private string company;
        private Url facebookUrl;
        private Url twitterUrl;
        private Url googlePlusUrl;
        private Url linkedInUrl; 
        #endregion

        public string AccountName { get { return accountName; } set { accountName = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public string FirstName { get { return firstName; } set { firstName = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public string LastName { get { return lastName; } set { lastName = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public string Company { get { return company; } set { company = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public byte SubscriptionID { get; set; }
        public Subscriptions.Subscription subscription { get; set; }
        public ICollection<Module> SubscribedModules { get; set; }
        public byte Culture { get; set; }
        public int ContactsCount { get; set; }
        public int EmailsCount { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
        public Email Email { get; set; }
        public IEnumerable<Email> SecondaryEmails { get; set; }
        public byte Status { get; set; }
        public string StatusMessage  { get; set; }
        public Phone WorkPhone { get; set; }
        public Phone HomePhone { get; set; }
        public Phone MobilePhone { get; set; }
        public string PrivacyPolicy { get; set; }
        public string DomainURL { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public int SenderReputationCount { get; set; }
        public int AccountID { get; set; }
        public bool IsDeleted { get; set; }
        public Image AccountLogo { get; set; }
        public byte DateFormatID { get; set; }
        public byte CurrencyID { get; set; }
        public string CountryID { get; set; }
        public string TimeZone { get; set; }
        public string GoogleDriveClientID { get; set; }
        public byte? OpportunityCustomers { get; set; }
        public string GoogleDriveAPIKey { get; set; }
        public string DropboxAppKey { get; set; }
        public WebAnalyticsProvider WebAnalyticsProvider{ get; set; }
        public int? LogoImageID { get; set; }
        public string AccountLogoUrl { get; set; }
        public string FacebookAPPID { get; set; }
        public string FacebookAPPSecret { get; set; }
        public string TwitterAPIKey { get; set; }
        public string TwitterAPISecret { get; set; }
        public string LitmusAPIKey { get; set; }
        string helpURL;
        public string HelpURL
        {
            get { return helpURL; }
            set
            {
                string plainUrl = !string.IsNullOrEmpty(value) ? value.Trim() : null;
                    if (!string.IsNullOrEmpty(plainUrl) &&
                        ((plainUrl.StartsWith("http://") || plainUrl.StartsWith("https://")))) helpURL = plainUrl.ToLower();
                    else if (!string.IsNullOrEmpty(plainUrl)) 
                        helpURL = "http://" + plainUrl.ToLower();
                    else helpURL = string.IsNullOrEmpty(plainUrl) ? string.Empty : plainUrl.ToLower();
                
            }}
        public bool ShowTC { get; set; }
        public string TC { get; set; }
        public bool? Disclaimer { get; set; }
        public int? UserLimit { get; set; }
        public string ExcludedRoles { get; set; }
        private Url SetSocialUrl(Url url, string startsWith)
        {
            string plainUrl = (url != null && !string.IsNullOrEmpty(url.URL)) ? url.URL.Trim() : null;
            return (plainUrl != null) ? CorrectTheUrl(new Url() { URL = plainUrl }, startsWith, true) : null;
        }
        public Url FacebookUrl
        {
            get { return facebookUrl; }
            set
            {
                facebookUrl = SetSocialUrl(value, "facebook.com");
            }
        }
        public Url TwitterUrl
        {
            get { return twitterUrl; }
            set
            {
                twitterUrl = SetSocialUrl(value, "twitter.com");
            }
        }
        public Url GooglePlusUrl
        {
            get { return googlePlusUrl; }
            set
            {
                googlePlusUrl = SetSocialUrl(value, "plus.google.com");
            }
        }
        public Url LinkedInUrl
        {
            get { return linkedInUrl; }
            set
            {
                linkedInUrl = SetSocialUrl(value, "linkedin.com");
            }
        }
        public Url BlogUrl { get; set; }
        public Url WebsiteUrl { get; set; }
        /// <summary>
        /// Validates Accounts model
        /// </summary>
        protected override void Validate()
        {
            if (string.IsNullOrEmpty(AccountName)) AddBrokenRule(AccountBusinessRule.AccountNameRequired);
            if (string.IsNullOrEmpty(FirstName)) AddBrokenRule(AccountBusinessRule.AccountFirstNameRequired);
            if (string.IsNullOrEmpty(LastName)) AddBrokenRule(AccountBusinessRule.AccountLastNameRequired);
            if (HomePhone != null && !string.IsNullOrEmpty(HomePhone.Number) && !IsValidPhoneNumberLength(HomePhone.Number)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            if (MobilePhone != null && !string.IsNullOrEmpty(MobilePhone.Number) && !IsValidPhoneNumberLength(MobilePhone.Number)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            if (WorkPhone != null && !string.IsNullOrEmpty(WorkPhone.Number) && !IsValidPhoneNumberLength(WorkPhone.Number)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            if (!string.IsNullOrEmpty(Email.EmailId) && !IsValidEmail(Email.EmailId)) AddBrokenRule(AccountBusinessRule.AccountPrimaryEmailRequired);
            if (string.IsNullOrEmpty(DomainURL)) AddBrokenRule(AccountBusinessRule.AccountDomainNameRequired);
            if (!IsDomainNameValid(DomainURL)) AddBrokenRule(AccountBusinessRule.AccountDomainNameInvalid);
            if (string.IsNullOrEmpty(HelpURL)) AddBrokenRule(AccountBusinessRule.HelpURLIsRequired);
            if (!IsValidURL()) AddBrokenRule(AccountBusinessRule.HelpURLIsInvalid);
            //Validates if all the secondary emails are valid
            if (SecondaryEmails != null)
            {
                foreach (Email email in SecondaryEmails)
                {
                    if (!string.IsNullOrEmpty(Email.EmailId) && !IsValidEmail(email.EmailId)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.EmailIsInvalid);
                }
            }
            //Validate Facebook URL:
            if (facebookUrl != null && !string.IsNullOrEmpty(facebookUrl.URL) && !ValidateURL(facebookUrl, "facebook.com/")) AddBrokenRule(AccountBusinessRule.AccountFacebookUrlInvalid);
            //Validate Twitter URL:
            if (twitterUrl != null && !string.IsNullOrEmpty(twitterUrl.URL) && !ValidateURL(twitterUrl, "twitter.com/")) AddBrokenRule(AccountBusinessRule.AccountTwitterUrlInvalid);
            //Validate Google+ URL:
            if (googlePlusUrl != null && !string.IsNullOrEmpty(googlePlusUrl.URL) && !ValidateURL(googlePlusUrl, "plus.google.com/")) AddBrokenRule(AccountBusinessRule.AccountGooglePlusUrlInvalid);
            //Validate LinkedIn URL:
            if (linkedInUrl != null && !string.IsNullOrEmpty(linkedInUrl.URL) && !ValidateURL(linkedInUrl, "linkedin.com/")) AddBrokenRule(AccountBusinessRule.AccountLinkedInUrlInvalid);
            //Validate Blog URL:
            if (BlogUrl != null && !string.IsNullOrEmpty(BlogUrl.URL) && !BlogUrl.IsValidURL()) AddBrokenRule(AccountBusinessRule.AccountBlogInUrlInvalid);
            //Validate Web URL:
            if (WebsiteUrl != null && !string.IsNullOrEmpty(WebsiteUrl.URL) && !WebsiteUrl.IsValidURL()) AddBrokenRule(AccountBusinessRule.AccountWebURLInvalid);
        }

        private bool IsDomainNameValid(string domain)
        {
            var domainName = domain.Substring(0,domain.IndexOf('.'));
            string pattern = @"^[a-zA-Z0-9]*\-*[a-zA-Z0-9]*$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(domainName)) return true;
            else return false;
        }

        private Url CorrectTheUrl(Url url, string domain, bool secured)
        {
            if (!string.IsNullOrEmpty(url.URL) && url.URL.Contains(domain))
            {
                var urlIndex = url.URL.IndexOf(domain);
                url.URL = (secured ? "https://" : "http://") + url.URL.Substring(urlIndex, url.URL.Length - urlIndex);
                return url;
            }
            else
            {
                if (!string.IsNullOrEmpty(url.URL) && !url.URL.Contains(domain)) return url;
                else return null;
            }
        }
        /// <summary>
        /// Returns true or false. Checks if the facebook url passed is valid or not
        /// </summary>
        /// <param name="url"></param>
        /// <param name="urlStartsWith"></param>
        /// <returns></returns>
        private bool ValidateURL(Url url, string urlStartsWith)
        {
            bool result = false;
            if (url != null && !string.IsNullOrEmpty(url.URL) && url.IsValidURL())
            {
                string url_new = url.URL.Replace("https://www", "").Replace("http://www", "").Replace("https://", "").Replace("http://", "");
                result = url_new.ToLower().StartsWith(urlStartsWith) ? true : false;
            }
            return result;
        }
        /// <summary>
        /// Returns true or false. Checks if the minimum length of the passed in phone number is 10 or not.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public bool IsValidPhoneNumberLength(string phoneNumber)
        {
            var matches = Regex.Matches(phoneNumber, @"[0-9]");
            var sb = new StringBuilder();
            foreach (Match match in matches)sb.Append(match.Value);
            var extractedNumber = sb.ToString().TrimStart('0', '1');
            return (extractedNumber.Length < 10 || extractedNumber.Length > 15) ? false : true;
        }

        /// <summary>
        /// Returns true or false. Checks if the passed in email is a valid email or not
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool IsValidEmail(string email)
        {
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";    //Source: http://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
        /// <summary>
        /// Validate United States zip code. Checks if the zip code is in either NNNNN or NNNNN-NNNN format or not
        /// </summary>
        /// <param name="zipCode"></param>
        /// <returns></returns>
        public bool IsValidUSZipCode(string zipCode)
        {
            return IsValidZip(zipCode, @"^[0-9]{5}$" + @"|[0-9]{5}(\ ?-? ?[0-9]{4})$");
        }
        /// <summary>
        /// Validate Candian postal code. Checks if the postal code is in either ANA NAN format or not.
        /// </summary>
        /// <param name="postalCode"></param>
        /// <returns></returns>
        public bool IsValidCanadianPostalCode(string postalCode)
        {
            return IsValidZip(postalCode, @"^[A-Z]\d[A-Z]\d[A-Z]\d$");
        }
        private bool IsValidZip(string zipCode, string pattern)
        {
            zipCode = zipCode.Replace(" ", "").Replace("-", "").ToUpper();
            return Regex.IsMatch(zipCode.ToUpper(), pattern);
        }
        public bool IsValidURL()
        {
            string pattern = @"^^((((ht|f)tp(s?))\:\/\/)|(www.))?[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$";
            return Regex.IsMatch(this.HelpURL.ToLower(), pattern);
        }
    }

    public class AccountBasicInfo
    {
        public int AccountID { get; set; }
        public string TimeZone { get; set; }
        public short? WebAnalyticsID { get; set; }
        public string AccountName { get; set; }
    }

}

