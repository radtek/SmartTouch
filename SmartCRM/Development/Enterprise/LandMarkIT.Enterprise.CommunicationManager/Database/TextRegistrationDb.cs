using LandmarkIT.Enterprise.CommunicationManager.Requests;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.Database
{
    public class TextRegistrationDb
    {
        [Key]
        public int TextRegistrationID { get; set; }

        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public string APIKey { get; set; }
        public string Token { get; set; }
        public TextProvider TextProviderID { get; set; }
    }
}
