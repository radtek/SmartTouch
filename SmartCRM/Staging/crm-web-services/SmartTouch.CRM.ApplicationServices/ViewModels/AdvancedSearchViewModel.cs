using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class AdvancedSearchViewModel
    {
        /// <summary>
        /// Search Definition ID
        /// </summary>
        public short SearchDefinitionID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SearchDefinitionName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<TagViewModel> TagsList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public short SearchPredicateTypeID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomPredicateScript { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<FilterViewModel> SearchFilters { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int AccountID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ElasticQuery { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastRunDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public short PageNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsFavoriteSearch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPreConfiguredSearch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<FieldViewModel> SelectedColumns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public SearchResult<ContactListEntry> SearchResult { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<FieldViewModel> SearchFields { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int TotalSearchsCount { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FilterViewModel
    {
        public short SearchDefinitionID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public short SearchFilterID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FieldId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public short SearchQualifierTypeID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SearchText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsCustomField { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsDropdownField { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? DropdownId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<int> SelectedValueOption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<FieldValueOption> ValueOptions { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte InputTypeId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SearchDefinitionEntry
    {
        /// <summary>
        /// 
        /// </summary>
        public short SearchDefinitionID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SearchDefinitionName { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SearchDefinitionListViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SearchDefinitionEntry> SearchDefinitions { get; set; }
    }
}
