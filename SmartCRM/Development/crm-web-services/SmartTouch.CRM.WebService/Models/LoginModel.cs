using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartTouch.CRM.WebService.Models
{
    /// <summary>
    /// LoginInfo
    /// </summary>
    public class LoginInfo
    {
        /// <summary>
        /// Email that is used to login to SmartTouch Web Application
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        ///  Password that is used to login to SmartTouch Web Application
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///  Client ID that is provided by SmartTouch Administrator
        /// </summary>
        public string ApiKey { get; set; }
    }
    
    /// <summary>
    /// for  Refresh Token Information
    /// </summary>
    public class RefreshTokenInfo
    {
        /// <summary>
        /// RefreshToken is a property for RefreshTokenInfo class
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// ApiKey is a property for RefreshTokenInfo class
        /// </summary>
        public string ApiKey { get; set; }
    }
}