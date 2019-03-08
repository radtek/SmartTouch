using System;

namespace SmartTouch.CRM.WebService.Areas.HelpPage
{
#pragma warning disable 1591
    /// <summary>
    /// This represents a preformatted text sample on the help page. There's a display template named TextSample associated with this class.
    /// </summary>
    public class TextSample
    {
        /// <summary>
        /// Text Sample
        /// </summary>
        /// <param name="text">text</param>
        public TextSample(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            Text = text;
        }

        /// <summary>
        /// Text is a property for TextSample class
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// object referenc
        /// </summary>
        /// <param name="obj">obj</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            TextSample other = obj as TextSample;
            return other != null && Text == other.Text;
        }

        /// <summary>
        /// Get HashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }

        /// <summary>
        /// To String format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Text;
        }
    }
}