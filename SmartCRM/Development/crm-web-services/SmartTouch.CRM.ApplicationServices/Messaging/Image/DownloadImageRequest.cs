using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Image
{
    public class DownloadImageRequest : ServiceRequestBase
    {
        public string ImageInputUrl { get; set; }
        public ImageCategory ImgCategory { get; set; }  
    }

    public class DownloadImageResponse : ServiceResponseBase
    {
        public ImageViewModel ImageViewModel { get; set; }
    }
}
