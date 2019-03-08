using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Geo
{
    public class GetCountriesAndStatesRequest : ServiceRequestBase
    {
    }

    public class GetCountriesAndStatesResponse : ServiceResponseBase
    {
        public IEnumerable<State> States { get; set; }
        public IEnumerable<Country> Countries { get; set; }
        public IEnumerable<FieldValueOption> StatesAsValueOptions { get; set; }
        public IEnumerable<FieldValueOption> CountriesAsValueOptions { get; set; }
    }
}
