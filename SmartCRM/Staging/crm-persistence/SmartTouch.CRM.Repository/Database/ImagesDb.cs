using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ImagesDb
    {
        [Key]
        public int ImageID { get; set; }
        public string FriendlyName { get; set; }
        public string StorageName { get; set; }
        public string OriginalName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public ImageCategory ImageCategoryID { get; set; }
        public int? AccountID { get; set; }

    }
}
