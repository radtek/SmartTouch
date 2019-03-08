using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System.Text.RegularExpressions;
using System;
using System.Text;
using System.Linq;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Phone : ValueObjectBase
    {
        public int ContactPhoneNumberID { get; set; }
        public int ContactID { get; set; }
        public int AccountID { get; set; }
        string countryCode { get; set; }
        public string CountryCode
        {
            get
            {
                return countryCode;
            }
            set
            {
                countryCode = !string.IsNullOrEmpty(value) ? value.Trim() : null;
                if (!string.IsNullOrEmpty(value))
                {
                    MatchCollection matches = Regex.Matches(value, @"[0-9]");
                    var sb = new StringBuilder();
                    foreach (Match match in matches) sb.Append(match.Value);
                    countryCode = sb.ToString();
                }
            }
        }
        string extension { get; set; }
        public string Extension
        {
            get
            {
                return extension;
            }
            set
            {
                extension = !string.IsNullOrEmpty(value) ? value.Trim() : null;
                if (!string.IsNullOrEmpty(value))
                {
                    MatchCollection matches = Regex.Matches(value, @"[0-9]");
                    var sb = new StringBuilder();
                    foreach (Match match in matches) sb.Append(match.Value);
                    extension = sb.ToString();
                }
            }
        }

        string number;
        public string Number
        {
            get
            {
                return number;
            }
            set
            {
                number = !string.IsNullOrEmpty(value) ? value.Trim() : null;
                if (!string.IsNullOrEmpty(value))
                {
                    MatchCollection matches = Regex.Matches(value, @"[0-9]");
                    var sb = new StringBuilder();
                    foreach (Match match in matches) sb.Append(match.Value);
                    number = sb.ToString();
                }
            }
        }

        public Int16 PhoneType { get; set; }
        public string PhoneTypeName { get; set; }

        public bool IsPrimary { get; set; }
        public bool IsDeleted { get; set; }
        public Int16 DropdownValueTypeID { get; set; }

        protected override void Validate()
        {
            if (!string.IsNullOrEmpty(Number) && !IsValidPhoneNumberLength(Number))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberMinimumLength);
            }
            if (string.IsNullOrEmpty(Number) && (!string.IsNullOrEmpty(countryCode) || !string.IsNullOrEmpty(extension)))
            {
                AddBrokenRule(ValueObjects.ValueObjectBusinessRule.PhoneNumberFormat); 
            }
        }

        //Returns true or false. Checks if the minimum length of the passed in phone number is 10 or not.
        public bool IsValidPhoneNumberLength(string phoneNumber)
        {
            MatchCollection matches = Regex.Matches(phoneNumber, @"[0-9]");
            string extractedNumber = "";
            var sb = new StringBuilder();
            foreach (Match match in matches) sb.Append(match.Value);
            extractedNumber = sb.ToString().TrimStart('0', '1');
            int phoneNumberLength = extractedNumber.Length;
            return (phoneNumberLength < 10 && phoneNumberLength > 15) ? false : true;
        }

    }
}
