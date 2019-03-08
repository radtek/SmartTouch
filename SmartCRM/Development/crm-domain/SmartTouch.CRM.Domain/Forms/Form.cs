using System;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.Tags;
using ET = SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Fields;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Domain.Dropdowns;

namespace SmartTouch.CRM.Domain.Forms
{
    public class Form : EntityBase<int>, IAggregateRoot
    {
        public string Name { get; set; }
        public IList<FormField> FormFields { get; set; }
        public IList<CustomField> CustomFields { get; set; }
        public IEnumerable<Tag> Tags { get; set; }

        public ET.AcknowledgementType AcknowledgementType { get; set; }
        string acknowledgement;
        public string Acknowledgement
        {
            get { return acknowledgement; }
            set
            {
                string plainUrl = !string.IsNullOrEmpty(value) ? value.Trim() : null;
                if (AcknowledgementType == ET.AcknowledgementType.Url)
                {
                    if (!string.IsNullOrEmpty(plainUrl) &&
                        ((plainUrl.StartsWith("http://") || plainUrl.StartsWith("https://")))) acknowledgement = plainUrl.ToLower();
                    else if (!string.IsNullOrEmpty(plainUrl)) acknowledgement = "http://" + plainUrl.ToLower();
                    else acknowledgement = string.IsNullOrEmpty(plainUrl) ? string.Empty : plainUrl.ToLower();
                }
                else acknowledgement = plainUrl;
            }
        }

        public string HTMLContent { get; set; }
        public int? Submissions { get; set; }
        public ET.FormStatus Status { get; set; }

        public int UniqueSubmissions { get; set; }
        public int AllSubmissions { get; set; }
        public int AccountID { get; set; }
        public DropdownValue LeadSource { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public bool? IsDeleted { get; set; }
        public bool IsAPIForm { get; set; }

        protected override void Validate()
        {
            if (!string.IsNullOrEmpty(Acknowledgement))
            {
                if (AcknowledgementType == ET.AcknowledgementType.Url && !IsValidURL()) AddBrokenRule(FormBusinessRules.InvalidAcknowledgmentUrl);
            }
            else if (string.IsNullOrEmpty(Name))
                AddBrokenRule(FormBusinessRules.FormNameCannotBeEmpty);
            else AddBrokenRule(FormBusinessRules.AcknowledgementCannotBeEmpty);
        }

        public bool IsValidURL()
        {
            string pattern = @"^^((((ht|f)tp(s?))\:\/\/)|(www.))?[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$";
            return Regex.IsMatch(this.Acknowledgement.ToLower(), pattern);
        }
    }
}
