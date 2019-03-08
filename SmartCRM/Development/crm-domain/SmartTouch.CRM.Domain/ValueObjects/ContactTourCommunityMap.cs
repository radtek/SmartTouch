using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ContactTourCommunityMap : ValueObjectBase
    {
        public int TourID { get; set; }
        public int TourType { get; set; }
        public int CommunityID { get; set; }
        public int ContactId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? TourDate { get; set; }
        public string Users { get; set; }
        public IEnumerable<int> AssociatedUsers
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Users))
                {
                    return this.Users.Split(',').Select(s => int.Parse(s));
                }
                else
                    return null;
            }
        }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
