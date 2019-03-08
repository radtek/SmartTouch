using SmartTouch.CRM.ApplicationServices.Messaging.Enterprices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IEnterpriseService
    {
        GetAllReportedCouponsResponse GetAllCoupons(GetAllReportedCouponsRequest request);
        SendEmailResponse SendEmail(SendEmailRequest request);
    }
}
