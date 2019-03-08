using System.ComponentModel.DataAnnotations;

namespace SmartTouch.CRM.Repository.Database
{
    public class StatusesDb
    {
        [Key]
        public short StatusID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //[ForeignKey("Modules")]
        //public short ModuleID { get; set; }
        //public ModulesDb Modules { get; set; }
    }
}
