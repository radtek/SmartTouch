
using System.Web;
using System.Web.UI;

namespace SmartTouch.CRM.Internationalization
{
    public class InternationalMaster : MasterPage
    {
        #region internationalizatoin

        string _languageCode;

        public string LanguageCode
        {
            get
            {
                if (_languageCode == null)
                {
                    if (HttpContext.Current.Session[Internationalization.Settings.LanguageCodeSessionKey] != null)
                    {
                        _languageCode = HttpContext.Current.Session[Internationalization.Settings.LanguageCodeSessionKey].ToString();
                    }
                    else
                    {
                        _languageCode = Internationalization.GetBestLanguage(HttpContext.Current.Request, "en");
                        HttpContext.Current.Session[Internationalization.Settings.LanguageCodeSessionKey] = _languageCode;
                    }
                }
                return _languageCode;
            }
            set
            {
                _languageCode = value;
                HttpContext.Current.Session[Internationalization.Settings.LanguageCodeSessionKey] = _languageCode;
            }
        }

        public string _(string text)
        {
            string translated = Internationalization.GetText(text, LanguageCode);
            return translated;
        }

        #endregion
    }
}
