using System;
using System.Collections.Generic;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;
using System.Text;

namespace SmartTouch.CRM.Domain.Search
{
    public class SearchFilter : ValueObjectBase, ICloneable
    {
        public int SearchDefinitionID { get; set; }
        public short SearchFilterId { get; set; }
        public ContactFields Field { get; set; }
        public SearchQualifier Qualifier { get; set; }
        public string SearchText { get; set; }
        public short? FieldOptionTypeId { get; set; }
        public bool IsCustomField { get; set; }
        public bool IsDropdownField { get; set; }
        public int? DropdownId { get; set; }
        public bool IsDateTime { get; set; }

        public short? DropdownValueId { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("SearchDefinitionID : " + SearchDefinitionID);
            builder.Append(", SearchFilterId : " + SearchFilterId);
            builder.Append(", SearchText : " + SearchText);
            builder.Append(", FieldOptionTypeId : " + FieldOptionTypeId);
            builder.Append(", IsCustomField : " + IsCustomField);
            builder.Append(", IsDropdownField : " + IsDropdownField);
            builder.Append(", DropdownId : " + DropdownId);
            builder.Append(", IsDateTime : " + IsDateTime);
            builder.Append(", Field : " + Field);

            return builder.ToString();
        }

        public object Clone()
        {
            return new SearchFilter()
            {
                Field = this.Field,
                Qualifier = this.Qualifier,
                SearchDefinitionID = this.SearchDefinitionID,
                SearchFilterId = this.SearchFilterId,
                SearchText = this.SearchText,
                IsCustomField = this.IsCustomField,
                FieldOptionTypeId = this.FieldOptionTypeId,
                DropdownValueId = this.DropdownValueId
            };
        }
    }
}
