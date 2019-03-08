using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities
{
    [Table("SocialRegistration")]
    public class SocialRegistration
    {
        [Key]
        public int SocialRegistrationId { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Key { get; set; }
        public string Token { get; set; }
        public byte SocialProviderId { get; set; }
    }
}
