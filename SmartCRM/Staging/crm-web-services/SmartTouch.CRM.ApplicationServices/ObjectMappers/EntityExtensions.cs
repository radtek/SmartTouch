using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.ObjectMappers
{
    public static class EntityExtensions
    {
        public static void ApplyNation(this Address address, TaxRate taxRate)
        {
            if (taxRate != null && string.IsNullOrEmpty(address.Country.Code) && string.IsNullOrEmpty(address.State.Code) && string.IsNullOrEmpty(address.City) && !string.IsNullOrEmpty(address.ZipCode))
            {
                string countryCode = taxRate.CountryID == 1 ? "US" : "CA";
                Country cntry = new Country()
                {
                    Code = countryCode.Trim()
                };
                string stateCode = taxRate.CountryID == 1 ? "US-" + taxRate.StateCode : "CA-" + taxRate.StateCode;
                State state = new State()
                {
                    Code = stateCode.Trim()
                };
                address.City = taxRate.CityName;
                address.State = state;
                address.Country = cntry;
            }
        }
    }
}
