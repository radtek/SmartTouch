using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadAdapterRecordStatusDb
    {
        [Key]
        public LeadAdapterRecordStatus LeadAdapterRecordStatusID { get; set; }
        public string Title { get; set; }
    }
}
