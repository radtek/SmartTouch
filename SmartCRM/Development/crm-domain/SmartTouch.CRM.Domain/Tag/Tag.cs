
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.ValueObjects;

using Nest;

namespace SmartTouch.CRM.Domain.Tag
{
    [ElasticType(IdProperty = "TagID")]
    public class Tag: EntityBase<int>, IAggregateRoot
    {
        string tagName;
        public string TagName { get { return tagName; } set { tagName = !string.IsNullOrEmpty(value)?value.Trim():null;  } }

        string description;
        public string Description { get { return description; } set { description = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public int? Count { get; set; }

        #region autocomplete fields
        /// <summary>
        /// Used only for indexing CompanyName into elastic search for autocomplete suggestions, need to remove this and put a permanent solution.
        /// </summary>
        public AutoCompleteSuggest TagNameAutoComplete { get; set; }

        #endregion
        protected override void Validate()
        {
        }
    }
}
