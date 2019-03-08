
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public static class ValueObjectBusinessRule
    {
        /*Address*/
        public static readonly BusinessRule CountryInAddressRequired = new BusinessRule("[|An address must have a country selected.\r\n|]");
        public static readonly BusinessRule StateInAddressRequired = new BusinessRule("[|An address must have a State selected.\r\n|]");
        public static readonly BusinessRule ZipCodeFormatInvalid = new BusinessRule("[|Zip code is invalid.\r\n|]");         //Reference: http://en.wikipedia.org/wiki/List_of_postal_codes
        public static readonly BusinessRule PostalCodeFormatInvalid = new BusinessRule("[|Postal code is invalid.\r\n|]");   //Reference: http://en.wikipedia.org/wiki/List_of_postal_codes

        /*Phone*/
        public static readonly BusinessRule PhoneNumberMinimumLength = new BusinessRule("[| Phone number is invalid.\r\n|]");
        public static readonly BusinessRule PhoneNumberFormat = new BusinessRule("[| Phone number is invalid.\r\n|]");


        /*Email*/
        public static readonly BusinessRule EmailIsInvalid = new BusinessRule("[| Email is invalid.\r\n|]");

        /*Url*/
        public static readonly BusinessRule UrlIsInvalid = new BusinessRule("[| Url is invalid.\r\n|]");
                
        /*Number*/
        public static readonly BusinessRule NumberIsInvalid = new BusinessRule("[| Number is Invalid.\r\n|]");

        /*Date*/
        public static readonly BusinessRule DateInvalid = new BusinessRule("[|Date is Invalid|]");

        /*Time*/
        public static readonly BusinessRule TimeInvalid = new BusinessRule("[|Time is Invalid|]");

        /*Reminder*/
        public static readonly BusinessRule ReminderMethodRequired = new BusinessRule("[| Reminder is required.\r\n|]");
        public static readonly BusinessRule ReminderTimeframeRequired = new BusinessRule("[| Reminder Timeframe is required.\r\n|]");
        public static readonly BusinessRule DateandTimeRequired = new BusinessRule("[| Date and Time are required.\r\n|]");
        public static readonly BusinessRule DateandTimeInvalid = new BusinessRule("[| DateTime are invalid.\r\n|]");

        /*Images*/
        public static readonly BusinessRule IamgeTypeFormat = new BusinessRule("[|Image format invalid.Only support .jpeg,.jpg,.png.\r\n|]");

        /* Contact*/
        public static readonly BusinessRule ContactNotValid = new BusinessRule("[|Required Fields missing.\r\n|]");   //Reference: http://en.wikipedia.org/wiki/List_of_postal_codes
        public static readonly BusinessRule InvalidNumber = new BusinessRule("[|Invalid Number.\r\n|]");
        public static readonly BusinessRule MaxAddressLength = new BusinessRule("[|Max length allowed in address can not be more than 95 characters|]");
        public static readonly BusinessRule MaxCityLength = new BusinessRule("[|Max length allowed in city can not be more than 50 characters|]");
        public static readonly BusinessRule MaxZipLength = new BusinessRule("[|Max length allowed in zip code can not be more than 11 characters|]");


    }
}
