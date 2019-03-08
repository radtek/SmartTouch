using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging
{
    public class IntegerIdRequest: ServiceRequestBase
    {
        public int Id { get; private set; }

        public IntegerIdRequest(int id)        
        {
            if (id < 1)
                throw new ArgumentException();
            this.Id = id;
        }
    }
}
