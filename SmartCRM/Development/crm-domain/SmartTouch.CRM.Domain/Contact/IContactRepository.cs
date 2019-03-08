using System.Collections.Generic;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Contact
{
    public interface IContactRepository : IRepository<Contact, int>
    {
        IEnumerable<Contact> FindAll(string name);
        IEnumerable<Contact> FindPersons(string name);
        IEnumerable<Contact> FindCompanies(string name);

        IEnumerable<State> GetStates(string countryCode);
        IEnumerable<Country> GetCountries();
        ContactType GetContactType(int contactId);
        bool IsDuplicatePerson(string firstName, string lastName, string primaryEmail, string company, int contactId);
        bool IsDuplicateCompany(string companyName, int contactId);

        bool DeactivateContact(int contactId);
        IList<dynamic> FindTimelines(int contactId, int limit, int pageNumber, string module, string period);
        int FindTimelinesTotalRecords(int contactId, string module, string period);
        void DeleteRelation(int relationId);
        IEnumerable<Contact> FetchImages(IEnumerable<Contact> lstContact);
    }
}
