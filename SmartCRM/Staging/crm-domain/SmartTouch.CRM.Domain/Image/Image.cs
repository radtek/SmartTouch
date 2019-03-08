using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Account;

namespace SmartTouch.CRM.Domain.Image
{
    public class Image : EntityBase<int>, IAggregateRoot
    {
        public string Name { get; set; }
        public string StorageKey { get; set; }
        public string ImageType { get; set; }
        public Account.Account Account { get; set; }
        public short ImageCategoryID { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
