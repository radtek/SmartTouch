using System;

namespace SmartTouch.CRM.WebService.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Describes a type model.
    /// </summary>
    public abstract class ModelDescription
    {
        /// <summary>
        /// Documentation is a property for ModelDescription class
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// ModelType is a property for ModelDescription class
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        /// Name is a property for ModelDescription class
        /// </summary>
        public string Name { get; set; }
    }
}