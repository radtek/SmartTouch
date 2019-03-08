using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CRMOutlookSyncViewModel
    {
        public int OutlookSyncId { get; set; }
        public int EntityID { get; set; }
        public AppModules EntityType { get; set; }
        public string OutlookKey { get; set; }
        public OutlookSyncStatus SyncStatus { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public int? LastSyncedBy { get; set; }
        public UserViewModel User { get; set; }

        public virtual PersonViewModel Contact { get; set; }
        public virtual ActionViewModel Action { get; set; }
        public virtual NoteViewModel Note { get; set; }
        public virtual TourViewModel Tour { get; set; }
    }
}
