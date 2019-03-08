using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SmartTouch.CRM.WebService.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Creating ParameterDescription class
    /// </summary>
    public class ParameterDescription
    {
        /// <summary>
        /// Creating constructor for ParameterDescription
        /// </summary>
        public ParameterDescription()
        {
            Annotations = new Collection<ParameterAnnotation>();
        }
        /// <summary>
        /// collection of ParameterAnnotations
        /// </summary>
        public Collection<ParameterAnnotation> Annotations { get; private set; }

        /// <summary>
        /// Documentation is a property for ParameterDescription class
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Name is a property for ParameterDescription class
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        /// Creatinng reference variable to ModelDescription class
        /// </summary>
        public ModelDescription TypeDescription { get; set; }
    }
}