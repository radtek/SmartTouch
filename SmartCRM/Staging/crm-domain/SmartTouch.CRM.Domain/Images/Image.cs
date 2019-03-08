using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Images
{
    public class Image : EntityBase<int>, IAggregateRoot
    {
        public int ImageID { get; set; }
        public string FriendlyName { get; set; }
        public string StorageName { get; set; }
        public string OriginalName { get; set; }
        public ImageCategory ImageCategoryID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? AccountID { get; set; }
        public byte CategoryId { get; set; }


        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
