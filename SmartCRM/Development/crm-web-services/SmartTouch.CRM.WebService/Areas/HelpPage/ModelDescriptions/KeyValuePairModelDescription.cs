namespace SmartTouch.CRM.WebService.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// KeyValuePairModelDescription class implementing ModelDescription class
    /// </summary>
    public class KeyValuePairModelDescription : ModelDescription
    {
        /// <summary>
        /// KeyModelDescription
        /// </summary>
        public ModelDescription KeyModelDescription { get; set; }

        /// <summary>
        /// ValueModelDescription
        /// </summary>
        public ModelDescription ValueModelDescription { get; set; }
    }
}