using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowTriggerTypesDb
    {
        [Key]
        public byte TriggerID { get; set; }
        public string TriggerName { get; set; }
        public byte TriggerCategory { get; set; }
    }
}
