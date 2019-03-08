
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Contacts
{
    public static class ContactBusinessRule
    {
        public static readonly BusinessRule ContactImageTypeInvalid = new BusinessRule("[|Please upload an image with  jpeg or jpg or bmp or png extension.\r\n|]");
        public static readonly BusinessRule ContactFirstNameAndLastNameRequired = new BusinessRule("[|Email or First and Last names are required.\r\n|]");
        public static readonly BusinessRule ContactLastNameRequired = new BusinessRule("[|Last Name is required.\r\n|]");
        public static readonly BusinessRule ContactFacebookUrlInvalid = new BusinessRule("[|Facebook URL is invalid. Please check if the URL starts with https://facebook.com/.\r\n|]");
        public static readonly BusinessRule ContactWebURLInvalid = new BusinessRule("[|Web URL provided is invalid.\r\n|]");
        public static readonly BusinessRule ContactTwitterUrlInvalid = new BusinessRule("[|Twitter URL is invalid. Please check if the URL starts with https://twitter.com/.\r\n|]");
        public static readonly BusinessRule ContactGooglePlusUrlInvalid = new BusinessRule("[|GooglePlus URL is invalid. Please check if the URL starts with https://plus.google.com/.\r\n|]");
        public static readonly BusinessRule ContactLinkedInUrlInvalid = new BusinessRule("[|LinkedIn URL is invalid. Please check if the URL starts with https://linkedin.com/.\r\n|]");
        public static readonly BusinessRule ContactBlogInUrlInvalid = new BusinessRule("[|Blog URL is invalid.\r\n|]");
        public static readonly BusinessRule ContactAddtionalWebOrSocialLink = new BusinessRule("[|The URL field should not be empty when 'Add Web & Social links' is clicked.\r\n|]");
        public static readonly BusinessRule ContactPrimaryEmailRequired = new BusinessRule("[|Email is invalid.\r\n|]");
        public static readonly BusinessRule ContactEmailShouldBeUnique = new BusinessRule("[|A contact can have only a list of unique emails|.\r\n]");
        public static readonly BusinessRule ContactSsnAndSinIsInvalid = new BusinessRule("[|Enter a valid 9 digit SSN or SIN.\r\n|]");
        public static readonly BusinessRule ContactFirstNameLength = new BusinessRule("[|First Name should not exceed 75 characters|]");
        public static readonly BusinessRule ContactLastNameLength = new BusinessRule("[|Last Name should not exceed 75 characters|]");
    }
}
