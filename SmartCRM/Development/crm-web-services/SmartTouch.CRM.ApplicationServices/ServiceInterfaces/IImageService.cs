using SmartTouch.CRM.ApplicationServices.Messaging.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IImageService
    {
        DownloadImageResponse DownloadImage(DownloadImageRequest request);
        SaveImageResponse SaveImage(SaveImageRequest request);
    }
}
