using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Forms
{
    public static class FormBusinessRules
    {
        public static readonly BusinessRule FormNameCannotBeEmpty = new BusinessRule("[|Form name cannot be empty|]");
        public static readonly BusinessRule InvalidAcknowledgmentUrl = new BusinessRule("[|Acknowledgment URL is invalid|]");
        public static readonly BusinessRule AcknowledgementCannotBeEmpty = new BusinessRule("[|Acknowledgment cannot be empty|]");

    }
}
