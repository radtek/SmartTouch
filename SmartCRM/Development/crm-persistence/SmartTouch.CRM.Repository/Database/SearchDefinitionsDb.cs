using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SearchDefinitionsDb
    {
        [Key]
        public int SearchDefinitionID { get; set; }
        public string SearchDefinitionName { get; set; }
        public string ElasticQuery { get; set; }
        [ForeignKey("SearchPredicateTypes")]
        public short SearchPredicateTypeID { get; set; }
        public virtual SearchPredicateTypesDb SearchPredicateTypes { get; set; }
        public string CustomPredicateScript { get; set; }
        public DateTime? LastRunDate { get; set; }
        [ForeignKey("Users")]
        public int CreatedBy { get; set; }
        public virtual UsersDb Users { get; set; }
        public DateTime CreatedOn { get; set; }
        [ForeignKey("Accounts")]
        public int? AccountID { get; set; }
        public bool IsFavoriteSearch { get; set; }
        public bool IsPreConfiguredSearch { get; set; }
        public virtual AccountsDb Accounts { get; set; }
        public bool? SelectAllSearch { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<SearchFiltersDb> SearchFilters { get; set; }
        public ICollection<SearchDefinitionTagMapDb> SearchTags { get; set; }
        public IEnumerable<FieldsDb> Fields { get; set; }
    }
}
