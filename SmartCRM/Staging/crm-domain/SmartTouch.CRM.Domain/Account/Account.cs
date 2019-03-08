using System.Collections.Generic;
using System.Text.RegularExpressions;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;


namespace SmartTouch.CRM.Domain.Account
{
    public class Account : EntityBase<int>, IAggregateRoot
    {
        string accountName;
        public string AccountName { get { return accountName; } set { accountName = !string.IsNullOrEmpty(value)?value.Trim():null;  } }

        string firstName;
        public string FirstName { get { return firstName; } set { firstName = !string.IsNullOrEmpty(value)?value.Trim():null;  } }

        string lastName;
        public string LastName { get { return lastName; } set { lastName = !string.IsNullOrEmpty(value)?value.Trim():null;  } }

        string company;
        public string Company { get { return company; } set { company = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        //public DateTime CreatedOn { get; set; }
        //public int CreatedBy { get; set; }
        //public Int16 TimeZoneID { get; set; }

        //string logoUrl;
        //public string LogoUrl { get { return logoUrl; } set { logoUrl = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        //public int CommunicationID { get; set; }
        public byte SubscriptionID { get; set; }
        public Subscription subscription { get; set; }

        public byte Culture { get; set; }

        public IEnumerable<Address> Addresses { get; set; }

        public Email Email { get; set; }
        public IEnumerable<Email> SecondaryEmails { get; set; }
        public string Status { get; set; }

        public Phone WorkPhone { get; set; }
        public Phone HomePhone { get; set; }
        public Phone MobilePhone { get; set; }
        public string PrivacyPolicy { get; set; }

        Url facebookUrl;
        public Url FacebookUrl
        {
            get { return facebookUrl; }
            set
            {
                string plainUrl = value != null && !string.IsNullOrEmpty(value.URL) ? value.URL.Trim() : null;
                facebookUrl = plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, "facebook.com", true) : null;
            }
        }

        Url twitterUrl;
        public Url TwitterUrl
        {
            get { return twitterUrl; }
            set
            {
                string plainUrl = value != null && !string.IsNullOrEmpty(value.URL) ? value.URL.Trim() : null;
                twitterUrl = plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, "twitter.com", true) : null;
            }
        }

        Url googlePlusUrl;
        public Url GooglePlusUrl
        {
            get { return googlePlusUrl; }
            set
            {
                string plainUrl = value != null && !string.IsNullOrEmpty(value.URL) ? value.URL.Trim() : null;
                googlePlusUrl = plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, "plusgoogle.com", true) : null;
            }
        }

