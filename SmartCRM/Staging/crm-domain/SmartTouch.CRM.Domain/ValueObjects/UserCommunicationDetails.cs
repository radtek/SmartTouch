using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class UserCommunicationDetails
    {
        public int UserId { get; set; }
        public string PrimaryEmail { get; set; }
        public string PrimaryPhone { get; set; }
    }
}
