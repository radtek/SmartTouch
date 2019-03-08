using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace SmartTouch.CRM.Domain.User
{
    public class User : EntityBase<int>, IAggregateRoot
    {
        //string userLoginId;
        //public string UserLoginID { get { return userLoginId; } set { userLoginId = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        //string userName;
        //public string UserName { get { return userName; } set { userName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        ////For Auto complete in relation
        //public string Text { get { return userName; } set { userName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }
        //// end

        //New Users starts 
        string firstName;
        public string FirstName { get { return firstName; } set { firstName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string lastName;
        public string LastName { get { return lastName; } set { lastName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string userName;
        public string UserName { get { return userName; } set { userName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        string company;
        public string Company { get { return company; } set { company = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        public bool Status { get; set; }

        public IEnumerable<Address> Addresses { get; set; }
        public Phone WorkPhone { get; set; }
        public Phone HomePhone { get; set; }
        public Phone MobilePhone { get; set; }

        public IEnumerable<Email> SecondaryEmails { get; set; }
        public Role Role { get; set; }
        public Int16 RoleID { get; set; }

        Url facebookUrl;
        public Url FacebookUrl
        {
            get { return facebookUrl; }
            set
            {
                string plainUrl = value != null && !string.IsNullOrEmpty(value.URL) ? value.URL.Trim() : null;
                facebookUrl = plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, "facebook.com", true) : null;
            }
        }

        Url twitterUrl;
        public Url TwitterUrl
        {
            get { return twitterUrl; }
            set
            {
                string plainUrl = value != null && !string.IsNullOrEmpty(value.URL) ? value.URL.Trim() : null;
                twitterUrl = plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, "twitter.com", true) : null;
            }
        }

        Url googlePlusUrl;
        public Url GooglePlusUrl
        {
            get { return googlePlusUrl; }
            set
            {
                string plainUrl = value != null && !string.IsNullOrEmpty(value.URL) ? value.URL.Trim() : null;
                googlePlusUrl = plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, "plusgoogle.com", true) : null;
            }
        }

        Url linkedInUrl;
        public Url LinkedInUrl
        {
            get { return linkedInUrl; }
            set
            {
                string plainUrl = value != null && !string.IsNullOrEmpty(value.URL) ? value.URL.Trim() : null;
                linkedInUrl = plainUrl != null ? CorrectTheUrl(new Url() { URL = plainUrl }, "linkedin.com", true) : null;
            }
        }

        public Url BlogUrl { get; set; }
        public Url WebsiteUrl { get; set; }
        public Email Email { get; set; }

        protected override void Validate()
        {
         
            if (string.IsNullOrEmpty(FirstName))
            {
                AddBrokenRule(UserBusinessRule.UserFirstNameRequired);
            }

            if (string.IsNullOrEmpty(LastName))
            {
                AddBrokenRule(UserBusinessRule.UserLastNameRequired);
            }
            if (!string.IsNullOrEmpty(Email.EmailId) && !IsValidEmail(Email.EmailId))
            {
                AddBrokenRule(UserBusinessRule.UserPrimaryEmailRequired);
            }
           
        }

        public Url CorrectTheUrl(Url url, string domain, bool secured)
        {
            if (!string.IsNullOrEmpty(url.URL) && url.URL.Contains(domain))
            {
                var urlIndex = url.URL.IndexOf(domain);
                url.URL = (secured ? "https://" : "http://") + url.URL.Substring(urlIndex, url.URL.Length - urlIndex);
                return url;
            }
            else
                if (!string.IsNullOrEmpty(url.URL) && !url.URL.Contains(domain))
                {
                    return url;
                }
                else
                    return null;
        }

        //Returns true or false. Checks if the passed in email is a valid email or not
        public bool IsValidEmail(string email)
        {
            bool isValidEmail = false;
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";    //Source: http://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return isValidEmail = regex.IsMatch(email);
        }
    }
}
