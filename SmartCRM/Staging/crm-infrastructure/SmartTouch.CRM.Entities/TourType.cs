using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum TourType : byte
    {
        First = 1,
        [Description("Be-Back")] BeBack= 2,
        Agent = 3
    }
}
