using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IUrlService
    {
        string GetUrl(int accountId, ImageCategory category, string imageStorageName);
        string GetUrl(ImageCategory category, string imageStorageName);
        string GetImageHostingUrl();
    }
}
