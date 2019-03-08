using System.Collections.Generic;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class FieldViewModel
    {
        public int FieldId { get; set; }
        public string Title { get; set; }
        public string FieldCode { get; set; }
        public string Value { get; set; }
        public FieldType FieldInputTypeId { get; set; }
        public string DisplayName { get; set; }
        public bool IsMandatory { get; set; }
        public string ValidationMessage { get; set; }
        public IEnumerable<FieldViewModel> SubFields { get; set; }
        public IEnumerable<CustomFieldValueOptionViewModel> ValueOptions { get; set; }
        public FieldStatus StatusId { get; set; }
        public int? AccountID { get; set; }
        public bool IsCustomField { get; set; }
        public bool IsDropdownField { get; set; }
        public int? DropdownId { get; set; }

        public bool IsLeadAdapterField { get; set; }
        public byte? LeadAdapterType { get; set; }
    }

    public class FormFieldViewModel : FieldViewModel
    {
        public int FormId { get; set; }
        public int FormFieldId { get; set; }
        public byte SortId { get; set; }
        public bool IsHidden { get; set; }
    }
}
