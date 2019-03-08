using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ImportDataSettings : ValueObjectBase
    {
        public int ImportDataSettingID { get; set; }
        public bool UpdateOnDuplicate { get; set; }
        public Guid UniqueImportIdentifier { get; set; }
        public byte DuplicateLogic { get; set; }
        public int AccountID { get; set; }
        public int ProcessBy { get; set; }
        public DateTime ProcessDate { get; set; }
        public int? LeadAdaperJobID { get; set; }
        public bool IncludeInReports { get; set; }
        public short? LeadSourceID { get; set; }
        protected override void Validate()
        {
        }
    }
}
