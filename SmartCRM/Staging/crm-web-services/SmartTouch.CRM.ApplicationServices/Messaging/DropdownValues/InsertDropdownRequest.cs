using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues
{
  public class InsertDropdownRequest:ServiceRequestBase
    {
      public DropdownViewModel DropdownViewModel { get; set; }
    }
    public class InsertDropdownResponse:ServiceResponseBase
    {
        public virtual DropdownViewModel DropdownViewModel { get; set; }
    }
}
