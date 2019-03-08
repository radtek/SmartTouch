using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class EmailLinkViewModel
    {
        public int EmailLinkId { get; set; }
        public int SendMailDetailId { get; set; }
        public Url URL { get; set; }
        public int LinkIndex { get; set; }
    }
}
