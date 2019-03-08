using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class AdvancedViewViewModel
    {
        public int AccountID { get; set; }
        public IEnumerable<int> SelectedFields { get; set; }
        public string SearchName { get; set; }
        public int EntityId { get; set; }
        public short EntityType { get; set; }
        public string SearchDescription { get; set; }

        public bool IsPreconfigured { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsDynamicGrid { get; set; }

        public ContactShowingFieldType ShowingType { get; set; }  //EntityType
        public string SearchText { get; set; }
        public int ItemsPerPage { get; set; }
        public int PageNumber { get; set; }
        public bool HasEmailPermission { get; set; }
        public bool IsAccountAdmin { get; set; }
        public IEnumerable<FieldViewModel> SearchFields { get; set; }
        public string SortField { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public IEnumerable<CustomFieldValueOptionViewModel> CustomFieldValueOptions { get; set; }

        public short ContactType { get; set; }
        public ContactSortFieldType SortBy { get; set; }

        public IEnumerable<LeadAdapterType> LeadAdapterTypes { get; set; }
        public bool IsSavedSearch { get; set; }
        public bool IsNewContactsSearch { get; set; }
        public Guid Guid { get; set; }
        public string ReportName { get; set; }

        public int TagID { get; set; } //TagContacts
        public int ActionID { get; set; } //Action Contacts
        public int OpportunityID { get; set; }
    }
}
