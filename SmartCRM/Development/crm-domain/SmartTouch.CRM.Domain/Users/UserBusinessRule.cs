
using SmartTouch.CRM.Infrastructure.Domain;
namespace SmartTouch.CRM.Domain.Users
{
  public static class UserBusinessRule
    {
        public static readonly BusinessRule UserFirstNameRequired = new BusinessRule("[|First Name is required.|]\r\n");
        public static readonly BusinessRule UserLastNameRequired = new BusinessRule("[|Last Name is required.|]\r\n");

        public static readonly BusinessRule UserFacebookUrlInvalid = new BusinessRule("[|Facebook URL is invalid. Please check if the URL starts with|] https://facebook.com/.\r\n");
        public static readonly BusinessRule UserWebURLInvalid = new BusinessRule("[|Web URL provided is invalid.|]");
        public static readonly BusinessRule UserTwitterUrlInvalid = new BusinessRule("[|Twitter URL is invalid. Please check if the URL starts with|] https://twitter.com/.\r\n");
        public static readonly BusinessRule UserGooglePlusUrlInvalid = new BusinessRule("[|GooglePlus URL is invalid. Please check if the URL starts with|] https://plus.google.com/.\r\n");
        public static readonly BusinessRule UserLinkedInUrlInvalid = new BusinessRule("[|LinkedIn URL is invalid. Please check if the URL starts with|] https://linkedin.com/.\r\n");
        public static readonly BusinessRule UserBlogInUrlInvalid = new BusinessRule("[|Blog URL is invalid.|]\r\n");
        public static readonly BusinessRule UserAddtionalWebOrSocialLink = new BusinessRule("[|The URL field should not be empty when 'Add Web & Social links' is clicked.|]\r\n");
        public static readonly BusinessRule UserPrimaryEmailRequired = new BusinessRule("[|Email is invalid.|]\r\n");
        public static readonly BusinessRule UserEmailShouldBeUnique = new BusinessRule("[|A contact can have only a list of unique emails.|]\r\n");
        public static readonly BusinessRule UserSsnAndSinIsInvalid = new BusinessRule("[|Enter a valid 9 digit SSN or SIN.|]\r\n");

        public static readonly BusinessRule PasswordsDoNotMatch = new BusinessRule("[|Entered passwords do not match.|]\r\n");
        public static readonly BusinessRule WrongPassword = new BusinessRule("[|Your password is incorrect.|]\r\n");
        public static readonly BusinessRule PasswordFormat = new BusinessRule("[|Password must contain one uppercase letter,one lowercase letter, one number, one special character and minimum 6 characters required.|]\r\n");

        public static readonly BusinessRule AlertNotificationInvalid = new BusinessRule("[|Enter valid Alert Notification settings.|]\r\n");
        public static readonly BusinessRule InvalidLeadscoreConfiguration = new BusinessRule("[|Check alert notifications and input valid Leadscore.|]\r\n");
        public static readonly BusinessRule InvalidLeadScore = new BusinessRule("[|Invalid Leadscore.|]\r\n");
    }
}
