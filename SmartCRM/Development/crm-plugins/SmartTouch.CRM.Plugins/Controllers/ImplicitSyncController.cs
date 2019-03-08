using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Plugins.Models;
using SmartTouch.CRM.Plugins.Utilities.ImplicitSync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SmartTouch.CRM.Plugins.Controllers
{
    public class ImplicitSyncController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        /// <returns>true on success, otherwise false or Exception should be thrown</returns>
        
        //public bool ISLogin()
        //{
        //    return true;
        //}

        //[Route("login")]
        //public ResponseXml ISWSLogin(XDocument login)
        //{
        //    ResponseXml xmlResp = new ResponseXml(string.Empty, "Login");
        //    string session = Guid.NewGuid().ToString("N");
        //    WASyncHandler handler = new WASyncHandler(session);
        //    XmlDocument doc = new XmlDocument();
        //    string username = "";
        //    string password = "";
        //    try
        //    {
        //        if (!handler.ISLogin(username, password))
        //            throw new Exception("Unknown error");
        //        xmlResp.ISResponse.InnerXml = "<SessionKey>" + session + "</SessionKey>";
        //    }
        //    catch (WAException ex)
        //    {
        //        xmlResp.RetStatus = ex.Status;
        //        xmlResp.ErrorString = ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        xmlResp.RetStatus = -1;
        //        xmlResp.ErrorString = ex.Message;
        //    }
        //    return xmlResp;
        //}

        //public ResponseXml ISAddContact(XDocument xmlContact)
        //{
        //    PersonViewModel personViewModel = new PersonViewModel();
        //    personViewModel.AccountID = 4218;
        //    personViewModel.FirstName = "Siva";
        //    personViewModel.LastName = "Kumar";
        //    personViewModel.CreatedBy = 6887;
        //    personViewModel.CreatedOn = DateTime.Now.ToUniversalTime();
        //    XmlSerializer deserializer = new XmlSerializer(typeof(ContactItem));
        //    TextReader reader = new StreamReader(xmlContact.ToString());
        //    ContactItem contactItem = (ContactItem)deserializer.Deserialize(reader);
        //    var contactService = IoC.Container.GetInstance<IContactService>();
        //    contactService.InsertPerson(new InsertPersonRequest() { PersonViewModel = personViewModel });

        //    return new ResponseXml("","");
        //}
    }
}