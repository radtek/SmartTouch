using LandmarkIT.Enterprise.CommunicationManager.Requests;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities
{
    [Table("StorageRegistration")]
    public class StorageRegistration
    {
        [Key]
        public int StorageRegistrationId { get; set; }
        public  Guid Guid { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Key { get; set; }
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? Port { get; set; }
        public StorageProvider StorageProviderId { get; set; }
    }
}
