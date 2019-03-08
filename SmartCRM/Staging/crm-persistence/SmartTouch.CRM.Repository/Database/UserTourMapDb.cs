using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartTouch.CRM.Repository.Database
{
    public class UserTourMapDb
    {
        [Key]
        public int UserTourMapID { get; set; }

        public virtual int TourID { get; set; }
        public virtual TourDb Tour { get; set; }

        public virtual int UserID { get; set; }
        public virtual UsersDb  User { get; set; }

        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public Guid UserEmailGuid { get; set; }
        public Guid UserTextGuid { get; set; }
    }
}