        Url linkedInUrl;
        public Url LinkedInUrl
        {
            get { return linkedInUrl; }
            set
            {
                string plainUrl = value != null && !string.IsNullOrEmpty(value.URL) ? value.URL.Trim() : null;
                linkedInUrl = plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, "linkedin.com", true) : null;
            }
        }

        public Url BlogUrl { get; set; }
        public Url WebsiteUrl { get; set; }

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(AccountName))
            {
                AddBrokenRule(AccountBusinessRule.AccountNameRequired);
            }

            if (string.IsNullOrEmpty(FirstName))
            {
                AddBrokenRule(AccountBusinessRule.AccountFirstNameRequired);
            }

            if (string.IsNullOrEmpty(LastName))
            {
                AddBrokenRule(AccountBusinessRule.AccountLastNameRequired);
            }

            if (HomePhone != null && !string.IsNullOrEmpty(HomePhone.Number) && !IsValidPhoneNumberLength(HomePhone.Number))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            }

            if (MobilePhone != null && !string.IsNullOrEmpty(MobilePhone.Number) && !IsValidPhoneNumberLength(MobilePhone.Number))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            }

            if (WorkPhone != null && !string.IsNullOrEmpty(WorkPhone.Number) && !IsValidPhoneNumberLength(WorkPhone.Number))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            }

            if (!string.IsNullOrEmpty(Email.EmailId) && !IsValidEmail(Email.EmailId))
            {
                AddBrokenRule(AccountBusinessRule.AccountPrimaryEmailRequired);
            }

            foreach (Address address in Addresses)
            {
                if (!address.IsValidCountry())
                {
                    AddBrokenRule(ValueObjectBusinessRule.CountryInAddressRequired);
                }
                if (!address.IsValidState())
                {
                    AddBrokenRule(ValueObjectBusinessRule.StateInAddressRequired);
                }
                if (address.Country.Code == "US" && !string.IsNullOrEmpty(address.ZipCode) && !IsValidUSZipCode(address.ZipCode))
                {
                    AddBrokenRule(ValueObjectBusinessRule.ZipCodeFormatInvalid);
                }
                if (address.Country.Code == "CA" && !string.IsNullOrEmpty(address.ZipCode) && !IsValidCanadianPostalCode(address.ZipCode))
                {
                    AddBrokenRule(ValueObjectBusinessRule.PostalCodeFormatInvalid);
                }
            }

            //Validates if all the secondary emails are valid
            if (SecondaryEmails != null)
            {
                foreach (Email email in SecondaryEmails)
                {
                    if (!string.IsNullOrEmpty(Email.EmailId) && !IsValidEmail(email.EmailId))
                    {
                        AddBrokenRule(ValueObjects.ValueObjectBusinessRule.EmailIsInvalid);
                    }
                }
            }
            //Validate Facebook URL:
            //if (FacebookUrl !=null && !string.IsNullOrEmpty(FacebookUrl.URL) && (!FacebookUrl.IsValidURL() || !this.FacebookUrl.URL.ToLower().Contains("facebook.com")))
            if (facebookUrl != null && !string.IsNullOrEmpty(facebookUrl.URL) && !IsFacebookURLValid(facebookUrl))
            {
                AddBrokenRule(AccountBusinessRule.AccountFacebookUrlInvalid);
            }

            //Validate Twitter URL:
            if (twitterUrl != null && !string.IsNullOrEmpty(twitterUrl.URL) && !IsTwitterURLValid(twitterUrl))
            {
                AddBrokenRule(AccountBusinessRule.AccountTwitterUrlInvalid);
            }

            //Validate Google+ URL:
            if (googlePlusUrl != null && !string.IsNullOrEmpty(googlePlusUrl.URL) && !IsGooglePlusURLValid(googlePlusUrl))
            {
                AddBrokenRule(AccountBusinessRule.AccountGooglePlusUrlInvalid);
            }

            //Validate LinkedIn URL:
            if (linkedInUrl != null && !string.IsNullOrEmpty(linkedInUrl.URL) && !IsLinkedInURLValid(linkedInUrl))
            {
                AddBrokenRule(AccountBusinessRule.AccountLinkedInUrlInvalid);
            }

            //Validate Blog URL:
            if (BlogUrl != null && !string.IsNullOrEmpty(BlogUrl.URL) && !BlogUrl.IsValidURL())
            {
                AddBrokenRule(AccountBusinessRule.AccountBlogInUrlInvalid);
            }

            //Validate Web URL:
            if (WebsiteUrl != null && !string.IsNullOrEmpty(WebsiteUrl.URL) && !WebsiteUrl.IsValidURL())
            {
                AddBrokenRule(AccountBusinessRule.AccountWebURLInvalid);
            }
        }

        public Url CorrectTheUrl(Url url, string domain, bool secured)
        {
            if (!string.IsNullOrEmpty(url.URL) && url.URL.Contains(domain))
            {
                var urlIndex = url.URL.IndexOf(domain);
                url.URL = (secured ? "https://" : "http://") + url.URL.Substring(urlIndex, url.URL.Length - urlIndex);
                return url;
            }
            else
                if (!string.IsNullOrEmpty(url.URL) && !url.URL.Contains(domain))
                {
                    return url;
                }
                else
                    return null;
        }

        //Returns true or false. Checks if the facebook url passed is valid or not
        public bool IsFacebookURLValid(Url facebookUrl)
        {
            bool result = facebookUrl != null && !string.IsNullOrEmpty(facebookUrl.URL) && (!facebookUrl.IsValidURL() || facebookUrl.URL.ToLower().Contains("facebook.com")) ? true : false;
            return result;
        }
        public bool IsTwitterURLValid(Url twitterUrl)
        {
            bool result = twitterUrl != null && !string.IsNullOrEmpty(twitterUrl.URL) && (!twitterUrl.IsValidURL() || twitterUrl.URL.ToLower().Contains("twitter.com")) ? true : false;
            return result;
        }
        public bool IsLinkedInURLValid(Url linkedInUrl)
        {
            bool result = linkedInUrl != null && !string.IsNullOrEmpty(linkedInUrl.URL) && (!linkedInUrl.IsValidURL() || linkedInUrl.URL.ToLower().Contains("linkedin.com")) ? true : false;
            return result;
        }
        public bool IsGooglePlusURLValid(Url googlePlusUrl)
        {
            bool result = googlePlusUrl != null && !string.IsNullOrEmpty(googlePlusUrl.URL) && (!googlePlusUrl.IsValidURL() || googlePlusUrl.URL.ToLower().Contains("plusgoogle.com")) ? true : false;
            return result;
        }

        //Returns true or false. Checks if the minimum length of the passed in phone number is 10 or not.
        public bool IsValidPhoneNumberLength(string phoneNumber)
        {
            MatchCollection matches = Regex.Matches(phoneNumber, @"[0-9]");
            string extractedNumber = "";
            foreach (Match match in matches)
            {
                extractedNumber += match.Value;
            }
            extractedNumber = extractedNumber.TrimStart('0', '1');
            int phoneNumberLength = extractedNumber.Length;
            return (phoneNumberLength < 10 || phoneNumberLength > 15) ? false : true;
        }

        //Returns true or false. Checks if the passed in email is a valid email or not
        public bool IsValidEmail(string email)
        {
            bool isValidEmail = false;
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";    //Source: http://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return isValidEmail = regex.IsMatch(email);
        }


        //Validate United States zip code. Checks if the zip code is in either NNNNN or NNNNN-NNNN format or not
        public bool IsValidUSZipCode(string zipCode)
        {
            zipCode = zipCode.Replace(" ", "").Replace("-", "");
            string pattern = @"^[0-9]{5}$" + @"|[0-9]{5}(\ ?-? ?[0-9]{4})$";
            return Regex.IsMatch(zipCode, pattern);
        }


        //Validate Candian postal code. Checks if the postal code is in either ANA NAN format or not.
        public bool IsValidCanadianPostalCode(string postalCode)
        {
            postalCode = postalCode.Replace(" ", "").Replace("-", "").ToUpper();
            string pattern = @"^[A-Z]\d[A-Z]\d[A-Z]\d$";
            return Regex.IsMatch(postalCode.ToUpper(), pattern);
        }
       
    }
}
