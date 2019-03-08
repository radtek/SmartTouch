using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum ExportFieldTypes : byte
    {
        [Description("FirstName")]
        FirstName = 1,
        [Description("LastName")]
        LastName = 2,
        [Description("Name")]
        FullName = 3,
        [Description("Email")]
        Email = 4,
        [Description("Company")]
        Company = 5,
        [Description("Phone")]
        Phonenumber = 6,
        [Description("Life cycle")]
        Lifecycle = 14,
        [Description("Last contacted")]
        LastContacted = 15,
        [Description("PhoneType")]
        PhoneType = 7,
        [Description("LastTouchedMethod")]
        LastTouchedMethod = 16,
        [Description("Address Line1")]
        AddressLine1 = 8,
        [Description("Address Line2")]
        AddressLine2 = 9,
        [Description("City")]
        City = 10,
        [Description("State ")]
        State = 11,
        [Description("Country ")]
        Country = 12,
        [Description("Zip Code")]
        ZipCode = 13,
        [Description("CreatedDate")]
        UpdatedDate = 17
    }
}
