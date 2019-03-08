using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Notes;
namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class CRMOutlookSync : ValueObjectBase
    {
        public int OutlookSyncId { get; set; }
        public int EntityID { get; set; }
        public AppModules EntityType { get; set; }
        public string OutlookKey { get; set; }
        public OutlookSyncStatus SyncStatus { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public int? LastSyncedBy { get; set; }
        public User User { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
