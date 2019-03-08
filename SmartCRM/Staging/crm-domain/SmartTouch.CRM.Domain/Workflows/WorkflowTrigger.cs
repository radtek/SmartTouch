using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowTrigger : ValueObjectBase
    {
        public int WorkflowTriggerID { get; set; }
        public WorkflowTriggerType TriggerTypeID { get; set; }
        public short WorkflowID { get; set; }
        public int? CampaignID { get; set; }
        public string CampaignName { get; set; }
        public int? FormID { get; set; }
        public string FormName { get; set; }
        public short? LifecycleDropdownValueID { get; set; }
        public string LifecycleName { get; set; }
        public int? TagID { get; set; }
        public string TagName { get; set; }
        public int? SearchDefinitionID { get; set; }
        public string SearchDefinitionName { get; set; }
        public bool IsStartTrigger { get; set; }
        public short? OpportunityStageID { get; set; }
        public string OpportunityStageName { get; set; }
        public IEnumerable<int> SelectedLinks { get; set; }
        public string SelectedURLs { get; set; }
        public int? LeadAdapterID { get; set; }
        public string LeadAdapterName { get; set; }
        public int? LeadScore { get; set; }
        public string WebPage { get; set; }
        public int? Duration { get; set; }
        public WebPageDurationOperator? Operator { get; set; }
        public bool IsAnyWebPage { get; set; }
        public short? ActionType { get; set; }
        public string ActionTypeName { get; set; }
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
