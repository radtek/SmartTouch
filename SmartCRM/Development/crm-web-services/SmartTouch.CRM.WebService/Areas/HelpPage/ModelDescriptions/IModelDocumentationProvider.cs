using System;
using System.Reflection;

namespace SmartTouch.CRM.WebService.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Creating interface for IModelDocumentationProvider
    /// </summary>
    public interface IModelDocumentationProvider
    {
        /// <summary>
        /// Getting Documentation details for member
        /// </summary>
        /// <param name="member">member</param>
        /// <returns></returns>
        string GetDocumentation(MemberInfo member);

        /// <summary>
        /// Getting Documentation details for type
        /// </summary>
        /// <param name="type">type</param>
        /// <returns></returns>
        string GetDocumentation(Type type);
    }
}