using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum APICodes: byte
    {
        AccessTokenPhpCode = 1,
        AccessTokenJavaScriptCode = 2,
        AccessTokenDotNetCode = 3,
        InsertPersonPhpCode = 4,
        InsertPersonJavaScriptCode = 5,
        InsertPersonDotNetCode = 6,
        DropDownValuesPhpCode = 7,
        DropDownValuesJavaScriptCode = 8,
        DropDownValuesDotNetCode = 9,
        DropDownFieldsPhpCode = 10,
        DropDownFieldsJavaScriptCode = 11,
        DropDownFieldsDotNetCode = 12,
    }
}
