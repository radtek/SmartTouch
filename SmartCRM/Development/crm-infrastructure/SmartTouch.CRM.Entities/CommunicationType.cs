using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
   public enum CommunicationType:byte
    {
       Mail=1,
       Text=2,
       Storage= 3,
       Facebook=4,
       Twitter=5,
       GooglePlus=6,
       LinkedIn=7
    }
}
