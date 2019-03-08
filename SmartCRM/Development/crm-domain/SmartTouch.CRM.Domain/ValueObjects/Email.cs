
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Text.RegularExpressions;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Email : ValueObjectBase
    {
        public int EmailID { get; set; }

        public int? UserID { get; set; }

        public int AccountID { get; set; }

        string emailId;
        public string EmailId { 
            get { return emailId; } 
            set { emailId = !string.IsNullOrEmpty(value) ? value.Trim().ToLower() : null; } }

        public bool IsVerified { get; set; }
        public bool IsUpdated { get; set; }
        public string EmailSignature { get; set; }

        public bool IsPrimary { get; set; }
        public bool IsDeleted { get; set; }
        public EmailStatus EmailStatusValue { get; set; }
        public byte EmailSatus { get; set; }
        public DateTime? SnoozeUntil { get; set; }
        public int? ContactID { get; set; }
        public int ServiceProviderID { get; set; }
        public byte MailProviderID { get; set; }
        public int ContactEmailID { get; set; }

        // public string Email { get; set; }


        protected override void Validate()
        {
            if (!IsValidEmail(EmailId))
            {
                AddBrokenRule(Domain.ValueObjects.ValueObjectBusinessRule.EmailIsInvalid);
            }
        }

        //Returns true or false. Checks if the passed in email is a valid email or not
        public bool IsValidEmail(string email)
        {
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";    //Source: http://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
    }
}
