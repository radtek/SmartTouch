using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class TrackActionLogDb
    {
        [Key]
        public long TrackActionLogID { get; set; }
        public long TrackActionID { get; set; }
        public string ErrorMessage { get; set; }
    }
}
