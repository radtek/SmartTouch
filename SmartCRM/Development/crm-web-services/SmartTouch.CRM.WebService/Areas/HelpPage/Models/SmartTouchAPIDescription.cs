using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace SmartTouch.CRM.WebService.Areas.HelpPage.Models
{
    public class SmartTouchAPIDescription
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Documentation { get; set; }
        public IEnumerable<SmartTouchApiParameterDescription> ParameterDescriptions { get; set; }

        
    }

    public class SmartTouchApiParameterDescription
    {
        public string Name { get; set; }
        public string Documentation { get; set; }
        public string ParameterType { get; set; }
    }
}