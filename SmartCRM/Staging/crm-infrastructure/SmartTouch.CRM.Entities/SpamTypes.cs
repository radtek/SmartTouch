using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum  SpamTypes : byte
    {
        InternalSpamIPCheck = 1,
        SpamIPAPICheck = 2,
        SubmissionIPLimit = 3,
        EmailValidation = 4,
        SpamKeyWordCheck = 5,
        RepeatedValueCheck = 6,
        InvalidDatatypeCheck = 7
    }
}
