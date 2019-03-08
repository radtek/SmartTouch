using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailChimp.Lists;
using System.Xml.Serialization;
using SmartTouch.CRM.Domain.Fields;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
    [CLSCompliant(false)]
    [DataContract]
    [Serializable]
    public class RecipientMergeVar : MergeVar
    {
        [DisplayName("Contact ID")]
        [DataMember(Name = "CID")]
        [XmlElement]
        public string CID { get; set; }

        [DisplayName("ContactID")]
        [DataMember(Name = "CONTACTID")]
        [XmlElement]
        public string CONTACTID { get; set; }

        [DisplayName("Campaign ID")]
        [DataMember(Name = "CAMPID")]
        [XmlElement]
        public string CAMPID { get; set; }

        [DisplayName("Email")]
        [DataMember(Name = "EMAIL")]
        [XmlElement]
        public string EMAIL { get; set; }

        [DisplayName("Campaign Recipient ID")]
        [DataMember(Name = "CRID")]
        [XmlElement]
        public string CRID { get; set; }

        [DisplayName("First Name")]
        [DataMember(Name = "FIRSTNAME")]
        [XmlElement]
        public string FIRSTNAME { get; set; }

        [DisplayName("Last Name")]
        [DataMember(Name = "LASTNAME")]
        [XmlElement]
        public string LASTNAME { get; set; }

        [DisplayName("Email")]
        [DataMember(Name = "EMAILID")]
        [XmlElement]
        public string EMAILID { get; set; }

        [DisplayName("Company")]
        [DataMember(Name = "COMPANY")]
        [XmlElement]
        public string COMPANY { get; set; }

        [DisplayName("Title")]
        [DataMember(Name = "TITLE")]
        [XmlElement]
        public string TITLE { get; set; }

        [DisplayName("Facebook URL")]
        [DataMember(Name = "FBURL")]
        [XmlElement]
        public string FBURL { get; set; }

        [DisplayName("Twitter URL")]
        [DataMember(Name = "TWITERURL")]
        [XmlElement]
        public string TWITERURL { get; set; }

        [DisplayName("LinkedIn URL")]
        [DataMember(Name = "LINKEDURL")]
        [XmlElement]
        public string LINKEDURL { get; set; }

        [DisplayName("Google Plus URL")]
        [DataMember(Name = "GPLUSURL")]
        [XmlElement]
        public string GPLUSURL { get; set; }

        [DisplayName("Website URL")]
        [DataMember(Name = "WEBSITEURL")]
        [XmlElement]
        public string WEBSITEURL { get; set; }

        [DisplayName("Blog URL")]
        [DataMember(Name = "BLOGURL")]
        [XmlElement]
        public string BLOGURL { get; set; }

        [DisplayName("Address Line1")]
        [DataMember(Name = "ADDLINE1")]
        [XmlElement]
        public string ADDLINE1 { get; set; }

        [DisplayName("Address Line2")]
        [DataMember(Name = "ADDLINE2")]
        [XmlElement]
        public string ADDLINE2 { get; set; }

        [DisplayName("City")]
        [DataMember(Name = "CITY")]
        [XmlElement]
        public string CITY { get; set; }

        [DisplayName("State")]
        [DataMember(Name = "STATE")]
        [XmlElement]
        public string STATE { get; set; }

        [DisplayName("Zipcode")]
        [DataMember(Name = "ZIPCODE")]
        [XmlElement]
        public string ZIPCODE { get; set; }

        [DisplayName("Country")]
        [DataMember(Name = "COUNTRY")]
        [XmlElement]
        public string COUNTRY { get; set; }
    }
}
