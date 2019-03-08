using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface IAddressViewModel
    {
        int AddressID { get; set; }
        short AddressTypeID { get; set; }
        string AddressLine1 { get; set; }
        string AddressLine2 { get; set; }
        string City { get; set; }
        State State { get; set; }
        Country Country { get; set; }
        string ZipCode { get; set; }
        bool IsDefault { get;set; }
        string AddressType { get; set; }
    }

    public class AddressViewModel //: IAddressViewModel
    {
        public int AddressID { get; set; }
        public short AddressTypeID { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public State State { get; set; }
        public Country Country { get; set; }
        public string ZipCode { get; set; }
        public bool IsDefault { get; set; }
        public IEnumerable<dynamic> States { get; set; }
        public IEnumerable<dynamic> Countries { get; set; }

        public override string ToString()
        {
            string addressToString = AddressLine1 + ", " + AddressLine2 + ", " + City + ", " 
                + (State!=null ? (string.IsNullOrEmpty(State.Name)?State.Code:State.Name): "") + " - " + ZipCode + ", " + 
                (Country!=null ?(string.IsNullOrEmpty(Country.Name)?Country.Code:Country.Name ): "");
            addressToString = addressToString.Replace(" ,","");
            var array = new char[] { ',',' '};
            addressToString = addressToString.TrimStart(array).TrimEnd(array);
            return addressToString;
        }

        //public string Address { get { return (this != null) ? ((AddressLine1 == null) ? string.Empty : this.AddressLine1) + ", " + ((this.AddressLine2 == null) ? string.Empty : this.AddressLine2) + ", " + ((this.City == null) ? string.Empty : this.City) + ", " + ((this.State.Name == null) ? null : this.State.Name) + ", " + ((this.Country.Name == null) ? null : this.Country.Name) + "" + ((this.ZipCode == null) ? string.Empty : "-" + this.ZipCode) : "No Address"; } }
        public string AddressType { get; set; }
    }
}
