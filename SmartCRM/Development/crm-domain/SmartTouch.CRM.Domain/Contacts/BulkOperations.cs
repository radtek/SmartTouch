using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class BulkOperations
    {
        public int BulkOperationID { get; set; }
        public int OperationType { get; set; }
        public int OperationID { get; set; }
        public string SearchCriteria { get; set; }
        public int? SearchDefinitionID { get; set; }
        public string AdvancedSearchCriteria { get; set; }
        public int AccountID { get; set; }
        public int UserID { get; set; }
        public short RoleID { get; set; }
        public string AccountPrimaryEmail { get; set; }
        public string AccountDomain { get; set; }
        public int RelationType { get; set; }
        public string ExportSelectedFields { get; set; }
        public int ExportType { get; set; }
        public string DateFormat { get; set; }
        public string TimeZone { get; set; }
        public string ExportFileKey { get; set; }
        public string ExportFileName { get; set; }
        public string UserEmailID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool ActionCompleted { get; set; }
    }
}
