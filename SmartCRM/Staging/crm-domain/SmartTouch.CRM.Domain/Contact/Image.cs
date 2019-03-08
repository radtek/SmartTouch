using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contact
{
    public class Image : EntityBase<int>, IAggregateRoot
    {
        public int ContactImageID { get; set; }
        public string OriginalName { get; set; }
        public string ImageContent { get; set; }
        public string ImageType { get; set; }
        public Guid StorageName { get; set; }
        protected override void Validate()
        {
        }
    }
}
