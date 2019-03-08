using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class NotificationData
    {
        public IEnumerable<NotificationContactFieldData> FieldsData { get; set; }
        public string SubmittedData { get; set; }

    }
}
