using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.Domain.Community
{
    public class Community : EntityBase<int>, IAggregateRoot
    {
        string communityName;
        public string CommunityName { get { return communityName; } set { communityName = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public int AccountID { get; set; }

        //Eventually this region will be replaced with Address data type
        #region      
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        #endregion

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
