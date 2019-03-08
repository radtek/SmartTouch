using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Roles;

namespace SmartTouch.CRM.Domain.Users
{
    public class User : EntityBase<string>, IAggregateRoot
    {
        public int AccountID { get; set; }
        public Account Account { get; set; }
        public bool? IsSTAdmin { get; set; }
        public string PrimaryPhoneType { get; set; }
        /*New Users starts*/ 
     
        string firstName;
        public string FirstName { get { return firstName; } set { firstName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string lastName;
        public string LastName { get { return lastName; } set { lastName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string userName;
        public string UserName { get { return Email.EmailId; } set { userName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string company;
        public string Company { get { return company; } set { company = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        public Status Status { get; set; }
        public string Title { get; set; }
        public string Password { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
        public IEnumerable<Email> Emails { get; set; }
        public Phone WorkPhone { get; set; }
        public Phone HomePhone { get; set; }
        public Phone MobilePhone { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public IEnumerable<Email> SecondaryEmails { get; set; }
        public Role Role { get; set; }
        public short RoleID { get; set; }
        public string EmailSignature { get; set; }
        public string FacebookAccessToken { get; set; }
        public string TwitterOAuthToken { get; set; }
        public string TwitterOAuthTokenSecret { get; set; }
        public bool? HasTourCompleted { get; set; }

        private Url SetSocialURL(Url value, string startsWith)
        {
            string plainUrl = value != null && !string.IsNullOrEmpty(value.URL) ? value.URL.Trim() : null;
            return plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, startsWith, true) : null;           
        }

        Url facebookUrl;
        public Url FacebookUrl
        {
            get { return facebookUrl; }
            set
            {
                facebookUrl = SetSocialURL(value, "facebook.com");
            }
        }

        Url twitterUrl;
        public Url TwitterUrl
        {
            get { return twitterUrl; }
            set
            {
                twitterUrl = SetSocialURL(value, "twitter.com");
            }
        }

        Url googlePlusUrl;
        public Url GooglePlusUrl
        {
            get { return googlePlusUrl; }
            set
            {
                googlePlusUrl = SetSocialURL(value, "plus.google.com");
            } 
        }

        Url linkedInUrl;
        public Url LinkedInUrl
        {
            get { return linkedInUrl; }
            set
            {
                linkedInUrl = SetSocialURL(value, "linkedin.com");
            }
        }

        public Url BlogUrl { get; set; }
        public Url WebsiteUrl { get; set; }
        public Email Email { get; set; }

        public Url CorrectTheUrl(Url url, string domain, bool secured)
        {
            if (!string.IsNullOrEmpty(url.URL) && url.URL.Contains(domain))
            {
                var urlIndex = url.URL.IndexOf(domain);
                url.URL = (secured ? "https://" : "http://") + url.URL.Substring(urlIndex, url.URL.Length - urlIndex);
                return url;
            }
            else
                if (!string.IsNullOrEmpty(url.URL) && !url.URL.Contains(domain)) return url;
                else return null;
        }

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(FirstName)) AddBrokenRule(UserBusinessRule.UserFirstNameRequired);
            if (string.IsNullOrEmpty(LastName)) AddBrokenRule(UserBusinessRule.UserLastNameRequired);
            if (HomePhone != null && !string.IsNullOrEmpty(HomePhone.Number) && !IsValidPhoneNumberLength(HomePhone.Number)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            if (MobilePhone != null && !string.IsNullOrEmpty(MobilePhone.Number) && !IsValidPhoneNumberLength(MobilePhone.Number)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            if (WorkPhone != null && !string.IsNullOrEmpty(WorkPhone.Number) && !IsValidPhoneNumberLength(WorkPhone.Number)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            if (Email != null && !string.IsNullOrEmpty(Email.EmailId) && !IsValidEmail(Email.EmailId)) AddBrokenRule(UserBusinessRule.UserPrimaryEmailRequired);
            /*Validates if all the secondary emails are valid*/
            if (SecondaryEmails != null)
            {
                foreach (Email email in SecondaryEmails)
                {
                    if (!string.IsNullOrEmpty(email.EmailId) && !IsValidEmail(email.EmailId)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.EmailIsInvalid);
                }
            }
            /*Validates if all the emails are valid*/
            if (Emails != null)
            {
                foreach (Email email in Emails)
                {
                    if (!string.IsNullOrEmpty(email.EmailId) && !IsValidEmail(email.EmailId)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.EmailIsInvalid);
                }
            }

            /*Validate Facebook URL:*/
            if (!IsValidUrl(facebookUrl,"facebook.com/")) AddBrokenRule(UserBusinessRule.UserFacebookUrlInvalid);
            /*Validate Twitter URL:*/
            if (!IsValidUrl(twitterUrl, "twitter.com/")) AddBrokenRule(UserBusinessRule.UserTwitterUrlInvalid);
            /*Validate Google+ URL:*/
            if (!IsValidUrl(googlePlusUrl,"plus.google.com/")) AddBrokenRule(UserBusinessRule.UserGooglePlusUrlInvalid);
            /*Validate LinkedIn URL:*/
            if (!IsValidUrl(linkedInUrl, "linkedin.com/")) AddBrokenRule(UserBusinessRule.UserLinkedInUrlInvalid);
            /*Validate Blog URL:*/
            if (BlogUrl != null && !string.IsNullOrEmpty(BlogUrl.URL) && !BlogUrl.IsValidURL()) AddBrokenRule(UserBusinessRule.UserBlogInUrlInvalid);
            /*Validate Web URL:*/
            if (WebsiteUrl != null && !string.IsNullOrEmpty(WebsiteUrl.URL) && !WebsiteUrl.IsValidURL()) AddBrokenRule(UserBusinessRule.UserWebURLInvalid);
        }

        private bool IsValidUrl(Url url,string startsWith)
        {
            bool result = false;
            if (url != null && !string.IsNullOrEmpty(url.URL) && url.IsValidURL())
            {
                string urlNew = url.URL.Replace("https://www", "").Replace("http://www", "").Replace("https://", "").Replace("http://", "");
                result = urlNew.ToLower().StartsWith(startsWith) ? true : false;
            }
            else if (url == null)
                result = true;

            return result;
        }

        public bool IsValidPhoneNumberLength(string phoneNumber)
        {
            string pattern = @"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(phoneNumber);
        }

        /*Returns true or false. Checks if the passed in email is a valid email or not*/
        public bool IsValidEmail(string email)
        {
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";    //Source: http://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }


        /*Validate United States zip code. Checks if the zip code is in either NNNNN or NNNNN-NNNN format or not*/
        public bool IsValidUSZipCode(string zipCode)
        {
            zipCode = zipCode.Replace(" ", "").Replace("-", "");
            string pattern = @"^[0-9]{5}$" + @"|[0-9]{5}(\ ?-? ?[0-9]{4})$";
            return Regex.IsMatch(zipCode, pattern);
        }


        /*Validate Candian postal code. Checks if the postal code is in either ANA NAN format or not.*/
        public bool IsValidCanadianPostalCode(string postalCode)
        {
            postalCode = postalCode.Replace(" ", "").Replace("-", "").ToUpper();
            string pattern = @"^[A-Z]\d[A-Z]\d[A-Z]\d$";
            return Regex.IsMatch(postalCode.ToUpper(), pattern);
        }
    }

    public class UserBasicInfo

    {
        public int UserID { get; set; }
        public int AccountID { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TimeZone { get; set; }
        public string AccountName { get; set; }
        public string RoleName { get; set; }
        public short RoleId { get; set; }
    }
}
