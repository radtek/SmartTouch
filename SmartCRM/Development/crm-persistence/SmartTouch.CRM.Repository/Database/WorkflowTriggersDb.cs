using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowTriggersDb
    {
        [Key]
        public int WorkflowTriggerID { get; set; }

        [ForeignKey("WorkflowTriggerTypes")]
        public byte TriggerTypeID { get; set; }
        public WorkflowTriggerTypesDb WorkflowTriggerTypes { get; set; }

        [ForeignKey("Workflows")]
        public short WorkflowID { get; set; }
        public WorkflowsDb Workflows { get; set; }

        [ForeignKey("Campaigns")]
        public int? CampaignID { get; set; }
        public CampaignsDb Campaigns { get; set; }

        [ForeignKey("Forms")]
        public int? FormID { get; set; }
        public FormsDb Forms { get; set; }

        [ForeignKey("DropdownValues")]
        public short? LifecycleDropdownValueID { get; set; }
        public DropdownValueDb DropdownValues { get; set; }

        [ForeignKey("Tags")]
        public int? TagID { get; set; }
        public TagsDb Tags { get; set; }

        [ForeignKey("SearchDefinitions")]
        public int? SearchDefinitionID { get; set; }
        public SearchDefinitionsDb SearchDefinitions { get; set; }

        public bool IsStartTrigger { get; set; }

        [ForeignKey("OpportunityStageValues")]
        public short? OpportunityStageID { get; set; }
        public DropdownValueDb OpportunityStageValues { get; set; }

        public string SelectedLinks { get; set; }

        [ForeignKey("LeadAdapter")]
        public int? LeadAdapterID { get; set; }
        public LeadAdapterAndAccountMapDb LeadAdapter { get; set; }

        [ForeignKey("ActionTypeDropdownValue")]
        public short? ActionType { get; set; }
        public DropdownValueDb ActionTypeDropdownValue { get; set; }

        [ForeignKey("TourTypeDropdownValue")]
        public short? TourType { get; set; }
        public DropdownValueDb TourTypeDropdownValue { get; set; }

        public int? LeadScore { get; set; }

        public string WebPage { get; set; }
        public Int16? Duration { get; set; }
        public WebPageDurationOperator? DurationOperator { get; set; }
        public bool IsAnyWebPage { get; set; }

    }
}
