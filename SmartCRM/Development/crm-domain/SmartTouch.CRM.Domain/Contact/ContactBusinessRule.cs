
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Contact
{
    public static class ContactBusinessRule
    {
        public static readonly BusinessRule ContactImageTypeInvalid = new BusinessRule("Please upload an image with  jpeg or jpg or bmp or png extension.");
        public static readonly BusinessRule ContactFirstNameRequired = new BusinessRule("First Name is required.");
        public static readonly BusinessRule ContactLastNameRequired = new BusinessRule("Last Name is required");
        public static readonly BusinessRule ContactFacebookUrlInvalid = new BusinessRule("Facebook URL is invalid. Please check if the URL starts with https://facebook.com");
        public static readonly BusinessRule ContactWebURLInvalid = new BusinessRule("Web URL provided is invalid.");
        public static readonly BusinessRule ContactTwitterUrlInvalid = new BusinessRule("Twitter URL is invalid. Please check if the URL starts with https://twitter.com");
        public static readonly BusinessRule ContactGooglePlusUrlInvalid = new BusinessRule("GooglePlus URL is invalid. Please check if the URL starts with https://plus.google.com");
        public static readonly BusinessRule ContactLinkedInUrlInvalid = new BusinessRule("LinkedIn URL is invalid. Please check if the URL starts with https://linkedin.com");
        public static readonly BusinessRule ContactBlogInUrlInvalid = new BusinessRule("Blog URL is invalid.");
        public static readonly BusinessRule ContactAddtionalWebOrSocialLink = new BusinessRule("The URL field should not be empty when 'Add Web & Social links' is clicked");
        public static readonly BusinessRule ContactPrimaryEmailRequired = new BusinessRule("Email is invalid");
        public static readonly BusinessRule ContactEmailShouldBeUnique = new BusinessRule("A contact can have only a list of unique emails");
        public static readonly BusinessRule ContactSsnAndSinIsInvalid = new BusinessRule("Enter a valid 9 digit SSN or SIN");
        public static readonly BusinessRule ContactLastContactedDateInvalid = new BusinessRule("Last contacted date cannot be a future date.");
    }
}
