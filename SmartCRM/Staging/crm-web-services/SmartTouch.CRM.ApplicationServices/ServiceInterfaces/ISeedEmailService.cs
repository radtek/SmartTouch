using SmartTouch.CRM.ApplicationServices.Messaging.SeedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
   public interface ISeedEmailService
    {
       InsertSeedListResponse InsertSeedList(InsertSeedListRequest request);
       GetSeedListResponse GetSeedList(GetSeedListRquest request);
    }

}
