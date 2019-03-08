using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadAdapterTypesDb
    {
        [Key]
        public byte LeadAdapterTypeID { get; set; }
        public string Name { get; set; }
        public byte LeadAdapterCommunicationTypeID { get; set; }
    }
}
