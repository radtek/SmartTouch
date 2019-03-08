using System;

using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Owner : ValueObjectBase
    {
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public bool IsDeleted { get; set; }
        public int AccountID { get; set; }
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
