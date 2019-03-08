using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SmartTouch.CRM.Domain.Forms
{
    [XmlType("response")]
    public class ApiResponse
    {
        [XmlElement("type")]
        public string Type { get; set; }
        [XmlElement("appears")]
        public string Apprears { get; set; }
        [XmlElement("frequency")]
        public string Frequency { get; set; }
        [XmlElement("error")]
        public string Error { get; set; }
    }
}
