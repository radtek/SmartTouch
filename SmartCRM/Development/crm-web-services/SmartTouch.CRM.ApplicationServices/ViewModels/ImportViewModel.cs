
using SmartTouch.CRM.Domain.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{


    public class ImportViewModel
    {
        public string OriginalName { get; set; }
        public string ImportContent { get; set; }
        public string ImportType { get; set; }
        public string StorageName { get; set; }
        public int ImportID { get; set; }
        public short AccountID { get; set; }
    }

    

    public class ImportDataListViewModel
    {       
        public IEnumerable<Field> Fields { get; set; }
        public IEnumerable<ImportDataViewModel> Imports { get; set; }
        public IEnumerable<TagViewModel> TagsList { get; set; }
        public string FileName { get; set; }
        public bool UpdateOnDuplicate { get; set; }
        public byte DuplicateLogic { get; set; }
        public bool NeverBounceValidation { get; set; }
        public int AccountID { get; set; }
        public int UserId { get; set; }
        public int OwnerId { get; set; }
        public bool IncludeInReports { get; set; }
        public short LeadSourceId { get; set; }
    }
}
