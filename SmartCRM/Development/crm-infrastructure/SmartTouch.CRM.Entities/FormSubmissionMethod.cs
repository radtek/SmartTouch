using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum FormSubmissionMethod : byte
    {
        PlainPost = 1,
        Ajax = 2,
        API =3,
        ClassicPost = 4
    }
}
