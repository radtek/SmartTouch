using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ImageDomainViewModel
    {
        public byte ImageDomainId { get; set; }
        public string Domain { get; set; }
        public bool Status { get; set; }
        public bool IsDefault { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
    }
}
