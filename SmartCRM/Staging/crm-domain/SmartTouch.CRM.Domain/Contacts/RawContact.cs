using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ImportData;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class RawContact : ValueObjectBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Title { get; set; }
        public string LeadSource { get; set; }
        public string LifecycleStage { get; set; }
        public string PartnerType { get; set; }
        public bool DoNotEmail { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public int AccountID { get; set; }
        public string PrimaryEmail { get; set; }
        public string SecondaryEmails { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string GooglePlusUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string BlogUrl { get; set; }
        public string WebSiteUrl { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public bool IsDefault { get; set; }
        public int ImportDataId { get; set; }
        public byte LeadAdapterRecordStatusId { get; set; }
        public Guid ReferenceId { get; set; }
        public int ContactID { get; set; }
        public bool IsDeleted { get; set; }
        public string BuilderNumber { get; set; }
        public string CustomFieldsData { get; set; }
        public string PhoneData { get; set; }
        public IEnumerable<Email> Emails { get; set; }
        public ContactType ContactType { get; set; }
        public byte EmailStatus { get; set; }           
        public byte ContactStatusID { get; set; }
        public Guid ReferenceID { get; set; }
        public byte ContactTypeID { get; set; }
        public int OwnerID { get; set; }
        public int JobID { get; set; }
        public int LeadSourceID { get; set; }
        public bool? EmailExists { get; set; }
        public bool? IsBuilderNumberPass { get; set; }
        public bool? IsCommunityNumberPass { get; set; }
        public string LeadAdapterSubmittedData { get; set; }
        public string LeadAdapterRowData { get; set; }
        public string OrginalRefId { get; set; }
        public bool IsDuplicate { get; set; }
        public bool ValidEmail { get; set; }
        public int SerialId { get; set; }
        protected override void Validate()
        {
            if (string.IsNullOrEmpty(PrimaryEmail) && (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))) {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.ContactNotValid);
            }

            if (!string.IsNullOrEmpty(PrimaryEmail) && !IsValidEmail(PrimaryEmail.Trim()))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.EmailIsInvalid);
            }
           
        }

        public bool IsFacebookURLValid(string facebookUrl)
        {
            bool result = facebookUrl.ToLower().Contains("facebook.com") ? true : false;
            return result;
        }

        public bool IsTwitterURLValid(string twitterUrl)
        {
            bool result = twitterUrl.ToLower().Contains("twitter.com") ? true : false;
            return result;
        }
        public bool IsLinkedInURLValid(string linkedInUrl)
        {
            bool result = linkedInUrl.ToLower().Contains("linkedin.com") ? true : false;
            return result;
        }
        public bool IsGooglePlusURLValid(string googlePlusUrl)
        {
            bool result =  googlePlusUrl.ToLower().Contains("plus.google.com") ? true : false;
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
            string pattern = @"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(phoneNumber);
        }

        public bool IsValidEmail(string email)
        {
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";    //Source: http://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }


        //Validate United States zip code. Checks if the zip code is in either NNNNN or NNNNN-NNNN format or not
        public bool IsValidUSZipCode(string zipCode)
        {
            return IsValidZip(zipCode, @"^[0-9]{5}$" + @"|[0-9]{5}(\ ?-? ?[0-9]{4})$");
        }

        private bool IsValidZip(string zipCode, string pattern)
        {
            zipCode = zipCode.Replace(" ", "").Replace("-", "").ToUpper();
            return Regex.IsMatch(zipCode.ToUpper(), pattern);
        }

        //Validate Candian postal code. Checks if the postal code is in either ANA NAN format or not.
        public bool IsValidCanadianPostalCode(string postalCode)
        {
            postalCode = postalCode.Replace(" ", "").Replace("-", "").ToUpper();
            string pattern = @"^[A-Z]\d[A-Z]\d[A-Z]\d$";
            return Regex.IsMatch(postalCode.ToUpper(), pattern);
        }

        public bool IsValidURL(string Url)
        {
            string pattern = @"^^((((ht|f)tp(s?))\:\/\/)|(www.))?[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$";
            bool result = Regex.IsMatch(Url.ToLower(), pattern);
            return result;
        }      

    }

    public class ContactCampaignEntry
    {
        public int ContactID { get; set; }
        public DateTime ActivityDate { get; set; }
    }
}
