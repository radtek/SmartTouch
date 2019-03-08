
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.ValueObjects
{ 
    public class Address:ValueObjectBase
    {
        public int AddressID { get; set; }
        
        string addressLine1;
        public string AddressLine1 { get { return addressLine1; } set { addressLine1 = !string.IsNullOrEmpty(value)?value.Trim():null;  } }

        string addressLine2;
        public string AddressLine2 { get { return addressLine2; } set { addressLine2 = !string.IsNullOrEmpty(value)?value.Trim():null;  } }

        string city;
        public string City { get { return city; } set { city = !string.IsNullOrEmpty(value)?value.Trim():null;  } }

        string zipCode;
        public string ZipCode { get { return zipCode; } set { zipCode = !string.IsNullOrEmpty(value)?value.Trim():null;  } }


        public State State { get; set ; }
        public Country Country { get; set; }

        public bool IsDefault { get; set; }

        public short AddressTypeID { get; set; }

        public string StateID { get; set; }
        public string CountryID { get; set; }

        public override string ToString()
        {
            string addressToString = AddressLine1 + ", " + AddressLine2 + ", " + City + ", "
                + (State != null ? State.Name : "") + " " + (ZipCode != null ? ZipCode : ",") + ", " + (Country != null ? Country.Name : "");
            addressToString = addressToString.Replace(" ,", "");
            var array = new char[] { ',', ' ' };
            addressToString = addressToString.TrimStart(array).TrimEnd(array);
            return addressToString;
        }

        protected override void Validate()
        {
            if (!string.IsNullOrEmpty(AddressLine1) && AddressLine1.Length > 95) AddBrokenRule(ValueObjectBusinessRule.MaxAddressLength);
            if (!string.IsNullOrEmpty(AddressLine2) && AddressLine2.Length > 95) AddBrokenRule(ValueObjectBusinessRule.MaxAddressLength);
            if (!string.IsNullOrEmpty(City) && City.Length > 50) AddBrokenRule(ValueObjectBusinessRule.MaxCityLength);
            if (!string.IsNullOrEmpty(ZipCode) && ZipCode.Length > 50) AddBrokenRule(ValueObjectBusinessRule.MaxZipLength);
        }

        //Returns false if Country is blank or null
        public bool IsValidCountry()
        {
            return (this.Country.Code == null || this.Country.Code=="") ? false : true;
        }

        public bool IsValidState()
        {
            return (this.State.Code == null || this.State.Code == "") ? false : true;
        }
    }
}
