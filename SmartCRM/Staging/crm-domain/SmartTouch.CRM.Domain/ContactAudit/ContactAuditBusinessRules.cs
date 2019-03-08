using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ContactAudit
{
   public static class ContactAuditBusinessRules
    {
       public static readonly BusinessRule ContactIdisRequired = new BusinessRule("Contact Id is required");
      
    }
}
