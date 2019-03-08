
using SmartTouch.CRM.Entities;
using System.Text.RegularExpressions;

namespace SmartTouch.CRM.Domain.Contact
{
    public class Person : Contact
    {
        string firstName;
        public string FirstName { get { return firstName; } set { firstName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string lastName;
        public string LastName { get { return lastName; } set { lastName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        public int LeadScore { get; set; }

        string leadSource;
        public string LeadSource { get { return leadSource; } set { leadSource = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string title;
        public string Title { get { return title; } set { title = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string ssn;
        public string SSN { get { return ssn; } set { ssn = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        public PartnerType? PartnerType { get; set; }
        public LifecycleStage LifecycleStage { get; set; }

        protected override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(FirstName))
            {
                AddBrokenRule(ContactBusinessRule.ContactFirstNameRequired);
            }

            if (string.IsNullOrEmpty(LastName))
            {
                AddBrokenRule(ContactBusinessRule.ContactLastNameRequired);
            }

            if (HomePhone != null && !string.IsNullOrEmpty(HomePhone.Number) && !IsValidPhoneNumberLength(HomePhone.Number))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            }

            if (MobilePhone != null && !string.IsNullOrEmpty(MobilePhone.Number) && !IsValidPhoneNumberLength(MobilePhone.Number))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            }


            if (!string.IsNullOrEmpty(SSN) && !IsValidSsnOrSin())
            {
                AddBrokenRule(ContactBusinessRule.ContactSsnAndSinIsInvalid);
            }

        }

        //Returns true or false. Checks if the minimum length of the passed in SSN/SIN is 9 or not
        public bool IsValidSsnOrSin()
        {
            var ssnOrSin = this.SSN.Replace(" ", "").Replace("-", "").TrimStart('0');
            string pattern = @"^\d{9}$";
            return Regex.IsMatch(ssnOrSin, pattern);
        }
    }
}