using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CustomFieldTabViewModel
    {
        public int CustomFieldTabId { get; set; }
        public string Name { get; set; }
        public CustomFieldTabStatus StatusId { get; set; }
        public byte SortID { get; set; }
        public int AccountId { get; set; }
        public bool IsLeadAdapterTab { get; set; }
        public IList<CustomFieldSectionViewModel> Sections { get; set; }
    }

    public class CustomFieldSectionViewModel
    {
        public int CustomFieldSectionId { get; set; }
        public CustomFieldSectionStatus StatusId { get; set; }
        public string Name { get; set; }
        public byte SortId { get; set; }
        public int TabId { get; set; }
        public byte SortorderID { get; set; }
        public IList<CustomFieldViewModel> CustomFields { get; set; }
    }

    public class CustomFieldViewModel : FieldViewModel
    {
        public int SectionId { get; set; }
        public byte SortId { get; set; }        
    }

    public class CustomFieldValueOptionViewModel
    {
        public int CustomFieldValueOptionId { get; set; }
        public int CustomFieldId { get; set; }
        public string Value { get; set; }
        public int? Order { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CustomFieldTabsViewModel
    {
        public IEnumerable<CustomFieldTabViewModel> CustomFieldTabs { get; set; }
        public CustomFieldTabViewModel TabTemplate { get; set; }
        public CustomFieldSectionViewModel SectionTemplate{ get; set; }
        public FieldViewModel CustomFieldTemplate { get; set; }
        public CustomFieldValueOptionViewModel ValueOptionTemplate { get; set; }
    }
}
