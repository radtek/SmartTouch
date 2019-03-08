using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Tours
{
    public class TourContact
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public ContactType ContactType { get; set; }
        public string Email { get; set; }
        public bool IsCompleted { get; set; }
        public int TourId { get; set; }
        public string TourDetails { get; set; }

        public DateTime? LastUpdatedOn { get; set; }
        public int? LastUpdatedBy { get; set; }
    }
}
