using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class AccountCustomReportsDb
    {
        [Key]
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public int AccountID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime LastRunOn { get; set; }
        public string SearchCriteria { get; set; }
        public bool IsDeleted { get; set; }
        public string ColumnList { get; set; }
        public string TableList { get; set; }
    }
}
