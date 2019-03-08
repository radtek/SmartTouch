
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Accounts
{
    public static class AccountBusinessRule
    {
        public static readonly BusinessRule AccountNameRequired = new BusinessRule("[|Account Name is required.|]\r\n");
        public static readonly BusinessRule AccountFirstNameRequired = new BusinessRule("[|First Name is required.|]\r\n");
        public static readonly BusinessRule AccountLastNameRequired = new BusinessRule("[|Last Name is required.|]\r\n");
        public static readonly BusinessRule AccountFacebookUrlInvalid = new BusinessRule("[|Facebook URL is invalid. Please check if the URL starts with|] https://facebook.com/.\r\n");
        public static readonly BusinessRule AccountWebURLInvalid = new BusinessRule("[|Web URL provided is invalid.\r\n|]");
        public static readonly BusinessRule AccountTwitterUrlInvalid = new BusinessRule("[|Twitter URL is invalid. Please check if the URL starts with|] https://twitter.com/.\r\n");
        public static readonly BusinessRule AccountGooglePlusUrlInvalid = new BusinessRule("[|GooglePlus URL is invalid. Please check if the URL starts with|] https://plus.google.com/.\r\n");
        public static readonly BusinessRule AccountLinkedInUrlInvalid = new BusinessRule("[|LinkedIn URL is invalid. Please check if the URL starts with|] https://linkedin.com/.\r\n");
        public static readonly BusinessRule AccountBlogInUrlInvalid = new BusinessRule("[|Blog URL is invalid.|]\r\n");
        public static readonly BusinessRule AccountAddtionalWebOrSocialLink = new BusinessRule("[|The URL field should not be empty when 'Add Web & Social links' is clicked.|]\r\n");
        public static readonly BusinessRule AccountPrimaryEmailRequired = new BusinessRule("[|Email is invalid.|]\r\n");
        public static readonly BusinessRule AccountEmailShouldBeUnique = new BusinessRule("[|A account can have only a list of unique emails.|]\r\n");
        public static readonly BusinessRule AccountDomainNameRequired = new BusinessRule("[|Domain name is required.|]\r\n");
        public static readonly BusinessRule AccountDomainNameInvalid = new BusinessRule("[|Invalid domain name.|]\r\n");
        public static readonly BusinessRule HelpURLIsRequired = new BusinessRule("[|Help URL is required.|]");
        public static readonly BusinessRule HelpURLIsInvalid = new BusinessRule("[|Help URL is Invalid.|]");
    }
}
