using System;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Fields
{
    public class Field : EntityBase<int>
    {
        public string Title { get; set; }
        public string FieldCode { get; set; }
        public string Value { get; set; }
        public FieldType FieldInputTypeId { get; set; }
        public string DisplayName { get; set; }
        public bool IsMandatory { get; set; }
        public string ValidationMessage { get; set; }        
        public IEnumerable<FieldValueOption> ValueOptions { get; set; }
        public FieldStatus StatusId { get; set; }
        public int? AccountID { get; set; }
        public int DropdownId { get; set; }
        
        public bool IsCustomField { get; set; }
        public bool IsDropdownField { get; set; }

        public bool IsLeadAdapterField { get; set; }
        public byte? LeadAdapterType { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class CustomField : Field
    {
        public int SectionId { get; set; }
        public byte SortId { get; set; }        

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class FormField : Field
    {
        public int FormId { get; set; }
        public int FormFieldId { get; set; }
        public byte SortId { get; set; }
        public bool IsHidden { get; set; }
    }
}
