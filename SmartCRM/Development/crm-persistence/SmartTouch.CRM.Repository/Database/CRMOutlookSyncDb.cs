using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CRMOutlookSyncDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OutlookSyncID { get; set; }
                
        public int EntityID { get; set; }

        [ForeignKey("Module")]
        public byte EntityType { get; set; }
        public ModulesDb Module { get; set; }

        public string OutlookKey { get; set; }
        public short SyncStatus { get; set; }
        public DateTime? LastSyncDate { get; set; }

        [ForeignKey("User")]
        public int? LastSyncedBy { get; set; }
        public UsersDb User { get; set; }

        //public virtual ContactsDb Contact { get; set; }
        //public virtual ActionsDb Action{ get; set; }
        //public virtual NotesDb Note { get; set; }
    }
}
