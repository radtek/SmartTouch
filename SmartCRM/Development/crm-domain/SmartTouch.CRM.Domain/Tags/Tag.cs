
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.ValueObjects;
using System;

namespace SmartTouch.CRM.Domain.Tags
{
    public class Tag: EntityBase<int>, IAggregateRoot
    {
        string tagName;
        public string TagName { get { return tagName; } set { tagName = !string.IsNullOrEmpty(value)?value.Trim():null;  } }

        string description;
        public string Description { get { return description; } set { description = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public int? Count { get; set; }
        public int AccountID { get; set; }
        public int? CreatedBy { get; set; }
        public bool LeadScoreTag { get; set; }
        public int TotalTagCount { get; set; }

        #region autocomplete fields
        /// <summary>
        /// Used only for indexing TagName into elastic search for autocomplete suggestions, need to remove this and put a permanent solution.
        /// </summary>
        public AutoCompleteSuggest TagNameAutoComplete { get; set; }

        #endregion
        protected override void Validate()
        {
        }
    }

    public class RecentPopularTag : Tag
    {
        public string TagType { get; set; }
    }

}
