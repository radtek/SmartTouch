using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;
using Nest;
using SmartTouch.CRM.Domain.User;
using DA = SmartTouch.CRM.Domain.Action;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Contact
{
    public abstract class Contact : EntityBase<int>, IAggregateRoot
    {
        string companyName { get; set; }
        public string CompanyName { get { return companyName; } set { companyName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string imageurl;
        public string ImageUrl { get { return imageurl; } set { imageurl = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }
        public Guid? ProfileImageKey { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
        public Phone WorkPhone { get; set; }
        public Phone HomePhone { get; set; }
        public Phone MobilePhone { get; set; }

        public Email Email { get; set; }
        public bool DoNotEmail { get; set; }
        public DateTime? LastContacted { get; set; }

        public IEnumerable<Email> SecondaryEmails { get; set; }

        Url facebookUrl;
        public Url FacebookUrl
        { 
            get { return facebookUrl; } 
            set 
            {
                string plainUrl = value!=null && !string.IsNullOrEmpty(value.URL) ? value.URL.Trim() : null;
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
                linkedInUrl= plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, "linkedin.com", true) : null;
            } 
        }

        public Url BlogUrl { get; set; }
        public Url WebsiteUrl { get; set; }
        public IEnumerable<Tag.Tag> Tags { get; set; }
        public Image ContactImages { get; set; }
        //string contactimageurl { get; set; }
        //public string ContactImageUrl { get { return contactimageurl; } set { contactimageurl = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        public IDictionary<RelationshipTypes, Contact> RelatedContacts { get; set; }
        public IDictionary<RelationshipTypes, User.User> RelatedUsers { get; set; }

        #region ElasticSerach autocomplete fields

        public DateTime IndexedOn { get; set; }
        /// <summary>
        /// Used only for indexing CompanyName into elastic search for autocomplete suggestions, need to remove this and put a permanent solution.
        /// </summary>
        public AutoCompleteSuggest CompanyNameAutoComplete { get; set; }

        /// <summary>
        /// Used only for indexing Title into elastic search for autocomplete suggestions, need to remove this and put a permanent solution.
        /// </summary>
        public AutoCompleteSuggest TitleAutoComplete { get; set; }
        
        /// <summary>
        /// Used only for indexing Contact Full Name (CompanyName for Companies & FirstName + LastName for People) 
        /// into elastic search for autocomplete suggestions, need to remove this and put a permanent solution.
        /// </summary>
        public AutoCompleteSuggest ContactFullNameAutoComplete { get; set; }
        #endregion

        public IList<ContactRelationship> ContactRelationships { get; set; }

        public IList<DA.Action> Actions { get; set; }

        protected override void Validate()
        {
            //if (!string.IsNullOrEmpty(ImageUrl) && !this.IsValidImageType())
            //{
            //        AddBrokenRule(ContactBusinessRule.ContactImageTypeInvalid);
            //}

            if (WorkPhone != null && !string.IsNullOrEmpty(WorkPhone.Number) && !IsValidPhoneNumberLength(WorkPhone.Number))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            }

            if (!string.IsNullOrEmpty(Email.EmailId) && !IsValidEmail(Email.EmailId))
            {
                AddBrokenRule(ContactBusinessRule.ContactPrimaryEmailRequired);
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

            //If "LastContacted" is entered manually by the user then the following validation is valid
            if (LastContacted > DateTime.Today)
            {
                AddBrokenRule(ContactBusinessRule.ContactLastContactedDateInvalid);
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
            if (facebookUrl !=null && !string.IsNullOrEmpty(facebookUrl.URL) && !IsFacebookURLValid(facebookUrl))
            {
                AddBrokenRule(ContactBusinessRule.ContactFacebookUrlInvalid);
            }

            //Validate Twitter URL:
            if (twitterUrl != null && !string.IsNullOrEmpty(twitterUrl.URL) && !IsTwitterURLValid(twitterUrl))
            {
                AddBrokenRule(ContactBusinessRule.ContactTwitterUrlInvalid);
            }

            //Validate Google+ URL:
            if (googlePlusUrl != null && !string.IsNullOrEmpty(googlePlusUrl.URL) && !IsGooglePlusURLValid(googlePlusUrl))
            {
                AddBrokenRule(ContactBusinessRule.ContactGooglePlusUrlInvalid);
            }

            //Validate LinkedIn URL:
            if (linkedInUrl != null && !string.IsNullOrEmpty(linkedInUrl.URL) && !IsLinkedInURLValid(linkedInUrl))
            {
                AddBrokenRule(ContactBusinessRule.ContactLinkedInUrlInvalid);
            }

            //Validate Blog URL:
            if (BlogUrl != null && !string.IsNullOrEmpty(BlogUrl.URL) && !BlogUrl.IsValidURL())
            {
                AddBrokenRule(ContactBusinessRule.ContactBlogInUrlInvalid);
            }

            //Validate Web URL:
            if (WebsiteUrl != null && !string.IsNullOrEmpty(WebsiteUrl.URL) && !WebsiteUrl.IsValidURL())
            {
                AddBrokenRule(ContactBusinessRule.ContactWebURLInvalid);
            }
        }

        /// <summary>
        /// Append http:// or https:// to the URL
        /// </summary>
        /// <param name="url">Url passed in</param>
        /// <param name="domain">Domain name to validate</param>
        /// <param name="secured">Secured URL (true/false)</param>
        /// <returns></returns>
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


        //Returns true or false. Checks if the mime type of the file uploaded is of type jpeg or jpg or bmp or png.
        public bool IsValidImageType()
        {
            string pattern = @"(?i)\.(jpeg|jpg|bmp|png)$";
            return Regex.IsMatch(this.ImageUrl, pattern);
        }
    }
}
