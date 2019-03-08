using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class FormAcknowledgement
    {
        public int FormID { get; set; }
        public string Acknowledgement { get; set; }
        public AcknowledgementType AcknowledgementType { get; set; }
    }
}
