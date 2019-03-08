using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.CustomFieldTab;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Fields;

namespace SmartTouch.CRM.Domain.CustomFields
{
    public class CustomFieldTab : EntityBase<int>, IAggregateRoot
    {
        public string Name { get; set; }
        
        public CustomFieldTabStatus StatusId { get; set; }
        public byte SortId { get; set; }
        public int AccountId { get; set; }
        public bool IsLeadAdapterTab { get; set; }
        public IEnumerable<CustomFieldSection> Sections { get; set; }

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(Name.Trim())) AddBrokenRule(CustomFieldBusinessRule.CustomFieldTabNameInvalid);
            if (Sections == null || !Sections.Any()) AddBrokenRule(CustomFieldBusinessRule.OneSectionPerTabRequired);
            else
            {
                foreach (var section in Sections.Where(c => c.StatusId != CustomFieldSectionStatus.Deleted))
                {
                    var sectionNameCount = Sections.Where(c => c.Name.ToLower() == section.Name.ToLower() && c.StatusId != CustomFieldSectionStatus.Deleted).Count();
                    if (sectionNameCount > 1)
                    {
                        AddBrokenRule(CustomFieldBusinessRule.DuplicateSectionName);
                        return;
                    }
                    if (string.IsNullOrEmpty(section.Name.Trim())) AddBrokenRule(CustomFieldBusinessRule.CustomFieldSectionNameInvalid);
                    if (section.CustomFields == null || !section.CustomFields.Any()) AddBrokenRule(CustomFieldBusinessRule.OneCustomFieldPerSectionRequired);
                    else
                    {
                        foreach (Field field in section.CustomFields.Where(c => c.StatusId != FieldStatus.Deleted))
                        {
                            var fieldNameCount = section.CustomFields.Where(c => c.Title.ToLower() == field.Title.ToLower() && c.StatusId != FieldStatus.Deleted).Count();
                            if (fieldNameCount > 1)
                            {
                                AddBrokenRule(CustomFieldBusinessRule.DuplicateFieldName);
                                return;
                            }
                            if (string.IsNullOrEmpty(field.Title.Trim())) AddBrokenRule(CustomFieldBusinessRule.CustomFieldNameInvalid);
                            else if (field.Title.Length > 75) AddBrokenRule(CustomFieldBusinessRule.CustomFieldNamelengthInvalid);
                            
                            if (field.FieldInputTypeId == FieldType.checkbox || field.FieldInputTypeId == FieldType.dropdown
                                || field.FieldInputTypeId == FieldType.multiselectdropdown || field.FieldInputTypeId == FieldType.radio)
                            {
                                if (field.ValueOptions == null || !field.ValueOptions.Any()) AddBrokenRule(CustomFieldBusinessRule.OneValueOptionPerCustomFieldRequired);
                                else
                                    foreach (FieldValueOption valueOption in field.ValueOptions.Where(c => c.IsDeleted != true))
                                    {
                                        var valueOptionNameCount = field.ValueOptions.Where(c => c.Value.ToLower() == valueOption.Value.ToLower() && c.IsDeleted != true).Count();
                                        if (valueOptionNameCount > 1) AddBrokenRule(CustomFieldBusinessRule.DuplicateFieldValueName);
                                        if (string.IsNullOrEmpty(valueOption.Value.Trim())) AddBrokenRule(CustomFieldBusinessRule.CustomFieldValueOptionInvalid);
                                        else if (valueOption.Value.Length > 120) AddBrokenRule(CustomFieldBusinessRule.OptionValueLengthExceeding);
                                    }
                            }
                        }
                    }
                }
            }
        }
    }
}
