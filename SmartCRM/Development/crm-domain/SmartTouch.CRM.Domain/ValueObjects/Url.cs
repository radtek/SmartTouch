
using SmartTouch.CRM.Infrastructure.Domain;
using System.Text.RegularExpressions;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Url : ValueObjectBase
    {
        string url;
        public string MediaType { get; set; }

        public string URL
        {
            get { return url; }
            set
            {
                string plainUrl = !string.IsNullOrEmpty(value) ? value.Trim() : null;
                if (!string.IsNullOrEmpty(plainUrl) && (plainUrl.StartsWith("http://") || plainUrl.StartsWith("https://")))
                    url = plainUrl;
                else
                {
                    if (!string.IsNullOrEmpty(plainUrl))
                        url = "http://" + plainUrl;
                    else
                        url = plainUrl;
                }
            }
        }

        protected override void Validate()
        {
            if (!string.IsNullOrEmpty(URL) && !IsValidURL())
            {               
               AddBrokenRule(Contacts.ContactBusinessRule.ContactWebURLInvalid);              
            }
        }

        public bool IsValidURL()
        {
            string pattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";

            bool result = Regex.IsMatch(this.URL.ToLower(), pattern);
            return result;
        }
    }
}
