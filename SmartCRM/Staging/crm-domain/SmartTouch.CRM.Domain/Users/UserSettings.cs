using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;


namespace SmartTouch.CRM.Domain.Users
{
    public class UserSettings : EntityBase<int>, IAggregateRoot
    {
        public int AccountID { get; set; }
        public int UserID { get; set; }    
        public string CountryID { get; set; }
        public string TimeZone { get; set; }       
        public byte? CurrencyID { get; set; }
        public string EmailID { get; set; }
        public short ItemsPerPage { get; set; }
        public byte? DateFormat { get; set; }
        public bool DailySummary { get; set; }
        public bool AlertNotification { get; set; }
        public bool EmailNotification { get; set; }
        public bool TextNotification { get; set; }
        public bool LeadScoreNotification { get; set; }
        public int LeadScoreValue { get; set; }
        public bool HasAcceptedTC { get; set; }
        public bool IsIncludeSignature { get; set; }

        public string currentPassword { get; set; }
        public string newPassword { get; set; }
        public string confirmPassword { get; set; }
        public string CurrencyFormat { get; set; }
        public string DateFormatType { get; set; }
        public IEnumerable<Email> Emails { get; set; }

        protected override void Validate()
        {
            if (!string.IsNullOrEmpty(newPassword) && newPassword != confirmPassword && !string.IsNullOrEmpty(currentPassword))
            {
                AddBrokenRule(UserBusinessRule.PasswordsDoNotMatch);
            }

            if (!string.IsNullOrEmpty(newPassword) && !IsValidPassword(newPassword)) {
                AddBrokenRule(UserBusinessRule.PasswordFormat);
            }
            if (AlertNotification == false && (EmailNotification == true || TextNotification == true) || AlertNotification == true && (EmailNotification == false && TextNotification == false))
            {
                AddBrokenRule(UserBusinessRule.AlertNotificationInvalid);
            }
            if (AlertNotification == false && LeadScoreValue > 0)
            {
                AddBrokenRule(UserBusinessRule.InvalidLeadscoreConfiguration);
            }
            if (!IsValidLeadScore(LeadScoreValue))
            {
                AddBrokenRule(UserBusinessRule.InvalidLeadScore);
            }
        }

        public bool IsValidLeadScore(int leadScore)
        {
            return leadScore < 0 ? false : true;
        }

        public bool IsValidPassword(string _newPassword)
        {
            string pattern = @"^(?=.*?[A-Z])(?=(.*[a-z]){1,})(?=(.*[\d]){1,})(?=(.*[\W_]){1,})(?!.*\s).{6,}$";  
            Regex regex = new Regex(pattern);
            return regex.IsMatch(_newPassword);
        }

        public bool CheckPassword(string _currentPassword) {           
            return true;
        }

    }
}
