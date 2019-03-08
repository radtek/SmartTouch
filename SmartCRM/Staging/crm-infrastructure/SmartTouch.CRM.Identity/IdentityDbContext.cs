using System.Data.Entity;

namespace SmartTouch.CRM.Identity
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext() : base("CRMDb") { }
        public IdentityDbContext(string connectionString) : base(connectionString) { }

        //public DbSet<Accounts> Accounts { get; set; }
        //public DbSet<Users> Users { get; set; }
        //public DbSet<Roles> Roles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Accounts>().ToTable("Accounts");
            //modelBuilder.Entity<Users>().ToTable("Users");
            //modelBuilder.Entity<Roles>().ToTable("Roles");
        }
    }
}
