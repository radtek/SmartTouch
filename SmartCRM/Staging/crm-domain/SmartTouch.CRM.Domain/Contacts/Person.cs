
using SmartTouch.CRM.Entities;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.ValueObjects;
using System;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class Person : Contact
    {
        string firstName;
        public string FirstName { get { return firstName; } set { firstName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string lastName;
        public string LastName { get { return lastName; } set { lastName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        public int LeadScore { get; set; }

        string title;
        public string Title { get { return title; } set { title = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string ssn;
        public string SSN { get { return ssn; } set { ssn = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        public IEnumerable<DropdownValue> AllLeadSources { get; set; }

        /*Indexing to elastic*/
        public IEnumerable<WebVisit> WebVisits { get; set; }
        public IEnumerable<FormSubmission> FormSubmissions { get; set; }

        public short? PartnerType { get; set; }
        public bool IsActive { get; set; }

        protected override void Validate()
        {
            base.Validate();

            var primaryEmail = "";
            if (this.Emails != null)
            {
                foreach (Email email in this.Emails)
                {
                    if (email.IsPrimary == true)
                        primaryEmail = email.EmailId;
                }
            }
            if (string.IsNullOrEmpty(primaryEmail) && (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName)))
            {
                AddBrokenRule(ContactBusinessRule.ContactFirstNameAndLastNameRequired);
            }

            if (!string.IsNullOrEmpty(FirstName) && FirstName.Length > 75)
            {
                AddBrokenRule(ContactBusinessRule.ContactFirstNameLength);
            }

            if (!string.IsNullOrEmpty(LastName) && LastName.Length > 75)
            {
                AddBrokenRule(ContactBusinessRule.ContactLastNameLength);
            }
        }


        /*Returns true or false. Checks if the minimum length of the passed in SSN/SIN is 9 or not*/
        public bool IsValidSsnOrSin()
        {
            var ssnOrSin = this.SSN.Replace(" ", "").Replace("-", "").TrimStart('0');
            string pattern = @"^\d{9}$";
            return Regex.IsMatch(ssnOrSin, pattern);
        }

        public string GetContactName()
        {
            return FirstName + " " + LastName;
        }
    }
}