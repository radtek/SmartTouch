using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Users;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.Dropdowns;
using CFT = SmartTouch.CRM.Domain.CustomFields;
using System.Globalization;

namespace SmartTouch.CRM.Domain.Contacts
{
    public abstract class Contact : EntityBase<int>, IAggregateRoot
    {
        string companyName { get; set; }
        public string CompanyName { get { return companyName; } set { companyName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        public string Company_Name { get; set; }

        public int AccountID { get; set; }
        string imageurl;
        public string ImageUrl { get { return imageurl; } set { imageurl = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }
        public Guid? ProfileImageKey { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
        public IEnumerable<Phone> Phones { get; set; }

        public IEnumerable<DropdownValue> LeadSources { get; set; }
        public IEnumerable<DropdownValue> Communities { get; set; }
        public IEnumerable<Email> Emails { get; set; }
        public bool DoNotEmail { get; set; }

        public DateTime? LastContacted { get; set; }
        public byte? LastContactedThrough { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastTouchedThrough { get; set; }

        public int? OwnerId { get; set; }
        public int? CompanyID { get; set; }
        public int? PreviousOwnerId { get; set; }

        public short LifecycleStage { get; set; }

        public string Address { get; set; }
        public string Email { get; set; }
        public string DateFormat { get; set; }

        
        public IEnumerable<CFT.CustomFieldTab> CustomFieldTabs { get; set; }
        public IEnumerable<ContactCustomField> CustomFields { get; set; }
        public virtual CRMOutlookSync OutlookSync { get; set; }
        public virtual User Owner { get; set; }
        public DateTime? LastNoteDate { get; set; }
        public string LastNote { get; set; }
        public string NoteSummary { get; set; }
        public short LastNoteCategory { get; set; }
        public bool IncludeInReports { get; set; }

        # region Elastic Web&Social fields
        private string GetLink(Url link )
        {
            return (link != null && !string.IsNullOrEmpty(link.URL)) ? link.URL : null;
        }

        public string FacebookLink
        {
            get 
            {
                return GetLink(FacebookUrl);
            }
        }
        public string TwitterLink
        {
            get
            {
                return GetLink(TwitterUrl);
            }
        }
        public string LinkedInLink
        {
            get
            {
                return GetLink(LinkedInUrl);
            }
        }
        public string BlogLink
        {
            get
            {
                return GetLink(BlogUrl);
            }
        }
        public string WebsiteLink
        {
            get
            {
                return GetLink(WebsiteUrl);
            }
        }
        public string GooglePlusLink
        {
            get
            {
                return GetLink(GooglePlusUrl);
            }
        }
        #endregion

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
                googlePlusUrl = plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, "plus.google.com", true) : null;
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
        public IEnumerable<Tags.Tag> Tags { get; set; }
        public Image ContactImage { get; set; }
        public bool IsDeleted { get; set; }
        public IDictionary<RelationshipTypes, Contact> RelatedContacts { get; set; }
        public IDictionary<RelationshipTypes, Users.User> RelatedUsers { get; set; }

        #region ElasticSerach autocomplete fields

        public DateTime IndexedOn { get { return DateTime.Now.ToUniversalTime(); } set { } }
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

        public IEnumerable<AutoCompleteSuggest> EmailAutoComplete { get; set; }
        public IEnumerable<AutoCompleteSuggest> PhoneNumberAutoComplete { get; set; }
        #endregion
        public List<ContactTourCommunityMap> TourCommunity { get; set; }
        public List<ContactActionMap> ContactActions { get; set; }
        public List<ContactNoteMap> ContactNotes { get; set; }
        public IList<ContactRelationship> ContactRelationships { get; set; }

        public ContactRelationship ContactRelationship { get; set; }

        public IList<DA.Action> Actions { get; set; }

        public Guid ReferenceId { get; set; }

        /*These two are for contact sources (leadadpater,import,form,application) and type of source (bdx,nhg,import,formid)*/
        public ContactSource? ContactSource { get; set; }
        public ContactSource? FirstContactSource { get; set; }
        public int? SourceType { get; set; }
        public int? FirstSourceType { get; set; }
        protected override void Validate()
        {
            if (Emails != null)
            {
                foreach (Email email in Emails)
                {
                    if (!string.IsNullOrEmpty(email.EmailId) && !IsValidEmail(email.EmailId)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.EmailIsInvalid);
                }
            }

            if (Phones != null)
            {
                foreach (Phone phone in Phones)
                {
                    if (!string.IsNullOrEmpty(phone.Number) && !IsValidPhoneNumberLength(phone.Number)) AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
                }
            }

            if (facebookUrl != null && !string.IsNullOrEmpty(facebookUrl.URL) && !IsFacebookURLValid(facebookUrl))
            {
                AddBrokenRule(ContactBusinessRule.ContactFacebookUrlInvalid);
            }

            if (twitterUrl != null && !string.IsNullOrEmpty(twitterUrl.URL) && !IsTwitterURLValid(twitterUrl))
            {
                AddBrokenRule(ContactBusinessRule.ContactTwitterUrlInvalid);
            }

            if (googlePlusUrl != null && !string.IsNullOrEmpty(googlePlusUrl.URL) && !IsGooglePlusURLValid(googlePlusUrl))
            {
                AddBrokenRule(ContactBusinessRule.ContactGooglePlusUrlInvalid);
            }

            if (linkedInUrl != null && !string.IsNullOrEmpty(linkedInUrl.URL) && !IsLinkedInURLValid(linkedInUrl))
            {
                AddBrokenRule(ContactBusinessRule.ContactLinkedInUrlInvalid);
            }

            if (BlogUrl != null && !string.IsNullOrEmpty(BlogUrl.URL) && !BlogUrl.IsValidURL())
            {
                AddBrokenRule(ContactBusinessRule.ContactBlogInUrlInvalid);
            }

            if (WebsiteUrl != null && !string.IsNullOrEmpty(WebsiteUrl.URL) && !WebsiteUrl.IsValidURL())
            {
                AddBrokenRule(ContactBusinessRule.ContactWebURLInvalid);
            }


            foreach (ContactCustomField customfiled in CustomFields)
            {
                if (!string.IsNullOrEmpty(customfiled.Value) && customfiled.FieldInputTypeId == (int)FieldType.email)
                {
                    if (!IsValidEmail(customfiled.Value))
                    {
                        AddBrokenRule(ValueObjects.ValueObjectBusinessRule.EmailIsInvalid);
                    }
                }
                else if (!string.IsNullOrEmpty(customfiled.Value) && customfiled.FieldInputTypeId == (int)FieldType.url)
                {
                    if (!IsUrlValid(customfiled.Value))
                    {
                        AddBrokenRule(ValueObjects.ValueObjectBusinessRule.UrlIsInvalid);
                    }
                }

                else if (!string.IsNullOrEmpty(customfiled.Value) && customfiled.FieldInputTypeId == (int)FieldType.number)
                {
                    double num;

                    if (customfiled.Value != "" && !double.TryParse(customfiled.Value, out num))
                    {
                        /* It's not  a number!*/
                        AddBrokenRule(ValueObjects.ValueObjectBusinessRule.NumberIsInvalid);
                    }
                }

                else if (!string.IsNullOrEmpty(customfiled.Value) && (customfiled.FieldInputTypeId == (int)FieldType.date || customfiled.FieldInputTypeId == (int)FieldType.time))
                {
                    DateTime dt;
                    CultureInfo ci = CultureInfo.GetCultureInfo("en-us");
                    string[] formats = ci.DateTimeFormat.GetAllDateTimePatterns();
                    if (DateTime.TryParseExact(customfiled.Value, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                    {
                        Match matchResult = Regex.Match(dt.ToShortDateString(), "^([1-9]|0[1-9]|1[012])[- /.]([1-9]|0[1-9]|[12][0-9]|3[01])[- /.][0-9]{4}$");
                        if (matchResult.Success == false)
                        {
                            AddBrokenRule(ValueObjects.ValueObjectBusinessRule.DateInvalid);
                        }
                        customfiled.Value = dt.ToString("yyyy-MM-dd" );
                    }
                    else
                    {
                        customfiled.Value="";
                    }
                }
                //else if (!string.IsNullOrEmpty(customfiled.Value) && customfiled.FieldInputTypeId == (int)FieldType.time)
                //{

                //    Regex matchResult = new Regex(@"^(?:(?:0?[0-9]|1[0-2]):[0-5][0-9] [ap]m|(?:[01][0-9]|2[0-3]):[0-5][0-9])$", RegexOptions.IgnoreCase);
                //    bool result = matchResult.IsMatch(customfiled.Value);
                //    if (!result)
                //    {
                //        AddBrokenRule(ValueObjects.ValueObjectBusinessRule.TimeInvalid);
                //    }
                //}
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
                return null;
        }
        
        public bool IsFacebookURLValid(Url _facebookUrl)
        {
            bool result = false;
            if (_facebookUrl != null && !string.IsNullOrEmpty(_facebookUrl.URL) && _facebookUrl.IsValidURL())
            {
                string url = _facebookUrl.URL.Replace("https://www", "").Replace("http://www", "").Replace("https://", "").Replace("http://", "");
                result = url.ToLower().StartsWith("facebook.com/") ? true : false;
            }
            return result;
        }
        public bool IsTwitterURLValid(Url _twitterUrl)
        {
            bool result = false;
            if (_twitterUrl != null && !string.IsNullOrEmpty(_twitterUrl.URL) && _twitterUrl.IsValidURL())
            {
                string url = _twitterUrl.URL.Replace("https://www", "").Replace("http://www", "").Replace("https://", "").Replace("http://", "");
                result = url.ToLower().StartsWith("twitter.com/") ? true : false;
            }
            return result;
        }
        public bool IsLinkedInURLValid(Url _linkedInUrl)
        {
            bool result = false;
            if (_linkedInUrl != null && !string.IsNullOrEmpty(_linkedInUrl.URL) && _linkedInUrl.IsValidURL())
            {
                string url = _linkedInUrl.URL.Replace("https://www.", "").Replace("http://www.", "").Replace("https://", "").Replace("http://", "");
                result = url.ToLower().StartsWith("linkedin.com/") ? true : false;
            }
            return result;
        }
        public bool IsGooglePlusURLValid(Url _googlePlusUrl)
        {
            bool result = false;
            if (_googlePlusUrl != null && !string.IsNullOrEmpty(_googlePlusUrl.URL) && _googlePlusUrl.IsValidURL())
            {
                string url = _googlePlusUrl.URL.Replace("https://www", "").Replace("http://www", "").Replace("https://", "").Replace("http://", "");
                result = url.ToLower().StartsWith("plus.google.com/") ? true : false;
            }
            return result;
        }

        /// <summary>
        /// Validates a U.S. phone number. It must consist of 3 numeric characters, optionally enclosed in parentheses, 
        /// followed by a set of 3 numeric characters and then a set of 4 numeric characters
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        
        public bool IsValidPhoneNumberLength(string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                int phoneNumberLength = phoneNumber.Length;
                return (phoneNumberLength < 10 || phoneNumberLength > 15) ? false : true;
            }
            else
                return false;
        }

        public bool IsValidEmail(string email)
        {
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";    //Source: http://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }

        public bool IsUrlValid(string url)
        {
            string pattern = @"(https?:\/\/(?:www\.|(?!www))[^\s\.]+\.[^\s]{2,}|www\.[^\s]+\.[^\s]{2,})";
            Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return regex.IsMatch(url);
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

        public bool IsValidImageType()
        {
            string pattern = @"(?i)\.(jpeg|jpg|bmp|png)$";
            return Regex.IsMatch(this.ImageUrl, pattern);
        }
    }
}
