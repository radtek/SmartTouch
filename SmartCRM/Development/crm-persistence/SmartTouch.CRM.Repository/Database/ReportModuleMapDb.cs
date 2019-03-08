using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartTouch.CRM.Repository.Database
{
    public class ReportModuleMapDb
    {
        [Key]
        public byte ReportModuleMapID { get; set; }
        public byte ReportType { get; set; }
        public byte ModuleID { get; set; }
    }
}
