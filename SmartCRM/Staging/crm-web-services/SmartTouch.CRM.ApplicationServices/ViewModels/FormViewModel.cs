using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class FormViewModel
    {
        public int FormId { get; set; }
        public string Name { get; set; }
        public IEnumerable<TagViewModel> TagsList { get; set; }
        public string Acknowledgement { get; set; }
        public AcknowledgementType AcknowledgementType { get; set; }
        public string HTMLContent { get; set; }
        public int? Submissions { get; set; }
        public IList<FieldViewModel> Fields { get; set; }
        public IList<FormFieldViewModel> FormFields { get; set; }
        public IList<FieldViewModel> CustomFields { get; set; }
        public FormStatus Status { get; set; }
        public int AccountId { get; set; }
        public DropdownValueViewModel LeadSource { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public string DateFormat { get; set; }
        public int UniqueSubmissions { get; set; }
        public int AllSubmissions { get; set; }
        public bool IsAPIForm { get; set; }
        public IList<DropdownValueViewModel> AccountLeadSources { get; set; }

        public IEnumerable<TagViewModel> PopularTags { get; set; }
        public IEnumerable<TagViewModel> RecentTags { get; set; }
    }
}
