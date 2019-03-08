using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.CustomFieldTab
{
    public static class CustomFieldBusinessRule
    {
        public static readonly BusinessRule CustomFieldTabNameInvalid = new BusinessRule("[|Tab name cannot be empty. |]\r\n\r\n");
        public static readonly BusinessRule OneSectionPerTabRequired = new BusinessRule("[|Add at least one section for each tab. |]\r\n\r\n");

        public static readonly BusinessRule DuplicateSectionName = new BusinessRule("[|Section name cannot be duplicate within same tab. |]\r\n\r\n");
        public static readonly BusinessRule CustomFieldSectionNameInvalid = new BusinessRule("[|Section name cannot be empty. |]\r\n\r\n");
        public static readonly BusinessRule OneCustomFieldPerSectionRequired = new BusinessRule("[|Add at least one custom field for each section. |]\r\n\r\n");

        public static readonly BusinessRule DuplicateFieldName = new BusinessRule("[|Field name cannot be duplicate within same section. |]\r\n\r\n");
        public static readonly BusinessRule CustomFieldNameInvalid = new BusinessRule("[|Field name cannot be empty. |]\r\n\r\n");

        public static readonly BusinessRule DuplicateFieldValueName = new BusinessRule("[|Option value cannot be duplicate for a field. |]\r\n\r\n");
        public static readonly BusinessRule CustomFieldValueOptionInvalid = new BusinessRule("[|Option value name cannot be empty. |]\r\n\r\n");
        public static readonly BusinessRule OneValueOptionPerCustomFieldRequired = new BusinessRule("[|Add at least one value option for each predefined field. |]\r\n\r\n");
        public static readonly BusinessRule OptionValueLengthExceeding = new BusinessRule("[|Option value exceeding 120 characters. |]\r\n\r\n");
        public static readonly BusinessRule CustomFieldNamelengthInvalid = new BusinessRule("[|Field Name exceeding 75 characters. |]\r\n\r\n");

    }
}
