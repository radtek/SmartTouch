using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{

    public interface IUserSettingsViewModel
    {
        int UserSettingId { get; set; }
        int AccountId { get; set; }
        int UserId { get; set; }
        string currentPassword { get; set; }
        string newPassword { get; set; }
        string confirmPassword { get; set; }
        short ItemsPerPage { get; set; }
        string CountryId { get; set; }
        byte CurrencyID { get; set; }
        string TimeZone { get; set; }
        string EmailId { get; set; }
        byte DateFormat { get; set; }
        bool DailySummary { get; set; }
        bool AlertNotification { get; set; }
        bool LeadScoreNotification { get; set; }
        int LeadScoreValue { get; set; }
        bool EmailNotification { get; set; }
        bool TextNotification { get; set; }
        bool HasAcceptedTC { get; set; }

        IEnumerable<Email> Emails { get; set; }
        string EmailSignature { get; set; }
    }

    public class UserSettingsViewModel : IUserSettingsViewModel
    {
        public UserSettingsViewModel()
        {
            //Get the list of communities. This has to be developed once service layer for communities is created.
        }

        public virtual int UserSettingId { get; set; }
        public virtual int AccountId { get; set; }
        public virtual int UserId { get; set; }
        public virtual string currentPassword { get; set; }
        public virtual string newPassword { get; set; }
        public virtual string confirmPassword { get; set; }
        public virtual short ItemsPerPage { get; set; }
        public virtual string CountryId { get; set; }
        public virtual byte CurrencyID { get; set; }
        public virtual string TimeZone { get; set; }
        public virtual string EmailId { get; set; }
        public virtual byte DateFormat { get; set; }
        public virtual string EmailSignature { get; set; }
        public virtual bool DailySummary { get; set; }
        public virtual bool AlertNotification { get; set; }
        public bool HasAcceptedTC { get; set; }
        public bool IsIncludeSignature { get; set; }

        public bool EmailNotification { get; set; }
        public bool TextNotification { get; set; }
        public virtual bool LeadScoreNotification { get; set; }
        public virtual int LeadScoreValue { get; set; }
        public virtual IEnumerable<Email> Emails { get; set; }
    }
}
