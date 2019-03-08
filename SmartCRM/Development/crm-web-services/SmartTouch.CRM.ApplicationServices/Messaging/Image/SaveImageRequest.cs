using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Image
{
    public class SaveImageRequest : ServiceRequestBase
    {
        public ImageViewModel ViewModel { get; set; }
        public ImageCategory ImageCategory { get; set; }
    }

    public class SaveImageResponse : ServiceResponseBase
    {
        public ImageViewModel ImageViewModel { get; set; }
    }
}
