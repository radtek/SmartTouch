using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Enterprises
{
   public interface IEnterpriseServicesRepository
    {
        IEnumerable<ReportedCoupons> GetAllReportedCoupons(int pagenumber,int pagesize);
        void BulkInvalidCouponEngagedContacts(IList<InvalidCouponEnagedFrom> invalidCouponData);
    }
}
