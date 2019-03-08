using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace SmartTouch.CRM.Plugins.Models
{
    public class ContactItem
    {
        [XmlElement("Account")]
        public string Account { get; set; }
        [XmlElement("BusinessAddress")]
        public string BusinessAddress { get; set; }
        [XmlElement("BusinessAddressCity")]
        public string BusinessAddressCity { get; set; }
        [XmlElement("BusinessAddressCountry")]
        public string BusinessAddressCountry { get; set; }
        [XmlElement("BusinessAddressPostalCode")]
        public string BusinessAddressPostalCode { get; set; }
        [XmlElement("BusinessAddressState")]
        public string BusinessAddressState { get; set; }
        [XmlElement("BusinessAddressStreet")]
        public string BusinessAddressStreet { get; set; }
        [XmlElement("BusinessTelephoneNumber")]
        public string BusinessTelephoneNumber { get; set; }
        [XmlElement("CompanyName")]
        public string CompanyName { get; set; }
        [XmlElement("CustomerID")]
        public string CustomerID { get; set; }
        [XmlElement("Email1Address")]
        public string Email1Address { get; set; }
        [XmlElement("Email2Address")]
        public string Email2Address { get; set; }
        [XmlElement("Email3Address")]
        public string Email3Address { get; set; }
        [XmlElement("FirstName")]
        public string FirstName { get; set; }
        [XmlElement("HasPicture")]
        public bool HasPicture { get; set; }
        [XmlElement("HomeAddress")]
        public string HomeAddress { get; set; }
        [XmlElement("HomeAddressCity")]
        public string HomeAddressCity { get; set; }
        [XmlElement("HomeAddressCountry")]
        public string HomeAddressCountry { get; set; }
        [XmlElement("HomeAddressPostalCode")]
        public string HomeAddressPostalCode { get; set; }
        [XmlElement("HomeAddressState")]
        public string HomeAddressState { get; set; }
        [XmlElement("HomeAddressStreet")]
        public string HomeAddressStreet { get; set; }
        [XmlElement("HomeTelephoneNumber")]
        public string HomeTelephoneNumber { get; set; }
        [XmlElement("LastName")]
        public string LastName { get; set; }
        [XmlElement("MailingAddress")]
        public string MailingAddress { get; set; }
        [XmlElement("MailingAddressCity")]
        public string MailingAddressCity { get; set; }
        [XmlElement("MailingAddressCountry")]
        public string MailingAddressCountry { get; set; }
        [XmlElement("MailingAddressPostalCode")]
        public string MailingAddressPostalCode { get; set; }
        [XmlElement("MailingAddressState")]
        public string MailingAddressState { get; set; }
        [XmlElement("MailingAddressStreet")]
        public string MailingAddressStreet { get; set; }
        [XmlElement("MailingTelephoneNumber")]
        public string MailingTelephoneNumber { get; set; }
        [XmlElement("MiddleName")]
        public string MiddleName { get; set; }
        [XmlElement("MobilePhoneNumber")]
        public string MobilePhoneNumber { get; set; }
        [XmlElement("PersonalHomePage")]
        public string PersonalHomePage { get; set; }
        [XmlElement("PrimaryTelephoneNumber")]
        public string PrimaryTelephoneNumber { get; set; }
        [XmlElement("ReminderSet")]
        public bool ReminderSet { get; set; }
        [XmlElement("ReminderTime")]
        public DateTime ReminderTime { get; set; }
        [XmlElement("TaskSubject")]
        public string TaskSubject { get; set; }
        [XmlElement("Title")]
        public string Title { get; set; }
        [XmlElement("WebPage")]
        public string WebPage { get; set; }
    }
}