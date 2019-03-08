using SmartTouch.CRM.Infrastructure.Domain;
using System;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class TaxRate 
    {        
        public long TaxRateId { get; set; }
        public string ZIPCode { get; set; }
        public string CountyName { get; set; }
        public string StateCode { get; set; }
        public string CityName { get; set; }
        public double? CountySalesTax { get; set; }
        public double? CountyUseTax { get; set; }
        public double? StateSalesTax { get; set; }
        public double? StateUseTax { get; set; }
        public double? CitySalesTax { get; set; }
        public double? CityUseTax { get; set; }
        public double? TotalSalesTax { get; set; }
        public double? TotalUseTax { get; set; }
        public bool? TaxShippingAlone { get; set; }
        public bool? ShippingAndHandlingTax { get; set; }
        public long CountryID { get; set; }
        public string CountryName { get; set; }

    }
}
