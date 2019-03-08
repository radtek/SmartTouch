using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging.Geo;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class GeoService : IGeoService
    {
        IContactRepository repository;
        public GeoService(IContactRepository repository)
        {
            this.repository = repository;
        }

        public GetCountriesResponse GetCountries(GetCountriesRequest request)
        {
            GetCountriesResponse response = new GetCountriesResponse();
            IEnumerable<dynamic> countries;

            countries = repository.GetCountries().ToList();
            if (countries == null)
                throw new ResourceNotFoundException("The requested countries list was not found.");
            response.Countries = countries;
            return response;
        }

        public GetStatesResponse GetStates(GetStatesRequest request)
        {
            GetStatesResponse response = new GetStatesResponse();
            IEnumerable<dynamic> states;
            states = repository.GetStates(request.CountryCode);
            if (states == null)
                throw new ResourceNotFoundException("The requested states list was not found.");
            response.States = states;
            return response;
        }

        public GetAllStatesResponse GetAllStates(GetAllStatesRequest request)
        {
            GetAllStatesResponse response = new GetAllStatesResponse();
            IEnumerable<State> states;

            states = repository.GetAllStates();
            if (states == null)
                throw new ResourceNotFoundException("The requested states list was not found.");
            response.States = states;
            return response;
        }

        public GetCountriesAndStatesResponse GetCountriesAndStates(GetCountriesAndStatesRequest request)
        {
            GetCountriesAndStatesResponse response = new GetCountriesAndStatesResponse();
            response.States = repository.GetAllStates();
            response.Countries = repository.GetCountries();
            return response;
        }
    }
}
