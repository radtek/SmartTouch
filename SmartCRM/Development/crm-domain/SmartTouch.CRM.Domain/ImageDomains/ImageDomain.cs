using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ImageDomains
{
    public class ImageDomain : EntityBase<byte>, IAggregateRoot
    {
        string domain;
        public string Domain
        {
            get { return domain; }
            set
            {
                string plainUrl = !string.IsNullOrEmpty(value) ? value.Trim() : null;
                if (!string.IsNullOrEmpty(plainUrl) && ((plainUrl.StartsWith("http://") || plainUrl.StartsWith("https://"))))
                    domain = plainUrl;
                else
                {
                    if (!string.IsNullOrEmpty(plainUrl)) domain = "http://" + plainUrl;
                    else domain = plainUrl;
                }
            }
        }
        public bool Status { get; set; }
        public bool IsDefault { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        protected override void Validate()
        {
            if (string.IsNullOrEmpty(Domain)) AddBrokenRule(ImageDomainBusinessRule.EmptyImageDomain);
            else if (!IsValidURL()) AddBrokenRule(ImageDomainBusinessRule.InvalidImageDomain);
        }

        public bool IsValidURL()
        {
            string pattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";
            return Regex.IsMatch(this.Domain.ToLower(), pattern);
        }
    }
}
