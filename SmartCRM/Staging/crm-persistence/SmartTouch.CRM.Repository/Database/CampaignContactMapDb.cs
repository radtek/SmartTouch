using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CampaignContactMapDb
    {
        [Key]
        public int CampaignContactMapID { get; set; }
        public int CampaignID { get; set; }
        [ForeignKey("CampaignID")]
        public CampaignsDb Campaign { get; set; }
        public int ContactID { get; set; }
        [ForeignKey("ContactID")]
        public ContactsDb Contacts { get; set; }
        public DateTime CreatedDate { get; set; }
        public Email To { get; set; }
    }
}
