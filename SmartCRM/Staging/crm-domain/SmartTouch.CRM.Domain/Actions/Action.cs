using System;
using System.Collections.Generic;
using System.Linq;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Actions
{
    public class Action : EntityBase<int>, IAggregateRoot
    {
        private string details;
        public string Details { get { return details; } set { details = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public int? AccountId { get; set; }
        public IEnumerable<ReminderType> ReminderTypes { get; set; }
        public DateTime? RemindOn { get; set; }
        public IList<Contacts.RawContact> Contacts { get; set; }
        public IEnumerable<Tags.Tag> Tags { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? MarkAsCompleted { get; set; }
        public virtual int OppurtunityId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public NotificationStatus? NotificationStatus { get; set; }
        public string ToEmail { get; set; }
        public Guid? EmailRequestGuid { get; set; }
        public Guid? TextRequestGuid { get; set; }
        public string ContactName { get; set; }
        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public bool IcsCanlender { get; set; }
        public bool IcsCanlenderToContact { get; set; }//Added for LIT Deployment on 06/05/2018
        public string ActionTypeValue { get; set; }
        public DateTime ActionDateTime { get; set; }
        //Elastic Search
        public List<ActionContact> ActionContacts { get; set; }

        public short? ActionType { get; set; }
        public DateTime? ActionDate { get; set; }
        public DateTime? ActionStartTime { get; set; }
        public DateTime? ActionEndTime { get; set; }
        public bool SelectAll { get; set; }
        public IList<int> OwnerIds { get; set; }
        public string UserName { get; set; }
        public IEnumerable<int> ContactIDS { get; set; }
        public IDictionary<int, Guid> EmailGuids { get; set; }
        public IDictionary<int, Guid> TextGuids { get; set; }
        public string ActionTemplateHtml { get; set; }
        public int? MailBulkId { get; set; }
        public Guid? GROUPID { get; set; }
        public int TotalCount { get; set; }
        public int ContactId { get; set; }
        public byte ContactType { get; set; }

        /// <summary>
        /// Validates action 
        /// </summary>
        protected override void Validate()
        {
            if (string.IsNullOrEmpty(Details)) AddBrokenRule(ActionBusinessRule.ActionMessageRequired);
            if (Details != null && Details.Length > 1000) AddBrokenRule(ActionBusinessRule.ActionMessageLengthNotExceed1000);
            if ((Contacts == null || Contacts.Count == 0) && SelectAll == false) AddBrokenRule(ActionBusinessRule.ContactsRequired);
            if (RemindOn != null)
            {
                DateTime outResult;
                if (!DateTime.TryParse(RemindOn.ToString(), out outResult)) AddBrokenRule(ActionBusinessRule.RemindOnInvalid);
            }
            DateTime today = DateTime.Now.ToUniversalTime();
            if (RemindOn.HasValue)
            {
                DateTime reminderDate = RemindOn.Value;
                var result = DateTime.Compare(reminderDate, today);
                if (result < 0 && (ReminderTypes.Contains(ReminderType.Email) || ReminderTypes.Contains(ReminderType.TextMessage) || ReminderTypes.Contains(ReminderType.PopUp)))
                    AddBrokenRule(ActionBusinessRule.RemindOndateInvalid);
            }
        }

        public IList<BusinessRule> OutlookValidation()
        {
            var brokenRules = new List<BusinessRule>();
            if (string.IsNullOrEmpty(Details)) brokenRules.Add(ActionBusinessRule.ActionMessageRequired);
            if (Details != null && Details.Length > 1000) brokenRules.Add(ActionBusinessRule.ActionMessageLengthNotExceed1000);
            if (Contacts == null || Contacts.Count == 0) brokenRules.Add(ActionBusinessRule.ContactsRequired);
            if (RemindOn != null)
            {
                DateTime outResult;
                if (!DateTime.TryParse(RemindOn.ToString(), out outResult)) brokenRules.Add(ActionBusinessRule.RemindOnInvalid);
            }
            return brokenRules;
        }
    }

    public class ActionContactsSummary
    {
        public int ContactId { get; set; }
        public ContactType ContactType { get; set; }
        public string ContactName { get; set; }
        public bool Status { get; set; }
        public string PrimaryEmail { get; set; }
        public string PrimaryPhone { get; set; }
        public string Lifecycle { get; set; }
        public string PhoneCountryCode { get; set; }
        public string PhoneExtension { get; set; }
    }

    public class UserActions
    {
        public IEnumerable<Action> Actions { get; set; }
        public int TotalCount { get; set; }
    }
}
