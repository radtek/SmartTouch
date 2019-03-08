using System;

namespace SmartTouch.CRM.WebService.Areas.HelpPage
{
#pragma warning disable 1591
    /// <summary>
    /// This represents an invalid sample on the help page. There's a display template named InvalidSample associated with this class.
    /// </summary>
    public class InvalidSample
    {
        /// <summary>
        /// Invalid Sample messages
        /// </summary>
        /// <param name="errorMessage">errorMessage</param>
        public InvalidSample(string errorMessage)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException("errorMessage");
            }
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// ErrorMessage is a property for InvalidSample class
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// object reference
        /// </summary>
        /// <param name="obj">obj</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            InvalidSample other = obj as InvalidSample;
            return other != null && ErrorMessage == other.ErrorMessage;
        }

        /// <summary>
        /// Get HashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ErrorMessage.GetHashCode();
        }

        /// <summary>
        /// To string format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ErrorMessage;
        }
    }
}