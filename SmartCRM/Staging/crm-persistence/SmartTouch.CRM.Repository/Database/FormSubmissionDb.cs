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
    public class FormSubmissionDb
    {
        [Key]
        public int FormSubmissionID { get; set; }

        [ForeignKey("Form")]
        public int FormID { get; set; }

        [ForeignKey("Contact")]
        public int ContactID { get; set; }

        [ForeignKey("Status")]
        public short StatusID { get; set; }

        public string IPAddress { get; set; }
        public DateTime SubmittedOn { get; set; }   
        public string SubmittedData { get; set; }
        
        [ForeignKey("LeadSource")]
        public short? LeadSourceID { get; set; }
        public LeadSourceDb LeadSource { get; set; }

        public FormsDb Form { get; set; }
        public ContactsDb Contact { get; set; }
        public StatusesDb Status { get; set; }        
    }
}
