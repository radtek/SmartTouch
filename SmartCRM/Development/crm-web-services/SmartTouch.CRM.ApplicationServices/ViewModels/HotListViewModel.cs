using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class HotListViewModel
    {
        public int ReportId { get; set; }      
        public IEnumerable<DropdownValueViewModel> LifecycleStages { get; set; }
        public int LifecycleStage { get; set; }
        public int OwnerId { get; set; }
        public int ShowTop { get; set; }   
        public IEnumerable<Owner> Users { get; set; }

        public int SearchDefinitionID { get; set; }
        public IEnumerable<SearchFilter> Filters { get; set; }
        public SearchPredicateType PredicateType { get; set; }
        public int SearchPredicateTypeID { get; set; }
        public string CustomPredicateScript { get; set; }
        public DateTime? LastRunDate { get; set; }  
        public short PageNumber { get; set; }
        public IList<Field> SelectedColumns { get; set; }
        public IEnumerable<Field> Fields { get; set; }  
        public int[] Module { get; set; }

        public DateTime CustomStartDate { get; set; }
        public DateTime CustomEndDate { get; set; }

        // for OpportunityPipeLine
        public int[] OwnerIds { get; set; }
        public int[] LifeStageIds { get; set; }
        public int[] CommunityIds { get; set; }
        public int[] TourTypeIds { get; set; }
        public bool IsTrafficType { get; set; }
        public int GroupId { get; set; }
        public IEnumerable<DropdownValueViewModel> Communities { get; set; }
        public IEnumerable<DropdownValueViewModel> TourTypes { get; set; }

    }
   
}
