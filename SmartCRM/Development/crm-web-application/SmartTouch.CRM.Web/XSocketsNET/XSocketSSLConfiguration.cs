using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using XSockets.Core.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace SmartTouch.CRM.Web.XSocketsNET
{
    //public class XSocketSSLConfiguration : ConfigurationSetting
    //{
    //    public XSocketSSLConfiguration()
    //        : base(new Uri("wss://" + ConfigurationManager.AppSettings["MASTER_ACCOUNT_DNS"]
    //            + ":" + ConfigurationManager.AppSettings["WEBSOCKET_PORT"]))
    //    {
    //        this.CertificateLocation = StoreLocation.LocalMachine;
    //        this.CertificateSubjectDistinguishedName = ConfigurationManager.AppSettings["CERTIFICATE_SUBJECT"];
    //    }
    //}

    //public class XSocketBasicConfiguration : ConfigurationSetting
    //{
    //    public XSocketBasicConfiguration()
    //        : base("ws://" + ConfigurationManager.AppSettings["MASTER_ACCOUNT_DNS"]
    //            + ":" + ConfigurationManager.AppSettings["WEBSOCKET_PORT"])
    //    { }
    //}
}