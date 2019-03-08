using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class WebVisitsDb
    {
        [Key]
        public int ContactWebVisitID { get; set; }

        [ForeignKey("Contact")]
        public int ContactID { get; set; }
        public ContactsDb Contact { get; set; }
        
        public DateTime VisitedOn { get; set; }
        public string PageVisited { get; set; }
        
        public short Duration { get; set; }
        public string IPAddress { get; set; }
        public bool IsVisit { get; set; }
        public string VisitReference { get; set; }
        public string ContactReference { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string ISPName { get; set; }
    }
}
