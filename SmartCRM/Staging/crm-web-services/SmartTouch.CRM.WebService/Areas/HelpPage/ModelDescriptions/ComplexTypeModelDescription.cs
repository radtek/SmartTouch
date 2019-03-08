using System.Collections.ObjectModel;

namespace SmartTouch.CRM.WebService.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Creating ComplexTypeModelDescription class
    /// </summary>
    public class ComplexTypeModelDescription : ModelDescription
    {
        /// <summary>
        /// ComplexTypeModelDescription
        /// </summary>
        public ComplexTypeModelDescription()
        {
            Properties = new Collection<ParameterDescription>();
        }

        /// <summary>
        ///  collection of ParameterDescription properties
        /// </summary>
        public Collection<ParameterDescription> Properties { get; set; }
    }
}