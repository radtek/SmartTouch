using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SmartTouch.CRM.WebService.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// EnumTypeModelDescription class implementing ModelDescription class
    /// </summary>
    public class EnumTypeModelDescription : ModelDescription
    {
        /// <summary>
        /// Creating constructor for EnumTypeModelDescription
        /// </summary>
        public EnumTypeModelDescription()
        {
            Values = new Collection<EnumValueDescription>();
        }

        /// <summary>
        /// collection of EnumValueDescription values
        /// </summary>
        public Collection<EnumValueDescription> Values { get; private set; }
    }
}