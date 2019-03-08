using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging.Geo;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IGeoService
    {
        GetCountriesResponse GetCountries(GetCountriesRequest request);
        GetStatesResponse GetStates(GetStatesRequest request);
        GetAllStatesResponse GetAllStates(GetAllStatesRequest request);
        GetCountriesAndStatesResponse GetCountriesAndStates(GetCountriesAndStatesRequest request);
    }
}
