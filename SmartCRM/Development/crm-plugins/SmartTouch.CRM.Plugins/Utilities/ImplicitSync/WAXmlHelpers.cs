using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace SmartTouch.CRM.Plugins.Utilities.ImplicitSync
{
    public interface WAXmlHelpers
    {
    }

    public class ResponseXml : WAXmlHelpers
    {
        public ResponseXml(string sessionID, string action)
        {
            doc_.LoadXml(@"<Session SessionKey="""" Action=""""><RetStatus>0</RetStatus><ErrorString/><ISResponse/></Session>");
            SessionID = sessionID;
            Action = action;
        }

        private XmlDocument doc_ = new XmlDocument();

        public string SessionID
        {
            get { return doc_.DocumentElement.GetAttribute("SessionKey"); }
            private set { doc_.DocumentElement.SetAttribute("SessionKey", string.IsNullOrEmpty(value) ? string.Empty : value); }
        }
        public string Action
        {
            get { return doc_.DocumentElement.GetAttribute("Action"); }
            private set { doc_.DocumentElement.SetAttribute("Action", string.IsNullOrEmpty(value) ? string.Empty : value); }
        }
        public int RetStatus
        {
            get { return Convert.ToInt32(doc_.SelectSingleNode("//RetStatus").InnerText); }
            set { doc_.SelectSingleNode("//RetStatus").InnerText = value.ToString(); }
        }
        public string ErrorString
        {
            get { return doc_.SelectSingleNode("//ErrorString").InnerText; }
            set { doc_.SelectSingleNode("//ErrorString").InnerText = (string.IsNullOrEmpty(value) ? string.Empty : value); }
        }
        public XmlElement ISResponse { get { return doc_.SelectSingleNode("//ISResponse") as XmlElement; } }

        public string Xml { get { return doc_.OuterXml; } }

        public XmlDocument XmlDoc { get { return doc_; } }
    }


    public class RequestXml
    {
        public RequestXml(System.IO.Stream xml)
        {
            doc_.Load(xml);
            if (ISRequest == null)
                throw new Exception("Invalid input xml");
        }

        public string SessionID
        {
            get { return doc_.DocumentElement.GetAttribute("SessionKey"); }
        }
        public string Action
        {
            get { return doc_.DocumentElement.GetAttribute("Action"); }
        }

        public XmlElement ISRequest { get { return doc_.SelectSingleNode("//ISRequest") as XmlElement; } }

        private XmlDocument doc_ = new XmlDocument();
    }
}