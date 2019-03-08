using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Dropdowns
{
    public interface IDropdownRepository : IRepository<Dropdown, int>
    {
        IEnumerable<Dropdown> GetAllDropdownValues();

        IEnumerable<Dropdown> FindAll(string name, int limit, int pageNumber, int accountId);

        IEnumerable<Dropdown> FindAll(string name, int? accountId);

        IEnumerable<OpportunityGroups> GetOppoertunityStageGroups();

        Dropdown FindBy(int DropdownID, int AccountID);

        Dropdown FindDropdownBy(byte dropdownId);

        IEnumerable<DropdownValue> GetLeadSources(int dropdownID, int accountId);

        void InsertDefaultDropdownValues(int accountId);

        void InsertDefaultOpportunityStageGroups(int accountId);

        string GetDropdownFieldValueBy(Int16 dropdownValueID);

        DropdownValue GetDropdownValue(Int16 dropdownValueID);
    }
}
