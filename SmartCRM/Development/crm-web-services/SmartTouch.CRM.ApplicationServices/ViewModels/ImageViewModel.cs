using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    interface IImageViewModel
    {
        string OriginalName { get; set; }
        string ImageContent { get; set; }
        string ImageType { get; set; }
        string StorageName { get; set; }
        ImageCategory ImageCategoryID { get; set; }
        string FriendlyName { get; set; }
        int AccountID { get; set; }
    }

    public class ImageViewModel : IImageViewModel
    {
        public string OriginalName { get; set; }
        public string ImageContent { get; set; }
        public string ImageType { get; set; }
        public string StorageName { get; set; }
        public int ImageID { get; set; }
        public ImageCategory ImageCategoryID { get; set; }
        public string FriendlyName { get; set; }
        public int AccountID { get; set; }
        public byte CategoryId { get; set; }
    }
}
