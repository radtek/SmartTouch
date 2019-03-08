using Facebook;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqToTwitter;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class SocialIntegrationService : ISocialIntegrationService
    {
        readonly IUserRepository userRepository;
        public SocialIntegrationService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        /// <summary>
        /// Post to facebook
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="message"></param>
        /// <param name="image"></param>
        public void PostToFacebook(int uid, string message, string image)
        {
            var user = userRepository.FindBy(uid);
            var accessToken = user.FacebookAccessToken;
            var clientId = user.Account.FacebookAPPID;
            var clientSecret = user.Account.FacebookAPPSecret;
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Access token is empty for user " + uid);
            }
            accessToken = GetExtendedFacebookAccessToken(accessToken, clientId, clientSecret);
            userRepository.UpdateFacebookAccessToken(uid, accessToken);
            var fb = new FacebookClient(accessToken);
            fb.AppId = clientId;
            fb.AppSecret = clientSecret;
            var fbValues = (IDictionary<string, object>)fb.Get("me");
            var parameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(image))
            {
                byte[] byteArray = null;
                using (var webClient = new WebClient())
                {
                    byteArray = webClient.DownloadData(image);
                }
                parameters["picture"] = new FacebookMediaObject
                {
                    ContentType = "image/jpeg",
                    FileName = Path.GetFileName(image)
                }.SetValue(byteArray);

                parameters["caption"] = message;
                fb.Post(fbValues["id"].ToString() + "/photos", parameters);
            }
            else
            {
                parameters["message"] = message;
                fb.Post(fbValues["id"].ToString() + "/feed", parameters);
            }
        }

        /// <summary>
        /// Get extended access token for facebook
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        private string GetExtendedFacebookAccessToken(string accessToken, string clientId, string clientSecret)
        {
            FacebookClient client = new FacebookClient();
            string extendedToken = "";
            try
            {
                dynamic result = client.Get("/oauth/access_token", new
                {
                    grant_type = "fb_exchange_token",
                    client_id = clientId,
                    client_secret = clientSecret,
                    fb_exchange_token = accessToken
                });
                extendedToken = result.access_token;
            }
            catch
            {
                extendedToken = accessToken;
                throw;
            }
            return extendedToken;
        }

        /// <summary>
        /// Get Facebook Login Url
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Uri GetFacebookLoginUri(Uri uri, string guid, int uid)
        {
            FacebookClient fb = new FacebookClient();
            var user = userRepository.FindBy(uid);
            return fb.GetLoginUrl(new
            {
                client_id = user.Account.FacebookAPPID,
                redirect_uri = uri.AbsoluteUri + "?" + "buster=" + guid,
                response_type = "code",
                scope = "publish_actions" // Add other permissions as needed
            });
        }

        /// <summary>
        /// Get facebook access token by code
        /// </summary>
        /// <param name="rediretUri"></param>
        /// <param name="code"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public string GetFacebookAccessTokenByCode(Uri rediretUri, string code, string guid, int uid)
        {
            FacebookClient fb = new FacebookClient();
            var user = userRepository.FindBy(uid);
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = user.Account.FacebookAPPID,
                client_secret = user.Account.FacebookAPPSecret,
                redirect_uri = rediretUri.AbsoluteUri + "?" + "buster=" + guid,
                code = code
            });
            return result.access_token;
        }
        /// <summary>
        /// Revoke facebook connection
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public bool RevokeFacebookConnection(int uid)
        {
            try
            {
                var user = userRepository.FindBy(uid);
                var accessToken = user.FacebookAccessToken;
                var clientId = user.Account.FacebookAPPID;
                var clientSecret = user.Account.FacebookAPPSecret;
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Access token is empty for user " + uid);
                }
                accessToken = GetExtendedFacebookAccessToken(accessToken, clientId, clientSecret);
                userRepository.UpdateFacebookAccessToken(uid, accessToken);
                var fb = new FacebookClient(accessToken);
                fb.AppId = clientId;
                fb.AppSecret = clientSecret;
                var fbValues = (IDictionary<string, object>)fb.Get("me");
                return (bool)fb.Delete(fbValues["id"].ToString());
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while revoking access to the user " + uid, ex);
            }
            return false;
        }
        
        /// <summary>
        /// Get twitter oauth tokens
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public dynamic GetTwitterTokens(Uri uri)
        {
            var auth = new MvcAuthorizer
            {
                CredentialStore = new SessionStateCredentialStore()
            };
            Task task = auth.CompleteAuthorizeAsync(uri);
            task.Wait(3000);
            return auth;
        }

        /// <summary>
        /// Tweet to twitter
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="message"></param>
        /// <param name="twitterKey"></param>
        /// <param name="twitterSecret"></param>
        public void Tweet(int uid, string message)
        {
            var user = userRepository.FindBy(uid);
            var auth = new MvcAuthorizer()
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = user.Account.TwitterAPIKey,
                    ConsumerSecret = user.Account.TwitterAPISecret,
                    OAuthToken = user.TwitterOAuthToken,
                    OAuthTokenSecret = user.TwitterOAuthTokenSecret
                }
            };
            var twitterContext = new TwitterContext(auth);
            twitterContext.TweetAsync(message);
        }
    }
}
