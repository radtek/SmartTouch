using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class FullContact
    {
        public string requestId { get; set; }
        public List<Photo> photos { get; set; }
        public ContactInfo contactInfo { get; set; }
        public List<SocialProfile> socialProfiles { get; set; }
    }
}
