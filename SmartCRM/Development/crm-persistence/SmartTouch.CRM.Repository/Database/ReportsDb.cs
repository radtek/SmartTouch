using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.Repository.Database
{
    public class ReportsDb
    {
        [Key]
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public int? AccountID { get; set; }
        public DateTime? LastRunOn { get; set; }

        public byte ReportType { get; set; }
        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public int? CreatedOn { get; set; }
        public DateTime? CreatedBy { get; set; }
    }

    public class DashBoardUserSettingsMap
    {
        [Key]
        public int UserSettingsMapID { get; set; }
        [ForeignKey("Users")]
        public virtual int UserID { get; set; }
        public virtual UsersDb Users { get; set; }
        [ForeignKey("DashboardSettings")]
        public virtual byte DashBoardID { get; set; }
        public virtual DashboardSettingsDb DashboardSettings { get; set; }
    }

    public class DashboardSettingsDb
    {
        [Key]
        public byte DashBoardID { get; set; }
        public string Report { get; set; }
        public bool? Value { get; set; }
    }
}