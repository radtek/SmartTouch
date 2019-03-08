using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ImportDataSettingsDb
    {
        [Key]
        public int ImportDataSettingID { get; set; }
        public bool UpdateOnDuplicate { get; set; }
        public Guid UniqueImportIdentifier { get; set; }
        public AccountsDb Accounts { get; set; }
        [ForeignKey("Accounts")]
        public int AccountID { get; set; }
        public UsersDb Users { get; set; }
        [ForeignKey("Users")]
        public int ProcessBy { get; set; }
        public DateTime ProcessDate { get; set; }
        public byte DuplicateLogic { get; set; }
       // [ForeignKey("LeadAdapterJobLogs")]
        public int? LeadAdaperJobID { get; set; }
       // public LeadAdapterJobLogsDb LeadAdapterJobLogs { get; set; }
    }
}
