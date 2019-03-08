using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SmartTouch.CRM.LeadAdapters.LeadStructure
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "developments")]
    public partial class Developments
    {
        /// <remarks/>
        [XmlElement(ElementName = "developerbuilderagency")]
        public string DeveloperBuilderAgency { get; set; }
        
        /// <remarks/>
        [XmlElement(ElementName = "general")]
        public Development General { get; set; }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("development")]
        public Development[] Development { get; set; }

        public IEnumerable<Development> AllLeads
        {
            get
            {
                var allLeads = new List<Development>();
                allLeads.AddRange(Development);
                allLeads.Add(General);
                return allLeads;
            }
        }

    }
    
    /// <remarks/>
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [XmlRoot(ElementName = "lead")]
    public partial class Lead
    {
        /// <remarks/>
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }


        /// <remarks/>
        [XmlElement(ElementName = "email")]
        public string Email { get; set; }


        /// <remarks/>
        [XmlElement(ElementName = "subject")]
        public string Subject { get; set; }


        /// <remarks/>
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        /// <remarks/>
        [XmlElement(ElementName = "timestamp")]
        public string Timestamp { get; set; }

        /// <remarks/>
        [XmlElement(ElementName = "URL")]
        public string URL { get; set; }

        /// <remarks/>
        [XmlElement(ElementName = "budget")]
        public string Budget { get; set; }

        /// <remarks/>
        [XmlElement(ElementName = "purchasetype")]
        public string Purchasetype { get; set; }


        /// <remarks/>
        [XmlElement(ElementName = "roomsCount")]
        public string RoomsCount { get; set; }

        /// <remarks/>
        [XmlElement(ElementName = "size")]
        public string Size { get; set; }

    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [XmlRoot(ElementName = "development")]
    public partial class Development
    {

        /// <remarks/>
        [XmlElement(ElementName = "developmentname")]
        public string CommunityName { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("lead", IsNullable = false)]
        public Lead[] leads { get; set; }

    }



}
