using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ThirdPartyClientsDb
    {
        [Key]
        public string ID { get; set; }
        public string Name { get; set; }

        [ForeignKey("Account")]
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Account { get; set; }

        public bool IsActive { get; set; }
        /// <summary>
        /// In minutes
        /// </summary>
        public int RefreshTokenLifeTime { get; set; }
        public string AllowedOrigin { get; set; }

        [ForeignKey("User")]
        public virtual int? LastUpdatedBy { get; set; }
        public virtual UsersDb User { get; set; }

        public DateTime LastUpdatedOn { get; set; }
    }
}
