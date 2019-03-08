using LandmarkIT.Enterprise.Common;
using LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities;
using System.Data.Entity;

namespace LandmarkIT.Enterprise.CommunicationManager.Repository
{
    internal class RegistrationContext : DbContext
    {
        public RegistrationContext() : base(EnterpriseConfigurationSection.Instance.CommunicationManager.ConnectionStringName) { }

        public DbSet<SocialRegistration> SocialRegistrations { get; set; }
        public DbSet<MailRegistration> MailRegistrations { get; set; }
        public DbSet<TextRegistration> TextRegistrations { get; set; }
        public DbSet<StorageRegistration> StorageRegistrations { get; set; }


        public DbSet<TextResponse> TextResponse { get; set; }
        public DbSet<MailResponse> MailResponse { get; set; }
    }
}
