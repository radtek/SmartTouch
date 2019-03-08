using System;

namespace SmartTouch.CRM.WebService.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Use this attribute to change the name of the <see cref="ModelDescription"/> generated for a type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public sealed class ModelNameAttribute : Attribute
    {
        /// <summary>
        /// ModelNameAttribute is a constructor ModelNameAttribute class
        /// </summary>
        /// <param name="name">name</param>
        public ModelNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Name  is a property for ModelNameAttribute class
        /// </summary>
        public string Name { get; private set; }
    }
}