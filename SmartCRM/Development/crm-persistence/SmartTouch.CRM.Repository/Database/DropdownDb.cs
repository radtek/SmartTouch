using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
  public  class DropdownDb
    {
      [Key]
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public byte DropdownID { get; set; }
    
      public string DropdownName { get; set; }
      [NotMapped]
      public int TotalDropdownCount { get; set; }
      public virtual List<DropdownValueDb> DropdownValues { get; set; }
    }
}
