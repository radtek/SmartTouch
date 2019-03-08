using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ISocialIntegrationService
    {
        void PostToFacebook(int uid, string message, string image);
        Uri GetFacebookLoginUri(Uri uri, string guid, int uid);
        string GetFacebookAccessTokenByCode(Uri rediretUri, string code, string guid, int uid);
        //Task<ActionResult> GetTwitterLoginUriAsync(Uri redirectUri, int uid);
        void Tweet(int uid, string message);
        dynamic GetTwitterTokens(Uri uri);
        bool RevokeFacebookConnection(int uid);
    }
}
