using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
  public enum FileType:byte
    {
      image=1,
      pdf=2,
      word=3,
      excel=4,
      csv=5,
      txt=6,
      rtf=7,
      others=8

    }
}
